﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RiseDiary.WebUI.Data;

#nullable disable

namespace RiseDiary.WebUI.Migrations
{
    [DbContext(typeof(DiaryDbContext))]
    [Migration("20220901080257_AddDescription_Scope_Theme")]
    partial class AddDescription_Scope_Theme
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.7");

            modelBuilder.Entity("RiseDiary.Model.AppSetting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("AppSettings");
                });

            modelBuilder.Entity("RiseDiary.Model.Cogitation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("RecordId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RecordId");

                    b.ToTable("Cogitations");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CameraModel")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifyDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SizeByte")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Taken")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryImageFull", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<Guid>("ImageId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ImageId")
                        .IsUnique();

                    b.ToTable("FullSizeImages");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifyDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Records");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordImage", b =>
                {
                    b.Property<Guid>("RecordId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ImageId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.HasKey("RecordId", "ImageId");

                    b.HasIndex("ImageId");

                    b.HasIndex("RecordId", "ImageId");

                    b.ToTable("RecordImages");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordTheme", b =>
                {
                    b.Property<Guid>("RecordId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ThemeId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.HasKey("RecordId", "ThemeId");

                    b.HasIndex("ThemeId");

                    b.HasIndex("RecordId", "ThemeId");

                    b.ToTable("RecordThemes");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryScope", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ScopeName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Scopes");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryTheme", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Actual")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThemeName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ScopeId");

                    b.ToTable("Themes");
                });

            modelBuilder.Entity("RiseDiary.Model.TempImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Modification")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SizeByte")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("SourceImageId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SourceImageId")
                        .IsUnique();

                    b.ToTable("TempImages");
                });

            modelBuilder.Entity("RiseDiary.Model.Cogitation", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryRecord", "Record")
                        .WithMany("Cogitations")
                        .HasForeignKey("RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Record");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryImageFull", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryImage", null)
                        .WithOne("FullImage")
                        .HasForeignKey("RiseDiary.Model.DiaryImageFull", "ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordImage", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryImage", "Image")
                        .WithMany("RecordsRefs")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RiseDiary.Model.DiaryRecord", "Record")
                        .WithMany("ImagesRefs")
                        .HasForeignKey("RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");

                    b.Navigation("Record");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecordTheme", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryRecord", "Record")
                        .WithMany("ThemesRefs")
                        .HasForeignKey("RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RiseDiary.Model.DiaryTheme", "Theme")
                        .WithMany("RecordsRefs")
                        .HasForeignKey("ThemeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Record");

                    b.Navigation("Theme");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryTheme", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryScope", "Scope")
                        .WithMany("Themes")
                        .HasForeignKey("ScopeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Scope");
                });

            modelBuilder.Entity("RiseDiary.Model.TempImage", b =>
                {
                    b.HasOne("RiseDiary.Model.DiaryImage", null)
                        .WithOne("TempImage")
                        .HasForeignKey("RiseDiary.Model.TempImage", "SourceImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryImage", b =>
                {
                    b.Navigation("FullImage");

                    b.Navigation("RecordsRefs");

                    b.Navigation("TempImage");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryRecord", b =>
                {
                    b.Navigation("Cogitations");

                    b.Navigation("ImagesRefs");

                    b.Navigation("ThemesRefs");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryScope", b =>
                {
                    b.Navigation("Themes");
                });

            modelBuilder.Entity("RiseDiary.Model.DiaryTheme", b =>
                {
                    b.Navigation("RecordsRefs");
                });
#pragma warning restore 612, 618
        }
    }
}
