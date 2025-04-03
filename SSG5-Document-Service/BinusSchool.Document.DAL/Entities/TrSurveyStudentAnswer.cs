using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrSurveyStudentAnswer : AuditEntity, IDocumentEntity
    {
        public string IdRespondent { get; set; }      
        public string IdSurveyQuestionMapping { get; set; }
        public string IdSurveyAnswerMapping { get; set; }
        public string? IdClearanceWeekPeriod { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }

        public virtual MsRespondent Respondent { get; set; }
        public virtual TrSurveyQuestionMapping SurveyQuestionMapping { get; set; }
        public virtual TrSurveyAnswerMapping SurveyAnswerMapping { get; set; }
        public virtual MsClearanceWeekPeriod ClearanceWeekPeriod { get; set; }
        //public virtual ICollection<HTrSurveyStudentAnswer> HSurveyStudentAnswers { get; set; }
    }
    internal class TrSurveyStudentAnswerConfiguration : AuditEntityConfiguration<TrSurveyStudentAnswer>
    {
        public override void Configure(EntityTypeBuilder<TrSurveyStudentAnswer> builder)
        {
            builder.Property(p => p.IdRespondent)
               .HasMaxLength(36)
               .IsRequired();    

            builder.Property(p => p.IdSurveyQuestionMapping)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdSurveyAnswerMapping)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdClearanceWeekPeriod)
                .HasMaxLength(36);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.FilePath)
                .HasMaxLength(1000);

            builder.HasOne(x => x.Respondent)
                .WithMany(x => x.SurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdRespondent)
                .HasConstraintName("FK_TrSurveyStudentAnswer_MsRespondent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SurveyQuestionMapping)
                .WithMany(x => x.SurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdSurveyQuestionMapping)
                .HasConstraintName("FK_TrSurveyStudentAnswer_TrSurveyQuestionMapping")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SurveyAnswerMapping)
                .WithMany(x => x.SurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdSurveyAnswerMapping)
                .HasConstraintName("FK_TrSurveyStudentAnswer_TrSurveyAnswerMapping")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClearanceWeekPeriod)
                .WithMany(x => x.SurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdClearanceWeekPeriod)
                .HasConstraintName("FK_TrSurveyStudentAnswer_MsClearanceWeekPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
