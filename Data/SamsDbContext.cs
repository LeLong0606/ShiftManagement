using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models.Sams;
using ShiftManagement.Models.Sams.Views;

namespace ShiftManagement.Data
{
    public class SamsDbContext : DbContext
    {
        public SamsDbContext(DbContextOptions<SamsDbContext> options) : base(options) { }

        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<CalendarDate> CalendarDates => Set<CalendarDate>();
        public DbSet<Holiday> Holidays => Set<Holiday>();
        public DbSet<ShiftBase> ShiftBases => Set<ShiftBase>();
        public DbSet<TeamSettings> TeamSettings => Set<TeamSettings>();
        public DbSet<TeamShiftAlias> TeamShiftAliases => Set<TeamShiftAlias>();
        public DbSet<TeamShiftOverride> TeamShiftOverrides => Set<TeamShiftOverride>();
        public DbSet<RosterPeriod> RosterPeriods => Set<RosterPeriod>();
        public DbSet<RosterEntry> RosterEntries => Set<RosterEntry>();
        public DbSet<TimesheetBatch> TimesheetBatches => Set<TimesheetBatch>();
        public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
        public DbSet<ExportTemplate> ExportTemplates => Set<ExportTemplate>();
        public DbSet<TeamTemplateBinding> TeamTemplateBindings => Set<TeamTemplateBinding>();
        public DbSet<TemplateFieldMap> TemplateFieldMaps => Set<TemplateFieldMap>();
        public DbSet<ExportRun> ExportRuns => Set<ExportRun>();

        // Views
        public DbSet<VRosterForExport> VRosterForExport => Set<VRosterForExport>();
        public DbSet<VTimesheetForExport> VTimesheetForExport => Set<VTimesheetForExport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("sams");

            // Unique constraints and indexes (existing)
            modelBuilder.Entity<Department>()
                .HasIndex(x => new { x.LocationID, x.Code })
                .IsUnique();
            // FK lookup by Location
            modelBuilder.Entity<Department>()
                .HasIndex(x => x.LocationID);

            modelBuilder.Entity<Team>()
                .HasIndex(x => new { x.DepartmentID, x.Code })
                .IsUnique();
            // Optional: fast lookup teams by department
            modelBuilder.Entity<Team>()
                .HasIndex(x => x.DepartmentID);

