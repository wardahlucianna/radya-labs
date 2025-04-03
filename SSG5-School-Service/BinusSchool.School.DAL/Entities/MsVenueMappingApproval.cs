using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsVenueMappingApproval : AuditEntity, ISchoolEntity
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
            .HasConstraintName("FK_MsVenueMappingApproval_MsVenueMapping")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

            builder.HasOne(x => x.Staff)
            .WithMany(x => x.VenueMappingApprovals)
            .HasForeignKey(fk => fk.IdBinusian)
            .HasConstraintName("FK_MsVenueMappingApproval_MsStaff")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
