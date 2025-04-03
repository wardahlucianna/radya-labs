using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageFor : AuditEntity, IUserEntity
    {
        public string IdMessage { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public MessageForOption Option { get; set; }
        public virtual TrMessage Message { get; set; }
        public virtual ICollection<TrMessageForDepartement> MessageForDepartements { get; set; }
        public virtual ICollection<TrMessageForGrade> MessageForGrades { get; set; }
        public virtual ICollection<TrMessageForPosition> MessageForPositions { get; set; }
        public virtual ICollection<TrMessageForPersonal> MessageForPersonals { get; set; }

    }

    internal class TrMessageForConfiguration : AuditEntityConfiguration<TrMessageFor>
    {
        public override void Configure(EntityTypeBuilder<TrMessageFor> builder)
        {
            builder.Property(e => e.Role).HasMaxLength(maxLength: 7)
                .HasConversion(valueToDb =>
                        valueToDb.ToString(),
                    valueFromDb =>
                        (UserRolePersonalOptionRole)Enum.Parse(typeof(UserRolePersonalOptionRole), valueFromDb))
                .IsRequired();

            builder.Property(e => e.Option).HasMaxLength(maxLength: 30)
                .HasConversion(valueToDb =>
                        valueToDb.ToString(),
                    valueFromDb =>
                        (MessageForOption)Enum.Parse(typeof(MessageForOption), valueFromDb))
                .IsRequired(); 

            builder.HasOne(x => x.Message)
               .WithMany(x => x.MessageFors)
               .HasForeignKey(fk => fk.IdMessage)
               .HasConstraintName("FK_TrMessageFor_TrMessage")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
