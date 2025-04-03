using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class MsGrade : CodeEntity, IDocumentEntity
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsLevel Level { get; set; }
        public virtual ICollection<MsGradePathway> GradePathways { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsSurveyPeriod> SurveyPeriods { get; set; }
        public virtual ICollection<MsPeriod> Periods { get; set; }
    }
    internal class MsGradeConfiguration : CodeEntityConfiguration<MsGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGrade> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.Grades)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsGrade_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
