using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSurveySection : AuditEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string IdSurveyCategory { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }
        public bool Status { get; set; }
        
        public virtual MsSchool School { get; set; }
        public virtual LtSurveyCategory SurveyCategory { get; set; }
        public virtual ICollection<TrSurveyQuestionMapping> SurveyQuestionMappings { get; set; }
    }

    internal class MsSurveySectionConfiguration : AuditEntityConfiguration<MsSurveySection>
    {
        public override void Configure(EntityTypeBuilder<MsSurveySection> builder)
        {

            builder.Property(p => p.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdSurveyCategory)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.SectionName)
                .HasMaxLength(100);

            builder.Property(p => p.Description)
               .HasMaxLength(1000);

            builder.HasOne(x => x.School)
                .WithMany(x => x.SurveySections)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSurveySection_MsSchool")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SurveyCategory)
                .WithMany(x => x.SurveySections)
                .HasForeignKey(fk => fk.IdSurveyCategory)
                .HasConstraintName("FK_MsSurveySection_LtSurveyCategory")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
