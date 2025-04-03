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
    public class HTrEventApprover : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public virtual HTrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class HTrEventApproverConfiguration : AuditNoUniqueEntityConfiguration<HTrEventApprover>
    {
        public override void Configure(EntityTypeBuilder<HTrEventApprover> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventApprover).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventApprovers)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_HTrEventApprover_HTrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.HistoryEventApprovers)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_HTrEventApprover_Msuser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
