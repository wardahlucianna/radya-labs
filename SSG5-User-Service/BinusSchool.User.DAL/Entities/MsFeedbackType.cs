using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsFeedbackType : CodeEntity , IUserEntity
    {
        public string IdSchool { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual MsUser UserTo { get; set; }
        public virtual MsUser UserCc { get; set; }
        public virtual ICollection<TrMessage> Messages { get; set; }
    }

    internal class MsFeedbackTypeConfiguration : CodeEntityConfiguration<MsFeedbackType>
    {
        public override void Configure(EntityTypeBuilder<MsFeedbackType> builder)
        {
            builder.HasOne(x => x.UserTo)
            .WithMany(x => x.FeedbackTypeTo)
            .HasForeignKey(fk => fk.To)
            .HasConstraintName("FK_MsFeedbackTypeTo_MsUser")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
            builder.HasOne(x => x.UserCc)
            .WithMany(x => x.FeedbackTypeCc)
            .HasForeignKey(fk => fk.Cc)
            .HasConstraintName("FK_MsFeedbackTypeCc_MsUser")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
            builder.HasOne(x => x.School)
            .WithMany(x => x.FeedbackTypes)
            .HasForeignKey(fk => fk.IdSchool)
            .HasConstraintName("FK_MsFeedbackType_MsSchool")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
            base.Configure(builder);
        }
    }
}
