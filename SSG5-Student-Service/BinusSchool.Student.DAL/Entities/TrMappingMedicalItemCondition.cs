using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMappingMedicalItemCondition : AuditEntity, IStudentEntity
    {
        public string IdMedicalItem { get; set; }
        public string IdMedicalCondition { get; set; }
        public virtual MsMedicalItem MedicalItem { get; set; }
        public virtual MsMedicalCondition MedicalCondition { get; set; }
    }

    internal class TrMappingMedicalItemConditionConfiguration : AuditEntityConfiguration<TrMappingMedicalItemCondition>
    {
        public override void Configure(EntityTypeBuilder<TrMappingMedicalItemCondition> builder)
        {
            builder.HasOne(x => x.MedicalItem)
                .WithMany(x => x.MappingMedicalItemConditions)
                .HasForeignKey(fk => fk.IdMedicalItem)
                .HasConstraintName("FK_MsMedicalItem_TrMappingMedicalItemCondition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicalCondition)
                .WithMany(x => x.MappingMedicalItemConditions)
                .HasForeignKey(fk => fk.IdMedicalCondition)
                .HasConstraintName("FK_MsMedicalCondition_TrMappingMedicalItemCondition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
