using ShiftManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

// THÊM DÒNG NÀY ĐỂ DÙNG BCrypt
using BCrypt.Net;

namespace ShiftManagement.Data
{
    public static class SeedData
    {
        // Hash password bằng BCrypt để tương thích khi login
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

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

            // Departments: mỗi store đều có đủ các phòng ban
            if (!context.Departments.Any())
            {
                var departmentNames = new[]
                {
                    "TPTS - Thực phẩm tươi sống",
                    "TPCN - Thực phẩm công nghệ",
                    "PTP - Phi thực phẩm",
                    "MKT - Marketing",
                    "BPKHO - Bộ phận kho",
                    "VT - Tổ vi tính"
                };
                var stores = context.Stores.ToList();
                var allDepartments = new List<Department>();
                foreach (var store in stores)
                {
                    foreach (var deptName in departmentNames)
                    {
                        allDepartments.Add(new Department
                        {
                            DepartmentName = deptName,
                            StoreID = store.StoreID
                        });
                    }
                }
                context.Departments.AddRange(allDepartments);
                context.SaveChanges();
            }

            // Users & UserRoles: mỗi quyền đều đủ tài khoản cho mỗi store, passhash = BCrypt(cm565)
            if (!context.Users.Any())
            {
                var stores = context.Stores.ToList();
                var roles = context.Roles.ToList();
                var departments = context.Departments.ToList();
                var passwordHash = HashPassword("cm565");
                var users = new List<User>();
                var userRoles = new List<UserRole>();
                int userIndex = 1;

                foreach (var store in stores)
                {
                    // Lấy các phòng ban thuộc store
                    var storeDepartments = departments.Where(d => d.StoreID == store.StoreID).ToList();

                    // Admin
                    var admin = new User
                    {
                        Username = $"admin_{store.StoreID}",
                        PasswordHash = passwordHash,
                        FullName = $"Quản trị viên {store.StoreName}",
                        Email = $"admin{store.StoreID}@supermarket.com",
                        PhoneNumber = $"0901{store.StoreID:D2}1111",
                        DepartmentID = null,
                        StoreID = store.StoreID,
                        Status = true
                    };
                    users.Add(admin);
                    // Director
                    var director = new User
                    {
                        Username = $"director_{store.StoreID}",
                        PasswordHash = passwordHash,
                        FullName = $"Giám đốc {store.StoreName}",
                        Email = $"director{store.StoreID}@supermarket.com",
                        PhoneNumber = $"0902{store.StoreID:D2}2222",
                        DepartmentID = null,
                        StoreID = store.StoreID,
                        Status = true
                    };
                    users.Add(director);

                    // TeamLeader & Employee cho mỗi phòng ban
                    foreach (var dept in storeDepartments)
                    {
                        // TeamLeader
                        var leader = new User
                        {
                            Username = $"leader_{store.StoreID}_{dept.DepartmentID}",
                            PasswordHash = passwordHash,
                            FullName = $"Trưởng phòng {dept.DepartmentName} {store.StoreName}",
                            Email = $"leader_{store.StoreID}_{dept.DepartmentID}@supermarket.com",
                            PhoneNumber = $"0903{userIndex:D4}3333",
                            DepartmentID = dept.DepartmentID,
                            StoreID = store.StoreID,
                            Status = true
                        };
                        users.Add(leader);

                        // Employee
                        var employee = new User
                        {
                            Username = $"employee_{store.StoreID}_{dept.DepartmentID}",
                            PasswordHash = passwordHash,
                            FullName = $"Nhân viên {dept.DepartmentName} {store.StoreName}",
                            Email = $"employee_{store.StoreID}_{dept.DepartmentID}@supermarket.com",
                            PhoneNumber = $"0904{userIndex:D4}4444",
                            DepartmentID = dept.DepartmentID,
                            StoreID = store.StoreID,
                            Status = true
                        };
                        users.Add(employee);
                        userIndex++;
                    }
                }
                context.Users.AddRange(users);
                context.SaveChanges();

                // Gán quyền cho user
                foreach (var user in context.Users)
                {
                    if (user.Username.StartsWith("admin"))
                    {
                        userRoles.Add(new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = roles.First(r => r.RoleName == "Admin").RoleID
                        });
                    }
                    else if (user.Username.StartsWith("director"))
                    {
                        userRoles.Add(new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = roles.First(r => r.RoleName == "Director").RoleID
                        });
                    }
                    else if (user.Username.StartsWith("leader"))
                    {
                        userRoles.Add(new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = roles.First(r => r.RoleName == "TeamLeader").RoleID
                        });
                    }
                    else if (user.Username.StartsWith("employee"))
                    {
                        userRoles.Add(new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = roles.First(r => r.RoleName == "Employee").RoleID
                        });
                    }
                }
                context.UserRoles.AddRange(userRoles);
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

            // ShiftSchedules + ShiftScheduleDetails (giữ nguyên mẫu, bạn có thể thêm seed thêm nhiều lịch phân ca nếu muốn)
            if (!context.ShiftSchedules.Any())
            {
                var users = context.Users.ToList();
                var departments = context.Departments.ToList();
                var stores = context.Stores.ToList();
                var shiftCodes = context.ShiftCodes.ToList();

                // Ví dụ: Phân ca mẫu cho 1 số user (bạn tự thêm nhiều nếu muốn)
                var sched1 = new ShiftSchedule
                {
                    EmployeeID = users.First(u => u.Username.StartsWith("employee_1")).UserID,
                    DepartmentID = departments.First(d => d.DepartmentName.StartsWith("TPTS") && d.StoreID == stores[0].StoreID).DepartmentID,
                    StoreID = stores[0].StoreID,
                    Date = new DateTime(2025, 8, 5),
                    CreatedBy = users.First(u => u.Username.StartsWith("leader_1")).UserID,
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
            }

            // ShiftHistory
            if (!context.ShiftHistories.Any())
            {
                var schedules = context.ShiftSchedules.ToList();
                var users = context.Users.ToList();
                if (schedules.Count > 0)
                {
                    context.ShiftHistories.AddRange(
                        new ShiftHistory
                        {
                            ScheduleID = schedules[0].ScheduleID,
                            ChangedBy = users.First(u => u.Username.StartsWith("admin_")).UserID,
                            OldValue = "Ca cũ: null",
                            NewValue = "Ca mới: X",
                            ChangeDate = DateTime.UtcNow
                        }
                    );
                    context.SaveChanges();
                }
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
                    new Log { UserID = users.First(u => u.Username.StartsWith("leader_1")).UserID, Action = "Phân ca cho nhân viên 1 ngày 2025-08-05 (ca X)", Timestamp = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}