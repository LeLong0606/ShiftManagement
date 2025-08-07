using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShiftSchedule_DepartmentID",
                table: "ShiftSchedule");

            migrationBuilder.DropIndex(
                name: "IX_ShiftSchedule_StoreID",
                table: "ShiftSchedule");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftScheduleDetail_ScheduleID_ShiftCodeID",
                table: "ShiftScheduleDetail",
                columns: new[] { "ScheduleID", "ShiftCodeID" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_DepartmentID_Date",
                table: "ShiftSchedule",
                columns: new[] { "DepartmentID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_StoreID_Date",
                table: "ShiftSchedule",
                columns: new[] { "StoreID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ShiftScheduleDetail_ScheduleID_ShiftCodeID",
                table: "ShiftScheduleDetail");

            migrationBuilder.DropIndex(
                name: "IX_ShiftSchedule_DepartmentID_Date",
                table: "ShiftSchedule");

            migrationBuilder.DropIndex(
                name: "IX_ShiftSchedule_StoreID_Date",
                table: "ShiftSchedule");

            migrationBuilder.DropIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_Date",
                table: "Holidays");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_DepartmentID",
                table: "ShiftSchedule",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedule_StoreID",
                table: "ShiftSchedule",
                column: "StoreID");
        }
    }
}
