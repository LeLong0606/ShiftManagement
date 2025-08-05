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

            // Map tên bảng đúng với SQL
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Store>().ToTable("Stores");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<ShiftCode>().ToTable("ShiftCodes");
            modelBuilder.Entity<Holiday>().ToTable("Holidays");
            modelBuilder.Entity<ShiftSchedule>().ToTable("ShiftSchedule");
            modelBuilder.Entity<ShiftScheduleDetail>().ToTable("ShiftScheduleDetail");
            modelBuilder.Entity<ShiftHistory>().ToTable("ShiftHistory");
            modelBuilder.Entity<Log>().ToTable("Logs");
            modelBuilder.Entity<WorkUnitRule>().ToTable("WorkUnitRules");

            // Unique constraints & Indexes
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserID, ur.RoleID })
                .IsUnique();

            modelBuilder.Entity<ShiftSchedule>()
                .HasIndex(s => new { s.EmployeeID, s.Date })
                .IsUnique();

            // Đánh chỉ mục cho User.Username và User.Email (unique)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Đánh chỉ mục cho DepartmentName
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DepartmentName);

            // Đánh chỉ mục cho StoreName
            modelBuilder.Entity<Store>()
                .HasIndex(s => s.StoreName);

            // Đánh chỉ mục cho ShiftCode.Code (nếu cần tra cứu nhanh mã ca)
            modelBuilder.Entity<ShiftCode>()
                .HasIndex(sc => sc.Code)
                .IsUnique();

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

            // ShiftSchedule - Department
            modelBuilder.Entity<ShiftSchedule>()
                .HasOne(s => s.Department)
                .WithMany(d => d.ShiftSchedules)
                .HasForeignKey(s => s.DepartmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftSchedule - Store
            modelBuilder.Entity<ShiftSchedule>()
                .HasOne(s => s.Store)
                .WithMany(st => st.ShiftSchedules)
                .HasForeignKey(s => s.StoreID)
                .OnDelete(DeleteBehavior.Restrict);

            // ShiftScheduleDetail - ShiftSchedule
            modelBuilder.Entity<ShiftScheduleDetail>()
                .HasOne(sd => sd.ShiftSchedule)
                .WithMany(s => s.ShiftScheduleDetails)
                .HasForeignKey(sd => sd.ScheduleID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftScheduleDetail - ShiftCode
            modelBuilder.Entity<ShiftScheduleDetail>()
                .HasOne(sd => sd.ShiftCode)
                .WithMany(sc => sc.ShiftScheduleDetails)
                .HasForeignKey(sd => sd.ShiftCodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRole - User
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // UserRole - Role
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftHistory - ShiftSchedule
            modelBuilder.Entity<ShiftHistory>()
                .HasOne(h => h.ShiftSchedule)
                .WithMany(s => s.ShiftHistories)
                .HasForeignKey(h => h.ScheduleID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftHistory - ChangedUser
            modelBuilder.Entity<ShiftHistory>()
                .HasOne(h => h.ChangedUser)
                .WithMany(u => u.ShiftHistories)
                .HasForeignKey(h => h.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Holiday - DefaultShiftCode
            modelBuilder.Entity<Holiday>()
                .HasOne(h => h.DefaultShiftCode)
                .WithMany(sc => sc.Holidays)
                .HasForeignKey(h => h.DefaultShiftCodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkUnitRule - ShiftCode
            modelBuilder.Entity<WorkUnitRule>()
                .HasOne(w => w.ShiftCode)
                .WithMany(sc => sc.WorkUnitRules)
                .HasForeignKey(w => w.ShiftCodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // Department - Store
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Store)
                .WithMany(s => s.Departments)
                .HasForeignKey(d => d.StoreID)
                .OnDelete(DeleteBehavior.Cascade);

            // Department - Manager (User)
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Department (SetNull)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentID)
                .OnDelete(DeleteBehavior.SetNull);

            // User - Store (Restrict to avoid cascade path error)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Store)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.StoreID)
                .OnDelete(DeleteBehavior.Restrict);

            // Log - User
            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}