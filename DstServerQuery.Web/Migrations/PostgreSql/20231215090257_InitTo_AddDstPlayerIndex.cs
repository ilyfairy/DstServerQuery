using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DstServerQuery.Web.Migrations.PostgreSql
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    DaysElapsedInSeason = table.Column<int>(type: "integer", nullable: false),
                    DaysLeftInSeason = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IP = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    GameMode = table.Column<string>(type: "text", nullable: true),
                    Intent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryCountInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AllServerCount = table.Column<int>(type: "integer", nullable: false),
                    AllPlayerCount = table.Column<int>(type: "integer", nullable: false),
                    SteamServerCount = table.Column<int>(type: "integer", nullable: false),
                    WeGameServerCount = table.Column<int>(type: "integer", nullable: false),
                    PlayStationServerCount = table.Column<int>(type: "integer", nullable: false),
                    XboxServerCount = table.Column<int>(type: "integer", nullable: false),
                    SwitchServerCount = table.Column<int>(type: "integer", nullable: true),
                    SteamPlayerCount = table.Column<int>(type: "integer", nullable: false),
                    WeGamePlayerCount = table.Column<int>(type: "integer", nullable: false),
                    PlayStationPlayerCount = table.Column<int>(type: "integer", nullable: false),
                    XboxPlayerCount = table.Column<int>(type: "integer", nullable: false),
                    SwitchPlayerCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerHistoryCountInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagColors",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagColors", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ServerHistoryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Season = table.Column<string>(type: "text", nullable: true),
                    PlayerCount = table.Column<int>(type: "integer", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ServerId = table.Column<string>(type: "text", nullable: false),
                    IsDetailed = table.Column<bool>(type: "boolean", nullable: false),
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
                    PlayerId = table.Column<string>(type: "text", nullable: false),
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
                        name: "FK_HistoryServerItemPlayerPair_ServerHistoryItems_HistoryServe~",
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
