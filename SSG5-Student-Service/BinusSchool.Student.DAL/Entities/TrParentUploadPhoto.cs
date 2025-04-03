using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrParentUploadPhoto : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdSiblingGroup  { get; set; }
        public string RealFileName  { get; set; }
        public string ServerFileName  { get; set; }
        public string IdParentRole  { get; set; }
        public string RequestParentCardStatus  { get; set; }
        public string ApprovalStatus  { get; set; }


    }
    internal class TrParentUploadPhotoConfiguration : AuditNoUniqueEntityConfiguration<TrParentUploadPhoto>
    {
        public override void Configure(EntityTypeBuilder<TrParentUploadPhoto> builder)
        {
            builder.HasKey(p => p.IdSiblingGroup);

            builder.Property(x => x.IdSiblingGroup)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.RealFileName)   
                .HasColumnType("VARCHAR(200)")            
                .HasMaxLength(200);

            builder.Property(x => x.ServerFileName)   
                .HasColumnType("VARCHAR(200)")            
                .HasMaxLength(200);

            builder.Property(x => x.IdParentRole)
                .HasMaxLength(36); 

            builder.Property(x => x.RequestParentCardStatus)   
                .HasColumnType("VARCHAR(1)")            
                .HasMaxLength(1);    

            builder.Property(x => x.ApprovalStatus)   
                .HasColumnType("VARCHAR(1)")            
                .HasMaxLength(1);  
            

            base.Configure(builder);
        }

    }
}
