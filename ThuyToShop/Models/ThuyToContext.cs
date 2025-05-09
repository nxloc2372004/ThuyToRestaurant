﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ThuyTo.Models;

public partial class ThuyToContext : DbContext
{
    public ThuyToContext()
    {
    }

    public ThuyToContext(DbContextOptions<ThuyToContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<Category> Categories { get; set; }
	public DbSet<Payment> Payments { get; set; }
	public virtual DbSet<Commune> Communes { get; set; }
    public DbSet<Feedbacks> Feedbacks { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<FeeShip> FeeShips { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public DbSet<Reservation> Reservations { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.Property(e => e.BlogId).HasColumnName("BlogID");
            entity.Property(e => e.BlogDesc).HasMaxLength(1000);
            entity.Property(e => e.BlogDetail).HasColumnType("ntext");
            entity.Property(e => e.BlogImage).HasMaxLength(1000);
            entity.Property(e => e.BlogName).HasMaxLength(1000);
            entity.Property(e => e.BlogSlug).HasMaxLength(1000);
            entity.Property(e => e.BlogViewCount).HasDefaultValueSql("((0))");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SeoDescription).HasMaxLength(1000);
            entity.Property(e => e.SeoKeyword).HasMaxLength(1000);
            entity.Property(e => e.SeoTitle).HasMaxLength(1000);

            entity.HasOne(d => d.Category).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Blogs_Categories");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.BlogId).HasColumnName("BlogID");
            entity.Property(e => e.Detail).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Levels).HasDefaultValueSql("((1))");
            entity.Property(e => e.ParrentId).HasColumnName("ParrentID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BlogComments_Blogs");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BlogComments_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Alias).HasMaxLength(500);
            entity.Property(e => e.CategoryName).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Detail).HasColumnType("ntext");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ParentCateId).HasColumnName("ParentCateID");
            entity.Property(e => e.SeoDescription).HasMaxLength(500);
            entity.Property(e => e.SeoKeyword).HasMaxLength(500);
            entity.Property(e => e.SeoTitle).HasMaxLength(500);
        });

        modelBuilder.Entity<Commune>(entity =>
        {
            entity.ToTable("Commune");

            entity.Property(e => e.CommuneId)
                .ValueGeneratedNever()
                .HasColumnName("CommuneID");
            entity.Property(e => e.CommuneName).HasMaxLength(50);
            entity.Property(e => e.CommuneType).HasMaxLength(50);
            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

            entity.HasOne(d => d.District).WithMany(p => p.Communes)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK_Commune_Districts");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Message).HasColumnType("ntext");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.Property(e => e.DistrictId)
                .ValueGeneratedNever()
                .HasColumnName("DistrictID");
            entity.Property(e => e.DistricType).HasMaxLength(50);
            entity.Property(e => e.DistrictName).HasMaxLength(50);
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
        });

        modelBuilder.Entity<FeeShip>(entity =>
        {
            entity.HasKey(e => e.FeeShipId).HasName("PK_Shippings");

            entity.Property(e => e.CommuneId).HasColumnName("CommuneID");
            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.ShipPrice).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Commune).WithMany(p => p.FeeShips)
                .HasForeignKey(d => d.CommuneId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_FeeShips_Commune");

            entity.HasOne(d => d.District).WithMany(p => p.FeeShips)
                .HasForeignKey(d => d.DistrictId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_FeeShips_Districts");

            entity.HasOne(d => d.Province).WithMany(p => p.FeeShips)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_FeeShips_Provinces");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.Alias).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ParrentId).HasColumnName("ParrentID");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Address).HasMaxLength(1000);
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TotalPayment).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.IsBestSeller).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ProductDesc).HasMaxLength(1000);
            entity.Property(e => e.ProductDetail).HasColumnType("ntext");
            entity.Property(e => e.ProductDisCount).HasDefaultValueSql("((0))");
            entity.Property(e => e.ProductImage).HasMaxLength(500);
            entity.Property(e => e.ProductName).HasMaxLength(500);
            entity.Property(e => e.ProductPrice)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ProductSlug).HasMaxLength(500);
            entity.Property(e => e.ProductViewCount).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.Property(e => e.ProvinceId)
                .ValueGeneratedNever()
                .HasColumnName("ProvinceID");
            entity.Property(e => e.ProvinceName).HasMaxLength(50);
            entity.Property(e => e.ProvinceSlug).HasMaxLength(50);
            entity.Property(e => e.ProvinceType).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Avatar).HasMaxLength(100);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.ConfirmCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.IsBlocked).HasDefaultValueSql("((1))");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.ReservationId);

            entity.Property(r => r.FullName)
                  .HasMaxLength(100);

            entity.Property(r => r.Phone)
                  .HasMaxLength(15);

            entity.Property(r => r.Email)
                  .HasMaxLength(100);

            entity.Property(r => r.NumberOfGuests);

            // Xóa cấu hình giá trị mặc định cho ReservationDate
            entity.Property(r => r.ReservationDate);

            entity.Property(r => r.SpecialRequest)
                  .HasMaxLength(1000);

            // Xóa cấu hình giá trị mặc định cho CreatedDate
            entity.Property(r => r.CreatedDate);

            entity.Property(r => r.Status)
                  .HasConversion<string>()
                  .HasDefaultValue(ReservationStatus.Available);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
