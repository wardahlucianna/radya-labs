using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtSurveyQuestionType : AuditEntity, IDocumentEntity
    {
        public string AnswerType { get; set; }

        public virtual ICollection<TrSurveyQuestionMapping> SurveyQuestionMappings { get; set; }
    }

    internal class LtSurveyQuestionTypeConfiguration : AuditEntityConfiguration<LtSurveyQuestionType>
    {
        public override void Configure(EntityTypeBuilder<LtSurveyQuestionType> builder)
        {

            builder.Property(p => p.AnswerType)
                .HasMaxLength(1000)
                .IsRequired();                    
         
            base.Configure(builder);
        }
    }
}
