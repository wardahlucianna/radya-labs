using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.Scheduling
{
    public class MsHomeroomTeacher : AuditEntity, ITeachingEntity
    {
        public string IdHomeroom { get; set; }
        public string IdBinusian { get; set; }
        public string IdTeacherPosition { get; set; }
        public bool IsAttendance { get; set; }
        public bool IsScore { get; set; }
        public bool IsShowInReportCard { get; set; }

        public virtual MsTeacherPosition TeacherPosition { get; set; }
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
                
            builder.Property(x => x.IdTeacherPosition)
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
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.HomeroomTeachers)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsHomeroomTeacher_MsTeacherPosition")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
