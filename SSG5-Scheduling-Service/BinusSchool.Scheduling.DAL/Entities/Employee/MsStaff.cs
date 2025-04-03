using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Employee
{
    public class MsStaff : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdBinusian { get; set; }    
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InitialName { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
        public virtual ICollection<MsExtracurricularSpvCoach> ExtracurricularSpvCoach { get; set; }
        public virtual ICollection<TrEventActivityAwardTeacher> EventActivityAwardTeachers { get; set; }
        public virtual ICollection<HTrEventActivityAwardTeacher> HistoryEventActivityAwardTeachers { get; set; }
        public virtual ICollection<MsImmersion> Immersions { get; set; }
        public virtual ICollection<TrScheduleRealization> ScheduleRealizations { get; set; }
        public virtual ICollection<TrScheduleRealization> ScheduleRealizationsSubtitutes { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsSettingEmailScheduleRealization> SettingEmailScheduleRealizations { get; set; }
        public virtual ICollection<MsEmailRecepient> EmailRecepients { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2sSubtitutes { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2 { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2Subtitutes { get; set; }
        //public virtual ICollection<TrClassDiary> ClassDiaries { get; set; }
        public virtual ICollection<MsVenueMappingApproval> VenueMappingApprovals { get; set; }
    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {

            builder.HasKey(x => x.IdBinusian);

            builder.Property(x => x.InitialName)
              .HasMaxLength(5);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();
         
            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.BinusianEmailAddress)
               .HasMaxLength(60);

            builder.Property(x => x.MobilePhoneNumber1)
                .HasMaxLength(25);

            base.Configure(builder);
        }
    }
}
