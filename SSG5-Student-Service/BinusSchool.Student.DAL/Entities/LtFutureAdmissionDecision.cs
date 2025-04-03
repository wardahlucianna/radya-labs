using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtFutureAdmissionDecision : AuditEntity, IStudentEntity
    {
        public string BINUSUnit { get; set; }
        public bool IsMutipleAnswer { get; set; }
        public bool IsRequired { get; set; }
        public int OrderNo { get; set; }
        public virtual ICollection<LtFutureAdmissionDecisionDetail> FutureAdmissionDecisionDetails { get; set; }
    }

    internal class LtFutureAdmissionDecisionConfiguration : AuditEntityConfiguration<LtFutureAdmissionDecision>
    {
        public override void Configure(EntityTypeBuilder<LtFutureAdmissionDecision> builder)
        {
            base.Configure(builder);
        }
    }
}