using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtFutureAdmissionDecisionDetail : AuditEntity, IStudentEntity
    {
        public string IdFutureAdmissionDecision { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int OrderNo { get; set; }
        public virtual LtFutureAdmissionDecision FutureAdmissionDecision { get; set; }
        public virtual ICollection<TrStudentFutureAdmissionDecision> StudentFutureAdmissionDecisions { get; set; }
    }

    internal class LtFutureAdmissionDecisionDetailConfiguration : AuditEntityConfiguration<LtFutureAdmissionDecisionDetail>
    {
        public override void Configure(EntityTypeBuilder<LtFutureAdmissionDecisionDetail> builder)
        {
            builder.HasOne(x => x.FutureAdmissionDecision)
                .WithMany(x => x.FutureAdmissionDecisionDetails)
                .HasForeignKey(x => x.IdFutureAdmissionDecision)
                .HasConstraintName("FK_LtFutureAdmissionDecisionDetail_LtFutureAdmissionDecision")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}