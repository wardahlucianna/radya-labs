using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsGroupMailingList : AuditEntity, IUserEntity
    {
        public string GroupName { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string Description { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsGroupMailingListMember> GroupMailingListMembers { get; set; }
        public virtual ICollection<TrMessageGroupMember> MessageGroupMembers { get; set; }
    }

    internal class MsGroupMailingListConfiguration : AuditEntityConfiguration<MsGroupMailingList>
    {
        public override void Configure(EntityTypeBuilder<MsGroupMailingList> builder)
        {
            builder.Property(x => x.GroupName)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1054)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.GroupMailingLists)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsGroupMailingList_MsUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.GroupMailingLists)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsGroupMailingList_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
