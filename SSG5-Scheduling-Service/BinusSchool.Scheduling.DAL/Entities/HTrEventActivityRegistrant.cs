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
    public class HTrEventActivityRegistrant : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEventActivity { get; set; }
        public string IdUser { get; set; }
        public virtual HTrEventActivity EventActivity { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class HTrEventActivityRegistrantConfiguration : AuditNoUniqueEntityConfiguration<HTrEventActivityRegistrant>
    {
        public override void Configure(EntityTypeBuilder<HTrEventActivityRegistrant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventActivityRegistrant).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventActivity)
              .WithMany(x => x.EventActivityRegistrants)
              .HasForeignKey(fk => fk.IdEventActivity)
              .HasConstraintName("FK_HTrEventActivityRegistrant_HTrEventActivity")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.HistoryEventActivityRegistrants)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_HTrEventActivityRegistrant_MsUser")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
