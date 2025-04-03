using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentProfileConfirmation : AuditNoUniqueEntity, IStudentEntity 
    {
        public string IdStudent  { get; set; }
        public string IdUser  { get; set; }

        public virtual MsStudent Student { get; set; }

    }
     internal class TrStudentProfileConfirmationConfiguration : AuditNoUniqueEntityConfiguration<TrStudentProfileConfirmation>
    {
        public override void Configure(EntityTypeBuilder<TrStudentProfileConfirmation> builder)
        {
            builder.HasKey(p => p.IdStudent);

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdUser)
                .HasMaxLength(36);   

             builder.HasOne(x => x.Student)
                .WithOne(x => x.StudentProfileConfirmation)
                .HasForeignKey<TrStudentProfileConfirmation>(fk => fk.IdStudent)
                .HasConstraintName("FK_TrStudentProfileConfirmation_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);         
        }
    }
}
