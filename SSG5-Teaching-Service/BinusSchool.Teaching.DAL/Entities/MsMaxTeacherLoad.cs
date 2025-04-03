using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsMaxTeacherLoad : AuditEntity, ITeachingEntity
    {
        public int Max { get; set; }
        public string IdAcademicYear { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsMaxTeacherLoadConfiguration : AuditEntityConfiguration<MsMaxTeacherLoad>
    {
        public override void Configure(EntityTypeBuilder<MsMaxTeacherLoad> builder)
        {
            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.MaxTeacherLoads)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_MsMaxTeacherLoad_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
