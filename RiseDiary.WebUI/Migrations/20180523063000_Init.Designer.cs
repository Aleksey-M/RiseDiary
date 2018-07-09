﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Migrations
{
    [DbContext(typeof(DiaryDbContext))]
    [Migration("20180523063000_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rc1-32029");

            modelBuilder.Entity("RiseDiary.Model.Cogitation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<int>("RecordId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.ToTable("Cogitations");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<byte[]>("Data");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<DateTime>("Date");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.ToTable("Records");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordImage", b =>
                {
                    b.Property<int>("RecordId");

                    b.Property<int>("ImageId");

                    b.HasKey("RecordId", "ImageId");

                    b.ToTable("RecordImages");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordTheme", b =>
                {
                    b.Property<int>("RecordId");

                    b.Property<int>("ThemeId");

                    b.HasKey("RecordId", "ThemeId");

                    b.ToTable("RecordThemes");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryScope", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ScopeName");

                    b.HasKey("Id");

                    b.ToTable("Scopes");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryTheme", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DiaryScopeId");

                    b.Property<string>("ThemeName");

                    b.HasKey("Id");

                    b.ToTable("Themes");
                });
#pragma warning restore 612, 618
        }
    }
}