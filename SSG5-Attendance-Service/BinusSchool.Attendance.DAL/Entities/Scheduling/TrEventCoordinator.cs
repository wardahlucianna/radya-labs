using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventCoordinator : AuditEntity, IAttendanceEntity
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrEventCoordinatorConfiguration : AuditEntityConfiguration<TrEventCoordinator>
    {
        public override void Configure(EntityTypeBuilder<TrEventCoordinator> builder)
        {
            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventCoordinators)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_TrEventCoordinator_TrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.EventCoordinators)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_TrEventCoordinator_MsUser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
