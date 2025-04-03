using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Student
{
    public class MsStudentParent : AuditEntity, IDocumentEntity
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
