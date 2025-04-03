using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrNotificationUser : AuditEntity, IUserEntity
    {
        public string IdNotification { get; set; }
        public string IdUser { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsDeleteBySystem { get; set; }
        
        public virtual TrNotification Notification { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrNotificationUserConfiguration : AuditEntityConfiguration<TrNotificationUser>
    {
        public override void Configure(EntityTypeBuilder<TrNotificationUser> builder)
        {
            builder.HasOne(x => x.Notification)
                .WithMany(x => x.NotificationUsers)
                .HasForeignKey(fk => fk.IdNotification)
                .HasConstraintName("FK_TrNotificationUser_TrNotification")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(x => x.User)
                .WithMany(x => x.NotificationUsers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_TrNotificationUser_MsUser")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
