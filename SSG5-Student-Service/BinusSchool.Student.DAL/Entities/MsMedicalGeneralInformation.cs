using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.Formula.Functions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMedicalGeneralInformation : AuditEntity, IStudentEntity
    {
        public string IdUser { get; set; }
        public string HealthCondition { get; set; }
        public string ApprovalMedicine { get; set; }
        public string AllergiesDetails { get; set; }
        public string HealthNotesFromParents { get; set; }
        public string MedicationNotesFromParents { get; set; }
    }

    internal class MsMedicalGeneralInformationConfiguration : AuditEntityConfiguration<MsMedicalGeneralInformation>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalGeneralInformation> builder)
        {
            builder.Property(x => x.HealthCondition)
                .HasMaxLength(100);

            builder.Property(x => x.ApprovalMedicine)
                .HasMaxLength(100);

            builder.Property(x => x.AllergiesDetails)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }

}
