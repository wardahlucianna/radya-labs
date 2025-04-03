using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtStudentStatus : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdStudentStatus { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<HTrStudentStatus> HTrStudentStatuss { get; set; }    
    }
    internal class LtStudentStatusConfiguration : AuditNoUniqueEntityConfiguration<LtStudentStatus>
    {
        public override void Configure(EntityTypeBuilder<LtStudentStatus> builder)
        {
            builder.HasKey(p => p.IdStudentStatus);

            builder.Property(x => x.ShortDesc)
              .HasMaxLength(50);

            builder.Property(x => x.LongDesc)
              .HasMaxLength(128);

            base.Configure(builder);
        }
    }
}
