using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMappingTreatmentCondition : AuditEntity, IStudentEntity
    {
        public string IdMedicalTreatment { get; set; }
        public string IdMedicalCondition { get; set; }
        public virtual MsMedicalTreatment MedicalTreatment { get; set; }
        public virtual MsMedicalCondition MedicalCondition { get; set; }
    }

    internal class TrMappingTreatmentConditionConfiguration : AuditEntityConfiguration<TrMappingTreatmentCondition>
    {
        public override void Configure(EntityTypeBuilder<TrMappingTreatmentCondition> builder)
        {
            builder.HasOne(x => x.MedicalTreatment)
                .WithMany(x => x.MappingTreatmentConditions)
                .HasForeignKey(fk => fk.IdMedicalTreatment)
                .HasConstraintName("FK_MsMedicalTreatment_TrMappingTreatmentCondition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicalCondition)
                .WithMany(x => x.MappingTreatmentConditions)
                .HasForeignKey(fk => fk.IdMedicalCondition)
                .HasConstraintName("FK_MsMedicalCondition_TrMappingTreatmentCondition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
