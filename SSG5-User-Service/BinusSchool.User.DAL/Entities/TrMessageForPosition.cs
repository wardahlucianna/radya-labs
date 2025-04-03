using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageForPosition : AuditEntity, IUserEntity
    {
        public string IdMessageFor { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual TrMessageFor MessageFor { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
    }

    internal class TrMessageForPositionConfiguration : AuditEntityConfiguration<TrMessageForPosition>
    {
        public override void Configure(EntityTypeBuilder<TrMessageForPosition> builder)
        {
            builder.HasOne(x => x.MessageFor)
               .WithMany(x => x.MessageForPositions)
               .HasForeignKey(fk => fk.IdMessageFor)
               .HasConstraintName("FK_TrMessageForPosition_TrMessageFor")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.MessageForPositions)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_TrMessageForPosition_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
