using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsPreviousSchoolNew : AuditEntity, IStudentEntity
    {
        public string NPSN { get; set; }
        public string TypeLevel { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string Kota_kab { get; set; }
        public string Website { get; set; }

        public virtual ICollection<MsStudentPrevSchoolInfo> StudentPrevSchoolInfo { get; set; }
    }

    internal class MsPreviousSchoolNewConfiguration : AuditEntityConfiguration<MsPreviousSchoolNew>
    {
        public override void Configure(EntityTypeBuilder<MsPreviousSchoolNew> builder)
        {
            builder.Property(x => x.TypeLevel)           
                .HasMaxLength(20);     

            builder.Property(x => x.SchoolName)
               .HasMaxLength(100);
                     
            builder.Property(x => x.Address)
                .HasMaxLength(300);   
            
            builder.Property(x => x.Country)           
                .HasMaxLength(50);       

            builder.Property(x => x.Province)           
                .HasMaxLength(50);     

            builder.Property(x => x.Kota_kab)           
                .HasMaxLength(50);

            builder.Property(x => x.Website)            
                .HasMaxLength(150);     

            builder.Property(x => x.NPSN)            
                .HasMaxLength(50);         

            base.Configure(builder);

        }
    }
}
