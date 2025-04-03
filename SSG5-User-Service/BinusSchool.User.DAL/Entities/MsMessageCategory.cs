using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsMessageCategory : CodeEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public string IdMessageApproval { get; set; }
        public ICollection<TrMessage> Messages { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsMessageApproval MessageApproval { get; set; }

    }
    internal class MsMessageCategoryConfiguration : CodeEntityConfiguration<MsMessageCategory>
    {
        public override void Configure(EntityTypeBuilder<MsMessageCategory> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.MessageCategories)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsMessageCategory_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasOne(x => x.MessageApproval)
              .WithMany(x => x.MessageCategories)
              .HasForeignKey(fk => fk.IdMessageApproval)
              .HasConstraintName("FK_MsMessageCategory_MsMessageApproval")
              .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
