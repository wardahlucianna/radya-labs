using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsGradePathway : AuditEntity, IUserEntity
    {
        public string IdGrade { get; set; }

        public virtual MsGrade MsGrade { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> MsGradePathwayClassrooms { get; set; }

        public virtual ICollection<MsGradePathwayDetail> GradePathwayDetails { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsGradePathwayConfiguration : AuditEntityConfiguration<MsGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathway> builder)
        {
            builder.HasOne(x => x.MsGrade)
                .WithMany(x => x.MsGradePathways)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsGradePathway_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
