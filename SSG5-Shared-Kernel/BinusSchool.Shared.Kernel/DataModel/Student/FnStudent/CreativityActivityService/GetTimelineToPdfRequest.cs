﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetTimelineToPdfRequest
    {
        public string IdUser { get; set; }
        public string Role { get; set; }
        public string IdStudent { get; set; }
        public List<string> IdAcademicYear { get; set; }
    }
}
