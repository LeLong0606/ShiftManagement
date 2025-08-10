using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftManagement.Migrations.Sams
{
    /// <inheritdoc />
    public partial class AddSamsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimesheetEntry_TimesheetBatchID",
                schema: "sams",
                table: "TimesheetEntry");

            migrationBuilder.DropIndex(
                name: "IX_TimesheetBatch_TeamID",
                schema: "sams",
                table: "TimesheetBatch");

            migrationBuilder.DropIndex(
                name: "IX_ExportRun_TeamID",
                schema: "sams",
                table: "ExportRun");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntry_TimesheetBatchID_EmployeeID_WorkDate",
                schema: "sams",
                table: "TimesheetEntry",
                columns: new[] { "TimesheetBatchID", "EmployeeID", "WorkDate" })
                .Annotation("SqlServer:Include", new[] { "Kind", "CodeDisplay" });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetBatch_TeamID_PeriodStartDate_PeriodEndDate",
                schema: "sams",
                table: "TimesheetBatch",
                columns: new[] { "TeamID", "PeriodStartDate", "PeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetBatch_TeamID_Status_PeriodStartDate",
                schema: "sams",
                table: "TimesheetBatch",
                columns: new[] { "TeamID", "Status", "PeriodStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateFieldMap_TemplateID_Placeholder",
                schema: "sams",
                table: "TemplateFieldMap",
                columns: new[] { "TemplateID", "Placeholder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamTemplateBinding_TeamID_Target",
                schema: "sams",
                table: "TeamTemplateBinding",
                columns: new[] { "TeamID", "Target" },
                filter: "[EffectiveTo] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftOverride_TeamID",
                schema: "sams",
                table: "TeamShiftOverride",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamShiftAlias_TeamID_AliasCode",
                schema: "sams",
                table: "TeamShiftAlias",
                columns: new[] { "TeamID", "AliasCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_DepartmentID",
                schema: "sams",
                table: "Team",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftBase_Category",
                schema: "sams",
                table: "ShiftBase",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_RosterPeriod_TeamID_Status_PeriodStartDate",
                schema: "sams",
                table: "RosterPeriod",
                columns: new[] { "TeamID", "Status", "PeriodStartDate" })
                .Annotation("SqlServer:Include", new[] { "PeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntry_WorkDate",
                schema: "sams",
                table: "RosterEntry",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_Holiday_Date",
                schema: "sams",
                table: "Holiday",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportTemplate_Name",
                schema: "sams",
                table: "ExportTemplate",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportTemplate_Target_IsActive",
                schema: "sams",
                table: "ExportTemplate",
                columns: new[] { "Target", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ExportRun_Status",
                schema: "sams",
                table: "ExportRun",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExportRun_TeamID_Target_CreatedAt",
                schema: "sams",
                table: "ExportRun",
                columns: new[] { "TeamID", "Target", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_TeamID_Status",
                schema: "sams",
                table: "Employee",
                columns: new[] { "TeamID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Department_LocationID",
                schema: "sams",
                table: "Department",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDate_IsHoliday",
                schema: "sams",
                table: "CalendarDate",
                column: "IsHoliday",
                filter: "[IsHoliday] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDate_IsWeekend",
                schema: "sams",
                table: "CalendarDate",
                column: "IsWeekend",
                filter: "[IsWeekend] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDate_Year_Month",
                schema: "sams",
                table: "CalendarDate",
                columns: new[] { "Year", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimesheetEntry_TimesheetBatchID_EmployeeID_WorkDate",
                schema: "sams",
                table: "TimesheetEntry");

            migrationBuilder.DropIndex(
                name: "IX_TimesheetBatch_TeamID_PeriodStartDate_PeriodEndDate",
                schema: "sams",
                table: "TimesheetBatch");

            migrationBuilder.DropIndex(
                name: "IX_TimesheetBatch_TeamID_Status_PeriodStartDate",
                schema: "sams",
                table: "TimesheetBatch");

            migrationBuilder.DropIndex(
                name: "IX_TemplateFieldMap_TemplateID_Placeholder",
                schema: "sams",
                table: "TemplateFieldMap");

            migrationBuilder.DropIndex(
                name: "IX_TeamTemplateBinding_TeamID_Target",
                schema: "sams",
                table: "TeamTemplateBinding");

            migrationBuilder.DropIndex(
                name: "IX_TeamShiftOverride_TeamID",
                schema: "sams",
                table: "TeamShiftOverride");

            migrationBuilder.DropIndex(
                name: "IX_TeamShiftAlias_TeamID_AliasCode",
                schema: "sams",
                table: "TeamShiftAlias");

            migrationBuilder.DropIndex(
                name: "IX_Team_DepartmentID",
                schema: "sams",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_ShiftBase_Category",
                schema: "sams",
                table: "ShiftBase");

            migrationBuilder.DropIndex(
                name: "IX_RosterPeriod_TeamID_Status_PeriodStartDate",
                schema: "sams",
                table: "RosterPeriod");

            migrationBuilder.DropIndex(
                name: "IX_RosterEntry_WorkDate",
                schema: "sams",
                table: "RosterEntry");

            migrationBuilder.DropIndex(
                name: "IX_Holiday_Date",
                schema: "sams",
                table: "Holiday");

            migrationBuilder.DropIndex(
                name: "IX_ExportTemplate_Name",
                schema: "sams",
                table: "ExportTemplate");

            migrationBuilder.DropIndex(
                name: "IX_ExportTemplate_Target_IsActive",
                schema: "sams",
                table: "ExportTemplate");

            migrationBuilder.DropIndex(
                name: "IX_ExportRun_Status",
                schema: "sams",
                table: "ExportRun");

            migrationBuilder.DropIndex(
                name: "IX_ExportRun_TeamID_Target_CreatedAt",
                schema: "sams",
                table: "ExportRun");

            migrationBuilder.DropIndex(
                name: "IX_Employee_TeamID_Status",
                schema: "sams",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Department_LocationID",
                schema: "sams",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_CalendarDate_IsHoliday",
                schema: "sams",
                table: "CalendarDate");

            migrationBuilder.DropIndex(
                name: "IX_CalendarDate_IsWeekend",
                schema: "sams",
                table: "CalendarDate");

            migrationBuilder.DropIndex(
                name: "IX_CalendarDate_Year_Month",
                schema: "sams",
                table: "CalendarDate");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntry_TimesheetBatchID",
                schema: "sams",
                table: "TimesheetEntry",
                column: "TimesheetBatchID");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetBatch_TeamID",
                schema: "sams",
                table: "TimesheetBatch",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_ExportRun_TeamID",
                schema: "sams",
                table: "ExportRun",
                column: "TeamID");
        }
    }
}
