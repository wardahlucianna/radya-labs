using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf
{
    public class StudentPassToPdfRequest
    {
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string NameUser { get; set; }
    }
}
