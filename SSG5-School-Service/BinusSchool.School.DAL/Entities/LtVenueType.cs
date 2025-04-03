using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class LtVenueType : AuditEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }
        public string VenueTypeName { get; set; }
        public string? Description { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
    }

    internal class LtVenueTypeConfiguration : AuditEntityConfiguration<LtVenueType>
    {
        public override void Configure(EntityTypeBuilder<LtVenueType> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.VenueTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_LtVenueType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
