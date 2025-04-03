using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsMessageApproval : CodeEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public UserMessageType Type { get; set; }

        public virtual MsSchool School { get; set; }
        public ICollection<MsMessageCategory> MessageCategories { get; set; }
        public ICollection<MsMessageApprovalState> ApprovalStates { get; set; }
    }
    internal class MsMessageApprovalConfiguration : CodeEntityConfiguration<MsMessageApproval>
    {
        public override void Configure(EntityTypeBuilder<MsMessageApproval> builder)
        {
            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.MessageApprovals)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsMessageApproval_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            base.Configure(builder);
        }
    }
}
