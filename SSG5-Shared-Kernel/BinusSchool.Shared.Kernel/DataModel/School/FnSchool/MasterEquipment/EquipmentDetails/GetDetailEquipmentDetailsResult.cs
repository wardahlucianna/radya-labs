using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class GetDetailEquipmentDetailsResult
    {
        public string IdEquipment { get; set; }
        public ItemValueVm EquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentDescription { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
    }
}
