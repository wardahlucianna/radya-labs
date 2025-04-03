using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageRecepient : AuditEntity, IUserEntity
    {
        public string IdRecepient { get; set; }
        public string IdMessage { get; set; }
        public MessageFolder MessageFolder { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public virtual TrMessage Message { get; set; }
        public virtual MsUser User { get; set; }
       
    }

    internal class TrMessageRecepientConfiguration : AuditEntityConfiguration<TrMessageRecepient>
    {
        public override void Configure(EntityTypeBuilder<TrMessageRecepient> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.MessageRecepients)
                .HasForeignKey(fk => fk.IdRecepient)
                .HasConstraintName("FK_TrMessageRecepient_MsUser")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Message)
                .WithMany(x => x.MessageRecepients)
                .HasForeignKey(fk => fk.IdMessage)
                .HasConstraintName("FK_TrMessageRecepient_TrMessage")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(x => x.MessageFolder)
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();
          
            base.Configure(builder);
        }
    }
}
