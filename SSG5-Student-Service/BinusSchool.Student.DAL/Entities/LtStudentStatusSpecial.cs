using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtStudentStatusSpecial : AuditNoUniqueEntity, IStudentEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdStudentStatusSpecial { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public string Remarks { get; set; }
        public bool NeedFutureAdmissionDecision { get; set; }
        public bool NeedAttention { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuses { get; set; }
    }

    internal class LtStudentStatusSpecialConfiguration : AuditNoUniqueEntityConfiguration<LtStudentStatusSpecial>
    {
        public override void Configure(EntityTypeBuilder<LtStudentStatusSpecial> builder)
        {
            builder.HasKey(p => p.IdStudentStatusSpecial);

            base.Configure(builder);
        }
    }
}