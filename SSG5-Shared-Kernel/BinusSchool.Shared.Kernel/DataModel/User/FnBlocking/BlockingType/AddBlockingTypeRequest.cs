using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class AddBlockingTypeRequest
    {
        public string IdSchool { get; set; }

        public string BlockingType { get; set; }

        public string IdMenu { get; set; }

        public List<SubMenuBlockingType> IdSubMenu { get; set; }
    }

    public class SubMenuBlockingType
    {
        public string Id { get; set; }
    }
}
