using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrSurveySummaryLog : AuditEntity, ISchoolEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public bool IsProcess { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal class TrSurveySummaryLogConfiguration : AuditEntityConfiguration<TrSurveySummaryLog>
    {
        public override void Configure(EntityTypeBuilder<TrSurveySummaryLog> builder)
        {
            base.Configure(builder);
        }
    }
}
