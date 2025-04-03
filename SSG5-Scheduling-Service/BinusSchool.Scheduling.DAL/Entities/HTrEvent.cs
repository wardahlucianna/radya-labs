using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEvent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
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
        public virtual TrEventChange EventChange { get; set; }
        public virtual MsEventType EventType { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsCertificateTemplate CertificateTemplate { get; set; }
        public virtual ICollection<HTrEventIntendedFor> EventIntendedFor { get; set; }
        public virtual ICollection<HTrEventDetail> EventDetails { get; set; }
        public virtual ICollection<HTrEventBudget> EventBudgets { get; set; }
        public virtual ICollection<HTrEventActivity> EventActivities { get; set; }
        public virtual ICollection<HTrEventApprover> EventApprovers { get; set; }
        public virtual ICollection<HTrEventAttachment> EventAttachments { get; set; }
        //public virtual ICollection<HTrEventApproval> EventApprovals { get; set; }
        public virtual ICollection<HTrEventAwardApprover> EventAwardApprovers { get; set; }
        public virtual ICollection<HTrEventCoordinator> EventCoordinators { get; set; }
    }

    internal class HTrEventConfiguration : AuditNoUniqueEntityConfiguration<HTrEvent>
    {
        public override void Configure(EntityTypeBuilder<HTrEvent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEvent).Name)
                .HasMaxLength(36);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(x => x.EventType)
                .WithMany(x => x.HistoryEvents)
                .HasForeignKey(fk => fk.IdEventType)
                .HasConstraintName("FK_HTrEvent_MsEventType")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.HistoryEvent)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_HTrEvent_MsAcademicYear")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.CertificateTemplate)
              .WithMany(x => x.HistoryEvents)
              .HasForeignKey(fk => fk.IdCertificateTemplate)
              .HasConstraintName("FK_HTrEvent_MsCertificateTemplate")
              .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.IsShowOnCalendarAcademic)
            .IsRequired();

            builder.Property(x => x.IsShowOnSchedule)
            .IsRequired();

            builder.Property(x => x.Objective)
               .HasMaxLength(450);

            builder.Property(x => x.EventLevel)
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
