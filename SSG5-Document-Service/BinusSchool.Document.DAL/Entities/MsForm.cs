using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsForm : AuditEntity, IDocumentEntity
    {
        public string IdDocCategory { get; set; }
        public string IdSchool { get; set; }
        public string IdApprovalType { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdPeriod { get; set; }
        public string Semester { get; set; }
        public string IdSubject { get; set; }
        public bool IsApprovalForm { get; set; }
        public bool IsMultipleForm { get; set; }
        public string JsonFormElement { get; set; }
        public string JsonSchema { get; set; }
        public virtual MsDocCategory DocCategory { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsFormDoc> FormDocs { get; set; }
        public virtual MsFormAssignmentRole FormAssignmentRole { get; set; }

    }

    internal class MsFormConfiguration : AuditEntityConfiguration<MsForm>
    {
        public override void Configure(EntityTypeBuilder<MsForm> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .IsRequired();

            builder.Property(x => x.IdLevel)
                .IsRequired();

            builder.Property(x => x.IdGrade)
                .IsRequired();

            builder.Property(x => x.IdPeriod)
                .IsRequired();
                
            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.IdSubject)
                .IsRequired();

            builder.Property(x => x.IsApprovalForm)
                .IsRequired();

            builder.Property(x => x.IsMultipleForm)
                .IsRequired();

            builder.Property(x => x.JsonFormElement)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.JsonSchema)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();
            
            builder.Property(x => x.IdApprovalType)
                .HasMaxLength(36)
                .IsRequired();
            
            builder.HasOne(x => x.DocCategory)
                .WithMany(x => x.Forms)
                .HasForeignKey(fk => fk.IdDocCategory)
                .HasConstraintName("FK_MsForm_MsDocCategory")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.Forms)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsForm_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
