using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HMsExtracurricularParticipant : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdExtracurricularParticipant { get; set; }
        public string IdExtracurricular { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public DateTime JoinDate { get; set; }
        public bool Status { get; set; }
        public bool IsPrimary { get; set; }
        public string Reason { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsGrade Grade { get; set; }
    }
    internal class HMsExtracurricularParticipantConfiguration : AuditNoUniqueEntityConfiguration<HMsExtracurricularParticipant>
    {
        public override void Configure(EntityTypeBuilder<HMsExtracurricularParticipant> builder)
        {
            builder.HasKey(x => x.IdExtracurricularParticipant);

            builder.Property(p => p.IdExtracurricularParticipant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdGrade)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.Reason)
                .HasMaxLength(100);

            builder.Property(x => x.JoinDate)
                .HasColumnType(typeName: "datetime2");

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.HMsExtracurricularParticipants)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_HMsExtracurricularParticipant_MsExtracurricular")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.HMsExtracurricularParticipants)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_HMsExtracurricularParticipant_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.HMsExtracurricularParticipants)
                .IsRequired()
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_HMsExtracurricularParticipant_MsGrade")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }

}
