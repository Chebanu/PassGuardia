﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PassGuardia.Domain.DbContexts;

#nullable disable

namespace PassGuardia.Domain.Migrations
{
    [DbContext(typeof(PasswordDbContext))]
    partial class PasswordDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Audit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Exception")
                        .HasColumnType("text")
                        .HasColumnName("exception");

                    b.Property<string>("RequestMethod")
                        .HasColumnType("text")
                        .HasColumnName("request_method");

                    b.Property<string>("RequestPath")
                        .HasColumnType("text")
                        .HasColumnName("request_path");

                    b.Property<int>("StatusCode")
                        .HasColumnType("integer")
                        .HasColumnName("status_code");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("Id");

                    b.ToTable("audit");
                });

            modelBuilder.Entity("PassGuardia.Contracts.Models.Password", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<byte[]>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("encrypted_password");

                    b.HasKey("Id");

                    b.ToTable("passwords");
                });
#pragma warning restore 612, 618
        }
    }
}
