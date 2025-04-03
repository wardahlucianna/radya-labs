using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Employee
{
    public class LtDesignation : AuditNoUniqueEntity, IStudentEntity
    {
        public int Id { get; set; }
        public string DesignationDescription { get; set; }
        public virtual ICollection<MsStaff> Staffs { get; set; }
    }

    internal class LtDesignationConfiguration : AuditNoUniqueEntityConfiguration<LtDesignation>
    {
        public override void Configure(EntityTypeBuilder<LtDesignation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("IdDesignation");

            builder.Property(x => x.DesignationDescription)
                .HasMaxLength(20);

            base.Configure(builder);
        }
    }
}
