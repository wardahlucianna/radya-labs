using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsVenueMappingApproval : AuditEntity, ISchedulingEntity
    {
        public string IdVenueMapping { get; set; }
        public string IdBinusian { get; set; }
        public virtual MsVenueMapping VenueMapping { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsVenueMappingApprovalConfiguration : AuditEntityConfiguration<MsVenueMappingApproval>
    {
        public override void Configure(EntityTypeBuilder<MsVenueMappingApproval> builder)
        {
            builder.HasOne(x => x.VenueMapping)
            .WithMany(x => x.VenueMappingApprovals)
            .HasForeignKey(fk => fk.IdVenueMapping)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Staff)
            .WithMany(x => x.VenueMappingApprovals)
            .HasForeignKey(fk => fk.IdBinusian)
            .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
