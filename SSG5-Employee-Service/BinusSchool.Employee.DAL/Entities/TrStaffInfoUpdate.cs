using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class TrStaffInfoUpdate : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdBinusian { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string Constraint1 { get; set; }
        public string Constraint2 { get; set; }
        public string Constraint3 { get; set; }
        public string OldFieldValue { get; set; }
        public string CurrentFieldValue { get; set; }
        public string Constraint1Value { get; set; }
        public string Constraint2Value { get; set; }
        public string Constraint3Value { get; set; }    
        public DateTime? RequestedDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int IdApprovalStatus { get; set; }
        public string Notes { get; set; }
        public string RequestedBy { get; set; }
        public virtual MsStaff Staff { get; set; }
       
    }
    internal class TrStaffInfoUpdateConfiguration : AuditNoUniqueEntityConfiguration<TrStaffInfoUpdate>
    {
        public override void Configure(EntityTypeBuilder<TrStaffInfoUpdate> builder)
        {
            builder.HasKey(p => new {p.IdBinusian,p.TableName,p.FieldName,p.DateIn});  

            builder.Property(x => x.IdBinusian)             
                .HasMaxLength(36);

            builder.Property(x => x.TableName)             
                .HasMaxLength(50);

            builder.Property(x => x.FieldName)             
                .HasMaxLength(50);
            
            builder.Property(x => x.Constraint1)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint2)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint3)             
                .HasMaxLength(50);        

            builder.Property(x => x.OldFieldValue)             
                .HasMaxLength(350);      

            builder.Property(x => x.CurrentFieldValue)             
                .HasMaxLength(350);    

            builder.Property(x => x.Constraint1Value)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint2Value)             
                .HasMaxLength(50);

            builder.Property(x => x.Constraint3Value)             
                .HasMaxLength(50);      

            builder.Property(x => x.RequestedDate)              
                .HasColumnType(typeName: "datetime2");    

             builder.Property(x => x.ApprovalDate)              
                .HasColumnType(typeName: "datetime2");        

            builder.Property(x => x.Notes)             
                .HasMaxLength(300);    

            builder.Property(x => x.RequestedBy)             
                .HasMaxLength(100);        

            builder.HasOne(x => x.Staff)
                .WithMany( y => y.StaffInfoUpdate)
                .HasForeignKey( fk => fk.IdBinusian)
                .HasConstraintName("FK_TrStaffInfoUpdate_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);    

            base.Configure(builder);                 

        }
    }
}
