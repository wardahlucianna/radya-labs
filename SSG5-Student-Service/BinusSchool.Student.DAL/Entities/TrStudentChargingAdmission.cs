using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentChargingAdmission  : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string FormNumber { get; set; }
        public string AdmissionYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdAcademicSemester { get; set; }
        public string IdSchool { get; set; }
        public string IdSchoolLevel { get; set; }
        public string IdYearLevel { get; set; }
        public string ComponentClass { get; set; }
        public string FeeGroupName { get; set; }
        public decimal ChargingAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public Int16 ChargingStatus { get; set; }

        public virtual MsStudent Student { get; set; }

    }
    internal class TrStudentChargingAdmissionConfiguration : AuditEntityConfiguration<TrStudentChargingAdmission>
    {
        public override void Configure(EntityTypeBuilder<TrStudentChargingAdmission> builder)
        {            

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36);

            builder.Property(x => x.FormNumber)             
                .HasMaxLength(10);

            builder.Property(x => x.AdmissionYear)
                .HasMaxLength(36);

            builder.Property(x => x.AcademicYear)
                .HasMaxLength(36);

            builder.Property(x => x.IdAcademicSemester)   
                .HasColumnType("CHAR(1)");

            builder.Property(x => x.IdSchool)   
                .HasMaxLength(36); 

            builder.Property(x => x.IdSchoolLevel)   
                .HasMaxLength(36); 

            builder.Property(x => x.IdYearLevel)   
                .HasMaxLength(36);     
            
            builder.Property(x => x.ComponentClass)   
                .HasMaxLength(50);

            builder.Property(x => x.FeeGroupName)   
                .HasMaxLength(50);

            builder.Property(x => x.ChargingAmount)   
                .HasColumnType("money");

            builder.Property(x => x.DueDate)
                .HasColumnType(typeName: "datetime2");     
            
            builder.HasOne(x => x.Student)
                .WithMany( y => y.StudentChargingAdmission)
                .HasForeignKey( fk => fk.IdStudent)
                .HasConstraintName("FK_TrStudentChargingAdmission_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }

    }
}
