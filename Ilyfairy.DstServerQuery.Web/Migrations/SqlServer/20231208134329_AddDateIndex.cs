using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_Id_DateTime",
                table: "ServerHistoryItems",
                columns: new[] { "Id", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistories_Id_UpdateTime",
                table: "ServerHistories",
                columns: new[] { "Id", "UpdateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerHistoryItems_Id_DateTime",
                table: "ServerHistoryItems");

            migrationBuilder.DropIndex(
                name: "IX_ServerHistories_Id_UpdateTime",
                table: "ServerHistories");
        }
    }
}
