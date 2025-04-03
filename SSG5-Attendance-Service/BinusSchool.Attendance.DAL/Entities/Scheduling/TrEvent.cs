using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEvent : AuditEntity, IAttendanceEntity
    {
        public string IdEventType { get; set; }
        public string IdAcademicYear { get; set; }
        public string Name { get; set; }
        //public EventAttendanceType AttendanceType { get; set; }
        public bool IsShowOnCalendarAcademic { get; set; }
        public bool IsShowOnSchedule { get; set; }
        public string Objective { get; set; }
        public string Place { get; set; }
        public EventLevel EventLevel { get; set; }
        public string StatusEvent { get; set; }
        public string DescriptionEvent { get; set; }
        public string StatusEventAward { get; set; }
        public string DescriptionEventAward { get; set; }
        public string IdCertificateTemplate { get; set; }
        public bool IsStudentInvolvement { get; set; }
        public virtual MsEventType EventType { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrEventIntendedFor> EventIntendedFor { get; set; }
        public virtual ICollection<TrEventDetail> EventDetails { get; set; }

        public virtual ICollection<TrEventCoordinator> EventCoordinators { get; set; }
        //public virtual ICollection<TrEventBudget> EventBudgets { get; set; }
        //public virtual ICollection<TrEventActivity> EventActivities { get; set; }
        //public virtual ICollection<TrEventApprover> EventApprovers { get; set; }
        //public virtual ICollection<TrEventAttachment> EventAttachments { get; set; }
        //public virtual ICollection<HsEventApproval> EventApprovals { get; set; }
        //public virtual ICollection<TrEventAwardApprover> EventAwardApprovers { get; set; }
        //public virtual ICollection<TrEventCoordinator> EventCoordinators { get; set; }
        //public virtual ICollection<TrEventChange> EventChanges { get; set; }
    }

    internal class TrEventConfiguration : AuditEntityConfiguration<TrEvent>
    {
        public override void Configure(EntityTypeBuilder<TrEvent> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdCertificateTemplate)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventType)
                .WithMany(x => x.TrEvents)
                .HasForeignKey(fk => fk.IdEventType)
                .HasConstraintName("FK_TrEvent_MsEventType")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.TrEvents)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrEvent_MsAcademicYear")
              .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(x => x.IsShowOnCalendarAcademic)
            .IsRequired();

            builder.Property(x => x.IsShowOnSchedule)
            .IsRequired();

            builder.Property(x => x.Objective)
               .HasMaxLength(450);

            builder.Property(x => x.EventLevel)
               .HasConversion<string>()
               .HasMaxLength(14)
               .IsRequired();

            builder.Property(x => x.StatusEvent).HasMaxLength(50);

            builder.Property(x => x.DescriptionEvent).HasMaxLength(450);

            builder.Property(x => x.StatusEventAward).HasMaxLength(50);

            builder.Property(x => x.DescriptionEventAward).HasMaxLength(450);

            base.Configure(builder);
        }
    }
}
