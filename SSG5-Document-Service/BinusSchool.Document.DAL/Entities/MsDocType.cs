using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocType : CodeEntity, IDocumentEntity
    {
        public virtual ICollection<MsDocCategory> DocCategories { get; set; }
    }

    internal class MsDocTypeConfiguration : CodeEntityConfiguration<MsDocType>
    {
        public override void Configure(EntityTypeBuilder<MsDocType> builder)
        {
            base.Configure(builder);
        }
    }
}