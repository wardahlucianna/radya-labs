using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
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
    public class TrStudentFutureAdmissionDecision : AuditEntity, IStudentEntity
    {
        public string IdTrStudentStatus { get; set; }
        public string IdFutureAdmissionDecisionDetail { get; set; }
        public string? Reason { get; set; }
        public virtual TrStudentStatus TrStudentStatus { get; set; }
        public virtual LtFutureAdmissionDecisionDetail FutureAdmissionDecisionDetail { get; set; }
    }

    internal class TrStudentFutureAdmissionDecisionConfiguration : AuditEntityConfiguration<TrStudentFutureAdmissionDecision>
    {
        public override void Configure(EntityTypeBuilder<TrStudentFutureAdmissionDecision> builder)
        {
            builder.HasOne(x => x.TrStudentStatus)
                .WithMany(x => x.StudentFutureAdmissionDecisions)
                .HasForeignKey(x => x.IdTrStudentStatus)
                .HasConstraintName("FK_TrStudentFutureAdmissionDecision_TrStudentStatus")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.FutureAdmissionDecisionDetail)
                .WithMany(x => x.StudentFutureAdmissionDecisions)
                .HasForeignKey(x => x.IdFutureAdmissionDecisionDetail)
                .HasConstraintName("FK_TrStudentFutureAdmissionDecision_LtFutureAdmissionDecisionDetail")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}