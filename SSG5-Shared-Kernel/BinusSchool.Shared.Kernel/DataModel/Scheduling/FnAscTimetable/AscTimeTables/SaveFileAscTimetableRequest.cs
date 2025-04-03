using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class SaveFileAscTimetableRequest
    {
        public string IdAscTimetable { get; set; }

        /// <summary>
        /// type yg berfungsi unutk flaging type yg di pakai upload atau reupload
        /// example value Type="Upload" untuk menandai bahwa api ini di pakai setelah upload 
        /// example value Type="Reupload" unutk menandai bahwa api ini di pakai setelah re upload 
        /// </summary>
        public string Type { get; set; }
    }
}
