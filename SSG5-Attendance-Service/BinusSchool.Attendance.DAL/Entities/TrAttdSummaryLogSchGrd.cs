using System;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttdSummaryLogSchGrd : AuditEntity, IAttendanceEntity
    {
        public TrAttdSummaryLogSchGrd()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string IdAttdSummaryLogSch { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public int TotalStudent { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual TrAttdSummaryLogSch AttdSummaryLogSch { get; set; }
        public virtual ICollection<TrAttdSummaryLogSchGrdStu> AttdSummaryLogSchGrdStu { get; set; }
    }

    internal class TrAttdSummaryLogSchGrdConfiguration : AuditEntityConfiguration<TrAttdSummaryLogSchGrd>
    {
        public override void Configure(EntityTypeBuilder<TrAttdSummaryLogSchGrd> builder)
        {
            builder.HasOne(x => x.Grade)
               .WithMany(x => x.AttdSummaryLogSchGrd)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_TrAttdSummaryLogSchGrd_MsGrade")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.AttdSummaryLogSch)
               .WithMany(x => x.AttdSummaryLogSchGrd)
               .HasForeignKey(fk => fk.IdAttdSummaryLogSch)
               .HasConstraintName("FK_TrAttdSummaryLogSchGrd_TrAttdSummaryLogSch")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.Property(x => x.GradeName)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
