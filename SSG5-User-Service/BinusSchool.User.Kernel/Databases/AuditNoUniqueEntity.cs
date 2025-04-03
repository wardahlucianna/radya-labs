using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.User.Kernel.Databases;

public abstract class AuditNoUniqueEntity : ActiveEntity
{
    public string UserIn { get; set; }
    public DateTime? DateIn { get; set; }
    public string UserUp { get; set; }
    public DateTime? DateUp { get; set; }
}

public class AuditNoUniqueEntityConfiguration<T> : ActiveEntityConfiguration<T> where T : AuditNoUniqueEntity
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