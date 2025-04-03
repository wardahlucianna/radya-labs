using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Util.Kernel.Databases;

public abstract class AuditEntity : UniqueEntity
{
    public string UserIn { get; set; }
    public DateTime? DateIn { get; set; }
    public string UserUp { get; set; }
    public DateTime? DateUp { get; set; }
}

public class AuditEntityConfiguration<T> : UniqueEntityConfiguration<T> where T : AuditEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(p => p.UserIn)
            .HasMaxLength(maxLength: 36)
            .IsRequired();

        builder.Property(p => p.DateIn)
            .IsRequired();

        builder.Property(p => p.UserUp)
            .HasMaxLength(maxLength: 36);

        base.Configure(builder);
    }
}