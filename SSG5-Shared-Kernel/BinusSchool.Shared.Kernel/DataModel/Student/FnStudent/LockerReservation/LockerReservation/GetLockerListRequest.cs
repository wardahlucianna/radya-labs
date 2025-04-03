﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation
{
    public class GetLockerListRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
        public int? SearchBy { get; set; }
        public string Keyword { get; set; }
    }
}
