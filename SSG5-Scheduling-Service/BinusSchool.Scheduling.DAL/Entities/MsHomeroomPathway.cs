using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsHomeroomPathway : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroom { get; set; }
        public string IdGradePathwayDetail { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual ICollection<MsLessonPathway> LessonPathways { get; set; }
        public virtual MsGradePathwayDetail GradePathwayDetail { get; set; }        
        
    }

    internal class MsHomeroomPathwayConfiguration : AuditEntityConfiguration<MsHomeroomPathway>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomPathway> builder)
        {
            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomPathways)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomPathway_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.GradePathwayDetail)
               .WithMany(x => x.HomeroomPathways)
               .HasForeignKey(fk => fk.IdGradePathwayDetail)
               .HasConstraintName("FK_MsHomeroomPathway_MsGradePathwayDetail")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
