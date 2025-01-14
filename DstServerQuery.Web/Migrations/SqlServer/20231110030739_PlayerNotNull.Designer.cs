﻿// <auto-generated />
using System;
using DstServerQuery.EntityFrameworkCore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DstServerQuery.Web.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerDstDbContext))]
    [Migration("20231110030739_PlayerNotNull")]
    partial class PlayerNotNull
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("Chinese_PRC_BIN")
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstDaysInfo", b =>
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

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstPlayer", b =>
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

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistory", b =>
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

                    b.Property<DateTime>("UpdateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("ServerHistories");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistoryItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

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

                    b.ToTable("ServerHistoryItems");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.HistoryServerItemPlayer", b =>
                {
                    b.Property<long>("HistoryServerItemId")
                        .HasColumnType("bigint");

                    b.Property<string>("PlayerId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("HistoryServerItemId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("HistoryServerItemPlayerPair");
                });

            modelBuilder.Entity("DstServerQuery.Models.Entities.ServerCountInfo", b =>
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

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

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

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistoryItem", b =>
                {
                    b.HasOne("DstServerQuery.EntityFrameworkCore.Model.Entities.DstDaysInfo", "DaysInfo")
                        .WithOne("ServerItem")
                        .HasForeignKey("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistoryItem", "DaysInfoId");

                    b.HasOne("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistory", "Server")
                        .WithMany("Items")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DaysInfo");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.HistoryServerItemPlayer", b =>
                {
                    b.HasOne("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistoryItem", "HistoryServerItem")
                        .WithMany()
                        .HasForeignKey("HistoryServerItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DstServerQuery.EntityFrameworkCore.Model.Entities.DstPlayer", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("HistoryServerItem");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstDaysInfo", b =>
                {
                    b.Navigation("ServerItem")
                        .IsRequired();
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Model.Entities.DstServerHistory", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
