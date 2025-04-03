using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtCity : AuditUniquelessEntity, IStudentEntity
    {
        public string CityName { get; set; }
        public string IdProvince { get; set; }
        public string IdCountry { get; set; }
        public virtual LtCountry Country { get; set; }

    }
    internal class LtCityConfiguration : AuditUniquelessEntityConfiguration<LtCity>
    {
        public override void Configure(EntityTypeBuilder<LtCity> builder)
        {

            builder.HasKey(p => new{p.Id,p.IdProvince,p.IdCountry});
            
            builder.Property(x => x.CityName)
                .HasMaxLength(50);

            builder.Property(x => x.IdProvince)
                .HasMaxLength(36);

            builder.Property(x => x.IdCountry)
                .HasMaxLength(36);

            builder.HasOne(x => x.Country)
                .WithMany( y => y.City)
                .HasForeignKey( fk => fk.IdCountry)
                .HasConstraintName("FK_LtCity_LtCountry")
                .OnDelete(DeleteBehavior.NoAction);    

            base.Configure(builder);
        }

    }
}
