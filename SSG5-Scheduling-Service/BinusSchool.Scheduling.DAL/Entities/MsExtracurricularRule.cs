using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularRule : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Name { get; set; }
        public int MinEffectives { get; set; }
        public int MaxEffectives { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool Status { get; set; }    
        public DateTime? ReviewDate { get; set; }
        public int DueDayInvoice { get; set; }
        public bool? RegisterForOneAY { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrExtracurricularRuleGradeMapping> ExtracurricularRuleGradeMappings { get; set; }
    }
    internal class MsExtracurricularRuleConfiguration : AuditEntityConfiguration<MsExtracurricularRule>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularRule> builder)
        {
            builder.Property(p => p.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RegistrationStartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.RegistrationEndDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.ReviewDate)
                .HasColumnType(typeName: "datetime2");

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.ExtracurricularRules)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsExtracurricularRule_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }

}
