﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentHandlerRequest :  CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }

    }
}
