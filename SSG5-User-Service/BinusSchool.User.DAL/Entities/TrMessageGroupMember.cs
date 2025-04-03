using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageGroupMember : AuditEntity, IUserEntity
    {
        public string IdMessage { get; set; }
        public string IdGroupMailingList { get; set; }
        public virtual TrMessage Message { get; set; }
        public virtual MsGroupMailingList GroupMailingList { get; set; }
    }

    internal class TrMessageGroupMemberConfiguration : AuditEntityConfiguration<TrMessageGroupMember>
    {
        public override void Configure(EntityTypeBuilder<TrMessageGroupMember> builder)
        {
            builder.HasOne(x => x.Message)
                .WithMany(x => x.MessageGroupMembers)
                .HasForeignKey(fk => fk.IdMessage)
                .HasConstraintName("FK_TrMessageGroupMember_TrMessage")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.GroupMailingList)
                .WithMany(x => x.MessageGroupMembers)
                .HasForeignKey(fk => fk.IdGroupMailingList)
                .HasConstraintName("FK_TrMessageGroupMember_MsGroupMailingList")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

          
            base.Configure(builder);
        }
    }
}
