using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsFormula : AuditEntity, IAttendanceEntity
    {
        public string IdLevel {get;set;}
        public string AttendanceRate {get;set;}
        public string PresenceInClass {get;set;}
        public double MinimumPercentageAttendanceRate { get; set; }
        public virtual MsLevel Level {get;set;}

    }

    internal class MsFormulaConfiguration : AuditEntityConfiguration<MsFormula>
    {
        public override void Configure(EntityTypeBuilder<MsFormula> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.Formulas)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsFormula_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.AttendanceRate)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.PresenceInClass)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x=>x.MinimumPercentageAttendanceRate).HasColumnType("decimal(18,2)").IsRequired();

            base.Configure(builder);
        }
    }
}
