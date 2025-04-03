using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtServiceAsActionSdgs : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdServiceAsActionSdgs { get; set; }
        public string SdgsDesc { get; set; }

        public virtual ICollection<TrServiceAsActionMappingSdgs> TrServiceAsActionMappingSdgs { get; set; }
    }

    internal class LtServiceAsActionSdgsConfiguration : AuditNoUniqueEntityConfiguration<LtServiceAsActionSdgs>
    {
        public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<LtServiceAsActionSdgs> builder)
        {
            builder.HasKey(x => x.IdServiceAsActionSdgs);

            builder.Property(x => x.SdgsDesc)
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
