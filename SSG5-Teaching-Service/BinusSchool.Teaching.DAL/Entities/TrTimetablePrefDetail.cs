using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrTimetablePrefDetail : AuditEntity, ITeachingEntity
    {
        public string IdTimetablePrefHeader { get; set; }
        public string IdDivision { get; set; }
        public string IdVenue { get; set; }
        public string IdPeriod { get; set; }
        public string Week { get; set; }
        public int Count { get; set; }
        public int Length { get; set; }

        public virtual TrTimeTablePrefHeader TimetablePrefHeader { get; set; }
        public virtual ICollection<TrTeachingLoad> TeachingLoads { get; set; }
        public virtual MsDivision Division { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsPeriod Period { get; set; }
    }

    internal class TrTimetablePrefDetailConfiguration : AuditEntityConfiguration<TrTimetablePrefDetail>
    {
        public override void Configure(EntityTypeBuilder<TrTimetablePrefDetail> builder)
        {
            builder.HasOne(x => x.Division)
               .WithMany(x => x.TimetablePrefDetails)
               .HasForeignKey(fk => fk.IdDivision)
               .HasConstraintName("FK_TrTimetablePrefDetail_MsDivision")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Venue)
                .WithMany(x => x.TimetablePrefDetails)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_TrTimetablePrefDetail_MsVenue")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Period)
                .WithMany(x => x.TimetablePrefDetails)
                .HasForeignKey(fk => fk.IdPeriod)
                .HasConstraintName("FK_TrTimetablePrefDetail_IdPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.Week)
                .HasMaxLength(36);

            builder.Property(x => x.Count)
                .IsRequired();

            builder.Property(x => x.Length)
                .IsRequired();

            builder.HasOne(x => x.TimetablePrefHeader)
                .WithMany(x => x.TimetablePrefDetails)
                .HasForeignKey(fk => fk.IdTimetablePrefHeader)
                .HasConstraintName("FK_TrTimetablePrefDetail_TrTimetablePrefHeader")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
