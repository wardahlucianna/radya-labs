using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsClassroomDivision : AuditEntity, ISchoolEntity
    {
        public string IdGradePathwayClassroom { get; set; }
        public string IdDivision { get; set; }

        public virtual MsGradePathwayClassroom PathwayClassroom { get; set; }
        public virtual MsDivision Division { get; set; }
    }

    internal class MsClassroomDivisionConfiguration : AuditEntityConfiguration<MsClassroomDivision>
    {
        public override void Configure(EntityTypeBuilder<MsClassroomDivision> builder)
        {
            builder.HasOne(x => x.PathwayClassroom)
                .WithMany(x => x.ClassroomDivisions)
                .HasForeignKey(fk => fk.IdGradePathwayClassroom)
                .HasConstraintName("FK_MsClassroomDivision_MsGradePathwayClassroom")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Division)
               .WithMany(x => x.ClassroomDivisions)
               .HasForeignKey(fk => fk.IdDivision)
               .HasConstraintName("FK_MsClassroomDivision_MsDiviision")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
