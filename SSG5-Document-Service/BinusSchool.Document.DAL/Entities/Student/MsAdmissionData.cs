using System;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Student
{
    public class MsAdmissionData  : AuditNoUniqueEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }
        public DateTime? JoinToSchoolDate { get; set; }
        public string IdSchool { get; set; }

        public virtual MsStudent Student { get; set; }
    }
    internal class MsAdmissionDataConfiguration : AuditNoUniqueEntityConfiguration<MsAdmissionData>
    {
        public override void Configure(EntityTypeBuilder<MsAdmissionData> builder)
        {
            builder.HasKey(p => p.IdStudent);

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.Property(x => x.JoinToSchoolDate)
                .HasColumnType(typeName: "datetime2"); 

            builder.HasOne(x => x.Student)
                .WithOne(y => y.AdmissionData)
                .HasForeignKey<MsAdmissionData>(fk => fk.IdStudent)
                .HasConstraintName("FK_MsAdmissionData_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }
}
