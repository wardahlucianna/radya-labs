using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentHistoryEmail : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdStudent { get; set; }    
        public string NewEmail { get; set; } 
        public string OldEmail { get; set; }   
        public virtual MsStudent Student { get; set; }
    }
    internal class TrStudentHistoryEmailConfiguration : AuditNoUniqueEntityConfiguration<TrStudentHistoryEmail>
    {
        public override void Configure(EntityTypeBuilder<TrStudentHistoryEmail> builder)
        {
            builder.HasKey(p => p.IdStudent);           

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.NewEmail)             
                .HasMaxLength(200);

            builder.Property(x => x.OldEmail)             
                .HasMaxLength(200); 

             builder.HasOne(x => x.Student)
                    .WithOne( y => y.StudentHistoryEmail)
                    .HasForeignKey<TrStudentHistoryEmail>( fk => fk.IdStudent)
                    .HasConstraintName("FK_TrStudentHistoryEmail_MsStudent")
                    .OnDelete(DeleteBehavior.Cascade);    

            base.Configure(builder);
        }

    }
}
