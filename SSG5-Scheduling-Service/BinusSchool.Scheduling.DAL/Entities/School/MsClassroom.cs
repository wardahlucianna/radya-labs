using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsClassroom : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool MsSchool { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> MsGradePathwayClassrooms { get; set; }
    }

    internal class MsClassroomConfiguration : CodeEntityConfiguration<MsClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsClassroom> builder)
        {
            builder.HasOne(x => x.MsSchool)
                .WithMany(x => x.MsClassrooms)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsClassroom_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
