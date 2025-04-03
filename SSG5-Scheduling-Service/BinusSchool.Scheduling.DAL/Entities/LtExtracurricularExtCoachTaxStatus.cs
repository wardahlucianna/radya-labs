using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class LtExtracurricularExtCoachTaxStatus : CodeEntity, ISchedulingEntity
    {
        public int Percentage { get; set; }
        public virtual ICollection<MsExtracurricularExternalCoach> ExtracurricularExternalCoachs { get; set; }
    }
    internal class LtExtracurricularExtCoachTaxStatusConfiguration : CodeEntityConfiguration<LtExtracurricularExtCoachTaxStatus>
    {
        public override void Configure(EntityTypeBuilder<LtExtracurricularExtCoachTaxStatus> builder)
        {


            base.Configure(builder);
        }
    }
}
