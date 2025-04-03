using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Student.Kernel.Databases;

public abstract class ActiveEntity
{
    public bool IsActive { get; set; }
}

public class ActiveEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : ActiveEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(p => p.IsActive)
            .HasColumnName("Stsrc");

        builder.HasIndex(p => p.IsActive);
        builder.HasQueryFilter(p => p.IsActive);
    }
}