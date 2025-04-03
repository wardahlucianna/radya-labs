using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsExemplaryStudentBlock : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public DateTime BlockingStartDate { get; set; }
        public virtual MsStudent Student { get; set; }
    }

    internal class MsExemplaryStudentBlockConfiguration : AuditEntityConfiguration<MsExemplaryStudentBlock>
    {
        public override void Configure(EntityTypeBuilder<MsExemplaryStudentBlock> builder)
        {

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36);

            builder.HasOne(x => x.Student)
                    .WithMany(y => y.ExemplaryStudentBlocks)
                    .HasForeignKey(fk => fk.IdStudent)
                    .HasConstraintName("FK_MsExemplaryStudentBlock_MsStudent")
                    .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }
}
