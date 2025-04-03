using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrSurveyAnswerMapping : AuditEntity, IDocumentEntity
    {
        public string IdSurveyQuestionMapping { get; set; }
        public string IdSurveyAnswer { get; set; }
        public int GroupNumber { get; set; }
        public int? NextGroupNumber { get; set; }
        public bool? IsNotAllowed { get; set; }       
        public int OrderNumber { get; set; }

        public virtual TrSurveyQuestionMapping SurveyQuestionMapping { get; set; }
        public virtual MsSurveyAnswer SurveyAnswer { get; set; }
        public virtual ICollection<TrSurveyStudentAnswer> SurveyStudentAnswers { get; set; }
        public virtual ICollection<HTrSurveyStudentAnswer> HSurveyStudentAnswers { get; set; }

    }

    internal class TrSurveyAnswerMappingConfiguration : AuditEntityConfiguration<TrSurveyAnswerMapping>
    {
        public override void Configure(EntityTypeBuilder<TrSurveyAnswerMapping> builder)
        {
            builder.Property(p => p.IdSurveyQuestionMapping)
                .HasMaxLength(36);      

            builder.Property(p => p.IdSurveyAnswer)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.SurveyQuestionMapping)
                .WithMany(x => x.SurveyAnswerMappings)
                .HasForeignKey(fk => fk.IdSurveyQuestionMapping)
                .HasConstraintName("FK_TrSurveyAnswerMapping_TrSurveyQuestionMapping")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SurveyAnswer)
                .WithMany(x => x.SurveyAnswerMappings)
                .HasForeignKey(fk => fk.IdSurveyAnswer)
                .HasConstraintName("FK_TrSurveyAnswerMapping_MsSurveyAnswer")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
