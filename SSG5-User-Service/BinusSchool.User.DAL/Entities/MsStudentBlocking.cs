using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsStudentBlocking : AuditEntity, IUserEntity
    {
        public string IdStudent { get; set; }
        public string IdBlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsBlockingCategory BlockingCategory { get; set; }
        public virtual MsBlockingType BlockingType { get; set; }
        public bool IsBlocked { get; set; }
    }

    internal class MsStudentBlockingConfiguration : AuditEntityConfiguration<MsStudentBlocking>
    {
        public override void Configure(EntityTypeBuilder<MsStudentBlocking> builder)
        {
            builder.Property(x => x.IsBlocked).IsRequired();


            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentBlockings)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentBlocking_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BlockingCategory)
                .WithMany(x => x.StudentBlockings)
                .HasForeignKey(fk => fk.IdBlockingCategory)
                .HasConstraintName("FK_MsStudentBlocking_MsBlockingCategory")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BlockingType)
                .WithMany(x => x.StudentBlockings)
                .HasForeignKey(fk => fk.IdBlockingType)
                .HasConstraintName("FK_MsStudentBlocking_MsBlockingType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
