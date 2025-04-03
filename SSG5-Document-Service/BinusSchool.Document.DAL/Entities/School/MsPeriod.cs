using System;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsPeriod : CodeEntity, IDocumentEntity
    {
        public string IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Semester { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrDocumentReqApplicantDetail> DocumentReqApplicantDetails { get; set; }
    }

    internal class MsPeriodConfiguration : CodeEntityConfiguration<MsPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsPeriod> builder)
        {
            builder.Property(x => x.StartDate)
                .IsRequired();
                
            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Periods)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsPeriod_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
