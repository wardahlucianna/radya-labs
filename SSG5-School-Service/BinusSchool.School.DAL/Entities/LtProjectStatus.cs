using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class LtProjectStatus : CodeEntity, ISchoolEntity
    {
        public virtual ICollection<MsProjectPipeline> ProjectPipelines { get; set; }
        public virtual ICollection<MsProjectFeedback> ProjectFeedbacks { get; set; }
    }

    internal class LtProjectStatusConfiguration : CodeEntityConfiguration<LtProjectStatus>
    {
        public override void Configure(EntityTypeBuilder<LtProjectStatus> builder)
        {
            base.Configure(builder);
        }
    } 
}
