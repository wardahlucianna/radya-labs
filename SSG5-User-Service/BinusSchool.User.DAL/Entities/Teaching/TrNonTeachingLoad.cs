using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.Teaching
{
    public class TrNonTeachingLoad : AuditNoUniqueEntity, IUserEntity
    {
        public string Id { get; set; }
        public string IdMsNonTeachingLoad { get; set; }
        public string IdUser { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }

        public virtual MsNonTeachingLoad MsNonTeachingLoad { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrNonTeachingLoadConfiguration : AuditNoUniqueEntityConfiguration<TrNonTeachingLoad>
    {
        public override void Configure(EntityTypeBuilder<TrNonTeachingLoad> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(TrNonTeachingLoad).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.User)
               .WithMany(x => x.NonTeachingLoads)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_TrNonTeachingLoad_MsUser")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.Property(x => x.Load)
                .IsRequired();

            builder.HasOne(x => x.MsNonTeachingLoad)
                .WithMany(x => x.TrNonTeachingLoads)
                .HasForeignKey(fk => fk.IdMsNonTeachingLoad)
                .HasConstraintName("FK_TrNonTeachingLoad_MsNonTeachingLoad")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
