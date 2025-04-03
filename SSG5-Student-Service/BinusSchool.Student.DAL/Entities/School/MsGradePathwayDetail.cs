using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsGradePathwayDetail : AuditEntity, IStudentEntity
    {
        public string IdGradePathway { get; set; }
        public string IdPathway { get; set; }

        public virtual MsGradePathway GradePathway { get; set; }
        public virtual MsPathway Pathway { get; set; }
        public virtual ICollection<MsHomeroomPathway> HomeroomPathways { get; set; }
    }
    internal class MsGradePathwayDetailConfiguration : AuditEntityConfiguration<MsGradePathwayDetail>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathwayDetail> builder)
        {
            builder.HasOne(x => x.GradePathway)
               .WithMany(x => x.GradePathwayDetails)
               .HasForeignKey(fk => fk.IdGradePathway)
               .HasConstraintName("FK_MsGradePathwayDetail_MsGradePathway")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.Pathway)
                .WithMany(x => x.GradePathwayDetails)
                .HasForeignKey(fk => fk.IdPathway)
                .HasConstraintName("FK_MsGradePathwayDetail_MsPathway")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
