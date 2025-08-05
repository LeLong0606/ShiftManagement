using ShiftManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ShiftManagement.Data
{
    public static class SeedData
    {
        public static void Initialize(ShiftManagementContext context)
        {
            // Roles
            if (!context.Roles.Any())
            {
                var roles = new[]
                {
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Director" },
                    new Role { RoleName = "TeamLeader" },
                    new Role { RoleName = "Employee" }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // Stores
            if (!context.Stores.Any())
            {
                var stores = new[]
                {
                    new Store { StoreName = "Chi nhánh 1 - Hà Nội", Address = "123 Đường A, Quận 1, Hà Nội", Phone = "0123456789" },
                    new Store { StoreName = "Chi nhánh 2 - TP.HCM", Address = "456 Đường B, Quận 3, TP.HCM", Phone = "0987654321" }
                };
                context.Stores.AddRange(stores);
                context.SaveChanges();
            }

            // Departments
            if (!context.Departments.Any())
            {
                var store1Id = context.Stores.First(s => s.StoreName.Contains("Hà Nội")).StoreID;
                var store2Id = context.Stores.First(s => s.StoreName.Contains("TP.HCM")).StoreID;

                var departments = new[]
                {
                    new Department { DepartmentName = "TPTS - Thực phẩm tươi sống", StoreID = store1Id },
                    new Department { DepartmentName = "TPCN - Thực phẩm công nghệ", StoreID = store1Id },
                    new Department { DepartmentName = "PTP - Phi thực phẩm", StoreID = store1Id },
                    new Department { DepartmentName = "MKT - Marketing", StoreID = store2Id },
                    new Department { DepartmentName = "BPKHO - Bộ phận kho", StoreID = store2Id },
                    new Department { DepartmentName = "VT - Tổ vi tính", StoreID = store2Id }
                };
                context.Departments.AddRange(departments);
                context.SaveChanges();
            }

            // Users
            if (!context.Users.Any())
            {
                var store1Id = context.Stores.First(s => s.StoreName.Contains("Hà Nội")).StoreID;
                var store2Id = context.Stores.First(s => s.StoreName.Contains("TP.HCM")).StoreID;
                var dep1Id = context.Departments.First(d => d.DepartmentName.StartsWith("TPTS")).DepartmentID;
                var dep4Id = context.Departments.First(d => d.DepartmentName.StartsWith("MKT")).DepartmentID;
                var dep2Id = context.Departments.First(d => d.DepartmentName.StartsWith("TPCN")).DepartmentID;
                var dep5Id = context.Departments.First(d => d.DepartmentName.StartsWith("BPKHO")).DepartmentID;

                var users = new[]
                {
                    new User { Username = "admin", PasswordHash = "123456hash", FullName = "Quản trị viên", Email = "admin@supermarket.com", PhoneNumber = "0901111111", DepartmentID = null, StoreID = store1Id, Status = true },
                    new User { Username = "director", PasswordHash = "123456hash", FullName = "Giám đốc", Email = "director@supermarket.com", PhoneNumber = "0902222222", DepartmentID = null, StoreID = store1Id, Status = true },
                    new User { Username = "leader1", PasswordHash = "123456hash", FullName = "Nguyễn Văn A", Email = "leader1@supermarket.com", PhoneNumber = "0903333333", DepartmentID = dep1Id, StoreID = store1Id, Status = true },
                    new User { Username = "leader2", PasswordHash = "123456hash", FullName = "Trần Thị B", Email = "leader2@supermarket.com", PhoneNumber = "0904444444", DepartmentID = dep4Id, StoreID = store2Id, Status = true },
                    new User { Username = "employee1", PasswordHash = "123456hash", FullName = "Lê Văn C", Email = "employee1@supermarket.com", PhoneNumber = "0905555555", DepartmentID = dep1Id, StoreID = store1Id, Status = true },
                    new User { Username = "employee2", PasswordHash = "123456hash", FullName = "Hoàng Thị D", Email = "employee2@supermarket.com", PhoneNumber = "0906666666", DepartmentID = dep2Id, StoreID = store1Id, Status = true },
                    new User { Username = "employee3", PasswordHash = "123456hash", FullName = "Phạm Văn E", Email = "employee3@supermarket.com", PhoneNumber = "0907777777", DepartmentID = dep5Id, StoreID = store2Id, Status = true }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // UserRoles
            if (!context.UserRoles.Any())
            {
                var users = context.Users.ToList();
                var roles = context.Roles.ToList();
                context.UserRoles.AddRange(
                    new UserRole { UserID = users.First(u => u.Username == "admin").UserID, RoleID = roles.First(r => r.RoleName == "Admin").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "director").UserID, RoleID = roles.First(r => r.RoleName == "Director").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "leader1").UserID, RoleID = roles.First(r => r.RoleName == "TeamLeader").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "leader2").UserID, RoleID = roles.First(r => r.RoleName == "TeamLeader").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "employee1").UserID, RoleID = roles.First(r => r.RoleName == "Employee").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "employee2").UserID, RoleID = roles.First(r => r.RoleName == "Employee").RoleID },
                    new UserRole { UserID = users.First(u => u.Username == "employee3").UserID, RoleID = roles.First(r => r.RoleName == "Employee").RoleID }
                );
                context.SaveChanges();
            }

            // ShiftCodes
            if (!context.ShiftCodes.Any())
            {
                var shiftCodes = new[]
                {
                    new ShiftCode { Code = "X", Description = "Làm cả ngày", WorkUnit = 1.0m, IsLeave = false },
                    new ShiftCode { Code = "0.5", Description = "Làm nửa ngày", WorkUnit = 0.5m, IsLeave = false },
                    new ShiftCode { Code = "Đ", Description = "Ca đêm", WorkUnit = 1.0m, IsLeave = false },
                    new ShiftCode { Code = "PN", Description = "Nghỉ phép năm", WorkUnit = 0.0m, IsLeave = true },
                    new ShiftCode { Code = "LE", Description = "Nghỉ lễ", WorkUnit = 0.0m, IsLeave = true },
                    new ShiftCode { Code = "T", Description = "Nghỉ Tết", WorkUnit = 0.0m, IsLeave = true }
                };
                context.ShiftCodes.AddRange(shiftCodes);
                context.SaveChanges();
            }

            // Holidays
            if (!context.Holidays.Any())
            {
                var shiftCodes = context.ShiftCodes.ToList();
                context.Holidays.AddRange(
                    new Holiday { Date = new DateTime(2025, 9, 2), Description = "Quốc khánh", DefaultShiftCodeID = shiftCodes.First(sc => sc.Code == "LE").ShiftCodeID },
                    new Holiday { Date = new DateTime(2025, 1, 1), Description = "Tết Dương Lịch", DefaultShiftCodeID = shiftCodes.First(sc => sc.Code == "T").ShiftCodeID }
                );
                context.SaveChanges();
            }

            // ShiftSchedules + ShiftScheduleDetails
            if (!context.ShiftSchedules.Any())
            {
                var users = context.Users.ToList();
                var departments = context.Departments.ToList();
                var stores = context.Stores.ToList();
                var shiftCodes = context.ShiftCodes.ToList();

                // employee1 làm cả ngày
                var sched1 = new ShiftSchedule
                {
                    EmployeeID = users.First(u => u.Username == "employee1").UserID,
                    DepartmentID = departments.First(d => d.DepartmentName.StartsWith("TPTS")).DepartmentID,
                    StoreID = stores.First(s => s.StoreName.Contains("Hà Nội")).StoreID,
                    Date = new DateTime(2025, 8, 5),
                    CreatedBy = users.First(u => u.Username == "leader1").UserID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ShiftScheduleDetails = new List<ShiftScheduleDetail>
                    {
                        new ShiftScheduleDetail
                        {
                            ShiftCodeID = shiftCodes.First(sc => sc.Code == "X").ShiftCodeID,
                            ShiftType = "Morning",
                            WorkUnit = 1.0m
                        }
                    }
                };
                context.ShiftSchedules.Add(sched1);
                context.SaveChanges();

                // employee2 làm nửa ngày sáng + chiều
                var sched2 = new ShiftSchedule
                {
                    EmployeeID = users.First(u => u.Username == "employee2").UserID,
                    DepartmentID = departments.First(d => d.DepartmentName.StartsWith("TPCN")).DepartmentID,
                    StoreID = stores.First(s => s.StoreName.Contains("Hà Nội")).StoreID,
                    Date = new DateTime(2025, 8, 5),
                    CreatedBy = users.First(u => u.Username == "leader1").UserID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ShiftScheduleDetails = new List<ShiftScheduleDetail>
                    {
                        new ShiftScheduleDetail
                        {
                            ShiftCodeID = shiftCodes.First(sc => sc.Code == "0.5").ShiftCodeID,
                            ShiftType = "Morning",
                            WorkUnit = 0.5m
                        },
                        new ShiftScheduleDetail
                        {
                            ShiftCodeID = shiftCodes.First(sc => sc.Code == "0.5").ShiftCodeID,
                            ShiftType = "Afternoon",
                            WorkUnit = 0.5m
                        }
                    }
                };
                context.ShiftSchedules.Add(sched2);
                context.SaveChanges();

                // employee3 nghỉ phép
                var sched3 = new ShiftSchedule
                {
                    EmployeeID = users.First(u => u.Username == "employee3").UserID,
                    DepartmentID = departments.First(d => d.DepartmentName.StartsWith("BPKHO")).DepartmentID,
                    StoreID = stores.First(s => s.StoreName.Contains("TP.HCM")).StoreID,
                    Date = new DateTime(2025, 8, 5),
                    CreatedBy = users.First(u => u.Username == "leader2").UserID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ShiftScheduleDetails = new List<ShiftScheduleDetail>
                    {
                        new ShiftScheduleDetail
                        {
                            ShiftCodeID = shiftCodes.First(sc => sc.Code == "PN").ShiftCodeID,
                            ShiftType = "Morning",
                            WorkUnit = 0.0m
                        }
                    }
                };
                context.ShiftSchedules.Add(sched3);
                context.SaveChanges();
            }

            // ShiftHistory
            if (!context.ShiftHistories.Any())
            {
                var schedules = context.ShiftSchedules.ToList();
                var users = context.Users.ToList();
                context.ShiftHistories.AddRange(
                    new ShiftHistory
                    {
                        ScheduleID = schedules[0].ScheduleID,
                        ChangedBy = users.First(u => u.Username == "admin").UserID,
                        OldValue = "Ca cũ: null",
                        NewValue = "Ca mới: X",
                        ChangeDate = DateTime.UtcNow
                    },
                    new ShiftHistory
                    {
                        ScheduleID = schedules[1].ScheduleID,
                        ChangedBy = users.First(u => u.Username == "leader1").UserID,
                        OldValue = "Ca cũ: null",
                        NewValue = "Ca mới: 0.5/0.5",
                        ChangeDate = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

            // WorkUnitRules
            if (!context.WorkUnitRules.Any())
            {
                var shiftCodes = context.ShiftCodes.ToList();
                context.WorkUnitRules.AddRange(
                    new WorkUnitRule
                    {
                        ShiftCodeID = shiftCodes.First(sc => sc.Code == "X").ShiftCodeID,
                        EffectiveDate = new DateTime(2025, 1, 1),
                        WorkUnit = 1.0m
                    },
                    new WorkUnitRule
                    {
                        ShiftCodeID = shiftCodes.First(sc => sc.Code == "0.5").ShiftCodeID,
                        EffectiveDate = new DateTime(2025, 1, 1),
                        WorkUnit = 0.5m
                    }
                );
                context.SaveChanges();
            }

            // Logs
            if (!context.Logs.Any())
            {
                var users = context.Users.ToList();
                context.Logs.AddRange(
                    new Log { UserID = users.First(u => u.Username == "leader1").UserID, Action = "Phân ca cho nhân viên 5 ngày 2025-08-05 (ca X)", Timestamp = DateTime.UtcNow },
                    new Log { UserID = users.First(u => u.Username == "leader1").UserID, Action = "Phân ca cho nhân viên 6 ngày 2025-08-05 (ca 0.5/0.5)", Timestamp = DateTime.UtcNow },
                    new Log { UserID = users.First(u => u.Username == "leader2").UserID, Action = "Phân ca cho nhân viên 7 ngày 2025-08-05 (ca PN)", Timestamp = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}