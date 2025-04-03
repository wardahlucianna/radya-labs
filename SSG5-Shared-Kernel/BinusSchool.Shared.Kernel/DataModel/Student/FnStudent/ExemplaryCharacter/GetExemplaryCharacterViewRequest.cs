using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetExemplaryCharacterViewRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdUserRequested { get; set; }
        public string Type { get; set; } //latest/toprating/category
        public string IdValueList { get; set; } //latest/toprating/category "using ~"
        public bool? IsParent { get; set; } // Access from parent desk or staff desk
    }
}
