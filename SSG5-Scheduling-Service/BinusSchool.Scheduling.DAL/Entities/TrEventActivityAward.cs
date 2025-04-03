using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventActivityAward : AuditEntity, ISchedulingEntity
    {
        public string IdEventActivity { get; set; }
        public string IdAward { get; set; }
        public string IdHomeroomStudent { get; set; }      
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
        public virtual TrEventActivity EventActivity { get; set; }
        public virtual MsAward Award { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
    }
    internal class TrEventActivityAwardConfiguration : AuditEntityConfiguration<TrEventActivityAward>
    {
        public override void Configure(EntityTypeBuilder<TrEventActivityAward> builder)
        {

            builder.HasOne(x => x.EventActivity)
            .WithMany(x => x.EventActivityAwards)
            .HasForeignKey(fk => fk.IdEventActivity)
            .HasConstraintName("FK_TrEventActivityAward_TrEventActivity")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Award)
            .WithMany(x => x.EventActivityAwards)
            .HasForeignKey(fk => fk.IdAward)
            .HasConstraintName("FK_TrEventActivityAward_MsAward")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
            .WithMany(x => x.EventActivityAwards)
            .HasForeignKey(fk => fk.IdHomeroomStudent)
            .HasConstraintName("FK_TrEventActivityAward_MsHomeroomStudent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.Filename).HasMaxLength(200);
            builder.Property(p => p.OriginalFilename).HasMaxLength(200);
            builder.Property(p => p.Filetype).HasMaxLength(10);
            builder.Property(x => x.Filesize)
            .HasColumnType("decimal(18,2)");

          

            base.Configure(builder);
        }
    }
}
