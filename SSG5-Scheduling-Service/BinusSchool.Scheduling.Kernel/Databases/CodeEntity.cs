using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Scheduling.Kernel.Databases;

public abstract class CodeEntity : AuditEntity
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class CodeEntityConfiguration<T> : AuditEntityConfiguration<T> where T : CodeEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.Code);

        base.Configure(builder);
    }
}