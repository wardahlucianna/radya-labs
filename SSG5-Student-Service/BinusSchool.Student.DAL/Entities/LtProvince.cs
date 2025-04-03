using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtProvince : AuditUniquelessEntity, IStudentEntity
    {
        public string ProvinceName { get; set; }
        public string IdCountry { get; set; }
        public virtual LtCountry Country { get; set; }

    }
    internal class LtProvinceConfiguration : AuditUniquelessEntityConfiguration<LtProvince>
    {
        public override void Configure(EntityTypeBuilder<LtProvince> builder)
        {
            
            builder.HasKey(p => new{p.Id,p.IdCountry});
       
            builder.Property(x => x.ProvinceName)
                .HasMaxLength(50);

            builder.Property(x => x.IdCountry)
                .HasMaxLength(36);

            builder.HasOne(x => x.Country)
                .WithMany( y => y.Province)
                .HasForeignKey( fk => fk.IdCountry)
                .HasConstraintName("FK_LtProvince_LtCountry")
                .OnDelete(DeleteBehavior.NoAction);    
    

         
            base.Configure(builder);
        }

    }
}
