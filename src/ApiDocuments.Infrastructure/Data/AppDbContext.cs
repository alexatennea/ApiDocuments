using ApiDocuments.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiDocuments.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the ApiDocuments application.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initialises a new instance of <see cref="AppDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>Gets or sets the documents table.</summary>
    public DbSet<Document> Documents { get; set; }

    /// <summary>Gets or sets the document audit table.</summary>
    public DbSet<DocumentAudit> DocumentAudits { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.BlobPath).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.FileSizeBytes).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.UpdatedAtUtc).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.Audits)
                  .WithOne(a => a.Document)
                  .HasForeignKey(a => a.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentAudit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Details).HasMaxLength(2048);
            entity.Property(e => e.PerformedAtUtc).IsRequired();
            entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(256);
        });
    }
}
