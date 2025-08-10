using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftManagement.Data;
using ShiftManagement.Models;
using ShiftManagement.Models.Sams;
using SamsDepartment = ShiftManagement.Models.Sams.Department;

namespace ShiftManagement.Services
{
    public class SamsDataMigrator : ISamsDataMigrator
    {
        private readonly ShiftManagementContext _main;   // DbContext cũ (main)
        private readonly SamsDbContext _sams;            // DbContext mới (sams)
        private readonly ILogger<SamsDataMigrator> _logger;

        private const int BatchSize = 2000;

        public SamsDataMigrator(
            ShiftManagementContext main,
            SamsDbContext sams,
            ILogger<SamsDataMigrator> logger)
        {
            _main = main;
            _sams = sams;
            _logger = logger;
        }

        public async Task RunAsync(bool overwrite = false, CancellationToken ct = default)
        {
            _logger.LogInformation("=== SAMS migration started. Overwrite={Overwrite} ===", overwrite);

            await EnsureBasePositionsAsync(ct);

            var locMap = await MigrateLocationsAsync(overwrite, ct);                 // StoreID -> LocationID
            var deptMap = await MigrateDepartmentsAsync(locMap, overwrite, ct);      // StoreID -> DepartmentID (MAIN)
            var teamMap = await MigrateTeamsAsync(deptMap, overwrite, ct);           // StoreID -> TeamID (DEFAULT)

            var posMap = await MigratePositionsFromRolesAsync(overwrite, ct);        // "EMP"/"LEAD" ids (already ensured)
            var empMap = await MigrateEmployeesAsync(teamMap, posMap, overwrite, ct);// UserID -> EmployeeID

            var shiftBaseMap = await MigrateShiftBasesAsync(overwrite, ct);          // Code -> ShiftBaseID
            await MigrateTeamShiftAliasesAsync(teamMap.Values, shiftBaseMap, overwrite, ct);

            await MigrateCalendarDatesAsync(ct);
            // Nếu có bảng holiday cũ, bạn có thể bổ sung migrate Holidays ở đây.

            await MigrateRosterAsync(teamMap, empMap, shiftBaseMap, overwrite, ct);

            _logger.LogInformation("=== SAMS migration finished ===");
        }

        // 0) Đảm bảo Position cơ bản: EMP, LEAD
        private async Task EnsureBasePositionsAsync(CancellationToken ct)
        {
            var existing = await _sams.Positions.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x, ct);
            var toAdd = new List<Position>();

            if (!existing.ContainsKey("EMP"))
                toAdd.Add(new Position { Code = "EMP", Name = "Employee" });

            if (!existing.ContainsKey("LEAD"))
                toAdd.Add(new Position { Code = "LEAD", Name = "Team Lead/Manager" });

            if (toAdd.Count > 0)
            {
                _sams.Positions.AddRange(toAdd);
                await _sams.SaveChangesAsync(ct);
            }
        }

        // 1) Store -> Location
        private async Task<Dictionary<int, int>> MigrateLocationsAsync(bool overwrite, CancellationToken ct)
        {
            _logger.LogInformation("Migrating Locations (Store -> Location)...");
            var existing = await _sams.Locations.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x, ct);

            var storeQuery = _main.Set<Store>().AsNoTracking();
            var storeToLoc = new Dictionary<int, int>();

            await foreach (var stores in BatchAsync(storeQuery, BatchSize, ct))
            {
                var toInsert = new List<Location>();
                var toUpdate = new List<Location>();

                foreach (var s in stores)
                {
                    var code = $"S{s.StoreID}";
                    if (existing.TryGetValue(code, out var loc))
                    {
                        if (overwrite)
                        {
                            loc.Name = s.StoreName?.Trim() ?? code;
                            toUpdate.Add(loc);
                        }
                        storeToLoc[s.StoreID] = loc.LocationID;
                    }
                    else
                    {
                        var newLoc = new Location
                        {
                            Code = code,
                            Name = s.StoreName?.Trim() ?? code,
                            IsActive = true
                        };
                        toInsert.Add(newLoc);
                    }
                }

                if (toInsert.Count > 0)
                {
                    _sams.Locations.AddRange(toInsert);
                    await _sams.SaveChangesAsync(ct);

                    foreach (var l in toInsert)
                        existing[l.Code] = l;
                }
                if (toUpdate.Count > 0)
                {
                    _sams.Locations.UpdateRange(toUpdate);
                    await _sams.SaveChangesAsync(ct);

                    foreach (var l in toUpdate)
                        existing[l.Code] = l;
                }

                // refresh mapping
                foreach (var s in stores)
                {
                    var code = $"S{s.StoreID}";
                    if (existing.TryGetValue(code, out var loc))
                        storeToLoc[s.StoreID] = loc.LocationID;
                }
            }

