using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrLogQueueEvent : AuditEntity, ISchedulingEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public bool IsProcess { get; set; }
        public string ErrorMessage { get; set; }
        public string IdSchool { get; set; }
        public MsSchool School { get; set; }
    }

    internal class TrLogQueueEventConfiguration : AuditEntityConfiguration<TrLogQueueEvent>
    {
        public override void Configure(EntityTypeBuilder<TrLogQueueEvent> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.LogQueueEvents)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_TrLogQueueEvent_MsSchools")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
