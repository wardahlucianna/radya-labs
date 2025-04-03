using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class TrServiceAsActionMappingSdgs : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionForm { get; set; }
        public string IdServiceAsActionSdgs { get; set; }

        public virtual TrServiceAsActionForm TrServiceAsActionForm { get; set; }
        public virtual LtServiceAsActionSdgs LTServiceAsActionSdgs { get; set; }
    }

    internal class TrServiceAsActionMappingSdgsConfiguration : AuditEntityConfiguration<TrServiceAsActionMappingSdgs>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionMappingSdgs> builder)
        {
            builder.HasKey(x => new { x.IdServiceAsActionForm, x.IdServiceAsActionSdgs });

            builder.HasOne(x => x.TrServiceAsActionForm)
                .WithMany(x => x.ServiceAsActionMappingSdgs)
                .HasForeignKey(x => x.IdServiceAsActionForm);

            builder.HasOne(x => x.LTServiceAsActionSdgs)
                .WithMany(x => x.TrServiceAsActionMappingSdgs)
                .HasForeignKey(x => x.IdServiceAsActionSdgs);

            base.Configure(builder);
        }
    }
}
