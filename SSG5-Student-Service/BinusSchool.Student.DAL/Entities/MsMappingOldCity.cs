using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMappingOldCity : AuditNoUniqueEntity, IStudentEntity
    { 
        public string IdCity { get; set; }
        public string CityName { get; set; }
        public string IdCountry { get; set; }
        public string CountryName { get; set; }
        public string IdProvince { get; set; }
        public string ProvinceName { get; set; }
        public int CityIDSerpong { get; set; }
        public string CityNameSerpong { get; set; }
        public int CityIDSimprug { get; set; }
        public string CityNameSimprug { get; set; }
        public int CityIDBekasi { get; set; }
        public string CityNameBekasi { get; set; }

        public virtual MsNationalityCountry NationalityCountry { get; set; }

    }
    internal class MsMappingOldCityConfiguration : AuditNoUniqueEntityConfiguration<MsMappingOldCity>
    {
        public override void Configure(EntityTypeBuilder<MsMappingOldCity> builder)
        {   
            builder.HasKey(p => new {p.IdCity,p.IdProvince,p.IdCountry});

            builder.Property(x => x.IdCity)                
                .HasMaxLength(36);

            builder.Property(x => x.CityName)                
                .HasMaxLength(50);

            builder.Property(x => x.IdCountry)                
                .HasMaxLength(36);

            builder.Property(x => x.CountryName)                
                .HasMaxLength(50);

            builder.Property(x => x.IdProvince)                
                .HasMaxLength(36);

            builder.Property(x => x.ProvinceName)                
                .HasMaxLength(50);

            builder.Property(x => x.CityNameSerpong)                
                .HasMaxLength(50);

            builder.Property(x => x.CityNameSimprug)                
                .HasMaxLength(50);

            builder.Property(x => x.CityNameBekasi)                
                .HasMaxLength(50);

            builder.HasOne(x => x.NationalityCountry)
                .WithMany( y => y.MappingOldCity)
                .HasForeignKey( fk => fk.IdCountry)
                .HasConstraintName("FK_MsMappingOldCity_MsNationalityCountry")
                .OnDelete(DeleteBehavior.NoAction);     
    

            base.Configure(builder);                


        }
    }
}
