using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsClassroom : CodeEntity, IUserEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> MsGradePathwayClassrooms { get; set; }
    }

    internal class MsClassroomConfiguration : CodeEntityConfiguration<MsClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsClassroom> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Classrooms)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsClassroom_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
