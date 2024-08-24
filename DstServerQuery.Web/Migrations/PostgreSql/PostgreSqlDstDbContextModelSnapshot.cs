﻿// <auto-generated />
using System;
using DstServerQuery.EntityFrameworkCore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DstServerQuery.Web.Migrations.PostgreSql
{
    [DbContext(typeof(PostgreSqlDstDbContext))]
    partial class PostgreSqlDstDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("C")
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstDaysInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Day")
                        .HasColumnType("integer");

                    b.Property<int>("DaysElapsedInSeason")
                        .HasColumnType("integer");

                    b.Property<int>("DaysLeftInSeason")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("DaysInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstPlayer", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Platform")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Name", "Platform");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("GameMode")
                        .HasColumnType("text");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IP")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Intent")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Platform")
                        .HasColumnType("integer");

                    b.Property<int>("Port")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdateTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Id", "UpdateTime", "Name");

                    b.ToTable("ServerHistories");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.DstServerHistoryItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("DateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("DaysInfoId")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDetailed")
                        .HasColumnType("boolean");

                    b.Property<int>("PlayerCount")
                        .HasColumnType("integer");

                    b.Property<string>("Season")
                        .HasColumnType("text");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("text");

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
                        .HasColumnType("bigint");

                    b.Property<string>("PlayerId")
                        .HasColumnType("text");

                    b.HasKey("HistoryServerItemId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("HistoryServerItemPlayerPair");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.ServerCountInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AllPlayerCount")
                        .HasColumnType("integer");

                    b.Property<int>("AllServerCount")
                        .HasColumnType("integer");

                    b.Property<int>("PlayStationPlayerCount")
                        .HasColumnType("integer");

                    b.Property<int>("PlayStationServerCount")
                        .HasColumnType("integer");

                    b.Property<int>("SteamPlayerCount")
                        .HasColumnType("integer");

                    b.Property<int>("SteamServerCount")
                        .HasColumnType("integer");

                    b.Property<int?>("SwitchPlayerCount")
                        .HasColumnType("integer");

                    b.Property<int?>("SwitchServerCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("WeGamePlayerCount")
                        .HasColumnType("integer");

                    b.Property<int>("WeGameServerCount")
                        .HasColumnType("integer");

                    b.Property<int>("XboxPlayerCount")
                        .HasColumnType("integer");

                    b.Property<int>("XboxServerCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ServerHistoryCountInfos");
                });

            modelBuilder.Entity("Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities.TagColorItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("text");

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
