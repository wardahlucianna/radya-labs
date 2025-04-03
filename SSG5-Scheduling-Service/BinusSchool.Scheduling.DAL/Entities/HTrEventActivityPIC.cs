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
    public class HTrEventActivityPIC : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEventActivity { get; set; }
        public string IdUser { get; set; }
        public virtual HTrEventActivity EventActivity { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class HTrEventActivityPICConfiguration : AuditNoUniqueEntityConfiguration<HTrEventActivityPIC>
    {
        public override void Configure(EntityTypeBuilder<HTrEventActivityPIC> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventActivityPIC).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventActivity)
              .WithMany(x => x.EventActivityPICs)
              .HasForeignKey(fk => fk.IdEventActivity)
              .HasConstraintName("FK_HTrEventActivityPIC_HTrEventActivity")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.HistoryEventActivityPICs)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_HTrEventActivityPIC_MsUser")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
