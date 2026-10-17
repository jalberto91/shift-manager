using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Infrastructure.Persistence.Configurations;

public sealed class CollaboratorConfiguration : IEntityTypeConfiguration<Collaborator>
{
    public void Configure(EntityTypeBuilder<Collaborator> builder)
    {
        builder.ToTable("collaborators");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name).HasMaxLength(Collaborator.NameMaxLength).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(Collaborator.EmailMaxLength).IsRequired();
        builder.Property(c => c.CountryCode).HasMaxLength(Collaborator.CountryCodeLength).IsFixedLength().IsRequired();
    }
}
