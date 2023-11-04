using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DaysInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<int>(type: "int", nullable: false),
                    DaysElapsedInSeason = table.Column<int>(type: "int", nullable: false),
                    DaysLeftInSeason = table.Column<int>(type: "int", nullable: false),
                    ServerItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Platform = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    GameMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Intent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryCountInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllServerCount = table.Column<int>(type: "int", nullable: false),
                    AllPlayerCount = table.Column<int>(type: "int", nullable: false),
                    SteamServerCount = table.Column<int>(type: "int", nullable: false),
                    WeGameServerCount = table.Column<int>(type: "int", nullable: false),
                    PlayStationServerCount = table.Column<int>(type: "int", nullable: false),
                    XboxServerCount = table.Column<int>(type: "int", nullable: false),
                    SwitchServerCount = table.Column<int>(type: "int", nullable: true),
                    SteamPlayerCount = table.Column<int>(type: "int", nullable: false),
                    WeGamePlayerCount = table.Column<int>(type: "int", nullable: false),
                    PlayStationPlayerCount = table.Column<int>(type: "int", nullable: false),
                    XboxPlayerCount = table.Column<int>(type: "int", nullable: false),
                    SwitchPlayerCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistoryCountInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerCount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDetailed = table.Column<bool>(type: "bit", nullable: false),
                    DaysInfoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerHistoryItems_DaysInfos_DaysInfoId",
                        column: x => x.DaysInfoId,
                        principalTable: "DaysInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServerHistoryItems_ServerHistories_ServerId",
                        column: x => x.ServerId,
                        principalTable: "ServerHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryServerItemPlayerPair",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HistoryServerItemId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryServerItemPlayerPair", x => new { x.HistoryServerItemId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_HistoryServerItemPlayerPair_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryServerItemPlayerPair_ServerHistoryItems_HistoryServerItemId",
                        column: x => x.HistoryServerItemId,
                        principalTable: "ServerHistoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryServerItemPlayerPair_PlayerId",
                table: "HistoryServerItemPlayerPair",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_DaysInfoId",
                table: "ServerHistoryItems",
                column: "DaysInfoId",
                unique: true,
                filter: "[DaysInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_ServerId",
                table: "ServerHistoryItems",
                column: "ServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryServerItemPlayerPair");

            migrationBuilder.DropTable(
                name: "ServerHistoryCountInfos");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "ServerHistoryItems");

            migrationBuilder.DropTable(
                name: "DaysInfos");

            migrationBuilder.DropTable(
                name: "ServerHistories");
        }
    }
}
