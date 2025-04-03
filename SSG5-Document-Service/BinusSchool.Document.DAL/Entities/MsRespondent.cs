using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using System.Collections.Generic;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsRespondent : AuditEntity, IDocumentEntity
    {
        public string IdSurveyPeriod { get; set; }
        public string? IdClearanceWeekPeriod { get; set; }
        public string IdStudent { get; set; }
        public string IdParent { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool ResultSummary { get; set; }

        public virtual MsParent Parent { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsSurveyPeriod SurveyPeriod { get; set; }
        public virtual MsClearanceWeekPeriod ClearanceWeekPeriod { get; set; }
        public virtual ICollection<TrSurveyStudentAnswer> SurveyStudentAnswers { get; set; }
        public virtual ICollection<HTrSurveyStudentAnswer> HSurveyStudentAnswers { get; set; }
    }

    internal class MsRespondentConfiguration : AuditEntityConfiguration<MsRespondent>
    {
        public override void Configure(EntityTypeBuilder<MsRespondent> builder)
        {
            builder.Property(x => x.IdSurveyPeriod)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdClearanceWeekPeriod)
                .HasMaxLength(36);

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdParent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.EmailAddress)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .IsRequired();

            builder.Property(x => x.ResultSummary)
                .IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Respondents)
                .HasForeignKey(fk => fk.IdParent)
                .HasConstraintName("FK_MsRespondent_MsParent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.Respondents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsRespondent_ MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.SurveyPeriod)
                .WithMany(x => x.Respondents)
                .HasForeignKey(fk => fk.IdSurveyPeriod)
                .HasConstraintName("FK_MsRespondent_MsSurveyPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ClearanceWeekPeriod)
                .WithMany(x => x.Respondents)
                .HasForeignKey(fk => fk.IdClearanceWeekPeriod)
                .HasConstraintName("FK_MsRespondent_MsClearanceWeekPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
