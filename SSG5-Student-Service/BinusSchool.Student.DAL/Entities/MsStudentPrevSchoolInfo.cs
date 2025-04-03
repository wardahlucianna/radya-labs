using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentPrevSchoolInfo : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string IdRegistrant { get; set; }    
        public string Grade { get; set; }
        public string YearAttended { get; set; }
        public string YearWithdrawn { get; set; }
        public Int16 IsHomeSchooling { get; set; }
        public string IdPreviousSchoolNew { get; set; }
        public string IdPreviousSchoolOld { get; set; }
        public virtual MsPreviousSchoolNew PreviousSchoolNew { get; set; }
        public virtual MsPreviousSchoolOld PreviousSchoolOld { get; set; }
        public virtual MsStudent Student { get; set; }


    }
    internal class MsStudentPrevSchoolInfoConfiguration : AuditNoUniqueEntityConfiguration<MsStudentPrevSchoolInfo>
    {
        public override void Configure(EntityTypeBuilder<MsStudentPrevSchoolInfo> builder)
        {   
            builder.HasKey(p => new{p.IdStudent,p.IdRegistrant});

            builder.Property(x => x.IdStudent)                
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdRegistrant)                
                .HasMaxLength(10)
                .IsRequired();    
          
            builder.Property(x => x.IdPreviousSchoolNew)                   
                .HasMaxLength(36);

            builder.Property(x => x.IdPreviousSchoolOld)                   
                .HasMaxLength(36);    
          
            builder.Property(x => x.Grade)    
                .HasColumnType("VARCHAR(10)")            
                .HasMaxLength(10);   

            builder.Property(x => x.YearAttended)    
                .HasColumnType("VARCHAR(10)")            
                .HasMaxLength(10); 

            builder.Property(x => x.YearWithdrawn)    
                .HasColumnType("VARCHAR(10)")            
                .HasMaxLength(10);       

            builder.HasOne(x => x.PreviousSchoolNew)
                .WithMany( y => y.StudentPrevSchoolInfo)
                .HasForeignKey( fk => fk.IdPreviousSchoolNew)
                .HasConstraintName("FK_MsStudentPrevSchoolInfo_MsPreviousSchoolNew")
                .OnDelete(DeleteBehavior.SetNull);             

            builder.HasOne(x => x.PreviousSchoolOld)
                .WithMany( y => y.StudentPrevSchoolInfo)
                .HasForeignKey( fk => fk.IdPreviousSchoolOld)
                .HasConstraintName("FK_MsStudentPrevSchoolInfo_MsPreviousSchoolOld")
                .OnDelete(DeleteBehavior.SetNull);  

            builder.HasOne(x => x.Student)
                    .WithOne( y => y.StudentPrevSchoolInfo)
                    .HasForeignKey<MsStudentPrevSchoolInfo>( fk => fk.IdStudent)
                    .HasConstraintName("FK_MsStudentPrevSchoolInfo_MsStudent")
                    .OnDelete(DeleteBehavior.Cascade);
    

            base.Configure(builder);
        }

    }
}
