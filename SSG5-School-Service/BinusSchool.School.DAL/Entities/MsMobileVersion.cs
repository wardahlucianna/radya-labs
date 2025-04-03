using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsMobileVersion : AuditEntity, ISchoolEntity 
    {
        public string MobileVersion { get; set; }
        public string OperatingSystem { get; set; }
    }

    internal class MsMobileVersionConfiguration : AuditEntityConfiguration<MsMobileVersion>
    {
        public override void Configure(EntityTypeBuilder<MsMobileVersion> builder)
        {
            base.Configure(builder);
        }
    }
}
