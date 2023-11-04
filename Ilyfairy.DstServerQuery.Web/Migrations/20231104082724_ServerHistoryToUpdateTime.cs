using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations
{
    /// <inheritdoc />
    public partial class ServerHistoryToUpdateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateDateTime",
                table: "ServerHistories",
                newName: "UpdateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateTime",
                table: "ServerHistories",
                newName: "CreateDateTime");
        }
    }
}
