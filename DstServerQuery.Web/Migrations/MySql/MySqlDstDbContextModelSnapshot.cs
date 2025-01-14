﻿// <auto-generated />
using System;
using DstServerQuery.EntityFrameworkCore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DstServerQuery.Web.Migrations.MySql
{
    [DbContext(typeof(MySqlDstDbContext))]
    partial class MySqlDstDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_bin")
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("Day")
                        .HasColumnType("int");

                    b.Property<int>("DaysElapsedInSeason")
                        .HasColumnType("int");

                    b.Property<int>("DaysLeftInSeason")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DaysInfos");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Platform")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name", "Platform");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("GameMode")
                        .HasColumnType("longtext");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Intent")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Platform")
                        .HasColumnType("int");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdateTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("Id", "UpdateTime", "Name");

                    b.ToTable("ServerHistories");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("DateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("DaysInfoId")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDetailed")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("PlayerCount")
                        .HasColumnType("int");

                    b.Property<string>("Season")
                        .HasColumnType("longtext");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("DaysInfoId")
                        .IsUnique();

                    b.HasIndex("ServerId");

                    b.HasIndex("Id", "DateTime");

                    b.ToTable("ServerHistoryItems");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.HistoryServerItemPlayer", b =>
                {
                    b.Property<long>("HistoryServerItemId")
                        .HasColumnType("bigint");

                    b.Property<string>("PlayerId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("HistoryServerItemId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("HistoryServerItemPlayerPair");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.ServerCountInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

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
                        .HasColumnType("datetime(6)");

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

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.TagColorItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Name");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("TagColors");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.HasOne("DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", "DaysInfo")
                        .WithOne("ServerItem")
                        .HasForeignKey("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", "DaysInfoId");

                    b.HasOne("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", "Server")
                        .WithMany("Items")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DaysInfo");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.HistoryServerItemPlayer", b =>
                {
                    b.HasOne("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", "HistoryServerItem")
                        .WithMany()
                        .HasForeignKey("HistoryServerItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("HistoryServerItem");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Navigation("ServerItem")
                        .IsRequired();
                });

            modelBuilder.Entity("DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
