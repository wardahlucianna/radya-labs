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
    public class HTrEventCoordinator : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public virtual HTrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class HTrEventCoordinatorConfiguration : AuditNoUniqueEntityConfiguration<HTrEventCoordinator>
    {
        public override void Configure(EntityTypeBuilder<HTrEventCoordinator> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventCoordinator).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventCoordinators)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_HTrEventCoordinator_HTrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.HistoryEventCoordinators)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_HTrEventCoordinator_MsUser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
