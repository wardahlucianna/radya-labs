using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class SaveEquipmentDetailsRequest
    {
        public string? IdEquipment { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string? EquipmentDescription { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public string IdSchool { get; set; }
    }
}
