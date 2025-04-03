using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsSurvey : AuditEntity, IStudentEntity
    {
        public string SurveyTitle { get; set; }
        public string SurveyMessage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }
        public bool IsBlocking { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }

        public virtual ICollection<TrSurveyResponse> SurveyResponses { get; set; }

    }
    internal class MsSurveyConfiguration : AuditEntityConfiguration<MsSurvey>
    {
        public override void Configure(EntityTypeBuilder<MsSurvey> builder)
        {
            builder.Property(x => x.SurveyTitle)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.SurveyMessage)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.IdSchool)
             .HasMaxLength(36)
             .IsRequired();
          
            base.Configure(builder);
        }
    }
}
