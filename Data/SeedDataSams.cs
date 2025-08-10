using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models.Sams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftManagement.Data
{
    /// <summary>
    /// Seed dữ liệu cho database Sams (ShiftManagementSams).
    /// Chỉ tạo dữ liệu mẫu phục vụ chạy thử module sams, không phụ thuộc DB cũ.
    /// </summary>
    public static class SeedDataSams
    {
        public static void Initialize(SamsDbContext context)
        {
            // Đảm bảo DB đã migrate
            context.Database.Migrate();

            // 1) Location
            if (!context.Locations.Any())
            {
                var locations = new[]
                {
                    new Location { Code = "HN",  Name = "Hà Nội" },
                    new Location { Code = "HCM", Name = "TP. Hồ Chí Minh" }
                };
                context.Locations.AddRange(locations);
                context.SaveChanges();
            }

            // 2) Department (thuộc Location)
            if (!context.Departments.Any())
            {
                var locations = context.Locations.AsNoTracking().ToList();
                var departmentNames = new[] { "TPTS - Thực phẩm tươi sống", "TPCN - Thực phẩm công nghệ", "PTP - Phi thực phẩm" };

                var depts = new List<Department>();
                foreach (var loc in locations)
                {
                    foreach (var (name, idx) in departmentNames.Select((n, i) => (n, i)))
                    {
                        depts.Add(new Department
                        {
                            LocationID = loc.LocationID,
                            Code = $"{(loc.Code)}-D{idx + 1}",
                            Name = name,
                            IsActive = true
                        });
                    }
                }
                context.Departments.AddRange(depts);
                context.SaveChanges();
            }

            // 3) Team (mỗi Department tạo 1 Team)
            if (!context.Teams.Any())
            {
                var depts = context.Departments.AsNoTracking().ToList();
                var teams = depts.Select(d => new Team
                {
                    DepartmentID = d.DepartmentID,
                    Code = $"{d.Code}-T1",
                    Name = $"Tổ {d.Name}",
                    IsActive = true
                }).ToList();

                context.Teams.AddRange(teams);
                context.SaveChanges();
            }

            // 4) Position
            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { Code = "EMP", Name = "Nhân viên" },
                    new Position { Code = "LEAD", Name = "Tổ trưởng" }
                );
                context.SaveChanges();
            }

            // 5) ShiftBase (ca chuẩn)
            if (!context.ShiftBases.Any())
            {
                var c1Start = new TimeSpan(7, 30, 0);
                var c1End = new TimeSpan(15, 30, 0);
                var c2Start = new TimeSpan(13, 30, 0);
                var c2End = new TimeSpan(21, 30, 0);

                context.ShiftBases.AddRange(
                    new ShiftBase
                    {
                        Code = "C1",
                        Name = "Ca sáng",
                        StartTime = c1Start,
                        EndTime = c1End,
                        BreakMinutes = 60,
                        IsOvernight = false,
                        Category = "WORK"
                    },
                    new ShiftBase
                    {
                        Code = "C2",
                        Name = "Ca chiều",
                        StartTime = c2Start,
                        EndTime = c2End,
                        BreakMinutes = 60,
                        IsOvernight = false,
                        Category = "WORK"
                    },
                    new ShiftBase
                    {
                        Code = "OFF",
                        Name = "Nghỉ",
                        StartTime = null,
                        EndTime = null,
                        BreakMinutes = 0,
                        IsOvernight = false,
                        Category = "OFF"
                    },
                    new ShiftBase
                    {
                        Code = "P",
                        Name = "Phép năm",
                        StartTime = null,
                        EndTime = null,
                        BreakMinutes = 0,
                        IsOvernight = false,
                        Category = "LEAVE"
                    }
                );
                context.SaveChanges();
            }

            // 6) TeamShiftAlias: alias cơ bản cho tất cả team
            if (!context.TeamShiftAliases.Any())
            {
                var teams = context.Teams.AsNoTracking().ToList();
                var shiftBases = context.ShiftBases.AsNoTracking().ToList();
                var c1 = shiftBases.First(s => s.Code == "C1");
                var c2 = shiftBases.First(s => s.Code == "C2");

                var aliases = new List<TeamShiftAlias>();
                foreach (var t in teams)
                {
                    aliases.Add(new TeamShiftAlias { TeamID = t.TeamID, ShiftBaseID = c1.ShiftBaseID, AliasCode = "X" });     // "X" = C1
                    aliases.Add(new TeamShiftAlias { TeamID = t.TeamID, ShiftBaseID = c2.ShiftBaseID, AliasCode = "C2" });    // alias trùng code base
                    aliases.Add(new TeamShiftAlias { TeamID = t.TeamID, LeaveCode = "OFF", AliasCode = "OFF" });              // OFF
                    aliases.Add(new TeamShiftAlias { TeamID = t.TeamID, LeaveCode = "P", AliasCode = "PN" });                 // PN = phép năm
                }

                context.TeamShiftAliases.AddRange(aliases);
                context.SaveChanges();
            }

            // 7) TeamSettings mặc định
            if (!context.TeamSettings.Any())
            {
                var teams = context.Teams.AsNoTracking().ToList();
                var settings = teams.Select(t => new TeamSettings
                {
                    TeamID = t.TeamID,
                    SettingsJson = "{ \"timezone\": \"Asia/Ho_Chi_Minh\" }",
                    UpdatedAt = DateTime.UtcNow
                });
                context.TeamSettings.AddRange(settings);
                context.SaveChanges();
            }

            // 8) Employees mẫu (mỗi Team 2 người)
            if (!context.Employees.Any())
            {
                var teams = context.Teams.AsNoTracking().ToList();
                var posEmp = context.Positions.First(p => p.Code == "EMP");
                var posLead = context.Positions.First(p => p.Code == "LEAD");

                var list = new List<Employee>();
                foreach (var t in teams)
                {
                    list.Add(new Employee
                    {
                        EmpCode = $"emp_{t.TeamID}_01",
                        FullName = $"Nhân viên Tổ {t.TeamID} - 01",
                        TeamID = t.TeamID,
                        PositionID = posEmp.PositionID,
                        Status = "Active",
                        HireDate = DateTime.Today.AddYears(-1)
                    });
                    list.Add(new Employee
                    {
                        EmpCode = $"emp_{t.TeamID}_02",
                        FullName = $"Nhân viên Tổ {t.TeamID} - 02",
                        TeamID = t.TeamID,
                        PositionID = posEmp.PositionID,
                        Status = "Active",
                        HireDate = DateTime.Today.AddMonths(-6)
                    });
                    // 1 leader (tùy chọn)
                    list.Add(new Employee
                    {
                        EmpCode = $"lead_{t.TeamID}",
                        FullName = $"Tổ trưởng Tổ {t.TeamID}",
                        TeamID = t.TeamID,
                        PositionID = posLead.PositionID,
                        Status = "Active",
                        HireDate = DateTime.Today.AddYears(-2)
                    });
                }

                context.Employees.AddRange(list);
                context.SaveChanges();
            }

            // 9) Holiday + CalendarDate cho cả năm hiện tại (đánh dấu cuối cùng)
            if (!context.CalendarDates.Any())
            {
                var year = DateTime.Today.Year;
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31);

                var cal = new List<CalendarDate>();
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    var weekday = (byte)(((int)d.DayOfWeek + 6) % 7 + 1); // 1=Mon..7=Sun
                    var isWeekend = weekday >= 6; // Sat/Sun
                    cal.Add(new CalendarDate
                    {
                        Date = d.Date,
                        Year = d.Year,
                        Month = d.Month,
                        Day = d.Day,
                        Weekday = weekday,
                        IsWeekend = isWeekend,
                        IsHoliday = false
                    });
                }
                context.CalendarDates.AddRange(cal);
                context.SaveChanges();
            }

            if (!context.Holidays.Any())
            {
                // Thêm 2 ngày lễ mẫu (đồng bộ với Seed cũ)
                var holidays = new[]
                {
                    new Holiday { Date = new DateTime(2025, 1, 1), Name = "Tết Dương lịch" },
                    new Holiday { Date = new DateTime(2025, 9, 2), Name = "Quốc khánh" }
                };
                context.Holidays.AddRange(holidays);
                context.SaveChanges();

                // Đánh dấu IsHoliday trong CalendarDate (nếu tồn tại record)
                var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();
                var calToUpdate = context.CalendarDates.Where(c => holidayDates.Contains(c.Date)).ToList();
                foreach (var c in calToUpdate) c.IsHoliday = true;
                context.SaveChanges();
            }

            // 10) RosterPeriod + RosterEntry mẫu cho một Team
            if (!context.RosterPeriods.Any())
            {
                var anyTeam = context.Teams.AsNoTracking().First();
                var empIds = context.Employees.AsNoTracking().Where(e => e.TeamID == anyTeam.TeamID)
                    .OrderBy(e => e.EmpCode).Select(e => e.EmployeeID).Take(2).ToList();

                var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var period = new RosterPeriod
                {
                    TeamID = anyTeam.TeamID,
                    PeriodStartDate = month,
                    PeriodEndDate = month.AddMonths(1).AddDays(-1),
                    PeriodType = "Month",
                    Status = "Draft",
                    CreatedBy = "seed",
                    CreatedAt = DateTime.UtcNow
                };
                context.RosterPeriods.Add(period);
                context.SaveChanges();

                var shiftC1 = context.ShiftBases.First(s => s.Code == "C1");
                var shiftOFF = context.ShiftBases.First(s => s.Code == "OFF");

                var entries = new List<RosterEntry>
                {
                    new RosterEntry
                    {
                        RosterPeriodID = period.RosterPeriodID,
                        EmployeeID = empIds[0],
                        WorkDate = month.AddDays(4), // 5th day of month
                        ShiftBaseID = shiftC1.ShiftBaseID,
                        LeaveCode = null,
                        Note = "Seed C1"
                    },
                    new RosterEntry
                    {
                        RosterPeriodID = period.RosterPeriodID,
                        EmployeeID = empIds[0],
                        WorkDate = month.AddDays(5),
                        ShiftBaseID = null,
                        LeaveCode = "OFF",
                        Note = "Seed OFF"
                    },
                    new RosterEntry
                    {
                        RosterPeriodID = period.RosterPeriodID,
                        EmployeeID = empIds[1],
                        WorkDate = month.AddDays(4),
                        ShiftBaseID = shiftC1.ShiftBaseID,
                        LeaveCode = null,
                        Note = "Seed C1"
                    }
                };

                context.RosterEntries.AddRange(entries);
                context.SaveChanges();
            }

            // 11) Export Template cơ bản + binding cho 1 team
            if (!context.ExportTemplates.Any())
            {
                var template = new ExportTemplate
                {
                    Name = "Timesheet Basic",
                    Target = "Timesheet",
                    Engine = "xlsx",
                    LayoutJson = "{\"sheets\":[{\"name\":\"Timesheet\",\"columns\":[\"EmpCode\",\"FullName\",\"WorkDate\",\"Kind\",\"CodeDisplay\",\"PlanStart\",\"PlanEnd\",\"PlannedMinutes\"]}]}",
                    Version = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.ExportTemplates.Add(template);
                context.SaveChanges();

                var anyTeam = context.Teams.AsNoTracking().First();
                context.TeamTemplateBindings.Add(new TeamTemplateBinding
                {
                    TeamID = anyTeam.TeamID,
                    TemplateID = template.TemplateID,
                    Target = "Timesheet",
                    EffectiveFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                });
                context.SaveChanges();
            }

            // 12) Không seed TimesheetBatch/Entry để dành cho quy trình Generate,
            //     nhưng nếu cần dữ liệu mẫu, bỏ comment block dưới.

            /*
            if (!context.TimesheetBatches.Any())
            {
                var anyTeam = context.Teams.AsNoTracking().First();
                var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var batch = new TimesheetBatch
                {
                    TeamID = anyTeam.TeamID,
                    PeriodStartDate = month,
                    PeriodEndDate = month.AddMonths(1).AddDays(-1),
                    Status = "Generated",
                    CreatedBy = "seed",
                    CreatedAt = DateTime.UtcNow
                };
                context.TimesheetBatches.Add(batch);
                context.SaveChanges();

                var roster = context.RosterEntries
                    .Where(r => r.RosterPeriod!.TeamID == anyTeam.TeamID
                                && r.RosterPeriod.PeriodStartDate == month)
                    .Include(r => r.ShiftBase)
                    .ToList();

                var entries = new List<TimesheetEntry>();
                foreach (var r in roster)
                {
                    if (r.ShiftBaseID.HasValue && r.ShiftBase != null && (r.ShiftBase.Category ?? "WORK") == "WORK")
                    {
                        var start = r.WorkDate.Date + (r.StartTimeOverride ?? r.ShiftBase.StartTime ?? new TimeSpan(8, 0, 0));
                        var end = r.WorkDate.Date + (r.EndTimeOverride ?? r.ShiftBase.EndTime ?? new TimeSpan(17, 0, 0));
                        if ((r.ShiftBase.IsOvernight || end <= start)) end = end.AddDays(1);

                        var planned = (int)(end - start).TotalMinutes - r.ShiftBase.BreakMinutes;

                        entries.Add(new TimesheetEntry
                        {
                            TimesheetBatchID = batch.TimesheetBatchID,
                            EmployeeID = r.EmployeeID,
                            WorkDate = r.WorkDate,
                            Kind = "WORK",
                            CodeDisplay = r.ShiftBase.Code,
                            PlanStart = start,
                            PlanEnd = end,
                            PlannedMinutes = planned,
                            Note = "Seed"
                        });
                    }
                    else
                    {
                        var kind = r.LeaveCode == "OFF" ? "OFF" : "LEAVE";
                        entries.Add(new TimesheetEntry
                        {
                            TimesheetBatchID = batch.TimesheetBatchID,
                            EmployeeID = r.EmployeeID,
                            WorkDate = r.WorkDate,
                            Kind = kind,
                            CodeDisplay = r.LeaveCode,
                            PlanStart = null,
                            PlanEnd = null,
                            PlannedMinutes = null,
                            Note = "Seed"
                        });
                    }
                }

                context.TimesheetEntries.AddRange(entries);
                context.SaveChanges();
            }
            */
        }
    }
}