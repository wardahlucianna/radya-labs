using System;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class UpdateMeritDemeritByIdRequest
    {
        public string Id { get; set; }
        public bool IsMerit { get; set; }
        public DateTime Date { get; set; }
        public string IdMeritDemeritMapping { get; set; }
        public string Note { get; set; }
        public string IdUser { get; set; }
    }
}
