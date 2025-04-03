using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableHomeroom : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroom { get; set; }
        public string IdAscTimetable { get; set; }
        public bool IsFromMaster { get; set; }

        public virtual MsHomeroom Homeroom { get; set; }
        public virtual TrAscTimetable AscTimetable { get; set; }
    }

    internal class TrAscTimetableHomeroomConfiguration : AuditEntityConfiguration<TrAscTimetableHomeroom>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableHomeroom> builder)
        {

            builder.Property(x => x.IsFromMaster)
            .HasDefaultValue(false);


            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.AscTimetableHomerooms)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TTrAscTimetableHomeroom_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableHomerooms)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableHomeroom_TrAscTimetable")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
