using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrUserEvent : AuditEntity, IAttendanceEntity
    {
        public string IdEventDetail { get; set; }
        public string IdUser { get; set; }
        /// <summary>
        /// Value will be true if event need approval from parent 
        /// </summary>
        public bool IsNeedApproval { get; set; }
        /// <summary>
        /// Value true if parent approved this event for student
        /// </summary>
        public bool IsApproved { get; set; }
        public string Reason { get; set; }

        public virtual TrEventDetail EventDetail { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<TrUserEventAttendance2> UserEventAttendance2s { get; set; }
        public virtual ICollection<TrUserEventAttendance> UserEventAttendances { get; set; }

    }

    internal class TrUserEventConfiguration : AuditEntityConfiguration<TrUserEvent>
    {
        public override void Configure(EntityTypeBuilder<TrUserEvent> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.TrUserEvents)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_TrUserEvent_MsUser")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.EventDetail)
                .WithMany(x => x.UserEvents)
                .HasForeignKey(fk => fk.IdEventDetail)
                .HasConstraintName("FK_TrUserEvent_TrEventDetail")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(x => x.Reason).HasMaxLength(450);

            base.Configure(builder);
        }
    }
}
