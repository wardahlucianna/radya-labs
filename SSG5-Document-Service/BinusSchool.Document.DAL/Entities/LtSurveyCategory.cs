using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtSurveyCategory : AuditEntity, IDocumentEntity
    {
        public string SurveyName { get; set; }

        public virtual ICollection<MsSurveyPeriod > SurveyPeriods  { get; set; }
        public virtual ICollection<MsSurveySection> SurveySections { get; set; }
        public virtual ICollection<MsBLPEmail> BLPEmails { get; set; }
    }

    internal class LtSurveyCategoryConfiguration : AuditEntityConfiguration<LtSurveyCategory>
    {
        public override void Configure(EntityTypeBuilder<LtSurveyCategory> builder)
        {
            builder.Property(x => x.SurveyName)
                .HasMaxLength(1000)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
