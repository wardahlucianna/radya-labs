using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrSurveyQuestionMapping : AuditEntity, IDocumentEntity
    {
        public string IdSurveyQuestion { get; set; }
        public string IdSurveySection { get; set; }
        public string IdSurveyQuestionType { get; set; }
        //public string Description { get; set; }
        public int OrderNumber { get; set; }
        public bool IsRequired { get; set; }

        public virtual MsSurveyQuestion SurveyQuestion { get; set; }
        public virtual MsSurveySection SurveySection { get; set; }
        public virtual LtSurveyQuestionType SurveyQuestionType { get; set; }
        public virtual ICollection<TrSurveyAnswerMapping> SurveyAnswerMappings { get; set; }
        public virtual ICollection<TrSurveyStudentAnswer> SurveyStudentAnswers { get; set; }
        public virtual ICollection<HTrSurveyStudentAnswer> HSurveyStudentAnswers { get; set; }

    }
    internal class TrSurveyQuestionMappingConfiguration : AuditEntityConfiguration<TrSurveyQuestionMapping>
    {
        public override void Configure(EntityTypeBuilder<TrSurveyQuestionMapping> builder)
        {
            builder.Property(p => p.IdSurveyQuestion)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdSurveySection)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdSurveyQuestionType)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.SurveyQuestion)
                .WithMany(x => x.SurveyQuestionMappings)
                .HasForeignKey(fk => fk.IdSurveyQuestion)
                .HasConstraintName("FK_TrSurveyQuestionMapping_MsSurveyQuestion")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SurveySection)
                .WithMany(x => x.SurveyQuestionMappings)
                .HasForeignKey(fk => fk.IdSurveySection)
                .HasConstraintName("FK_TrSurveyQuestionMapping_MsSurveySection")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SurveyQuestionType)
                .WithMany(x => x.SurveyQuestionMappings)
                .HasForeignKey(fk => fk.IdSurveyQuestionType)
                .HasConstraintName("FK_TrSurveyQuestionMapping_LtSurveyQuestionType")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
