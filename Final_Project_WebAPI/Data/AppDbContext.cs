using System;
using System.Collections.Generic;
using Final_Project_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_WebAPI.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assessment> Assessments { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=final-proj-sql-server.database.windows.net;Initial Catalog=Final_Proj;User ID=kaushal;Password=FinalProj@1234;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.ToTable("Assessment");

            entity.Property(e => e.AssessmentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.Assessments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Assessment_Course");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");

            entity.Property(e => e.CourseId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Course_User");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Result");

            entity.Property(e => e.ResultId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");
            entity.Property(e => e.Score).HasColumnType("int");

            entity.HasOne(d => d.Assessment).WithMany(p => p.Results)
                .HasForeignKey(d => d.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Result_Assessment");

            entity.HasOne(d => d.User).WithMany(p => p.Results)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UNQ_Email").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
