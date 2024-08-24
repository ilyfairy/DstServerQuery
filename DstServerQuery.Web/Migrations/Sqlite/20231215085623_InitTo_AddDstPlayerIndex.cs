using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DstServerQuery.Web.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitTo_AddDstPlayerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DaysInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysElapsedInSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysLeftInSeason = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IP = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<long>(type: "INTEGER", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    GameMode = table.Column<string>(type: "TEXT", nullable: true),
                    Intent = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryCountInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDate = table.Column<long>(type: "INTEGER", nullable: false),
                    AllServerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AllPlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SteamServerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WeGameServerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayStationServerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    XboxServerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SwitchServerCount = table.Column<int>(type: "INTEGER", nullable: true),
                    SteamPlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WeGamePlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayStationPlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    XboxPlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SwitchPlayerCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistoryCountInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagColors",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagColors", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Season = table.Column<string>(type: "TEXT", nullable: true),
                    PlayerCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<long>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDetailed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DaysInfoId = table.Column<long>(type: "INTEGER", nullable: true)
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
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    HistoryServerItemId = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "IX_Players_Name_Platform",
                table: "Players",
                columns: new[] { "Name", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistories_Id_UpdateTime_Name",
                table: "ServerHistories",
                columns: new[] { "Id", "UpdateTime", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_DaysInfoId",
                table: "ServerHistoryItems",
                column: "DaysInfoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_Id_DateTime",
                table: "ServerHistoryItems",
                columns: new[] { "Id", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerHistoryItems_ServerId",
                table: "ServerHistoryItems",
                column: "ServerId");

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
                name: "HistoryServerItemPlayerPair");

            migrationBuilder.DropTable(
                name: "ServerHistoryCountInfos");

            migrationBuilder.DropTable(
                name: "TagColors");

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
