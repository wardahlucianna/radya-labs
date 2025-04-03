using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentFirstLoginConfirmation : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public virtual MsStudent Student { get; set; }

    }
    internal class MsStudentFirstLoginConfirmationConfiguration : AuditNoUniqueEntityConfiguration<MsStudentFirstLoginConfirmation>
    {
        public override void Configure(EntityTypeBuilder<MsStudentFirstLoginConfirmation> builder)
        {
            builder.HasKey(p => p.IdStudent);  

            builder.Property(x => x.IdStudent)             
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithOne( y => y.StudentFirstLoginConfirmation)
                .HasForeignKey<MsStudentFirstLoginConfirmation>( fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentFirstLoginConfirmation_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);


            base.Configure(builder);      
        }
    }
}
