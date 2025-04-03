using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetMasterSearchingDataResult : CollectionRequest
    {
        #region StudentData
        public string Photo { get; set; }
        public int SmtId { get; set; }
        public string SchoolName { get; set; }
        public string AcademicYear { get; set; }
        public string YearLevelId { get; set; }
        public string SchoolLevelId { get; set; }
        public string StudentStatusID { get; set; }
        public string SchoolID { get; set; }
        public string BinusianID { get; set; }
        public string StudentName { get; set; }
        public Gender Gender { get; set; }
        public string ReligionID { get; set; }
        public string ReligionName{ get; set; }
        public string BinusEmailAddress { get; set; }
        public string Nationality { get; set; }
        public string PreviousSchool { get; set; }
        public string DOB { get; set; }
        #endregion

        #region Father Data
        public string FatherName { get; set; }
        public string FatherMobilePhoneNumber1 { get; set; }
        public string FatherResidenceAddress { get; set; }
        public string FatherEmailAddress { get; set; }
        public string FatherCompanyName { get; set; }
        public string FatherOccupationPosition { get; set; }        
        public string FatherOfficeEmail { get; set; }
        #endregion

        #region Mother Data
        public string MotherName { get; set; }
        public string MotherMobilePhoneNumber1 { get; set; }
        public string MotherResidenceAddress { get; set; }
        public string MotherEmailAddress { get; set; }
        public string MotherCompanyName { get; set; }
        public string MotherOccupationPosition { get; set; }
        public string MotherOfficeEmail { get; set; }
        #endregion

    }
}
