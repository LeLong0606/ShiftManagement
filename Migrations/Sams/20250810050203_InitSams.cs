using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftManagement.Migrations.Sams
{
    /// <inheritdoc />
    public partial class InitSams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sams");

            migrationBuilder.CreateTable(
                name: "CalendarDate",
                schema: "sams",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Weekday = table.Column<byte>(type: "tinyint", nullable: false),
                    IsWeekend = table.Column<bool>(type: "bit", nullable: false),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDate", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "ExportTemplate",
                schema: "sams",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Target = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LayoutJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportTemplate", x => x.TemplateID);
                });

            migrationBuilder.CreateTable(
                name: "Holiday",
                schema: "sams",
                columns: table => new
                {
                    HolidayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holiday", x => x.HolidayID);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "sams",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.LocationID);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                schema: "sams",
                columns: table => new
                {
                    PositionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.PositionID);
                });

            migrationBuilder.CreateTable(
                name: "ShiftBase",
                schema: "sams",
                columns: table => new
                {
                    ShiftBaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false),
                    IsOvernight = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftBase", x => x.ShiftBaseID);
                });

            migrationBuilder.CreateTable(
                name: "vRosterForExport",
                schema: "sams",
                columns: table => new
                {
                    RosterEntryID = table.Column<long>(type: "bigint", nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodeDisplay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "vTimesheetForExport",
                schema: "sams",
                columns: table => new
                {
                    TimesheetEntryID = table.Column<long>(type: "bigint", nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TemplateFieldMap",
                schema: "sams",
                columns: table => new
                {
                    TemplateFieldMapID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceExpr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFieldMap", x => x.TemplateFieldMapID);
                    table.ForeignKey(
                        name: "FK_TemplateFieldMap_ExportTemplate_TemplateID",
                        column: x => x.TemplateID,
                        principalSchema: "sams",
                        principalTable: "ExportTemplate",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                schema: "sams",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Department_Location_LocationID",
                        column: x => x.LocationID,
                        principalSchema: "sams",
                        principalTable: "Location",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                schema: "sams",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.TeamID);
                    table.ForeignKey(
                        name: "FK_Team_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalSchema: "sams",
                        principalTable: "Department",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                schema: "sams",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: true),
                    PositionID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    HireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employee_Position_PositionID",
                        column: x => x.PositionID,
                        principalSchema: "sams",
                        principalTable: "Position",
                        principalColumn: "PositionID");
                    table.ForeignKey(
                        name: "FK_Employee_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID");
                });

            migrationBuilder.CreateTable(
                name: "ExportRun",
                schema: "sams",
                columns: table => new
                {
                    ExportRunID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "date", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "date", nullable: true),
                    TemplateID = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRun", x => x.ExportRunID);
                    table.ForeignKey(
                        name: "FK_ExportRun_ExportTemplate_TemplateID",
                        column: x => x.TemplateID,
                        principalSchema: "sams",
                        principalTable: "ExportTemplate",
                        principalColumn: "TemplateID");
                    table.ForeignKey(
                        name: "FK_ExportRun_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RosterPeriod",
                schema: "sams",
                columns: table => new
                {
                    RosterPeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterPeriod", x => x.RosterPeriodID);
                    table.CheckConstraint("CK_RosterPeriod_Range", "[PeriodEndDate] >= [PeriodStartDate]");
                    table.ForeignKey(
                        name: "FK_RosterPeriod_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamSettings",
                schema: "sams",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSettings", x => x.TeamID);
                    table.ForeignKey(
                        name: "FK_TeamSettings_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamShiftAlias",
                schema: "sams",
                columns: table => new
                {
                    TeamShiftAliasID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    ShiftBaseID = table.Column<int>(type: "int", nullable: true),
                    LeaveCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AliasCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamShiftAlias", x => x.TeamShiftAliasID);
                    table.ForeignKey(
                        name: "FK_TeamShiftAlias_ShiftBase_ShiftBaseID",
                        column: x => x.ShiftBaseID,
                        principalSchema: "sams",
                        principalTable: "ShiftBase",
                        principalColumn: "ShiftBaseID");
                    table.ForeignKey(
                        name: "FK_TeamShiftAlias_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamShiftOverride",
                schema: "sams",
                columns: table => new
                {
                    TeamShiftOverrideID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    ShiftBaseID = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    BreakMinutes = table.Column<int>(type: "int", nullable: true),
                    IsOvernight = table.Column<bool>(type: "bit", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamShiftOverride", x => x.TeamShiftOverrideID);
                    table.ForeignKey(
                        name: "FK_TeamShiftOverride_ShiftBase_ShiftBaseID",
                        column: x => x.ShiftBaseID,
                        principalSchema: "sams",
                        principalTable: "ShiftBase",
                        principalColumn: "ShiftBaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamShiftOverride_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamTemplateBinding",
                schema: "sams",
                columns: table => new
                {
                    TeamTemplateBindingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTemplateBinding", x => x.TeamTemplateBindingID);
                    table.ForeignKey(
                        name: "FK_TeamTemplateBinding_ExportTemplate_TemplateID",
                        column: x => x.TemplateID,
                        principalSchema: "sams",
                        principalTable: "ExportTemplate",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamTemplateBinding_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RosterEntry",
                schema: "sams",
                columns: table => new
                {
                    RosterEntryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RosterPeriodID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    ShiftBaseID = table.Column<int>(type: "int", nullable: true),
                    LeaveCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StartTimeOverride = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    EndTimeOverride = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterEntry", x => x.RosterEntryID);
                    table.CheckConstraint("CK_RosterEntry_OneKind", "(([ShiftBaseID] IS NOT NULL AND [LeaveCode] IS NULL) OR ([ShiftBaseID] IS NULL AND [LeaveCode] IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_RosterEntry_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalSchema: "sams",
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterEntry_RosterPeriod_RosterPeriodID",
                        column: x => x.RosterPeriodID,
                        principalSchema: "sams",
                        principalTable: "RosterPeriod",
                        principalColumn: "RosterPeriodID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterEntry_ShiftBase_ShiftBaseID",
                        column: x => x.ShiftBaseID,
                        principalSchema: "sams",
                        principalTable: "ShiftBase",
                        principalColumn: "ShiftBaseID");
                });

            migrationBuilder.CreateTable(
                name: "TimesheetBatch",
                schema: "sams",
                columns: table => new
                {
                    TimesheetBatchID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    SourceRosterID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetBatch", x => x.TimesheetBatchID);
                    table.ForeignKey(
                        name: "FK_TimesheetBatch_RosterPeriod_SourceRosterID",
                        column: x => x.SourceRosterID,
                        principalSchema: "sams",
                        principalTable: "RosterPeriod",
                        principalColumn: "RosterPeriodID");
                    table.ForeignKey(
                        name: "FK_TimesheetBatch_Team_TeamID",
                        column: x => x.TeamID,
                        principalSchema: "sams",
                        principalTable: "Team",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetEntry",
                schema: "sams",
                columns: table => new
                {
                    TimesheetEntryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimesheetBatchID = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodeDisplay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlanStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedMinutes = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetEntry", x => x.TimesheetEntryID);
                    table.ForeignKey(
                        name: "FK_TimesheetEntry_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalSchema: "sams",
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimesheetEntry_TimesheetBatch_TimesheetBatchID",
                        column: x => x.TimesheetBatchID,
                        principalSchema: "sams",
                        principalTable: "TimesheetBatch",
                        principalColumn: "TimesheetBatchID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Department_LocationID_Code",
                schema: "sams",
                table: "Department",
                columns: new[] { "LocationID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmpCode",
                schema: "sams",
                table: "Employee",
                column: "EmpCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PositionID",
                schema: "sams",
                table: "Employee",
                column: "PositionID");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_TeamID",
                schema: "sams",
                table: "Employee",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_ExportRun_TeamID",
                schema: "sams",
                table: "ExportRun",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_ExportRun_TemplateID",
                schema: "sams",
                table: "ExportRun",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Position_Code",
                schema: "sams",
                table: "Position",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntry_EmployeeID_WorkDate",
                schema: "sams",
                table: "RosterEntry",
                columns: new[] { "EmployeeID", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntry_RosterPeriodID_EmployeeID_WorkDate",
                schema: "sams",
                table: "RosterEntry",
                columns: new[] { "RosterPeriodID", "EmployeeID", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntry_ShiftBaseID",
                schema: "sams",
                table: "RosterEntry",
                column: "ShiftBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_RosterPeriod_TeamID_PeriodStartDate_PeriodEndDate",
                schema: "sams",
                table: "RosterPeriod",
                columns: new[] { "TeamID", "PeriodStartDate", "PeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftBase_Code",
                schema: "sams",
                table: "ShiftBase",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_DepartmentID_Code",
                schema: "sams",
                table: "Team",
                columns: new[] { "DepartmentID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftAlias_ShiftBaseID",
                schema: "sams",
                table: "TeamShiftAlias",
                column: "ShiftBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftAlias_TeamID_ShiftBaseID_LeaveCode",
                schema: "sams",
                table: "TeamShiftAlias",
                columns: new[] { "TeamID", "ShiftBaseID", "LeaveCode" },
                unique: true,
                filter: "[ShiftBaseID] IS NOT NULL AND [LeaveCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftOverride_ShiftBaseID",
                schema: "sams",
                table: "TeamShiftOverride",
                column: "ShiftBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftOverride_TeamID_ShiftBaseID",
                schema: "sams",
                table: "TeamShiftOverride",
                columns: new[] { "TeamID", "ShiftBaseID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamTemplateBinding_TeamID_TemplateID_Target_EffectiveFrom",
                schema: "sams",
                table: "TeamTemplateBinding",
                columns: new[] { "TeamID", "TemplateID", "Target", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamTemplateBinding_TemplateID",
                schema: "sams",
                table: "TeamTemplateBinding",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateFieldMap_TemplateID",
                schema: "sams",
                table: "TemplateFieldMap",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetBatch_SourceRosterID",
                schema: "sams",
                table: "TimesheetBatch",
                column: "SourceRosterID");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetBatch_TeamID",
                schema: "sams",
                table: "TimesheetBatch",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntry_EmployeeID_WorkDate",
                schema: "sams",
                table: "TimesheetEntry",
                columns: new[] { "EmployeeID", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntry_TimesheetBatchID",
                schema: "sams",
                table: "TimesheetEntry",
                column: "TimesheetBatchID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarDate",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "ExportRun",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Holiday",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "RosterEntry",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TeamSettings",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TeamShiftAlias",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TeamShiftOverride",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TeamTemplateBinding",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TemplateFieldMap",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TimesheetEntry",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "vRosterForExport",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "vTimesheetForExport",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "ShiftBase",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "ExportTemplate",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Employee",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "TimesheetBatch",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Position",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "RosterPeriod",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Team",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Department",
                schema: "sams");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "sams");
        }
    }
}
