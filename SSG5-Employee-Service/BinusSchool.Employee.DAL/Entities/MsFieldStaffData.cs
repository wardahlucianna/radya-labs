using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class MsFieldStaffData : AuditNoUniqueEntity, IEmployeeEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdFieldStaffData { get; set; }
        public string StaffData { get; set; }
        public string AliasName { get; set; }  
    }

    internal class MsFieldStaffDataConfiguration : AuditNoUniqueEntityConfiguration<MsFieldStaffData>
    {
        public override void Configure(EntityTypeBuilder<MsFieldStaffData> builder)
        {
            builder.HasKey(x => x.IdFieldStaffData);

            builder.Property(x => x.StaffData)           
                .HasMaxLength(150);   

            builder.Property(x => x.AliasName)           
                .HasMaxLength(150);       


            base.Configure(builder);
        }
    }
}
