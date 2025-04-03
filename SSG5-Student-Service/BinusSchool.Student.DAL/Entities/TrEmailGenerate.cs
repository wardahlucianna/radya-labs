using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEmailGenerate : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdStudent { get; set; }       
        public string GivenName { get; set; } //fistname
        public string MiddleName { get; set; } //null
        public string Surname { get; set; } //last name
        public string DisplayName { get; set; } //nama lengkap
        public string Description { get; set; } //null
        public string EmailAddress { get; set; }
        public string SamAccountName { get; set; }//binusianid
        public string Password { get; set; }
        public string Division { get; set; }
        public int IsSync { get; set; }   
        public string Error_Descr { get; set; }         
        public virtual MsStudent Student { get; set; }

    }
    internal class TrEmailGenerateConfiguration : AuditNoUniqueEntityConfiguration<TrEmailGenerate>
    {
        public override void Configure(EntityTypeBuilder<TrEmailGenerate> builder)
        {      
            
            builder.HasKey(p => p.IdStudent);     

            builder.Property(x => x.GivenName)             
                .HasMaxLength(250);

            builder.Property(x => x.MiddleName)             
                .HasMaxLength(250);

            builder.Property(x => x.Surname)             
                .HasMaxLength(250);    

            builder.Property(x => x.DisplayName)             
                .HasMaxLength(500);

            builder.Property(x => x.Description)             
                .HasMaxLength(100);

            builder.Property(x => x.EmailAddress)             
                .HasMaxLength(200);   

            builder.Property(x => x.SamAccountName)             
                .HasMaxLength(100);   

            builder.Property(x => x.Password)             
                .HasMaxLength(200);   

            builder.Property(x => x.Division)             
                .HasMaxLength(50);     

            builder.Property(x => x.Error_Descr)    
                .HasColumnType("VARCHAR(200)");         
                     
            
            builder.HasOne(x => x.Student)
                .WithOne( y => y.EmailGenerate)
                .HasForeignKey<TrEmailGenerate>(fk => fk.IdStudent)
                .HasConstraintName("FK_TrEmailGenerate_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
