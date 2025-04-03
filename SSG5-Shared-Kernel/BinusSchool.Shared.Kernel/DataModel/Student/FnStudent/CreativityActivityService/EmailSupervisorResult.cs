using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class EmailSupervisorResult
    {
        public string Id { get; set; }
        public string IdUserSupervisor { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string StudentName { get; set; }
        public string SupervisorName { get; set; }
        public bool IsLabelPassword { get; set; }
    }
}
