using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Infrastructure.Persistence.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Date).IsRequired();
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();

        builder.HasOne<Collaborator>()
            .WithMany()
            .HasForeignKey(s => s.CollaboratorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.CollaboratorId, s.Date });
    }
}
