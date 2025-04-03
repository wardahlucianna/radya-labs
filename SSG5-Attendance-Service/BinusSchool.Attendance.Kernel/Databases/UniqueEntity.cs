using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Attendance.Kernel.Databases;

public abstract class UniqueEntity : UniquelessEntity
{
}

public class UniqueEntityConfiguration<T> : UniquelessEntityConfiguration<T> where T : UniqueEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        // set field Id as Primary Key
        builder.HasKey(p => p.Id);

        base.Configure(builder);
    }
}