using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsMessageOption : CodeEntity,IUserEntity
    {
        public string IdSchool { get; set; }
        public string Value { get; set; }
        public virtual MsSchool School { get; set; }
    }
    internal class MsMessageOptionConfiguration : CodeEntityConfiguration<MsMessageOption>
    {
        public override void Configure(EntityTypeBuilder<MsMessageOption> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.MessageOptions)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsMessageOption_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
