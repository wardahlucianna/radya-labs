using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocCategory : CodeEntity, IDocumentEntity
    {
        public string IdDocType { get; set; }
        public string IdSchool { get; set; }

        public virtual MsDocType DocType { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsForm> Forms { get; set; }
    }

    internal class MsDocCategoryConfiguration : CodeEntityConfiguration<MsDocCategory>
    {
        public override void Configure(EntityTypeBuilder<MsDocCategory> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocType)
                .WithMany(x => x.DocCategories)
                .HasForeignKey(fk => fk.IdDocType)
                .HasConstraintName("FK_MsDocCategory_MsDocType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.DocCategories)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsDocCategory_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);

            builder.Property(x => x.Code)
                .HasMaxLength(32);
        }
    }
}
