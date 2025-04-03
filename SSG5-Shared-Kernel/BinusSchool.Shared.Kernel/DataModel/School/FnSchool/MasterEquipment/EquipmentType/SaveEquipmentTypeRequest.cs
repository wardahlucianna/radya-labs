using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType
{
    public class SaveEquipmentTypeRequest
    {
        public string? IdEquipmentType { get; set; }
        public string IdSchool { get; set; }
        public string EquipmentTypeName { get; set; }
        public string ReservationOwner { get; set; }
    }
}
