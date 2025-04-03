using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class UpdateBlockingTypeRequest
    {
        public string IdBlockingType { get; set; }

        public string BlockingType { get; set; }

        public string IdMenu { get; set; }

        public List<SubMenuBlockingType> IdSubMenu { get; set; }
    }
}
