using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsNationalityCountry : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdCountry { get; set; }
        public string CountryName { get; set; }
        public int CountryIDSerpong { get; set; }
        public string CountryNameSerpong { get; set; }
        public int NationalityIDSerpong { get; set; }
        public string NationalityNameSerpong { get; set; }
        public int CountryIDSimprug { get; set; }
        public string CountryNameSimprug { get; set; }
        public int NationalityIDSimprug { get; set; }
        public string NationalityNameSimprug { get; set; }
        public int CountryIDBekasi { get; set; }
        public string CountryNameBekasi { get; set; }        
        public int NationalityIDBekasi { get; set; }
        public string NationalityNameBekasi { get; set; }        
        public virtual LtCountry Country { get; set; }
        public virtual ICollection<MsMappingOldCity> MappingOldCity { get; set; }
    
    }
    internal class MsNationalityCountryConfiguration : AuditNoUniqueEntityConfiguration<MsNationalityCountry>
    {
        public override void Configure(EntityTypeBuilder<MsNationalityCountry> builder)
        {   
            builder.HasKey(p => p.IdCountry);

            builder.Property(x => x.IdCountry)                
                .HasMaxLength(36);

            builder.Property(x => x.CountryName)                
                .HasMaxLength(50);

            builder.Property(x => x.CountryNameSerpong)                
                .HasMaxLength(50);

            builder.Property(x => x.NationalityNameSerpong)                
                .HasMaxLength(50);    

             builder.Property(x => x.CountryNameSimprug)                
                .HasMaxLength(50);

            builder.Property(x => x.NationalityNameSimprug)                
                .HasMaxLength(50);    

             builder.Property(x => x.CountryNameBekasi)                
                .HasMaxLength(50);

            builder.Property(x => x.NationalityNameBekasi)                
                .HasMaxLength(50);    

            builder.HasOne(x => x.Country)
                .WithOne( y => y.NationalityCountry)
                .HasForeignKey<MsNationalityCountry>( fk => fk.IdCountry)
                .HasConstraintName("FK_MsNationalityCountry_LtCountry")
                .OnDelete(DeleteBehavior.NoAction);     

          
            base.Configure(builder);
        }

    }
}
