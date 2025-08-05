using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeShiftSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "ShiftCodes",
                columns: table => new
                {
                    ShiftCodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsLeave = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftCodes", x => x.ShiftCodeID);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreID);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    HolidayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultShiftCodeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.HolidayID);
                    table.ForeignKey(
                        name: "FK_Holidays_ShiftCodes_DefaultShiftCodeID",
                        column: x => x.DefaultShiftCodeID,
                        principalTable: "ShiftCodes",
                        principalColumn: "ShiftCodeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkUnitRules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftCodeID = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkUnitRules", x => x.RuleID);
                    table.ForeignKey(
                        name: "FK_WorkUnitRules_ShiftCodes_ShiftCodeID",
                        column: x => x.ShiftCodeID,
                        principalTable: "ShiftCodes",
                        principalColumn: "ShiftCodeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StoreID = table.Column<int>(type: "int", nullable: false),
                    ManagerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Departments_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    StoreID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShiftSchedule",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    StoreID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSchedule", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_ShiftSchedule_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftSchedule_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftSchedule_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftSchedule_Users_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleID);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftHistory",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftHistory", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_ShiftHistory_ShiftSchedule_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "ShiftSchedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftHistory_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftScheduleDetail",
                columns: table => new
                {
                    DetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    ShiftCodeID = table.Column<int>(type: "int", nullable: false),
                    ShiftType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftScheduleDetail", x => x.DetailID);
                    table.ForeignKey(
                        name: "FK_ShiftScheduleDetail_ShiftCodes_ShiftCodeID",
                        column: x => x.ShiftCodeID,
                        principalTable: "ShiftCodes",
                        principalColumn: "ShiftCodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftScheduleDetail_ShiftSchedule_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "ShiftSchedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentName",
                table: "Departments",
                column: "DepartmentName");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerID",
                table: "Departments",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_StoreID",
                table: "Departments",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_DefaultShiftCodeID",
                table: "Holidays",
                column: "DefaultShiftCodeID");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserID",
                table: "Logs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCodes_Code",
                table: "ShiftCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHistory_ChangedBy",
                table: "ShiftHistory",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHistory_ScheduleID",
                table: "ShiftHistory",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_CreatedBy",
                table: "ShiftSchedule",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_DepartmentID",
                table: "ShiftSchedule",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_EmployeeID_Date",
                table: "ShiftSchedule",
                columns: new[] { "EmployeeID", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_StoreID",
                table: "ShiftSchedule",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftScheduleDetail_ScheduleID",
                table: "ShiftScheduleDetail",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftScheduleDetail_ShiftCodeID",
                table: "ShiftScheduleDetail",
                column: "ShiftCodeID");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreName",
                table: "Stores",
                column: "StoreName");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserID_RoleID",
                table: "UserRoles",
                columns: new[] { "UserID", "RoleID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentID",
                table: "Users",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreID",
                table: "Users",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkUnitRules_ShiftCodeID",
                table: "WorkUnitRules",
                column: "ShiftCodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerID",
                table: "Departments",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Stores_StoreID",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stores_StoreID",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerID",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "ShiftHistory");

            migrationBuilder.DropTable(
                name: "ShiftScheduleDetail");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WorkUnitRules");

            migrationBuilder.DropTable(
                name: "ShiftSchedule");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ShiftCodes");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
