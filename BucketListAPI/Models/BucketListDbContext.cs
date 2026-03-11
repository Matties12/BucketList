using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace BucketListAPI.Models;

public partial class BucketListDbContext : DbContext
{
    public BucketListDbContext()
    {
    }

    public BucketListDbContext(DbContextOptions<BucketListDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bucketlistitem> Bucketlistitems { get; set; }

    public virtual DbSet<Personalbucketlist> Personalbucketlists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=BucketListDB;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bucketlistitem>(entity =>
        {
            entity.HasKey(e => e.IdBucketListItem).HasName("PRIMARY");

            entity.ToTable("bucketlistitem");

            entity.HasIndex(e => e.NameBucketListItem, "nameBucketListItem").IsUnique();

            entity.Property(e => e.IdBucketListItem).HasColumnName("idBucketListItem");
            entity.Property(e => e.DescriptionBucketListItem)
                .HasMaxLength(600)
                .HasColumnName("descriptionBucketListItem");
            entity.Property(e => e.NameBucketListItem)
                .HasMaxLength(60)
                .HasColumnName("nameBucketListItem");
        });

        modelBuilder.Entity<Personalbucketlist>(entity =>
        {
            entity.HasKey(e => new { e.FkUser, e.FkBucketListItem })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("personalbucketlist");

            entity.HasIndex(e => e.FkBucketListItem, "fkBucketListItem");

            entity.Property(e => e.FkUser).HasColumnName("fkUser");
            entity.Property(e => e.FkBucketListItem).HasColumnName("fkBucketListItem");
            entity.Property(e => e.Executed)
                .HasDefaultValueSql("'0'")
                .HasColumnName("executed");

            entity.HasOne(d => d.FkBucketListItemNavigation).WithMany(p => p.Personalbucketlists)
                .HasForeignKey(d => d.FkBucketListItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("personalbucketlist_ibfk_2");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Personalbucketlists)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("personalbucketlist_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.NameUser, "nameUser").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.NameUser)
                .HasMaxLength(60)
                .HasColumnName("nameUser");
            entity.Property(e => e.PassWordUser)
                .HasMaxLength(60)
                .HasColumnName("passWordUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
