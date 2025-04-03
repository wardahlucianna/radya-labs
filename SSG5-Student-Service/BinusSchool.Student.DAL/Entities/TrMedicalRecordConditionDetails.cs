using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMedicalRecordConditionDetails : AuditEntity, IStudentEntity
    {
        public string IdMedicalRecordEntry { get; set; }
        public string IdMedicalCondition { get; set; }
        public virtual TrMedicalRecordEntry MedicalRecordEntry { get; set; }
        public virtual MsMedicalCondition MedicalCondition { get; set; }
    }

    internal class TrMedicalRecordConditionDetailsConfiguration : AuditEntityConfiguration<TrMedicalRecordConditionDetails>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalRecordConditionDetails> builder)
        {
            builder.HasOne(x => x.MedicalRecordEntry)
                .WithMany(x => x.MedicalRecordConditionDetails)
                .HasForeignKey(fk => fk.IdMedicalRecordEntry)
                .HasConstraintName("FK_TrMedicalRecordEntry_TrMedicalRecordConditionDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicalCondition)
                .WithMany(x => x.MedicalRecordConditionDetails)
                .HasForeignKey(fk => fk.IdMedicalCondition)
                .HasConstraintName("FK_MsMedicalCondition_TrMedicalRecordConditionDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
