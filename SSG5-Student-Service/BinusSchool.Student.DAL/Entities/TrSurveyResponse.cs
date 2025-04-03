using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrSurveyResponse : AuditEntity, IStudentEntity
    {
        public int IdResponse { get; set; }
        public string IdSurvey { get; set; }
        public string IdForm { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDtm { set; get; }
        public string EmailAddress { set; get; }
        public string Question1 { set; get; }
        public string Question2 { set; get; }
        public virtual MsSurvey Survey { get; set; }

    }
    internal class TrSurveyResponseConfiguration : AuditEntityConfiguration<TrSurveyResponse>
    {
        public override void Configure(EntityTypeBuilder<TrSurveyResponse> builder)
        {
            builder.Property(x => x.IdResponse)
                .IsRequired();

            builder.Property(x => x.IdForm)
              .HasMaxLength(200)
              .IsRequired();

            builder.HasOne(x => x.Survey)
             .WithMany(x => x.SurveyResponses)
             .HasForeignKey(fk => fk.IdSurvey)
             .HasConstraintName("FK_TrSurveyResponse_MsSurvey")
             .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.EmailAddress)
             .HasMaxLength(1000);

            builder.Property(x => x.Question1)
             .HasMaxLength(1000);

            builder.Property(x => x.Question2)
             .HasMaxLength(1000);

            base.Configure(builder);
        }
    }
}
