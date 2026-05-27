using DegeneraTube.Domain.Entities;
using DegeneraTube.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DegeneraTube.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("videos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Description)
            .HasMaxLength(5000);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasDefaultValue(VideoStatus.Processing);

        builder.Property(v => v.HlsPath)
            .HasMaxLength(500);

        builder.Property(v => v.ThumbnailPath)
            .HasMaxLength(500);

        builder.Property(v => v.Resolutions)
            .HasColumnType("jsonb");

        builder.Property(v => v.Tags)
            .HasColumnType("jsonb");

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.CreatedAt);

        builder.HasMany(v => v.Comments)
            .WithOne(c => c.Video)
            .HasForeignKey(c => c.VideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}