using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsAdmissionData  : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdRegistrant { get; set; }
        public string IdStudent { get; set; }
        public DateTime? DateofEntry { get; set; }
        public DateTime? DateofFormPurchased { get; set; }
        public DateTime? DateofApplReceived { get; set; }
        public DateTime? DateofReregistration { get; set; }
        public DateTime? JoinToSchoolDate { get; set; }
        public string AdmissionYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdAcademicSemester { get; set; }
        public decimal? TotalScore { get; set; }
        public int? Grade { get; set; }
        public int? IdSchoolSubject { get; set; }
        public int? IdSchoolTPKSStatus { get; set; }
        public string TPKSNotes { get; set; }
        public string IdSchool { get; set; }
        public string IdSchoolLevel { get; set; }
        public string IdYearLevel { get; set; }
        public int IsEnrolledForFirstTime  { get; set; }

        public virtual MsStudent Student { get; set; }


    }
    internal class MsAdmissionDataConfiguration : AuditNoUniqueEntityConfiguration<MsAdmissionData>
    {
        public override void Configure(EntityTypeBuilder<MsAdmissionData> builder)
        {
            builder.HasKey(p => p.IdStudent);

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36);

            builder.Property(x => x.IdRegistrant)             
                .HasMaxLength(10);    

            builder.Property(x => x.DateofEntry)
                .HasColumnType(typeName: "datetime2");     

            builder.Property(x => x.DateofFormPurchased)
                .HasColumnType(typeName: "datetime2"); 

            builder.Property(x => x.DateofApplReceived)
                .HasColumnType(typeName: "datetime2");        

            builder.Property(x => x.DateofReregistration)
                .HasColumnType(typeName: "datetime2");          

            builder.Property(x => x.JoinToSchoolDate)
                .HasColumnType(typeName: "datetime2"); 

            builder.Property(x => x.AdmissionYear)
                .HasMaxLength(36);

            builder.Property(x => x.AcademicYear)
                .HasMaxLength(36);

            builder.Property(x => x.IdAcademicSemester)   
                .HasColumnType("CHAR(1)");     

            builder.Property(x => x.TPKSNotes)   
                .HasColumnType("VARCHAR(50)");           

            builder.Property(x => x.IdSchool)   
                .HasMaxLength(36);

            builder.Property(x => x.IdSchoolLevel)   
                .HasMaxLength(36);      

            builder.Property(x => x.IdYearLevel)   
                .HasMaxLength(36);

            builder.HasOne(x => x.Student)
                .WithOne( y => y.AdmissionData)
                .HasForeignKey<MsAdmissionData>( fk => fk.IdStudent)
                .HasConstraintName("FK_MsAdmissionData_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }
}
