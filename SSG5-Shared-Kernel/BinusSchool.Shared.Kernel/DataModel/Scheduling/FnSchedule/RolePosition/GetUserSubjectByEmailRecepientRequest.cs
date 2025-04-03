using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetUserSubjectByEmailRecepientRequest
    {
        /// <summary>
        /// for user login
        /// </summary>
        public string IdUser { get; set; } 
        public string IdAcademicYear { get; set; } 
        public string IdSchool { get; set; } 
        public bool IsShowIdUser { get; set; } 
        public List<GetUserEmailRecepient> EmailRecepients { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class GetUserEmailRecepient
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdUser { get; set; }

    }
}
