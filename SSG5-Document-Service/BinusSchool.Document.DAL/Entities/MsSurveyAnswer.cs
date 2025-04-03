using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSurveyAnswer : AuditEntity, IDocumentEntity
    {
        public string Description { get; set; }       

        public virtual ICollection<TrSurveyAnswerMapping> SurveyAnswerMappings { get; set; }
    }

    internal class MsSurveyAnswerConfiguration : AuditEntityConfiguration<MsSurveyAnswer>
    {
        public override void Configure(EntityTypeBuilder<MsSurveyAnswer> builder)
        {
            base.Configure(builder);
        }
    }
}
