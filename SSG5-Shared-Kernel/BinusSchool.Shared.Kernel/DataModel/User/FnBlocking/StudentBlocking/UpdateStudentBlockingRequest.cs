using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class UpdateStudentBlockingRequest
    {
        public string IdUser { get; set; }

        public string IdBlockingType { get; set; }

        public string IdBlockingCategory { get; set; }

        public bool IsBlock { get; set; }

    }
}
