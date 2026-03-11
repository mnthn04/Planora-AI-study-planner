using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(s => s.Topics)
                    .WithOne(t => t.Subject)
                    .HasForeignKey(t => t.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.StudyPlan)
                    .WithOne(p => p.Subject)
                    .HasForeignKey<StudyPlan>(p => p.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<StudyPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(p => p.StudyTasks)
                    .WithOne(t => t.StudyPlan)
                    .HasForeignKey(t => t.StudyPlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            builder.Entity<StudyTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
            });
        }
    }
}
