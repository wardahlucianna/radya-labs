using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsVenue : CodeEntity, IUserEntity
    {
        public string IdBuilding { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsVenueConfiguration : CodeEntityConfiguration<MsVenue>
    {
        public override void Configure(EntityTypeBuilder<MsVenue> builder)
        {
            builder.Property(x => x.IdBuilding)
              .HasMaxLength(36)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
