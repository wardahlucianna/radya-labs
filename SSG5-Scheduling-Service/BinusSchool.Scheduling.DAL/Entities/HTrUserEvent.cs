using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrUserEvent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrUserEvent { get; set; }
        public string IdEventDetail { get; set; }
        public string IdUser { get; set; }

        public virtual HTrEventDetail EventDetail { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class HTrUserEventConfiguration : AuditNoUniqueEntityConfiguration<HTrUserEvent>
    {
        public override void Configure(EntityTypeBuilder<HTrUserEvent> builder)
        {
            builder.HasKey(x => x.IdHTrUserEvent);

            builder.Property(p => p.IdHTrUserEvent)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.HistoryUserEvents)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_HTrUserEvent_MsUser")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.EventDetail)
                .WithMany(x => x.UserEvents)
                .HasForeignKey(fk => fk.IdEventDetail)
                .HasConstraintName("FK_HTrUserEvent_HTrEventDetail")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
