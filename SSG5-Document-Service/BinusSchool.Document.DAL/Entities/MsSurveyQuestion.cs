using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSurveyQuestion : AuditEntity, IDocumentEntity
    {
        public string IdAcademicYear { get; set; }
        public string Description { get; set; }
        public bool IsParentQuestion { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrSurveyQuestionMapping> SurveyQuestionMappings { get; set; } 
    }

    internal class MsSurveyQuestionConfiguration : AuditEntityConfiguration<MsSurveyQuestion>
    {
        public override void Configure(EntityTypeBuilder<MsSurveyQuestion> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.SurveyQuestions)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsSurveyQuestion_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
