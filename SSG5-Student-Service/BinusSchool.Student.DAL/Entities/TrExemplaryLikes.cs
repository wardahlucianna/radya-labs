using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExemplaryLikes : AuditEntity, IStudentEntity
    {
        public string IdExemplary { get; set; }
        public virtual TrExemplary Exemplary { get; set; }
    }

    internal class TrExemplaryLikesConfiguration : AuditEntityConfiguration<TrExemplaryLikes>
    {
        public override void Configure(EntityTypeBuilder<TrExemplaryLikes> builder)
        {
            builder.Property(x => x.IdExemplary)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Exemplary)
                .WithMany(x => x.ExemplaryLikes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExemplary)
                .HasConstraintName("FK_TrExemplaryLikes_TrExemplary")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
