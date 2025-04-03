using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsUserPassword : AuditEntity, IUserEntity
    {
        public byte[] Salt { get; set; }
        public string HashedPassword { get; set; }

        public virtual MsUser User { get; set; }
    }

    internal class MsUserPasswordConfiguration : AuditEntityConfiguration<MsUserPassword>
    {
        public override void Configure(EntityTypeBuilder<MsUserPassword> builder)
        {
            builder.Property(x => x.HashedPassword).IsRequired();
            builder.Property(x => x.Salt).IsRequired();
            base.Configure(builder);
        }
    }
}
