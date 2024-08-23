using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddDstPlayerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerHistories_Id_UpdateTime",
                table: "ServerHistories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServerHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Players",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistories_Id_UpdateTime_Name",
                table: "ServerHistories",
                columns: new[] { "Id", "UpdateTime", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name_Platform",
                table: "Players",
                columns: new[] { "Name", "Platform" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerHistories_Id_UpdateTime_Name",
                table: "ServerHistories");

            migrationBuilder.DropIndex(
                name: "IX_Players_Name_Platform",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServerHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Players",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistories_Id_UpdateTime",
                table: "ServerHistories",
                columns: new[] { "Id", "UpdateTime" });
        }
    }
}
