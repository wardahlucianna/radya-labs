using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SendEmail
{
    public class SendEmailNotificationUnsubmittedScoreRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdUser { set; get; }
        public string Semester { set; get; }
        public int DayDifference { set; get; }
    }

    public class SendEmailNotificationUnsubmittedScoreResult_ParamEmail
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Role { get; set; }
        public ItemValueVm Period { get; set; }
        public ItemValueVm HomeroomClassroom { get; set; }
        public ItemValueVm? Student { get; set; }
        public string IdSubject { get; set; }
    }
}
