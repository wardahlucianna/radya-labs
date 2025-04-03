using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class GetListEquipmentDetailsResult
    {
        public string IdEquipment { get; set; }
        public NameValueVm EquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentDescription { get; set; }
        public int TotalStockQty { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public bool CanDelete { get; set; }

    }
}
