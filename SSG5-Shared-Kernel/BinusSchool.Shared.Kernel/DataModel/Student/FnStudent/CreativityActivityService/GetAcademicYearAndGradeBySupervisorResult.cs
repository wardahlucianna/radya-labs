﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetAcademicYearAndGradeBySupervisorResult
    {
        public ItemValueVm Year { get; set; }
        public List<ItemValueVm> Grade { get; set; }
    }
}
