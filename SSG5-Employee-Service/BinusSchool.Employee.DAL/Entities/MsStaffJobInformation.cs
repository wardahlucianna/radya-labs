using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class MsStaffJobInformation : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdBinusian { get; set; } 
        public string IdBusinessUnit { get; set; } 
        public string BusinessUnitName { get; set; } 
        public string IdDepartment { get; set; } 
        public string DepartmentName { get; set; } 
        public string IdPosition { get; set; } 
        public string PositionName { get; set; } 
        public string SubjectSpecialization { get; set; } 
        public int TeacherDurationWeek { get; set; } 
        public string NUPTK { get; set; } 
        public string IdEmployeeStatus { get; set; } 
        public string IdPTKType { get; set; } 
        public string NoSrtPengangkatan { get; set; } 
        public DateTime? TglSrtPengangkatan { get; set; } 
        public string NoSrtKontrak { get; set; } 
        public string NoIndukGuruKontrak { get; set; } 
        public bool IsPrincipalLicensed { get; set; } 
        public string IdLabSkillsLevel { get; set; } 
        public string IdExpSpecialTreatments { get; set; } 
        public string IdBrailleExpLevel { get; set; } 
        public string IdIsyaratLevel { get; set; } 
        public string AdditionalTaskNotes { get; set; } 
        public string IdBusinessUnitGroup { get; set; } 
        public string BusinessUnitGroupName { get; set; } 
        public DateTime? TglSrtKontrakKerja { get; set; } 
        public string Division { get; set; } 


        public virtual MsStaff Staff { get; set; }
        public virtual LtLabSkillsLevel LabSkillsLevel { get; set; }
        public virtual LtSpecialTreatmentsSkillsLevel SpecialTreatmentsSkillsLevel { get; set; }
        public virtual LtPTKType PTKType { get; set; }
        public virtual LtIsyaratLevel IsyaratLevel { get; set; }
        public virtual LtBrailleExpLevel BrailleExpLevel { get; set; }
        public virtual LtEmployeeStatus EmployeeStatus { get; set; }


    }
    
    internal class MsStaffJobInformationConfiguration : AuditNoUniqueEntityConfiguration<MsStaffJobInformation>
    {
        public override void Configure(EntityTypeBuilder<MsStaffJobInformation> builder)
        {
            builder.HasKey(x => x.IdBinusian);
         
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.Property(x => x.IdBusinessUnit)
                .HasMaxLength(36);

            builder.Property(x => x.BusinessUnitName)
                .HasMaxLength(100);    

            builder.Property(x => x.IdDepartment)
                .HasMaxLength(36);        
    
            builder.Property(x => x.DepartmentName)
                .HasMaxLength(100);  

            builder.Property(x => x.IdPosition)
                .HasMaxLength(36);        
    
            builder.Property(x => x.PositionName)
                .HasMaxLength(100);  

            builder.Property(x => x.SubjectSpecialization)
                .HasMaxLength(100);    

            builder.Property(x => x.NUPTK)
                .HasMaxLength(100);  

            builder.Property(x => x.IdEmployeeStatus)
                .HasMaxLength(36);         

            builder.Property(x => x.IdPTKType)
                .HasMaxLength(36);                
            
            builder.Property(x => x.NoSrtPengangkatan)
                .HasMaxLength(50);  

            builder.Property(x => x.TglSrtPengangkatan)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.NoSrtKontrak)
                .HasMaxLength(50);      

            builder.Property(x => x.NoIndukGuruKontrak)
                .HasMaxLength(50);     

            builder.Property(x => x.IdLabSkillsLevel)
                .HasMaxLength(36);    

            builder.Property(x => x.IdExpSpecialTreatments)
                .HasMaxLength(36);      

            builder.Property(x => x.IdBrailleExpLevel)
                .HasMaxLength(36);       

            builder.Property(x => x.IdIsyaratLevel)
                .HasMaxLength(36);           
            
            builder.Property(x => x.AdditionalTaskNotes)
                .HasMaxLength(100); 

            builder.Property(x => x.IdBusinessUnitGroup)
                .HasMaxLength(36);     

            builder.Property(x => x.BusinessUnitGroupName)
                .HasMaxLength(100);   

            builder.Property(x => x.TglSrtKontrakKerja)
                .HasColumnType(typeName: "datetime2");    

            builder.Property(x => x.Division)
                .HasMaxLength(50); 

            builder.HasOne(x => x.Staff)
                .WithOne( y => y.StaffJobInformation)
                .HasForeignKey<MsStaffJobInformation>( fk => fk.IdBinusian)
                .HasConstraintName("FK_MsStaffJobInformation_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LabSkillsLevel)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdLabSkillsLevel)
                .HasConstraintName("FK_MsStaffJobInformation_LtLabSkillsLevel")
                .OnDelete(DeleteBehavior.Restrict);    

            builder.HasOne(x => x.SpecialTreatmentsSkillsLevel)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdExpSpecialTreatments)
                .HasConstraintName("FK_MsStaffJobInformation_LtSpecialTreatmentsSkillsLevel")
                .OnDelete(DeleteBehavior.Restrict);   

            builder.HasOne(x => x.PTKType)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdPTKType)
                .HasConstraintName("FK_MsStaffJobInformation_LtPTKType")
                .OnDelete(DeleteBehavior.Restrict);       

            builder.HasOne(x => x.IsyaratLevel)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdIsyaratLevel)
                .HasConstraintName("FK_MsStaffJobInformation_LtIsyaratLevel")
                .OnDelete(DeleteBehavior.Restrict);   

            builder.HasOne(x => x.BrailleExpLevel)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdBrailleExpLevel)
                .HasConstraintName("FK_MsStaffJobInformation_LtBrailleExpLevel")
                .OnDelete(DeleteBehavior.Restrict);   

            builder.HasOne(x => x.EmployeeStatus)
                .WithMany( y => y.StaffJobInformation)
                .HasForeignKey( fk => fk.IdEmployeeStatus)
                .HasConstraintName("FK_MsStaffJobInformation_LtEmployeeStatus")
                .OnDelete(DeleteBehavior.Restrict);            


            base.Configure(builder);
        }

    }
}
