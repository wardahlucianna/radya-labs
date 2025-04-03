using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using BinusSchool.Persistence.DocumentDb.Entities.School;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsConsentCustomSchedule : AuditEntity, IDocumentEntity
    {
        public string IdSurveyPeriod { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public virtual LtDay FromDay { get; set; }
        public virtual LtDay ToDay { get; set; }
        public virtual MsSurveyPeriod SurveyPeriod { get; set; }
    }

    internal class MsConsentCustomScheduleConfiguration : AuditEntityConfiguration<MsConsentCustomSchedule>
    {
        public override void Configure(EntityTypeBuilder<MsConsentCustomSchedule> builder)
        {
            builder.Property(x => x.IdSurveyPeriod)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.StartDay)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.EndDay)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.StartTime)
                .IsRequired();

            builder.Property(x => x.EndTime)
                .IsRequired();

            builder.HasOne(x => x.SurveyPeriod)
                .WithMany(x => x.ConsentCustomSchedules)
                .HasForeignKey(fk => fk.IdSurveyPeriod)
                .HasConstraintName("FK_MsConsentCustomSchedule_MsSurveyPeriod")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ToDay)
                .WithMany(x => x.ToConsentCustomSchedules)
                .HasForeignKey(fk => fk.EndDay)
                .HasConstraintName("FK_MsConsentCustomSchedule_LtDay_To")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.FromDay)
                .WithMany(x => x.FromConsentCustomSchedules)
                .HasForeignKey(fk => fk.StartDay)
                .HasConstraintName("FK_MsConsentCustomSchedule_LtDay_From")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
