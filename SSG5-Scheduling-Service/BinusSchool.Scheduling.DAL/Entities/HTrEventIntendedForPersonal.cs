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
    public class HTrEventIntendedForPersonal : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsUser User { get; set; }
        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class HTrEventIntendedForPersonalConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForPersonal>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForPersonal> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForPersonal).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
            .WithMany(x => x.EventIntendedForPersonals)
            .HasForeignKey(fk => fk.IdEventIntendedFor)
            .HasConstraintName("FK_HTrEventIntendedForPersonal_HTrEventIntendedFor")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.User)
            .WithMany(x => x.HistoryEventIntendedForPersonals)
            .HasForeignKey(fk => fk.IdUser)
            .HasConstraintName("FK_HTrEventIntendedForPersonal_MsUser")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
