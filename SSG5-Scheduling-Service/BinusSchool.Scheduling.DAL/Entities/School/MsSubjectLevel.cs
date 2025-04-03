using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsSubjectLevel : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }

        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsNews { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsOlds { get; set; }
        //public virtual ICollection<HMsHomeroomStudentEnrollment> HMsHomeroomStudentEnrollmentsNews { get; set; }
        //public virtual ICollection<HMsHomeroomStudentEnrollment> HMsHomeroomStudentEnrollmentsOlds { get; set; }
    }

    internal class MsSubjectLevelConfiguration : CodeEntityConfiguration<MsSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectLevel> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectLevels)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectLevel_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
