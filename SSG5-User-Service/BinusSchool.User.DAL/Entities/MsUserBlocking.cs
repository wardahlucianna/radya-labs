using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsUserBlocking : AuditEntity, IUserEntity
    {
        public string IdUser { get; set; }
        public string IdBlockingCategory { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsBlockingCategory BlockingCategory { get; set; }
    }

    internal class MsUserBlockingConfiguration : AuditEntityConfiguration<MsUserBlocking>
    {
        public override void Configure(EntityTypeBuilder<MsUserBlocking> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.UserBlockings)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUserBlocking_MsUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BlockingCategory)
                .WithMany(x => x.UserBlockings)
                .HasForeignKey(fk => fk.IdBlockingCategory)
                .HasConstraintName("FK_MsUserBlocking_MsBlockingCategory")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
