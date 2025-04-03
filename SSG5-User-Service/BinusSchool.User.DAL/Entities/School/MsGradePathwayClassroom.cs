using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsGradePathwayClassroom : AuditEntity, IUserEntity
    {
        public string IdGradePathway { get; set; }
        public string IdClassroom { get; set; }
        
        public virtual MsGradePathway MsGradePathway { get; set; }
        public virtual MsClassroom MsClassroom { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsGradePathwayClassroomConfiguration : AuditEntityConfiguration<MsGradePathwayClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathwayClassroom> builder)
        {
            builder.HasOne(x => x.MsGradePathway)
                .WithMany(x => x.MsGradePathwayClassrooms)
                .HasForeignKey(fk => fk.IdGradePathway)
                .HasConstraintName("FK_MsGradePathwayClassroom_MsGradePathway")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.MsClassroom)
                .WithMany(x => x.MsGradePathwayClassrooms)
                .HasForeignKey(fk => fk.IdClassroom)
                .HasConstraintName("FK_MsGradePathwayClassroom_MsClassroom")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
