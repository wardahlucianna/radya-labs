using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrNotification : AuditEntity, IUserEntity
    {
        public string Title { get; set; }
        public string IdFeatureSchool { get; set; }
        public string ScenarioNotificationTemplate { get; set; }
        public string Content { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
        public bool IsBlast { get; set; }
        public NotificationType NotificationType { get; set; }
        public string IdSchool { get; set; }
        public virtual ICollection<TrNotificationUser> NotificationUsers { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsFeatureSchool FeatureSchool { get; set; }
    }
    internal class TrNotificationConfiguration : AuditEntityConfiguration<TrNotification>
    {
        public override void Configure(EntityTypeBuilder<TrNotification> builder)
        {
            builder.Property(x => x.Title).IsRequired().HasMaxLength(450);
            builder.Property(x => x.ScenarioNotificationTemplate).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Content);
            builder.Property(x => x.Action).IsRequired();
            builder.Property(x => x.Data).HasColumnType("text").HasComment("Meta data for this notification");

            builder.HasOne(x => x.School)
            .WithMany(x => x.Notifications)
            .HasForeignKey(fk => fk.IdSchool)
            .HasConstraintName("FK_MsSchool_TrNotification")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FeatureSchool)
            .WithMany(x => x.Notifications)
            .HasForeignKey(fk => fk.IdFeatureSchool)
            .HasConstraintName("FK_MsFeatureSchool_TrNotification")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
