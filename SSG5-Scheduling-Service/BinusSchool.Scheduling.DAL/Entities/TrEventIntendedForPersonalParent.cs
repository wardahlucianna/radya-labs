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
    public class TrEventIntendedForPersonalParent : AuditEntity, ISchedulingEntity
    {
        public string IdParent { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsParent Parent { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class TrEventIntendedForPersonalParentConfiguration : AuditEntityConfiguration<TrEventIntendedForPersonalParent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForPersonalParent> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
            .WithMany(x => x.EventIntendedForPersonalParents)
            .HasForeignKey(fk => fk.IdEventIntendedFor)
            .HasConstraintName("FK_TrEventIntendedForPersonalParent_TrEventIntendedFor")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Parent)
            .WithMany(x => x.EventIntendedForPersonalParents)
            .HasForeignKey(fk => fk.IdParent)
            .HasConstraintName("FK_TrEventIntendedForPersonalParent_MsParent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
