using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEvidences : AuditEntity, IStudentEntity
    {
        public string IdExperience { get; set; }
        public EvidencesType EvidencesType { get; set; }
        public string EvidencesValue { get; set; }
        public string Url { get; set; }
        public virtual TrExperience Experience { get; set; }
        public virtual ICollection<TrEvidencesComment> TrEvidencesComments { get; set; }
        public virtual ICollection<TrEvidencesAttachment> TrEvidencesAttachments { get; set; }
        public virtual ICollection<TrEvidenceLearning> TrEvidenceLearnings { get; set; }

    }

    internal class TrEvidencesConfiguration : AuditEntityConfiguration<TrEvidences>
    {
        public override void Configure(EntityTypeBuilder<TrEvidences> builder)
        {
            builder.Property(x => x.IdExperience)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.EvidencesType)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.EvidencesType)
                .HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (EvidencesType)Enum.Parse(typeof(EvidencesType), valueFromDb))
                .IsRequired();

            builder.Property(x => x.Url)
                .HasMaxLength(256);

            builder.HasOne(x => x.Experience)
                .WithMany(x => x.TrEvidences)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExperience)
                .HasConstraintName("FK_TrEvidences_TrExperience")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
