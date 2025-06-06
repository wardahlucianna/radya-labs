﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritDisciplineMappingRequest : CollectionSchoolRequest
    {
        public string IdAcademiYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Category { get; set; }
        public string IdLevelInfraction { get; set; }
    }
}
