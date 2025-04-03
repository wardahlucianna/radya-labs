using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsGrade : CodeEntity, ISchedulingEntity
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual ICollection<MsGradePathway> GradePathways { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<MsPeriod> Periods { get; set; }
        public virtual ICollection<MsExtracurricularSupportDocGrade> ExtracurricularSupportDocGrades { get; set; }
        public virtual ICollection<TrExtracurricularGradeMapping> ExtracurricularGradeMappings { get; set; }
        public virtual ICollection<MsExtracurricularParticipant> ExtracurricularParticipants { get; set; }
        public virtual ICollection<HMsExtracurricularParticipant> HMsExtracurricularParticipants { get; set; }
        public virtual ICollection<TrExtracurricularRuleGradeMapping> ExtracurricularRuleGradeMappings { get; set; }
        public virtual ICollection<TrGeneratedScheduleGrade> GeneratedScheduleGrades { get; set; }
        public virtual ICollection<TrImmersionGradeMapping> ImmersionGradeMappings { get; set; }
        public virtual ICollection<TrInvitationBookingSettingDetail> InvitationBookingSettingDetails { get; set; }
        public virtual ICollection<TrInvitationBookingSettingQuota> InvitationBookingSettingQuotas { get; set; }
        public virtual ICollection<TrInvitationBookingSettingExcludeSub> InvBookingSettingExcludeSub { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrInvitationBookingSettingBreak> InvitationBookingSettingBreaks { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2 { get; set; }
    }

    internal class MsGradeConfiguration : CodeEntityConfiguration<MsGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGrade> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.MsGrades)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLevel_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
