using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient
{
    public class AddEmailRecepientRequest
    {
        public TypeEmailRecepient Type { get; set; }
        public string IdSchool { get; set; }
        public List<AddEmailRecepient> Tos { get; set; }
        public List<AddEmailRecepient> Ccs { get; set; }
    }

    public class AddEmailRecepient
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdBinusian { get; set; }
    }
}