            modelBuilder.Entity<Position>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.EmpCode)
                .IsUnique();
            // Common filters: by Team and Status; by Position
            modelBuilder.Entity<Employee>()
                .HasIndex(x => new { x.TeamID, x.Status });
            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.TeamID);
            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.PositionID);

            modelBuilder.Entity<ShiftBase>()
                .HasIndex(x => x.Code)
                .IsUnique();
            modelBuilder.Entity<ShiftBase>()
                .HasIndex(x => x.Category);

            modelBuilder.Entity<TeamShiftOverride>()
                .HasIndex(x => new { x.TeamID, x.ShiftBaseID })
                .IsUnique();
            modelBuilder.Entity<TeamShiftOverride>()
                .HasIndex(x => x.TeamID);

            // Avoid duplicate alias per team and speed lookups by alias
            modelBuilder.Entity<TeamShiftAlias>()
                .HasIndex(x => new { x.TeamID, x.AliasCode })
                .IsUnique();
            // Existing rule: uniqueness on (TeamID, ShiftBaseID, LeaveCode)
            modelBuilder.Entity<TeamShiftAlias>()
                .HasIndex(x => new { x.TeamID, x.ShiftBaseID, x.LeaveCode })
                .IsUnique();

            // Period queries: by team, date range, status
            modelBuilder.Entity<RosterPeriod>()
                .HasIndex(x => new { x.TeamID, x.PeriodStartDate, x.PeriodEndDate });
            modelBuilder.Entity<RosterPeriod>()
                .HasIndex(x => new { x.TeamID, x.Status, x.PeriodStartDate })
                .IncludeProperties(x => new { x.PeriodEndDate });

            // RosterEntry common scans: by employee/date, by period
            modelBuilder.Entity<RosterEntry>()
                .HasIndex(x => new { x.EmployeeID, x.WorkDate });
            modelBuilder.Entity<RosterEntry>()
                .HasIndex(x => new { x.RosterPeriodID, x.EmployeeID, x.WorkDate })
                .IsUnique();
            // Cross-day filters by date
            modelBuilder.Entity<RosterEntry>()
                .HasIndex(x => x.WorkDate);

            // Timesheet batch access: by team, date, status
            modelBuilder.Entity<TimesheetBatch>()
                .HasIndex(x => new { x.TeamID, x.PeriodStartDate, x.PeriodEndDate });
            modelBuilder.Entity<TimesheetBatch>()
                .HasIndex(x => new { x.TeamID, x.Status, x.PeriodStartDate });

            // Timesheet entry access: by batch, employee/date; cover common projections
            modelBuilder.Entity<TimesheetEntry>()
                .HasIndex(x => new { x.EmployeeID, x.WorkDate });
            modelBuilder.Entity<TimesheetEntry>()
                .HasIndex(x => new { x.TimesheetBatchID, x.EmployeeID, x.WorkDate })
                .IncludeProperties(x => new { x.Kind, x.CodeDisplay });

            // Export template lookups
            modelBuilder.Entity<ExportTemplate>()
                .HasIndex(x => x.Name)
                .IsUnique();
            modelBuilder.Entity<ExportTemplate>()
                .HasIndex(x => new { x.Target, x.IsActive });

            // Template field uniqueness within a template
            modelBuilder.Entity<TemplateFieldMap>()
                .HasIndex(x => new { x.TemplateID, x.Placeholder })
                .IsUnique();
            modelBuilder.Entity<TemplateFieldMap>()
                .HasIndex(x => x.TemplateID);

            // Effective binding lookups: current active binding per team/target
            modelBuilder.Entity<TeamTemplateBinding>()
                .HasIndex(x => new { x.TeamID, x.TemplateID, x.Target, x.EffectiveFrom })
                .IsUnique();
            modelBuilder.Entity<TeamTemplateBinding>()
                .HasIndex(x => new { x.TeamID, x.Target })
                .HasFilter("[EffectiveTo] IS NULL");

            // Export runs: recent per team/target
            modelBuilder.Entity<ExportRun>()
                .HasIndex(x => new { x.TeamID, x.Target, x.CreatedAt });
            modelBuilder.Entity<ExportRun>()
                .HasIndex(x => x.Status);

            // CalendarDate helpers for month and quick holiday/weekend checks
            modelBuilder.Entity<CalendarDate>()
                .HasIndex(x => new { x.Year, x.Month });
            modelBuilder.Entity<CalendarDate>()
                .HasIndex(x => x.IsHoliday)
                .HasFilter("[IsHoliday] = 1");
            modelBuilder.Entity<CalendarDate>()
                .HasIndex(x => x.IsWeekend)
                .HasFilter("[IsWeekend] = 1");

            // Holiday unique by Date
            modelBuilder.Entity<Holiday>()
                .HasIndex(x => x.Date)
                .IsUnique();

            // Check constraints
            modelBuilder.Entity<RosterPeriod>()
                .HasCheckConstraint("CK_RosterPeriod_Range", "[PeriodEndDate] >= [PeriodStartDate]");

            modelBuilder.Entity<RosterEntry>()
                .HasCheckConstraint("CK_RosterEntry_OneKind",
                    "(([ShiftBaseID] IS NOT NULL AND [LeaveCode] IS NULL) OR ([ShiftBaseID] IS NULL AND [LeaveCode] IS NOT NULL))");

            // Views are keyless (already annotated)
            modelBuilder.Entity<VRosterForExport>().ToView("vRosterForExport", "sams");
            modelBuilder.Entity<VTimesheetForExport>().ToView("vTimesheetForExport", "sams");
        }

        // Wrapper to call stored procedure sams.sp_GenerateTimesheetFromRoster
        public async Task<long> GenerateTimesheetFromRosterAsync(
            int teamId, System.DateTime periodStart, System.DateTime periodEnd, string? createdBy = null, CancellationToken cancellationToken = default)
        {
            var pTeam = new SqlParameter("@TeamID", teamId);
            var pStart = new SqlParameter("@PeriodStart", periodStart) { SqlDbType = SqlDbType.Date };
            var pEnd = new SqlParameter("@PeriodEnd", periodEnd) { SqlDbType = SqlDbType.Date };
            var pCreatedBy = new SqlParameter("@CreatedBy", createdBy ?? (object)System.DBNull.Value);
            var pOut = new SqlParameter("@TimesheetBatchID", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };

            var sql = "EXEC sams.sp_GenerateTimesheetFromRoster @TeamID, @PeriodStart, @PeriodEnd, @CreatedBy, @TimesheetBatchID OUTPUT";

            await Database.ExecuteSqlRawAsync(sql, new object[] { pTeam, pStart, pEnd, pCreatedBy, pOut }, cancellationToken);

            return (long)(pOut.Value ?? 0L);
        }
    }
}