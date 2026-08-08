using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PipelineHub.Domain;

namespace PipelineHub.Infrastructure.Persistence;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Type)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        // Parameters is a small, immutable key/value bag — stored as a JSON column rather
        // than a child table. A ValueComparer is required because EF can't diff dictionaries
        // by reference equality.
        var parametersComparer = new ValueComparer<IReadOnlyDictionary<string, string>>(
            (a, b) => a!.OrderBy(kv => kv.Key).SequenceEqual(b!.OrderBy(kv => kv.Key)),
            d => d.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
            d => (IReadOnlyDictionary<string, string>)d.ToDictionary(kv => kv.Key, kv => kv.Value));

        builder.Property(j => j.Parameters)
            .HasConversion(
                d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
                s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .Metadata.SetValueComparer(parametersComparer);

        builder.Property(j => j.ResultOutputPath).HasMaxLength(1024);
        builder.Property(j => j.ErrorMessage).HasMaxLength(4000);

        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.CreatedAt);
    }
}
