using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtDistrict : AuditEntity, IStudentEntity
    {
        public string DistrictName { get; set; }
    }
    internal class LtDistrictConfiguration : AuditEntityConfiguration<LtDistrict>
    {
        public override void Configure(EntityTypeBuilder<LtDistrict> builder)
        {
             builder.Property(x => x.DistrictName)
                .HasMaxLength(80);

            base.Configure(builder);
        }

    }
}
