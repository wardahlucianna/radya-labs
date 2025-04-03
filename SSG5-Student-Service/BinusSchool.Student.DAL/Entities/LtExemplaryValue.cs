using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtExemplaryValue : AuditEntity, IStudentEntity
    {
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public int OrderNumber { get; set; }
        public bool CurrentStatus { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrExemplaryValue> TrExemplaryValues { get; set; }
    }

    internal class LtExemplaryValueConfiguration : AuditEntityConfiguration<LtExemplaryValue>
    {
        public override void Configure(EntityTypeBuilder<LtExemplaryValue> builder)
        {

            builder.Property(x => x.ShortDesc)
               .HasMaxLength(50)
               .IsRequired();

            builder.Property(x => x.LongDesc)
               .HasMaxLength(128)
               .IsRequired();

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.LtExemplaryValues)
                .IsRequired()
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_LtExemplaryValue_MsSchool")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
