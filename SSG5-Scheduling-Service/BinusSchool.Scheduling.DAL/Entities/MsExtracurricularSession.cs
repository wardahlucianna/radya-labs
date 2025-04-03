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
    public class MsExtracurricularSession : AuditEntity, ISchedulingEntity
    {
        public string IdDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdVenue { get; set; }

        public virtual LtDay Day { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual ICollection<TrExtracurricularGeneratedAtt> ExtracurricularGeneratedAtts { get; set; }
        public virtual ICollection<TrExtracurricularSessionMapping> ExtracurricularSessionMappings { get; set; }

    }

    internal class MsExtracurricularSessionConfiguration : AuditEntityConfiguration<MsExtracurricularSession>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularSession> builder)
        {

            builder.Property(x => x.IdDay)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdVenue)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.Day)
                 .WithMany(y => y.ExtracurricularSessions)
                 .HasForeignKey(fk => fk.IdDay)
                 .HasConstraintName("FK_MsExtracurricularSession_LtDay")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Venue)
                .WithMany(y => y.ExtracurricularSessions)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsExtracurricularSession_MsVenue")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
