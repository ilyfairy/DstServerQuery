using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddTagColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagColors",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TagColors = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagColors", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagColors_Name",
                table: "TagColors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagColors");
        }
    }
}
