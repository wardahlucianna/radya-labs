using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsVenue : CodeEntity, IDocumentEntity
    {
        public string IdBuilding { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<MsDocumentReqCollectionVenue> DocumentReqCollectionVenues { get; set; }
        public virtual ICollection<MsDocumentReqApplicantCollection> DocumentReqApplicantCollections { get; set; }
    }

    internal class MsVenueConfiguration : CodeEntityConfiguration<MsVenue>
    {
        public override void Configure(EntityTypeBuilder<MsVenue> builder)
        {
            builder.HasOne(x => x.Building)
                .WithMany(x => x.Venues)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsVenue_MsBuilding")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            //builder.Property(x => x.Capacity).HasConversion<string>();

            base.Configure(builder);
        }
    }
}
