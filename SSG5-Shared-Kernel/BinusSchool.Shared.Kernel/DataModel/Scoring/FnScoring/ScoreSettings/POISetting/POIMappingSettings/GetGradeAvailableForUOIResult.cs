using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using NPOI.OpenXmlFormats.Dml;
using Org.BouncyCastle.Bcpg;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class GetGradeAvailableForUOIResult
    {
        public ItemValueVm Level { get; set; }
        public List<ItemValueVm> Grades { get; set; }
    }
}
