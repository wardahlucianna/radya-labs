using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel
{
    public class UploadXmlFileResult : AscTimeTableUploadXmlRequest
    {
        public UploadXmlFileResult()
        {
            Class = new List<ClassPackageVm>();
            Lesson = new List<LessonUploadXMLVm>();
            Schedule = new List<ScheduleVm>();
            Enrollment = new EnrollmentDataVM();
            DayStructure = new List<DayStructureVm>();
            Weeks = new List<WeekTimeTableVm>();
            Terms = new List<TermTimeTableVm>();
            SessionSetFromXml = new List<SessionSetFromXmlVm>();
        }

        public List<SessionSetFromXmlVm> SessionSetFromXml { get; set; }
        public List<LessonUploadXMLVm> Lesson { get; set; }
        public List<ClassPackageVm> Class { get; set; }
        public List<ScheduleVm> Schedule { get; set; }
        public EnrollmentDataVM Enrollment { get; set; }
        public List<DayStructureVm> DayStructure { get; set; }
        public List<WeekTimeTableVm> Weeks { get; set; }
        public List<TermTimeTableVm> Terms { get; set; }
    }

    #region Lesson Model
    public class LessonUploadXMLVm
    {
        public LessonUploadXMLVm()
        {
            Teacher = new List<DataModelGeneral>();
            Class = new List<ClassLesonVm>();
            Subject = new SubjectLesson();
        }
        public string No { get; set; }
        public List<string> LessonIds { get; set; }
        public string SeminarGroupId { get; set; }
        public int TotalWeek { get; set; }
        public string WeekCode { get; set; }
        public string IdForSave { get; set; }
        public SubjectLesson Subject { get; set; }
        public GradeClassVM Grade { get; set; }
        public string ClassId { get; set; }
        public bool IsDataReadyFromMaster { get; set; }
        public List<DataModelGeneral> Teacher { get; set; }
        public List<ClassLesonVm> Class { get; set; }
    }

    public class SubjectLesson : DataModelGeneral
    {
        public SubjectLesson()
        {
            SubjectLevel = new List<SubjectLevelLesson>();
        }
        public List<SubjectLevelLesson> SubjectLevel { get; set; }
    }

    public class SubjectLevelLesson
    {
        public string SubjectLevelId { get; set; }
        public string SubjectLevelName { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ClassLesonVm
    {
        public List<string> IdClassFromXmls { get; set; }
        public string IdClassForSave { get; set; }
        public string IdClassFromMaster { get; set; }
        public string ClassCode { get; set; }
        public string ClassDescription { get; set; }
        public string ClassCombination { get; set; }
        public List<PathwayLesson> PathwayLesson { get; set; }
        public bool IsClassReadyFromMaster { get; set; }
        public bool IsDataReadyFromMaster { get; set; }
    }

    public class PathwayLesson
    {
        public string IdPathwayFromMaster { get; set; }
        public string PathwayCode { get; set; }
        public string PathwayDescription { get; set; }
        public bool IsPathwayReadyFromMaster { get; set; }
    }

    #endregion

    #region HomeRome Model

    public class ClassPackageVm
    {
        public ClassPackageVm()
        {
            Teacher = new List<DataModelGeneral>();
            Pathway = new List<DataModelGeneral>();
            StudentInHomeRoom = new List<StudentInHomeRoom>();
            Grade = new GradeClassVM();
        }
        public string No { get; set; }
        //public string IdClassFromXML { get; set; }
        public List<string> IdClassFromXMLS { get; set; }
        public string IdClassForSave { get; set; }
        public GradeClassVM Grade { get; set; }
        public List<DataModelGeneral> Pathway { get; set; }
        public List<DataModelGeneral> Teacher { get; set; }
        public ClassVmInClassData Class { get; set; }
        public DataModelGeneral Venue { get; set; }
        public bool IsDataReadyFromMaster { get; set; }
        public List<StudentInHomeRoom> StudentInHomeRoom { get; set; }
        public string MaleAndFemale { get; set; }
        public string Reqion { get; set; }
    }

    public class GradeClassVM : DataModelGeneral
    {
        public string IdGradePathway { get; set; }
    }


    public class ClassVmInClassData : CodeVm
    {
        public ClassVmInClassData()
        {
            DataIsUseInMaster = false;
        }

        public string Id { get; set; }
        public bool DataIsUseInMaster { get; set; }
        public string IdFromMasterData { get; set; }
        public string ClassCombination { get; set; }
    }

    public class StudentInHomeRoom
    {
        public StudentInHomeRoom()
        {
            StudentIsreadyFromMaster = false;
        }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Gender { get; set; }
        public string LastPathway { get; set; }
        public string NextPathway { get; set; }
        public string LastYearsRegister { get; set; }
        public string Religion { get; set; }
        public string SubjectReligion { get; set; }
        public bool StudentIsreadyFromMaster { get; set; }
    }

    #endregion

    #region Model Schedule
    public class ScheduleVm
    {
        public ScheduleVm()
        {
            Schedule = new List<ScheduleValueVM>();
        }
        public string IdSessionSet { get; set; }
        public string Session { get; set; }
        public List<ScheduleValueVM> Schedule { get; set; }
    }

    public class ScheduleValueVM
    {
        public ScheduleDayVM Days { get; set; }
        public List<ScheduleDataVM> ListSchedule { get; set; }

    }

    public class ScheduleDataVM
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ClassID { get; set; }
        public string LesonId { get; set; }
        public string DaysId { get; set; }
        public string SubjectId { get; set; }
        public bool LessonIsReadyFromMaster { get; set; }
        public DataModelGeneral Teacher { get; set; }
        public DataModelGeneral Venue { get; set; }
        public string Weeks { get; set; }
        public string Terms { get; set; }
        public bool DataIsUseInMaster { get; set; }
        public string IdDataFromMaster { get; set; }
    }
    #endregion

    #region Model Enrollment

    public class EnrollmentDataVM
    {
        public EnrollmentDataVM()
        {
            EnrollmentData = new List<EnrollmentVm>();
            EnrollmentColums = new List<ColumsStudentEnrollment>();
        }
        public List<EnrollmentVm> EnrollmentData { get; set; }
        public List<ColumsStudentEnrollment> EnrollmentColums { get; set; }
        public List<EnrollmentSubjectVm> EnrollmentSubject { get; set; }
    }

    public class EnrollmentVm
    {
        public StudentEnrollmentVM Student { get; set; }
        public List<EnrollmentStudentVM> EnrollmentStudent { get; set; }

    }
   
    public class EnrollmentStudentVM
    {
        public string ClassId { get; set; }
        public string LessonId { get; set; }
        public string SubjectID { get; set; }
        public bool DataIsUseInMaster { get; set; }
        public string SubjectLevelID { get; set; }
        public bool IsDataReadyFromMaster { get; set; }
    }


    public class StudentEnrollmentVM
    {
        public string IdFromXml { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Religion { get; set; }
        public string ClassCombination { get; set; }
        public string Class { get; set; }
        public DataModelGeneral Grade { get; set; }
        public bool DataIsUseInMaster { get; set; }
    }

    public class ColumsStudentEnrollment
    {
        public string ColumsName { get; set; }
    }

    public class EnrollmentSubjectVm
    {
        public string ClassId { get; set; }
        public string LessonId { get; set; }
        public string SubjectID { get; set; }
        public string SubjectLevelID { get; set; }
    }

    #endregion

    public class DayStructureVm
    {
        public string No { get; set; }
        public string Grade { get; set; }
        public string Pathway { get; set; }
        //public string IdLesson { get; set; }
        public string SchoolDay { get; set; }
        public string SchoolCode { get; set; }
        public string SessionName { get; set; }
        public string SessionID { get; set; }
        public string SessionAlias { get; set; }
        public string DurationInMinute { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IdSessionFormMaster { get; set; }
        public string IdSessionFormXml { get; set; }
        public bool IsDataReadyFromMaster { get; set; }
    }

    public class SessionSetFromXmlVm
    {
        public string IdFromXml { get; set; }
        public string IdGradepathway { get; set; }
        public string SessionId { get; set; }
        public string DaysCode { get; set; }
        public string DaysName { get; set; }
        public string DaysIdFromMaster { get; set; }
        public int DurationInMinute { get; set; }
        public string SessionName { get; set; }
        public string SessionAlias { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }


    /// <summary>
    /// class model generic
    /// </summary>
    public class DataModelGeneral : CodeVm
    {
        public DataModelGeneral()
        {
            DataIsUseInMaster = false;
        }

        public string Id { get; set; }
        public bool DataIsUseInMaster { get; set; }
        public string IdFromMasterData { get; set; }
    }



    public class WeekTimeTableVm
    {
        public string Id { get; set; }
        public string Weeks { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
    }

    public class TermTimeTableVm
    {
        public string Id { get; set; }
        public string Terms { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
    }
}
