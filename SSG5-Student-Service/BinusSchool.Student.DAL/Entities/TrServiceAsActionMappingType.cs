using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionMappingType : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionForm { get; set; }
        public string IdServiceAsActionType { get; set; }

        public virtual TrServiceAsActionForm ServiceAsActionForm { get; set; }
        public virtual MsServiceAsActionType ServiceAsActionType { get; set; }
    }

    public class TrServiceAsActionMappingTypeConfiguration : AuditEntityConfiguration<TrServiceAsActionMappingType>
    {
        public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TrServiceAsActionMappingType> builder)
        {
            builder.HasKey(x => x.IdServiceAsActionForm);
            builder.HasKey(x => x.IdServiceAsActionType);
            builder.Property(x => x.IdServiceAsActionForm).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdServiceAsActionType).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.ServiceAsActionForm)
                .WithMany(x => x.ServiceAsActionMappingTypes)
                .HasForeignKey(fk => fk.IdServiceAsActionForm)
                .HasConstraintName("FK_TrServiceAsActionMappingType_TrServiceAsActionForm")
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ServiceAsActionType)
                .WithMany(x => x.ServiceAsActionMappingTypes)
                .HasForeignKey(fk => fk.IdServiceAsActionType)
                .HasConstraintName("FK_TrServiceAsActionMappingType_MsServiceAsActionType")
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
