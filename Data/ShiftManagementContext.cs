using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models;

namespace ShiftManagement.Data
{
    public class ShiftManagementContext : DbContext
    {
        public ShiftManagementContext(DbContextOptions<ShiftManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ShiftCode> ShiftCodes { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<ShiftSchedule> ShiftSchedules { get; set; }
        public DbSet<ShiftScheduleDetail> ShiftScheduleDetails { get; set; }
        public DbSet<ShiftHistory> ShiftHistories { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<WorkUnitRule> WorkUnitRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⚙️ Map đúng tên bảng trong database:
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Store>().ToTable("Stores");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<User>().ToTable("Users"); 
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<ShiftCode>().ToTable("ShiftCodes");
            modelBuilder.Entity<Holiday>().ToTable("Holidays");
            modelBuilder.Entity<ShiftSchedule>().ToTable("ShiftSchedule");
            modelBuilder.Entity<ShiftScheduleDetail>().ToTable("ShiftScheduleDetail");
            modelBuilder.Entity<ShiftHistory>().ToTable("ShiftHistories");
            modelBuilder.Entity<Log>().ToTable("Logs");
            modelBuilder.Entity<WorkUnitRule>().ToTable("WorkUnitRules");

            // ▶️ Unique constraints
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserID, ur.RoleID })
                .IsUnique();

            modelBuilder.Entity<ShiftSchedule>()
                .HasIndex(s => new { s.EmployeeID, s.Date })
                .IsUnique();

            // ▶️ Quan hệ
            // ShiftSchedule - CreatedBy (User)
            modelBuilder.Entity<ShiftSchedule>()
                .HasOne(s => s.CreatedUser)
                .WithMany(u => u.CreatedSchedules)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ShiftSchedule - Employee (User)
            modelBuilder.Entity<ShiftSchedule>()
                .HasOne(s => s.Employee)
                .WithMany(u => u.ShiftSchedules)
                .HasForeignKey(s => s.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftScheduleDetail
            modelBuilder.Entity<ShiftScheduleDetail>()
                .HasOne(sd => sd.Schedule)
                .WithMany(s => s.ShiftScheduleDetails)
                .HasForeignKey(sd => sd.ScheduleID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ShiftScheduleDetail>()
                .HasOne(sd => sd.ShiftCode)
                .WithMany(sc => sc.ShiftScheduleDetails)
                .HasForeignKey(sd => sd.ShiftCodeID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
