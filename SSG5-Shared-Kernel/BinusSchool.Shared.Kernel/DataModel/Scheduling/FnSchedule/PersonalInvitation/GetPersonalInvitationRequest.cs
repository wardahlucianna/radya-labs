using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetPersonalInvitationRequest :  CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        //Role : PARENT,STUDENT,STAFF
        public string Role { get; set; }
        public DateTime? DateInvitation { get; set; }
        public string TypeInvitation { get; set; }
        public string Status { get; set; }
    }
}
