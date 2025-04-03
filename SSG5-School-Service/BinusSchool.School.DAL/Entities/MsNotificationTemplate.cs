using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsNotificationTemplate : CodeEntity, ISchoolEntity
    {
        public string IdFeatureSchool { get; set; }
        public string Scenario { get; set; }
        public string Title { get; set; }
        public string PushContent { get; set; }
        public string EmailContent { get; set; }
        public bool EmailContentIsHtml { get; set; }

        public virtual MsFeatureSchool FeatureSchool { get; set; }
    }

    internal class MsNotificationTemplateConfiguration : CodeEntityConfiguration<MsNotificationTemplate>
    {
        public override void Configure(EntityTypeBuilder<MsNotificationTemplate> builder)
        {
            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();
            
            builder.Property(x => x.Scenario)
                .HasMaxLength(15)
                .IsRequired();
            
            builder.Property(x => x.PushContent)
                .HasColumnType("text");
            
            builder.Property(x => x.EmailContent)
                .HasColumnType("text");
            
            builder.HasOne(x => x.FeatureSchool)
                .WithMany(x => x.NotificationTemplates)
                .HasForeignKey(fk => fk.IdFeatureSchool)
                .HasConstraintName("FK_MsNotificationTemplate_MsFeatureSchool")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            base.Configure(builder);

            builder.Property(x => x.Description)
                .HasMaxLength(450);
        }
    }
}
