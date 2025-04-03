using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Util.Kernel.Databases;

public abstract class UniquelessEntity : ActiveEntity
{
    public string Id { get; set; }
}

public class UniquelessEntityConfiguration<T> : ActiveEntityConfiguration<T> where T : UniquelessEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        // set column name with format Id{EntityName}
        builder.Property(p => p.Id)
            .HasColumnName("Id" + typeof(T).Name.Remove(0, 2))
            .HasMaxLength(36);

        base.Configure(builder);
    }
}