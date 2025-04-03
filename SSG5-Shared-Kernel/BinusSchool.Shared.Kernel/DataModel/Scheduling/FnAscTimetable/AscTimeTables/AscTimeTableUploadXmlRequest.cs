using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    /// <summary>
    /// untuk documentasi ui dari design 
    /// https://xd.adobe.com/view/89a56778-a5a6-4038-9faa-86d31538bab2-dcf6/screen/4c4454d3-586d-4556-b42a-7931ede4227b/?fullscreen
    /// </summary>
    public class AscTimeTableUploadXmlRequest
    {
        /// <summary>
        /// Nama dari asc time table 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// id school
        /// </summary>
        public string IdSchool { get; set; }
        /// <summary>
        /// id dari sesion set kalau mekanisme nya pilih yg dari database
        /// </summary>
        public string IdSessionSet { get; set; }
        /// <summary>
        /// flag sebagai type create asc time table dari yg ada atau bikin sesion set
        /// dari file xml
        /// </summary>
        public bool IsCreateSessionSetFromXml { get; set; }
        /// <summary>
        /// id grade pathway dari class mapping untuk princtpal yg akan buat session set dari file xml nya
        /// </summary>
        public List<string> IdGradepathwayforCreateSession { get; set; }
        /// <summary>
        /// Session set name di isi kalo buat dari xml 
        /// </summary>
        public string SessionSetName { get; set; }
        /// <summary>
        /// id school academic years 
        /// </summary>
        public string IdSchoolAcademicyears { get; set; }
        /// <summary>
        /// flag untuk menentukan apakah ini 
        /// </summary>
        public bool AutomaticGenerateClassId { get; set; }
        /// <summary>
        /// format class patern yg akan di bikin 
        /// </summary>
        public string FormatIdClass { get; set; }
        /// <summary>
        /// id dari class mapping kalo yg di pilih type nya membuat sesion set dari file xml
        /// </summary>
        public List<string> CodeGradeForAutomaticGenerateClassId { get; set; }

    }
}
