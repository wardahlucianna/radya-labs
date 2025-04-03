using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsGroupMailingListMember : AuditEntity, IUserEntity
    {
        public string IdGroupMailingList { get; set; }
        public string IdUser { get; set; }
        public bool IsCreateMessage { get; set; }
        public virtual MsGroupMailingList GroupMailingList { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsGroupMailingListMemberConfiguration : AuditEntityConfiguration<MsGroupMailingListMember>
    {
        public override void Configure(EntityTypeBuilder<MsGroupMailingListMember> builder)
        {
            builder.Property(x => x.IsCreateMessage)
                .HasDefaultValue(false)
             .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.GroupMailingListMembers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsGroupMailingListMember_MsUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.GroupMailingList)
                .WithMany(x => x.GroupMailingListMembers)
                .HasForeignKey(fk => fk.IdGroupMailingList)
                .HasConstraintName("FK_MsGroupMailingListMember_MsGroupMailingList")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
