﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetClassDiaryTypeSettingRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
