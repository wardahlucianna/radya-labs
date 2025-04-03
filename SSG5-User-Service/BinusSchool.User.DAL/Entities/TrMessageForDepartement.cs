using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.UserDb.Entities.School;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageForDepartement : AuditEntity, IUserEntity
    {
        public string IdMessageFor { get; set; }
        public string IdDepartment { get; set; }
        public virtual TrMessageFor MessageFor { get; set; }
        public virtual MsDepartment Department { get; set; }
    }

    internal class TrMessageForDepartementConfiguration : AuditEntityConfiguration<TrMessageForDepartement>
    {
        public override void Configure(EntityTypeBuilder<TrMessageForDepartement> builder)
        {
            builder.HasOne(x => x.MessageFor)
               .WithMany(x => x.MessageForDepartements)
               .HasForeignKey(fk => fk.IdMessageFor)
               .HasConstraintName("FK_TrMessageForDepartement_TrMessageFor")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Department)
               .WithMany(x => x.MessageForDepartements)
               .HasForeignKey(fk => fk.IdDepartment)
               .HasConstraintName("FK_TrMessageForDepartement_MsDepartment")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
