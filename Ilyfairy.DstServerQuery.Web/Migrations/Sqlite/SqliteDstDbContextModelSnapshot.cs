﻿// <auto-generated />
using System;
using Ilyfairy.DstServerQuery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Ilyfairy.DstServerQuery.Web.Migrations.Sqlite
{
    [DbContext(typeof(SqliteDstDbContext))]
    partial class SqliteDstDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("BINARY")
                .HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Day")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DaysElapsedInSeason")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DaysLeftInSeason")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("DaysInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Platform")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Name", "Platform");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameMode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Intent")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Platform")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UpdateTime")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id", "UpdateTime", "Name");

                    b.ToTable("ServerHistories");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("DateTime")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DaysInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDetailed")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Season")
                        .HasColumnType("TEXT");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DaysInfoId")
                        .IsUnique();

                    b.HasIndex("ServerId");

                    b.HasIndex("Id", "DateTime");

                    b.ToTable("ServerHistoryItems");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.HistoryServerItemPlayer", b =>
                {
                    b.Property<long>("HistoryServerItemId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerId")
                        .HasColumnType("TEXT");

                    b.HasKey("HistoryServerItemId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("HistoryServerItemPlayerPair");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.ServerCountInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AllPlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AllServerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayStationPlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayStationServerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SteamPlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SteamServerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SwitchPlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SwitchServerCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UpdateDate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WeGamePlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WeGameServerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("XboxPlayerCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("XboxServerCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ServerHistoryCountInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.TagColorItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

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
