using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentParent : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string IdParent { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsParent Parent { get; set; }
    }

    internal class MsStudentParentConfiguration : AuditEntityConfiguration<MsStudentParent>
    {
        public override void Configure(EntityTypeBuilder<MsStudentParent> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentParents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentParent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.StudentParents)
                .HasForeignKey(fk => fk.IdParent)
                .HasConstraintName("FK_MsStudentParent_MsParent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}