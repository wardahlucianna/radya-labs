﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation
{
    public class UpdateLockedLockerRequest
    {
        public string IdLocker { get; set; }
        public bool LockedLocker { get; set; }
    }
}
