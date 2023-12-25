﻿// <auto-generated />
using System;
using Ilyfairy.DstServerQuery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerDstDbContext))]
    [Migration("20231209045633_ToDateTimeOffset")]
    partial class ToDateTimeOffset
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("Chinese_PRC_BIN")
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<int>("Day")
                        .HasColumnType("int");

                    b.Property<int>("DaysElapsedInSeason")
                        .HasColumnType("int");

                    b.Property<int>("DaysLeftInSeason")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DaysInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Platform")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("GameMode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Intent")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Platform")
                        .HasColumnType("int");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("Id", "UpdateTime");

                    b.ToTable("ServerHistories");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("DateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("DaysInfoId")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDetailed")
                        .HasColumnType("bit");

                    b.Property<int>("PlayerCount")
                        .HasColumnType("int");

                    b.Property<string>("Season")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("DaysInfoId")
                        .IsUnique()
                        .HasFilter("[DaysInfoId] IS NOT NULL");

                    b.HasIndex("ServerId");

                    b.HasIndex("Id", "DateTime");

                    b.ToTable("ServerHistoryItems");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.HistoryServerItemPlayer", b =>
                {
                    b.Property<long>("HistoryServerItemId")
                        .HasColumnType("bigint");

                    b.Property<string>("PlayerId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("HistoryServerItemId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("HistoryServerItemPlayerPair");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.ServerCountInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AllPlayerCount")
                        .HasColumnType("int");

                    b.Property<int>("AllServerCount")
                        .HasColumnType("int");

                    b.Property<int>("PlayStationPlayerCount")
                        .HasColumnType("int");

                    b.Property<int>("PlayStationServerCount")
                        .HasColumnType("int");

                    b.Property<int>("SteamPlayerCount")
                        .HasColumnType("int");

                    b.Property<int>("SteamServerCount")
                        .HasColumnType("int");

                    b.Property<int?>("SwitchPlayerCount")
                        .HasColumnType("int");

                    b.Property<int?>("SwitchServerCount")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdateDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("WeGamePlayerCount")
                        .HasColumnType("int");

                    b.Property<int>("WeGameServerCount")
                        .HasColumnType("int");

                    b.Property<int>("XboxPlayerCount")
                        .HasColumnType("int");

                    b.Property<int>("XboxServerCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ServerHistoryCountInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.TagColorItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Name");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("TagColors");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.HasOne("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", "DaysInfo")
                        .WithOne("ServerItem")
                        .HasForeignKey("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", "DaysInfoId");

                    b.HasOne("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", "Server")
                        .WithMany("Items")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DaysInfo");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.HistoryServerItemPlayer", b =>
                {
                    b.HasOne("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", "HistoryServerItem")
                        .WithMany()
                        .HasForeignKey("HistoryServerItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("HistoryServerItem");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Navigation("ServerItem")
                        .IsRequired();
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}