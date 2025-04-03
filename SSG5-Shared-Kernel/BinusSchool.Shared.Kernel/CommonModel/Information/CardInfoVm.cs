using System;

namespace BinusSchool.Common.Model.Information
{
    public class CardInfoVm
    {
        public string FamilyCardNumber { get; set;}
        public string NIK { get; set;}
        public string KITASNumber { get; set;}
        public DateTime? KITASExpDate { get; set;}
        public string NSIBNumber { get; set;}
        public DateTime? NSIBExpDate { get; set;}
        public string PassportNumber { get; set;}
        public DateTime? PassportExpDate { get; set;}
        public Int16 IsHavingKJP { get; set; }
    }
}