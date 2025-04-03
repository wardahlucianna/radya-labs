using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMedicalCondition : AuditEntity, IStudentEntity
    {
        public string MedicalConditionName { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrMappingMedicalItemCondition> MappingMedicalItemConditions { get; set; }
        public virtual ICollection<TrMappingTreatmentCondition> MappingTreatmentConditions { get; set; }
        public virtual ICollection<TrMedicalRecordConditionDetails> MedicalRecordConditionDetails { get; set; }
    }

    internal class MsMedicalConditionConfiguration : AuditEntityConfiguration<MsMedicalCondition>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalCondition> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalConditions)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalCondition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.MedicalConditionName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
