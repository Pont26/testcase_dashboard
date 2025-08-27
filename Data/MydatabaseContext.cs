using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TestCaseDashboard.Models.mydatabase;

namespace TestCaseDashboard.Data
{
    public partial class mydatabaseContext : DbContext
    {
        public mydatabaseContext()
        {
        }

        public mydatabaseContext(DbContextOptions<mydatabaseContext> options) : base(options)
        {
        }

        partial void OnModelBuilding(ModelBuilder builder);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TestCaseDashboard.Models.mydatabase.Buglist>()
              .HasOne(i => i.Testcase)
              .WithMany(i => i.Buglists)
              .HasForeignKey(i => i.Testcaseid)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<TestCaseDashboard.Models.mydatabase.ProjectTeammember>()
              .HasOne(i => i.Project)
              .WithMany(i => i.ProjectTeammembers)
              .HasForeignKey(i => i.Projectid)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<TestCaseDashboard.Models.mydatabase.ProjectTeammember>()
              .HasOne(i => i.Teammember)
              .WithMany(i => i.ProjectTeammembers)
              .HasForeignKey(i => i.Teammemberid)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<TestCaseDashboard.Models.mydatabase.Testcase>()
              .HasOne(i => i.Project)
              .WithMany(i => i.Testcases)
              .HasForeignKey(i => i.Projectid)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<TestCaseDashboard.Models.mydatabase.TestcaseTeammember>()
              .HasOne(i => i.Teammember)
              .WithMany(i => i.TestcaseTeammembers)
              .HasForeignKey(i => i.Teammemberid)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<TestCaseDashboard.Models.mydatabase.TestcaseTeammember>()
              .HasOne(i => i.Testcase)
              .WithMany(i => i.TestcaseTeammembers)
              .HasForeignKey(i => i.Testcaseid)
              .HasPrincipalKey(i => i.Id);

      
        }

        public DbSet<TestCaseDashboard.Models.mydatabase.Buglist> Buglists { get; set; }

        public DbSet<TestCaseDashboard.Models.mydatabase.Project> Projects { get; set; }

        public DbSet<TestCaseDashboard.Models.mydatabase.ProjectTeammember> ProjectTeammembers { get; set; }

        public DbSet<TestCaseDashboard.Models.mydatabase.Teammember> Teammembers { get; set; }

        public DbSet<TestCaseDashboard.Models.mydatabase.Testcase> Testcases { get; set; }

        public DbSet<TestCaseDashboard.Models.mydatabase.TestcaseTeammember> TestcaseTeammembers { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    }
}