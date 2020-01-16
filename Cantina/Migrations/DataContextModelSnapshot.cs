﻿// <auto-generated />
using System;
using Cantina.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cantina.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Cantina.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<bool>("Confirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying(20)")
                        .HasMaxLength(20);

                    b.Property<byte>("Role")
                        .HasColumnType("smallint");

                    b.Property<string>("passwordHash")
                        .IsRequired()
                        .HasColumnName("PasswordHash")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<string>("salt")
                        .IsRequired()
                        .HasColumnName("salt")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasAlternateKey("Email", "Name");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Cantina.Models.UserHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<byte>("Type")
                        .HasColumnType("smallint");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserID");

                    b.ToTable("History");
                });

            modelBuilder.Entity("Cantina.Models.User", b =>
                {
                    b.OwnsOne("Cantina.Models.UserProfile", "Profile", b1 =>
                        {
                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.Property<DateTime?>("Birthday")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<byte>("Gender")
                                .HasColumnType("smallint");

                            b1.Property<DateTime?>("LastEnterDate")
                                .HasColumnType("date");

                            b1.Property<string>("Location")
                                .HasColumnType("character varying(32)")
                                .HasMaxLength(32);

                            b1.Property<int>("OnlineTime")
                                .HasColumnType("integer");

                            b1.Property<DateTime>("RegisterDate")
                                .HasColumnType("date");

                            b1.Property<string>("messageStyle")
                                .HasColumnName("Profile_MessageStyle")
                                .HasColumnType("text");

                            b1.HasKey("UserId");

                            b1.ToTable("Users");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });
                });

            modelBuilder.Entity("Cantina.Models.UserHistory", b =>
                {
                    b.HasOne("Cantina.Models.User", "User")
                        .WithMany("History")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
