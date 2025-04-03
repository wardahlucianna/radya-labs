using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricular : AuditEntity, ISchedulingEntity
    {
        public int Semester { get; set; }
        public string Name { get; set; }
        public string IdExtracurricularGroup { get; set; }     // FK: MsExtracurricularGroup
        public string Description { get; set; }
        public bool ShowAttendanceRC { get; set; }
        public bool ShowScoreRC { get; set; }
        public bool IsRegularSchedule { get; set; }
        public DateTime ElectivesStartDate { get; set; }
        public DateTime ElectivesEndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public ExtracurricularCategory Category { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public DateTime? ScoreStartDate { get; set; }
        public DateTime? ScoreEndDate { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public bool NeedObjective { get; set; }
        public bool? ShowParentStudent { get; set; }
        public string IdExtracurricularType { get; set; }

        public virtual MsExtracurricularGroup ExtracurricularGroup { get; set; }
        public virtual ICollection<MsExtracurricularParticipant> ExtracurricularParticipants { get; set; }
        public virtual ICollection<HMsExtracurricularParticipant> HMsExtracurricularParticipants { get; set; }
        public virtual ICollection<MsExtracurricularSpvCoach> ExtracurricularSpvCoach { get; set; }
        public virtual ICollection<TrExtracurricularScoreEntry> ExtracurricularScoreEntries { get; set; }
        public virtual ICollection<TrExtracurricularSessionMapping> ExtracurricularSessionMappings { get; set; }
        public virtual ICollection<TrExtracurricularGeneratedAtt> ExtracurricularGeneratedAtts { get; set; }
        public virtual ICollection<MsExtracurricularNoAttDateMapping> ExtracurricularNoAttDateMappings { get; set; }
        public virtual ICollection<TrExtracurricularGradeMapping> ExtracurricularGradeMappings { get; set; }
        public virtual ICollection<MsExtracurricularObjective> ExtracurricularObjectives { get; set; }
        public virtual ICollection<MsExtracurricularScoreLegendMapping> ExtracurricularScoreLegendMappings { get; set; }
        public virtual ICollection<MsExtracurricularScoreCompMapping> ExtracurricularScoreCompMappings { get; set; }
        public virtual ICollection<TrExtracurricularExternalCoachAtt> ExtracurricularExternalCoachAtts { get; set; }
        public virtual ICollection<MsExtracurricularExtCoachMapping> ExtracurricularExtCoachMappings { get; set; }
        public virtual MsExtracurricularType ExtracurricularType { get; set; }
    }

    internal class MsExtracurricularConfiguration : AuditEntityConfiguration<MsExtracurricular>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricular> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularGroup)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Category)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Price)
                .HasColumnType("money")
                .IsRequired();

            builder.Property(x => x.IdExtracurricularType)
                .HasMaxLength(36);

            builder.HasOne(x => x.ExtracurricularGroup)
                .WithMany(x => x.Extracurriculars)
                .HasForeignKey(fk => fk.IdExtracurricularGroup)
                .HasConstraintName("FK_MsExtracurricular_MsExtracurricularGroup")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ExtracurricularType)
                .WithMany(x => x.Extracurriculars)
                .HasForeignKey(fk => fk.IdExtracurricularType)
                .HasConstraintName("FK_MsExtracurricular_MsExtracurricularType")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
