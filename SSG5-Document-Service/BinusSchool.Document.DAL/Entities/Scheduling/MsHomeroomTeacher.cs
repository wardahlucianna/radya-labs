using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Scheduling
{
    public class MsHomeroomTeacher : AuditEntity, IDocumentEntity
    {
        public string IdHomeroom { get; set; }
        public string IdBinusian { get; set; }

        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsHomeroomTeacherConfiguration : AuditEntityConfiguration<MsHomeroomTeacher>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomTeacher> builder)
        {
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.HasOne(x => x.Staff)
                .WithMany(x => x.HomeroomTeachers)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsHomeroomTeacher_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomTeachers)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomTeacher_MsHomeroom")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
