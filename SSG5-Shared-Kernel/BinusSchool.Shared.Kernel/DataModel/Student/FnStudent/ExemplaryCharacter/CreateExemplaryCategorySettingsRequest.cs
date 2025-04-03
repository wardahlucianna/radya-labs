using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class CreateExemplaryCategorySettingsRequest
    {
        public string IdSchool { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public bool CurrentStatus { get; set; }
    }
}
