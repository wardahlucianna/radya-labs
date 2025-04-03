using System;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class HTrSurveyStudentAnswer : AuditNoUniqueEntity, IDocumentEntity
    {
        public string IdHTrSurveyStudentAnswer { get; set; }
        //public string IdSurveyStudentAnswer { get; set; }
        public string IdRespondent { get; set; }      
        public string IdSurveyQuestionMapping { get; set; }
        public string IdSurveyAnswerMapping { get; set; }
        public string? IdClearanceWeekPeriod { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public DateTime? ActionDate { get; set; }

        //public virtual TrSurveyStudentAnswer SurveyStudentAnswer { get; set; }
        public virtual MsRespondent Respondent { get; set; }
        public virtual TrSurveyQuestionMapping SurveyQuestionMapping { get; set; }
        public virtual TrSurveyAnswerMapping SurveyAnswerMapping { get; set; }
        public virtual MsClearanceWeekPeriod ClearanceWeekPeriod { get; set; }
    }
    internal class HTrSurveyStudentAnswerConfiguration : AuditNoUniqueEntityConfiguration<HTrSurveyStudentAnswer>
    {
        public override void Configure(EntityTypeBuilder<HTrSurveyStudentAnswer> builder)
        {
            builder.HasKey(x => x.IdHTrSurveyStudentAnswer);

            builder.Property(p => p.IdHTrSurveyStudentAnswer)
                .HasMaxLength(36)
                .IsRequired();

            //builder.Property(p => p.IdSurveyStudentAnswer)
            //    .HasMaxLength(36)
            //    .IsRequired();

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

            //builder.HasOne(x => x.SurveyStudentAnswer)
            //    .WithMany(x => x.HSurveyStudentAnswers)
            //    .HasForeignKey(fk => fk.IdSurveyStudentAnswer)
            //    .HasConstraintName("FK_HTrSurveyStudentAnswer_TrSurveyStudentAnswer")
            //    .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Respondent)
                .WithMany(x => x.HSurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdRespondent)
                .HasConstraintName("FK_HTrSurveyStudentAnswer_MsRespondent")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SurveyQuestionMapping)
                .WithMany(x => x.HSurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdSurveyQuestionMapping)
                .HasConstraintName("FK_HTrSurveyStudentAnswer_TrSurveyQuestionMapping")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SurveyAnswerMapping)
                .WithMany(x => x.HSurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdSurveyAnswerMapping)
                .HasConstraintName("FK_HTrSurveyStudentAnswer_TrSurveyAnswerMapping")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ClearanceWeekPeriod)
                .WithMany(x => x.HSurveyStudentAnswers)
                .HasForeignKey(fk => fk.IdClearanceWeekPeriod)
                .HasConstraintName("FK_HTrSurveyStudentAnswer_MsClearanceWeekPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
