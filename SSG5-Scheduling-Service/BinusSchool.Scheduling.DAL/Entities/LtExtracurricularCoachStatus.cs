using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class LtExtracurricularCoachStatus : CodeEntity, ISchedulingEntity
    {
        public virtual ICollection<MsExtracurricularSpvCoach> ExtracurricularSpvCoachs { get; set; }
    }
    internal class LtExtracurricularCoachStatusConfiguration : CodeEntityConfiguration<LtExtracurricularCoachStatus>
    {
        public override void Configure(EntityTypeBuilder<LtExtracurricularCoachStatus> builder)
        {


            base.Configure(builder);
        }
    }
}
