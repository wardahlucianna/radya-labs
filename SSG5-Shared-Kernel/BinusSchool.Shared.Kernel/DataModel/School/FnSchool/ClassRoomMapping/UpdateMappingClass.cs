using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class UpdateMappingClass
    {
        public string Id { get; set; }
        public string IdGrade { get; set; }
        public List<string> Pathways { get; set; }
        public List<ClassMappingDivison> Classrooms { get; set; }
    }

    public class ClassMappingDivison
    {
        public string IdPathwayClassroom { get; set; }
        /// <summary>
        /// opsional parameter di isi kalo mau di update
        /// </summary>
        public string IdGradePathway { get; set; }
        /// <summary>
        /// opsional param di isi kalo mau insert
        /// </summary>
        public string IdClassroom { get; set; }
        /// <summary>
        /// di isi kalo mu di update di hapus maka di isi true kalo tidak false 
        /// </summary>
        public bool IsDelete { get; set; }
        public List<string> Division { get; set; }
    }

    public class Division
    {
        /// <summary>
        /// opsional parameter di isi kalo mau di update
        /// </summary>
        public string IdDivisionMappingToClass { get; set; }
        /// <summary>
        /// opsional param di isi kalo mau insert
        /// </summary>
        public string IdDivision { get; set; }
        /// <summary>
        /// opsional 
        /// </summary>
        public string DivisionName { get; set; }
        /// <summary>
        /// di isi kalo mu di update di hapus maka di isi true kalo tidak false 
        /// </summary>
        public bool IsDelete { get; set; }
    }

    public class Pathway
    {
        /// <summary>
        /// opsional parameter di isi kalo mau di update
        /// </summary>
        public string IdPathwayDetail { get; set; }
        /// <summary>
        /// opsional param di isi kalo mau insert
        /// </summary>
        public string IdPathway { get; set; }
        public string StreamingName { get; set; }
        /// <summary>
        /// di isi kalo mu di update di hapus maka di isi true kalo tidak false 
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
