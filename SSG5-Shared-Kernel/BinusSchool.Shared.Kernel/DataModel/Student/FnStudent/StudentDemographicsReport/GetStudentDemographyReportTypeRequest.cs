﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentDemographyReportTypeRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
