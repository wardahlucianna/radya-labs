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
    public class HTrEventAwardApprover : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public virtual HTrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class HTrEventAwardApproverConfiguration : AuditNoUniqueEntityConfiguration<HTrEventAwardApprover>
    {
        public override void Configure(EntityTypeBuilder<HTrEventAwardApprover> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventAwardApprover).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventAwardApprovers)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_HTrEventAwardApprover_HTrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.HistoryEventAwardApprovers)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_HTrEventAwardApprover_MsUser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
