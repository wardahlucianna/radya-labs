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
    public class MsMedicalHospital : AuditEntity, IStudentEntity
    {
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalEmail { get; set; }
        public string HospitalPhoneNumber { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMedicalDoctor> MedicalDoctors { get; set; }
        public virtual ICollection<TrMedicalRecordEntry> MedicalRecordEntries { get; set; }
    }
    
    internal class MsMedicalHospitalConfiguration : AuditEntityConfiguration<MsMedicalHospital>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalHospital> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalHospitals)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalHospital")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.HospitalName)
                .HasMaxLength(100);

            builder.Property(x => x.HospitalAddress)
                .HasMaxLength(100);

            builder.Property(x => x.HospitalEmail)
                .HasMaxLength(100);

            builder.Property(x => x.HospitalPhoneNumber)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
