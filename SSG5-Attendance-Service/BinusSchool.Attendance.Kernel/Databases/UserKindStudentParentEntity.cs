using BinusSchool.Attendance.Kernel.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Attendance.Kernel.Databases;

public abstract class UserKindStudentParentEntity : AuditEntity
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public Gender Gender { get; set; }
}

public class UserKindStudentParentEntityConfiguration<T> : AuditEntityConfiguration<T>
    where T : UserKindStudentParentEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.FirstName)
            .HasMaxLength(250);

        builder.Property(x => x.MiddleName)
            .HasMaxLength(250);

        builder.Property(x => x.LastName)
            .HasMaxLength(250)
            .IsRequired();


        builder.Property(x => x.Gender)
            .HasConversion<string>()
            .HasMaxLength(6)
            .IsRequired();

        base.Configure(builder);
    }
}