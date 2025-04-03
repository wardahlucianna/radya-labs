using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentLicenseEmail : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdStudent { get; set; }       
        public string Email { get; set; }
        public int IsLicensed { get; set; }
        public virtual MsStudent Student { get; set; }
        
    }

    internal class MsStudentLicenseEmailConfiguration : AuditNoUniqueEntityConfiguration<MsStudentLicenseEmail>
    {
        public override void Configure(EntityTypeBuilder<MsStudentLicenseEmail> builder)
        {      
            builder.HasKey(p => new {p.IdStudent,p.Email});            

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36)
                .IsRequired();

             builder.Property(x => x.Email)                     
                .HasMaxLength(200);
            

            builder.HasOne(x => x.Student)
                .WithMany( y => y.StudentLicenseEmail)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentLicenseEmail_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);
    
            
            base.Configure(builder);
        }
    }
}
