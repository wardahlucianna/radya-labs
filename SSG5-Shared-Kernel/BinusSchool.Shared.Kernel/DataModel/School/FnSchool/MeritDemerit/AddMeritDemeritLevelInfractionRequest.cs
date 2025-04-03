using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritLevelInfractionRequest
    {
        public string IdSchool { get; set; }
        public List<MeritDemeritLevelInfraction> LevelInfraction { get; set; }
    }

    public class MeritDemeritLevelInfraction 
    {
        public string Id { get; set; }
        public string NameLevelOfInfraction { get; set; }

        //if level of infraction is level >> name parent is null
        //if level of infraction is sublevel >> name parent is level
        public string NameParent { get; set; }
        public bool IsUseApproval { get; set; }
    }
}
