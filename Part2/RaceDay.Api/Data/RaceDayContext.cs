// Maps the models and relationships to the database.
// References: EF relationships: [2]. SQL relationship concepts: [10]. See docs/references.md.

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RaceEvent> Events => Set<RaceEvent>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<RaceResult> Results => Set<RaceResult>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<User>(entity =>
            {
                entity.ToTable("Users", table => table.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser', 'Participant')"));
                entity.HasKey(user => user.UserId);
                entity.Property(user => user.FullName).HasMaxLength(100).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
                entity.HasIndex(user => user.Email).IsUnique();
                entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(user => user.Role).HasMaxLength(20).IsRequired();
                entity.Property(user => user.PhoneNumber).HasMaxLength(30).IsRequired();
                entity.Property(user => user.ProfilePictureUrl).HasMaxLength(2048);
                entity.Property(user => user.CreatedAt).HasColumnType("datetime2");
            });

            model.Entity<RaceEvent>(entity =>
            {
                entity.ToTable("Events", table =>
                {
                    table.HasCheckConstraint("CK_Events_Type", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                    table.HasCheckConstraint("CK_Events_Distance", "[DistanceKm] > 0");
                });
                entity.HasKey(race => race.EventId);
                entity.Property(race => race.Name).HasMaxLength(150).IsRequired();
                entity.Property(race => race.Description).HasMaxLength(2000).IsRequired();
                entity.Property(race => race.Location).HasMaxLength(200).IsRequired();
                entity.Property(race => race.EventType).HasMaxLength(10).IsRequired();
                entity.Property(race => race.BannerImageUrl).HasMaxLength(2048);
                entity.Property(race => race.DistanceKm).HasPrecision(8, 2);
                entity.HasOne<User>().WithMany().HasForeignKey(race => race.OrganiserId).OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<Category>(entity =>
            {
                entity.ToTable("Categories", table =>
                {
                    table.HasCheckConstraint("CK_Categories_Distance", "[DistanceKm] > 0");
                    table.HasCheckConstraint("CK_Categories_Fee", "[Fee] >= 0");
                });
                entity.HasKey(category => category.CategoryId);
                entity.Property(category => category.CategoryName).HasMaxLength(100).IsRequired();
                entity.Property(category => category.Description).HasMaxLength(1000).IsRequired();
                entity.Property(category => category.DistanceKm).HasPrecision(8, 2);
                entity.Property(category => category.Fee).HasPrecision(10, 2);
                entity.HasIndex(category => new { category.EventId, category.CategoryName }).IsUnique();
                entity.HasAlternateKey(category => new { category.CategoryId, category.EventId });
                entity.HasOne<RaceEvent>().WithMany().HasForeignKey(category => category.EventId).OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolments");
                entity.HasKey(enrolment => enrolment.EnrolmentId);
                entity.Property(enrolment => enrolment.Status).HasMaxLength(20).HasDefaultValue("Confirmed").IsRequired();
                entity.HasIndex(enrolment => new { enrolment.ParticipantId, enrolment.EventId }).IsUnique();
                entity.HasOne<User>().WithMany().HasForeignKey(enrolment => enrolment.ParticipantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<RaceEvent>().WithMany().HasForeignKey(enrolment => enrolment.EventId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Category>().WithMany()
                    .HasForeignKey(enrolment => new { enrolment.CategoryId, enrolment.EventId })
                    .HasPrincipalKey(category => new { category.CategoryId, category.EventId })
                    .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<RaceResult>(entity =>
            {
                entity.ToTable("Results", table =>
                {
                    table.HasCheckConstraint("CK_Results_Positions", "[OverallPosition] > 0 AND [CategoryPosition] > 0 AND [CategoryPosition] <= [OverallPosition]");
                    table.HasCheckConstraint("CK_Results_Time", "[FinishTime] > '00:00:00'");
                });
                entity.HasKey(result => result.ResultId);
                entity.Property(result => result.FinishTime).HasColumnType("time");
                entity.HasOne<Enrolment>().WithOne().HasForeignKey<RaceResult>(result => result.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(log => log.LogId);
                entity.Property(log => log.Action).HasMaxLength(500).IsRequired();
                entity.HasOne<User>().WithMany().HasForeignKey(log => log.UserId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