            _logger.LogInformation("Locations migrated: {Count}", storeToLoc.Count);
            return storeToLoc;
        }

        // 2) Department mặc định mỗi Store: Code = MAIN
        private async Task<Dictionary<int, int>> MigrateDepartmentsAsync(Dictionary<int, int> storeToLoc, bool overwrite, CancellationToken ct)
        {
            _logger.LogInformation("Migrating Departments (per Store -> MAIN)...");
            var deptMap = new Dictionary<int, int>(); // StoreID -> DepartmentID

            // build index by (LocationID, Code="MAIN")
            var existing = await _sams.Departments
                .AsNoTracking()
                .Where(d => d.Code == "MAIN")
                .ToListAsync(ct);

            var byLoc = existing.GroupBy(d => d.LocationID).ToDictionary(g => g.Key, g => g.First());

            // Need store list for names
            var stores = await _main.Set<Store>().AsNoTracking().ToListAsync(ct);

            var toInsert = new List<SamsDepartment>();
            var toUpdate = new List<SamsDepartment>();

            foreach (var s in stores)
            {
                if (!storeToLoc.TryGetValue(s.StoreID, out var locId)) continue;

                if (byLoc.TryGetValue(locId, out var dept))
                {
                    if (overwrite)
                    {
                        dept.Name = s.StoreName?.Trim() ?? dept.Name;
                        toUpdate.Add(dept);
                    }
                    deptMap[s.StoreID] = dept.DepartmentID;
                }
                else
                {
                    var nd = new SamsDepartment
                    {
                        LocationID = locId,
                        Code = "MAIN",
                        Name = s.StoreName?.Trim() ?? "Main",
                        IsActive = true
                    };
                    toInsert.Add(nd);
                }
            }

            if (toInsert.Count > 0)
            {
                _sams.Departments.AddRange(toInsert);
                await _sams.SaveChangesAsync(ct);
                foreach (var d in toInsert)
                    byLoc[d.LocationID] = d;
            }
            if (toUpdate.Count > 0)
            {
                _sams.Departments.UpdateRange(toUpdate);
                await _sams.SaveChangesAsync(ct);
                foreach (var d in toUpdate)
                    byLoc[d.LocationID] = d;
            }

            foreach (var s in stores)
            {
                if (storeToLoc.TryGetValue(s.StoreID, out var locId) && byLoc.TryGetValue(locId, out var dept))
                    deptMap[s.StoreID] = dept.DepartmentID;
            }

            _logger.LogInformation("Departments migrated: {Count}", deptMap.Count);
            return deptMap;
        }

        // 3) Team mặc định mỗi Department: Code = DEFAULT
        private async Task<Dictionary<int, int>> MigrateTeamsAsync(Dictionary<int, int> storeToDept, bool overwrite, CancellationToken ct)
        {
            _logger.LogInformation("Migrating Teams (per Department -> DEFAULT)...");
            var teamMap = new Dictionary<int, int>(); // StoreID -> TeamID

            var existing = await _sams.Teams
                .AsNoTracking()
                .Where(t => t.Code == "DEFAULT")
                .ToListAsync(ct);

            var byDept = existing.GroupBy(t => t.DepartmentID).ToDictionary(g => g.Key, g => g.First());

            var depts = await _sams.Departments.AsNoTracking().ToDictionaryAsync(d => d.DepartmentID, d => d, ct);

            var toInsert = new List<Team>();
            var toUpdate = new List<Team>();

            foreach (var kv in storeToDept)
            {
                var storeId = kv.Key;
                var deptId = kv.Value;

                if (byDept.TryGetValue(deptId, out var team))
                {
                    if (overwrite && depts.TryGetValue(deptId, out var dept))
                    {
                        team.Name = dept.Name;
                        toUpdate.Add(team);
                    }
                    teamMap[storeId] = team.TeamID;
                }
                else
                {
                    var name = depts.TryGetValue(deptId, out var dept) ? dept.Name : "Default Team";
                    var nt = new Team
                    {
                        DepartmentID = deptId,
                        Code = "DEFAULT",
                        Name = name,
                        IsActive = true
                    };
                    toInsert.Add(nt);
                }
            }

            if (toInsert.Count > 0)
            {
                _sams.Teams.AddRange(toInsert);
                await _sams.SaveChangesAsync(ct);

                foreach (var t in toInsert)
                    byDept[t.DepartmentID] = t;
            }
            if (toUpdate.Count > 0)
            {
                _sams.Teams.UpdateRange(toUpdate);
                await _sams.SaveChangesAsync(ct);

                foreach (var t in toUpdate)
                    byDept[t.DepartmentID] = t;
            }

            foreach (var kv in storeToDept)
            {
                if (byDept.TryGetValue(kv.Value, out var team))
                    teamMap[kv.Key] = team.TeamID;
            }

            _logger.LogInformation("Teams migrated: {Count}", teamMap.Count);
            return teamMap;
        }

        // 4) Positions từ roles (đảm bảo EMP/LEAD)
        private async Task<Dictionary<string, int>> MigratePositionsFromRolesAsync(bool overwrite, CancellationToken ct)
        {
            var map = new Dictionary<string, int>();
            var positions = await _sams.Positions.AsNoTracking().ToListAsync(ct);
            foreach (var p in positions) map[p.Code] = p.PositionID;
            return map;
        }

        // 5) Users -> Employees
        private async Task<Dictionary<int, int>> MigrateEmployeesAsync(
            Dictionary<int, int> storeToTeam,
            Dictionary<string, int> posMap,
            bool overwrite,
            CancellationToken ct)
        {
            _logger.LogInformation("Migrating Employees (User -> Employee)...");
            var usersQ = _main.Set<User>().AsNoTracking();

            // preload user roles
            var userRoles = await _main.Set<UserRole>().AsNoTracking().ToListAsync(ct);
            var roles = await _main.Set<Role>().AsNoTracking().ToDictionaryAsync(r => r.RoleID, r => r.RoleName, ct);
            var rolesByUser = userRoles.GroupBy(ur => ur.UserID)
                                       .ToDictionary(g => g.Key, g => g.Select(ur => roles.GetValueOrDefault(ur.RoleID, "")).ToList());

            var existingByEmpCode = await _sams.Employees.AsNoTracking().ToDictionaryAsync(e => e.EmpCode, e => e, ct);
            var userIdToEmployeeId = new Dictionary<int, int>();

            await foreach (var batch in BatchAsync(usersQ, BatchSize, ct))
            {
                var toInsert = new List<Employee>();
                var toUpdate = new List<Employee>();

                foreach (var u in batch)
                {
                    var empCode = u.Username?.Trim();
                    if (string.IsNullOrEmpty(empCode)) continue;

                    var teamId = u.StoreID.HasValue && storeToTeam.TryGetValue(u.StoreID.Value, out var tId) ? tId : (int?)null;

                    // simple role->position mapping
                    var pCode = "EMP";
                    if (rolesByUser.TryGetValue(u.UserID, out var rs))
                    {
                        var s = string.Join("|", rs).ToLowerInvariant();
                        if (s.Contains("lead") || s.Contains("leader") || s.Contains("manager"))
                            pCode = "LEAD";
                    }
                    var positionId = posMap.TryGetValue(pCode, out var pid) ? pid : (int?)null;

                    if (existingByEmpCode.TryGetValue(empCode, out var e))
                    {
                        if (overwrite)
                        {
                            e.FullName = string.IsNullOrWhiteSpace(u.FullName) ? e.FullName : u.FullName!.Trim();
                            e.TeamID = teamId ?? e.TeamID;
                            e.PositionID = positionId ?? e.PositionID;
                            e.Status = u.Status ? "Active" : "Inactive";
                            e.Phone = u.PhoneNumber?.Trim();
                            e.HireDate ??= u.CreatedAt.Date;
                            toUpdate.Add(e);
                        }
                        userIdToEmployeeId[u.UserID] = e.EmployeeID;
                    }
                    else
                    {
                        var ne = new Employee
                        {
                            EmpCode = empCode,
                            FullName = string.IsNullOrWhiteSpace(u.FullName) ? empCode : u.FullName!.Trim(),
                            TeamID = teamId,
                            PositionID = positionId,
                            Status = u.Status ? "Active" : "Inactive",
                            Phone = u.PhoneNumber?.Trim(),
                            HireDate = u.CreatedAt.Date,
                            CreatedAt = u.CreatedAt
                        };
                        toInsert.Add(ne);
                    }
                }

                if (toInsert.Count > 0)
                {
                    _sams.Employees.AddRange(toInsert);
                    await _sams.SaveChangesAsync(ct);
                    foreach (var e in toInsert)
                        existingByEmpCode[e.EmpCode] = e;
                }
                if (toUpdate.Count > 0)
                {
                    _sams.Employees.UpdateRange(toUpdate);
                    await _sams.SaveChangesAsync(ct);
                    foreach (var e in toUpdate)
                        existingByEmpCode[e.EmpCode] = e;
                }

                // refresh mapping
                foreach (var u in batch)
                {
                    var empCode = u.Username?.Trim();
                    if (!string.IsNullOrEmpty(empCode) &&
                        existingByEmpCode.TryGetValue(empCode, out var e))
                    {
                        userIdToEmployeeId[u.UserID] = e.EmployeeID;
                    }
                }
            }

            _logger.LogInformation("Employees migrated: {Count}", userIdToEmployeeId.Count);
            return userIdToEmployeeId;
        }

        // 6) ShiftCode -> ShiftBase
        private async Task<Dictionary<string, (int id, bool isLeave, bool isOff)>> MigrateShiftBasesAsync(bool overwrite, CancellationToken ct)
        {
            _logger.LogInformation("Migrating ShiftBases (ShiftCode -> ShiftBase)...");
            var existing = await _sams.ShiftBases.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x, ct);
            var map = new Dictionary<string, (int, bool, bool)>();

            var codesQ = _main.Set<ShiftCode>().AsNoTracking();
            await foreach (var batch in BatchAsync(codesQ, BatchSize, ct))
            {
                var toInsert = new List<ShiftBase>();
                var toUpdate = new List<ShiftBase>();

                foreach (var sc in batch)
                {
                    var code = sc.Code?.Trim();
                    if (string.IsNullOrEmpty(code)) continue;

                    var isLeave = sc.IsLeave;
                    var isOff = !isLeave && sc.WorkUnit == 0m;

                    var category = isLeave ? "LEAVE" : isOff ? "OFF" : "WORK";

                    if (existing.TryGetValue(code, out var sb))
                    {
                        if (overwrite)
                        {
                            sb.Name = sc.Description?.Trim();
                            sb.Category = category;
                            toUpdate.Add(sb);
                        }
                        map[code] = (sb.ShiftBaseID, isLeave, isOff);
                    }
                    else
                    {
                        var ns = new ShiftBase
                        {
                            Code = code,
                            Name = sc.Description?.Trim(),
                            Category = category,
                            StartTime = null,
                            EndTime = null,
                            BreakMinutes = 0,
                            IsOvernight = false
                        };
                        toInsert.Add(ns);
                    }
                }

                if (toInsert.Count > 0)
                {
                    _sams.ShiftBases.AddRange(toInsert);
                    await _sams.SaveChangesAsync(ct);
                    foreach (var s in toInsert)
                        existing[s.Code] = s;
                }
                if (toUpdate.Count > 0)
                {
                    _sams.ShiftBases.UpdateRange(toUpdate);
                    await _sams.SaveChangesAsync(ct);
                    foreach (var s in toUpdate)
                        existing[s.Code] = s;
                }

                // refresh local map
                foreach (var sc in batch)
                {
                    var code = sc.Code?.Trim();
                    if (!string.IsNullOrEmpty(code) && existing.TryGetValue(code, out var sb))
                    {
                        var isLeave = sc.IsLeave;
                        var isOff = !isLeave && sc.WorkUnit == 0m;
                        map[code] = (sb.ShiftBaseID, isLeave, isOff);
                    }
                }
            }

            _logger.LogInformation("ShiftBases migrated: {Count}", map.Count);
            return map;
        }

        // 7) TeamShiftAlias: alias theo ShiftCode cho mỗi Team
        private async Task MigrateTeamShiftAliasesAsync(
            IEnumerable<int> teamIds,
            Dictionary<string, (int id, bool isLeave, bool isOff)> shiftBaseMap,
            bool overwrite,
            CancellationToken ct)
        {
            _logger.LogInformation("Migrating TeamShiftAliases...");
            // Load existing as: (TeamID, AliasCode) to entity
            var existing = await _sams.TeamShiftAliases.AsNoTracking()
                .ToDictionaryAsync(x => (x.TeamID, x.AliasCode), x => x, ct);

            var toInsert = new List<TeamShiftAlias>();
            var toUpdate = new List<TeamShiftAlias>();

            foreach (var teamId in teamIds)
            {
                foreach (var kv in shiftBaseMap)
                {
                    var alias = kv.Key;
                    var (sbId, isLeave, isOff) = kv.Value;

                    if (existing.TryGetValue((teamId, alias), out var e))
                    {
                        if (overwrite)
                        {
                            if (isLeave || isOff)
                            {
                                e.ShiftBaseID = null;
                                e.LeaveCode = alias;
                            }
                            else
                            {
                                e.ShiftBaseID = sbId;
                                e.LeaveCode = null;
                            }
                            toUpdate.Add(e);
                        }
                    }
                    else
                    {
                        var na = new TeamShiftAlias
                        {
                            TeamID = teamId,
                            AliasCode = alias,
                            ShiftBaseID = (isLeave || isOff) ? (int?)null : sbId,
                            LeaveCode = (isLeave || isOff) ? alias : null
                        };
                        toInsert.Add(na);
                    }
                }
            }

            if (toInsert.Count > 0)
            {
                // bulk insert in chunks to avoid parameter bloat
                foreach (var chunk in Chunk(toInsert, 2000))
                {
                    _sams.TeamShiftAliases.AddRange(chunk);
                    await _sams.SaveChangesAsync(ct);
                }
            }
            if (toUpdate.Count > 0)
            {
                foreach (var chunk in Chunk(toUpdate, 2000))
                {
                    _sams.TeamShiftAliases.UpdateRange(chunk);
                    await _sams.SaveChangesAsync(ct);
                }
            }

            _logger.LogInformation("TeamShiftAliases migrated.");
        }

        // 8) CalendarDate range theo lịch
        private async Task MigrateCalendarDatesAsync(CancellationToken ct)
        {
            _logger.LogInformation("Migrating CalendarDates...");
            var datesQ = _main.Set<ShiftSchedule>().AsNoTracking().Select(s => s.Date);

            var has = await datesQ.AnyAsync(ct);
            if (!has)
            {
                _logger.LogInformation("No schedules found, skip CalendarDates.");
                return;
            }

            var minDate = await datesQ.MinAsync(ct);
            var maxDate = await datesQ.MaxAsync(ct);
            minDate = minDate.Date;
            maxDate = maxDate.Date;

            // Expand a bit to cover month bounds
            var start = new DateTime(minDate.Year, minDate.Month, 1);
            var endMonth = new DateTime(maxDate.Year, maxDate.Month, 1);
            var end = endMonth.AddMonths(1).AddDays(-1);

            var existing = await _sams.CalendarDates.AsNoTracking()
                .Where(d => d.Date >= start && d.Date <= end)
                .ToDictionaryAsync(d => d.Date, d => d, ct);

            var add = new List<CalendarDate>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (existing.ContainsKey(d)) continue;
                var wd = (byte)((int)d.DayOfWeek == 0 ? 7 : (int)d.DayOfWeek); // Monday=1..Sunday=7
                var isWeekend = wd >= 6;

                add.Add(new CalendarDate
                {
                    Date = d,
                    Year = d.Year,
                    Month = d.Month,
                    Day = d.Day,
                    Weekday = wd,
                    IsWeekend = isWeekend,
                    IsHoliday = false
                });
            }

            if (add.Count > 0)
            {
                foreach (var chunk in Chunk(add, 2000))
                {
                    _sams.CalendarDates.AddRange(chunk);
                    await _sams.SaveChangesAsync(ct);
                }
            }

            _logger.LogInformation("CalendarDates ensured for range {Start:d}..{End:d}", start, end);
        }

        // 9) ShiftSchedule/ShiftScheduleDetail -> RosterPeriod (Month) + RosterEntry (unique per day)
        private async Task MigrateRosterAsync(
            Dictionary<int, int> storeToTeam,
            Dictionary<int, int> userToEmployee,
            Dictionary<string, (int id, bool isLeave, bool isOff)> shiftBaseMap,
            bool overwrite,
            CancellationToken ct)
        {
            _logger.LogInformation("Migrating Roster (Schedules -> Periods & Entries)...");
            // Preload needed lookups
            var employeeSet = await _sams.Employees.AsNoTracking().ToDictionaryAsync(e => e.EmployeeID, e => e, ct);
            var empByUserId = userToEmployee; // already mapped

            // Period cache: (TeamID, Year, Month) -> RosterPeriod
            var existingPeriods = await _sams.RosterPeriods.AsNoTracking()
                .ToListAsync(ct);
            var periodKeyMap = existingPeriods.ToDictionary(
                p => (p.TeamID, p.PeriodStartDate.Year, p.PeriodStartDate.Month),
                p => p);

            // For conflict handling on RosterEntry: we will upsert by (RosterPeriodID, EmployeeID, WorkDate)
            // Load existing keys lazily per period when needed.

            // Build source query joining details
            var detailsQ =
                from d in _main.Set<ShiftScheduleDetail>().AsNoTracking()
                join s in _main.Set<ShiftSchedule>().AsNoTracking() on d.ScheduleID equals s.ScheduleID
                join sc in _main.Set<ShiftCode>().AsNoTracking() on d.ShiftCodeID equals sc.ShiftCodeID
                select new
                {
                    s.ScheduleID,
                    s.EmployeeID,       // main user id
                    s.StoreID,          // for team mapping
                    s.Date,
                    DetailID = d.DetailID,
                    ShiftCode = sc.Code,
                    IsLeave = sc.IsLeave,
                    WorkUnit = d.WorkUnit
                };

            // Group per day per user to select 1 representative code
            // We’ll stream ordered by date to keep memory sane
            var ordered = detailsQ.OrderBy(x => x.Date).ThenBy(x => x.EmployeeID).ThenBy(x => x.DetailID);

            // Batch by date windows
            var buffer = new List<dynamic>(BatchSize);
            DateTime? currentWindowStart = null;
            DateTime? currentWindowEnd = null;

            await foreach (var chunk in BatchAsync(ordered, BatchSize, ct))
            {
                buffer.Clear();
                buffer.AddRange(chunk);
                if (buffer.Count == 0) continue;

                currentWindowStart = buffer.Min(x => (DateTime)x.Date).Date;
                currentWindowEnd = buffer.Max(x => (DateTime)x.Date).Date;

                // Prepare staging grouped by (Store, User, Date)
                var groups = buffer
                    .GroupBy(x => new { x.StoreID, x.EmployeeID, Day = ((DateTime)x.Date).Date })
                    .ToList();

                // Create or get periods for all teams-months seen
                var neededPeriodKeys = groups
                    .Select(g =>
                    {
                        var storeId = g.Key.StoreID;
                        int t;
                        var teamId = storeToTeam.TryGetValue(storeId, out t) ? t : 0;
                        var day = g.Key.Day;
                        return (teamId, year: day.Year, month: day.Month);
                    })
                    .Where(k => k.teamId != 0)
                    .Distinct()
                    .ToList();

                var newPeriods = new List<RosterPeriod>();
                foreach (var pk in neededPeriodKeys)
                {
                    if (!periodKeyMap.ContainsKey(pk))
                    {
                        var start = new DateTime(pk.year, pk.month, 1);
                        var end = start.AddMonths(1).AddDays(-1);
                        var np = new RosterPeriod
                        {
                            TeamID = pk.teamId,
                            PeriodStartDate = start,
                            PeriodEndDate = end,
                            PeriodType = "Month",
                            Status = "Draft",
                            CreatedBy = "Migrator",
                            CreatedAt = DateTime.UtcNow
                        };
                        newPeriods.Add(np);
                        periodKeyMap[pk] = np; // temp; after save it will have ID
                    }
                }
                if (newPeriods.Count > 0)
                {
                    _sams.RosterPeriods.AddRange(newPeriods);
                    await _sams.SaveChangesAsync(ct);
                }

                // Build per-period existing entry keys for range
                var periodIds = neededPeriodKeys
                    .Select(pk => periodKeyMap[pk].RosterPeriodID)
                    .Where(id => id != 0)
                    .Distinct()
                    .ToList();

                var existingEntries = await _sams.RosterEntries.AsNoTracking()
                    .Where(r => periodIds.Contains(r.RosterPeriodID)
                             && r.WorkDate >= currentWindowStart && r.WorkDate <= currentWindowEnd)
                    .Select(r => new { r.RosterPeriodID, r.EmployeeID, r.WorkDate, r.RosterEntryID })
                    .ToListAsync(ct);
                var existingKeySet = existingEntries
                    .ToDictionary(x => (x.RosterPeriodID, x.EmployeeID, x.WorkDate.Date), x => x.RosterEntryID);

                var toInsert = new List<RosterEntry>();
                var toUpdate = new List<RosterEntry>();

                foreach (var g in groups)
                {
                    var storeId = g.Key.StoreID;
                    int teamId;
                    if (!storeToTeam.TryGetValue(storeId, out teamId) || teamId == 0) continue;

                    int empId;
                    if (!empByUserId.TryGetValue(g.Key.EmployeeID, out empId)) continue;

                    var day = g.Key.Day;

                    // select representative code:
                    // - prefer WORK (not leave/off) with max WorkUnit
                    // - else prefer OFF
                    // - else any LEAVE
                    var bestWork = g.Where(x =>
                    {
                        var code = ((string)x.ShiftCode)?.Trim() ?? "";
                        if (!shiftBaseMap.TryGetValue(code, out var m)) return false;
                        return !m.isLeave && !m.isOff;
                    })
                    .OrderByDescending(x => (decimal)x.WorkUnit)
                    .FirstOrDefault();

                    var off = g.FirstOrDefault(x =>
                    {
                        var code = ((string)x.ShiftCode)?.Trim() ?? "";
                        return shiftBaseMap.TryGetValue(code, out var m) && m.isOff;
                    });

                    var leave = g.FirstOrDefault(x =>
                    {
                        var code = ((string)x.ShiftCode)?.Trim() ?? "";
                        return shiftBaseMap.TryGetValue(code, out var m) && m.isLeave;
                    });

                    var selected = bestWork ?? off ?? leave;
                    if (selected == null) continue;

                    // compose entry
                    var selCode = ((string)selected.ShiftCode).Trim();
                    var (sbId, isLeave, isOff) = shiftBaseMap.TryGetValue(selCode, out var val) ? val : (0, false, false);

                    var periodKey = (teamId, day.Year, day.Month);
                    var period = periodKeyMap[periodKey];

                    var note = string.Join(",", g.Select(x => (string)x.ShiftCode).Distinct());

                    if (existingKeySet.TryGetValue((period.RosterPeriodID, empId, day), out var reId))
                    {
                        if (overwrite)
                        {
                            var e = new RosterEntry
                            {
                                RosterEntryID = reId,
                                RosterPeriodID = period.RosterPeriodID,
                                EmployeeID = empId,
                                WorkDate = day,
                                ShiftBaseID = (isLeave || isOff) ? (int?)null : sbId,
                                LeaveCode = (isLeave || isOff) ? selCode : null,
                                StartTimeOverride = null,
                                EndTimeOverride = null,
                                Note = note,
                                Attributes = null
                            };
                            toUpdate.Add(e);
                        }
                    }
                    else
                    {
                        toInsert.Add(new RosterEntry
                        {
                            RosterPeriodID = period.RosterPeriodID,
                            EmployeeID = empId,
                            WorkDate = day,
                            ShiftBaseID = (isLeave || isOff) ? (int?)null : sbId,
                            LeaveCode = (isLeave || isOff) ? selCode : null,
                            StartTimeOverride = null,
                            EndTimeOverride = null,
                            Note = note,
                            Attributes = null
                        });
                    }
                }

                if (toInsert.Count > 0)
                {
                    foreach (var chunkIns in Chunk(toInsert, 2000))
                    {
                        _sams.RosterEntries.AddRange(chunkIns);
                        await _sams.SaveChangesAsync(ct);
                    }
                }
                if (toUpdate.Count > 0)
                {
                    foreach (var chunkUpd in Chunk(toUpdate, 2000))
                    {
                        _sams.RosterEntries.UpdateRange(chunkUpd);
                        await _sams.SaveChangesAsync(ct);
                    }
                }
            }

            _logger.LogInformation("Roster migration done.");
        }

        // Helpers
        private static async IAsyncEnumerable<List<T>> BatchAsync<T>(
            IQueryable<T> query, int size,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            var offset = 0;
            while (true)
            {
                var page = await query.Skip(offset).Take(size).ToListAsync(ct);
                if (page.Count == 0) yield break;
                yield return page;
                offset += page.Count;
            }
        }

        private static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            var list = new List<T>(size);
            foreach (var item in source)
            {
                list.Add(item);
                if (list.Count >= size)
                {
                    yield return list;
                    list = new List<T>(size);
                }
            }
            if (list.Count > 0) yield return list;
        }
    }
}