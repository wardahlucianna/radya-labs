using System;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttdSummaryLogSchGrdStu : AuditEntity, IAttendanceEntity
    {
        public TrAttdSummaryLogSchGrdStu()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string IdAttdSummaryLogSchGrd { get; set; }
        public string IdStudent { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual TrAttdSummaryLogSchGrd AttdSummaryLogSchGrd { get; set; }
    }

    internal class TrAttdSummaryLogSchGrdStuConfiguration : AuditEntityConfiguration<TrAttdSummaryLogSchGrdStu>
    {
        public override void Configure(EntityTypeBuilder<TrAttdSummaryLogSchGrdStu> builder)
        {
            builder.HasOne(x => x.Student)
               .WithMany(x => x.AttdSummaryLogSchGrdStu)
               .HasForeignKey(fk => fk.IdStudent)
               .HasConstraintName("FK_TrAttdSummaryLogSchGrdStu_MsStudent")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.AttdSummaryLogSchGrd)
              .WithMany(x => x.AttdSummaryLogSchGrdStu)
              .HasForeignKey(fk => fk.IdAttdSummaryLogSchGrd)
              .HasConstraintName("FK_TrAttdSummaryLogSchGrdStu_TrAttdSummaryLogSchGrd")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
