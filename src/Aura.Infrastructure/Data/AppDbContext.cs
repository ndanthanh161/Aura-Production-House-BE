using Aura.Domain.Entity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    #region
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuraKnowledge> AuraKnowledge { get; set; }
    public DbSet<ChatLog> ChatLogs { get; set; }
    #endregion
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        base.OnModelCreating(modelBuilder);

        // ==================== Role ====================
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(200);
        });

        // ==================== User ====================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.Property(u => u.Avatar).HasMaxLength(500);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
        });

        // ==================== Package ====================
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Description).HasMaxLength(1000);

            // Serialize List<string> Benefits thành JSON column
            var listComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            entity.Property(p => p.Benefits)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("text")
                .Metadata.SetValueComparer(listComparer);
        });

        // ==================== Project ====================
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
            entity.Property(p => p.Revenue).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Description).HasMaxLength(2000);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // Snapshot Benefits từ Package tại thời điểm tạo Project, serialize thành JSON column
            var projectListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            entity.Property(p => p.Benefits)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("text")
                .Metadata.SetValueComparer(projectListComparer);

            entity.HasOne(p => p.Client)
                .WithMany(u => u.ClientProjects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Staff)
                .WithMany(u => u.StaffProjects)
                .HasForeignKey(p => p.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Package)
                .WithMany(pkg => pkg.Projects)
                .HasForeignKey(p => p.PackageId);
        });

        // ==================== Payment ====================
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Tax).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TransactionId).HasMaxLength(200);

            entity.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            entity.HasOne(p => p.Project)
                .WithMany(pr => pr.Payments)
                .HasForeignKey(p => p.ProjectId);

        });

        // ==================== PortfolioItem ====================
        modelBuilder.Entity<PortfolioItem>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).IsRequired().HasMaxLength(300);
            entity.Property(p => p.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(p => p.Description).HasMaxLength(2000);

            entity.Property(p => p.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);

            entity.HasOne(p => p.Project)
                .WithMany(pr => pr.PortfolioItems)
                .HasForeignKey(p => p.ProjectId)
                .IsRequired(false);
        });

        // ==================== ContactMessage ====================
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(256);
            entity.Property(c => c.Subject).IsRequired().HasMaxLength(300);
            entity.Property(c => c.Message).IsRequired().HasMaxLength(5000);
        });

        // ==================== RefreshToken ====================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).IsRequired();
            entity.HasIndex(r => r.Token).IsUnique();

            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);
        });

        // ==================== AuraKnowledge (RAG) ====================
        modelBuilder.Entity<AuraKnowledge>(entity =>
        {
            entity.HasKey(k => k.Id);
            entity.Property(k => k.Content).IsRequired();
            entity.Property(k => k.Embedding).HasColumnType("vector(1536)"); // 1536 for OpenAI text-embedding-3-small
        });
    }

    // Tự động set CreatedAt / UpdatedAt
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var createdAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAt != null) createdAt.CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                var updatedAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAt != null) updatedAt.CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
