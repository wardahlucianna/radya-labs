using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType
{
    public class GetListEquipmentTypeResult
    {
        public string IdEquipmentType { get; set; }
        public string EquipmentTypeName { get; set; }
        public NameValueVm ReservationOwner { get; set; }
        public bool CanDelete { get; set; }
    }
}
