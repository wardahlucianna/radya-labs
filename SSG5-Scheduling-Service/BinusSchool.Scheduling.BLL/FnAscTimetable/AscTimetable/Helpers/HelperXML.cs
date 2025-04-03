using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel.XML2012Type;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Helpers
{
    public class HelperXML
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IApiService<ISubject> _subjectService;
        public HelperXML(
            ISchedulingDbContext dbContext,
            IApiService<ISubject> subjectService)
        {
            _dbContext = dbContext;
            _subjectService = subjectService;
        }
        /// <summary>
        /// check version xml dan format xml
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> CheckVersionXmlHelpers(HttpRequest request)
        {
            var result = "";
            XmlDocument doc = new XmlDocument();
            var getData = request.Form.Files.FirstOrDefault();
            var file = request.Form.Files["file"];

            if (getData.Length == 0 || getData == null)
            {
                throw new BadRequestException("File Xml Not Null");
            }

            //get file name and file extention
            string extent = Path.GetExtension(getData.FileName);
            if (extent.ToLower() != ".xml")
            {
                throw new BadRequestException("Format Not Suported!");
            }

            var contents = "";
            using (StreamReader streamReader = new StreamReader(getData.OpenReadStream()))
            {
                contents = await streamReader.ReadToEndAsync();
            }
            doc.LoadXml(contents);

            XmlElement root = doc.DocumentElement;
            result = root.Attributes["displayname"].Value;
            return result;
        }

        /// <summary>
        /// Generate Class Id
        /// </summary>
        /// <param name="format"></param>
        /// <param name="ClassIds"></param>
        /// <param name="Grade"></param>
        /// <param name="initialteacher"></param>
        /// <param name="SubjectSortName"></param>
        /// <returns></returns>
        private string GenerateClassid(string format, string Increment, int AlfabetIncrement, string ClassIds, string Grade, string initialteacher, string SubjectSortName)
        {
            var result = format;
            result = result.Replace(@"[AutoIncrement]", Increment);
            result = result.Replace(@"[Class]", ClassIds);
            result = result.Replace(@"[Grade]", Grade);
            result = result.Replace(@"[InitialTeacher]", initialteacher);
            result = result.Replace(@"[Subject]", SubjectSortName);
            result = result.Replace(@"[AutoIncrementAlphabet]", GetCharacterFromIndex(AlfabetIncrement));

            return result.Replace("%", "");
        }

        /// <summary>
        /// convert angka to abjad
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        private string GetCharacterFromIndex(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// get code for days
        /// </summary>
        /// <param name="text"></param>
        /// <param name="stopAt"></param>
        /// <returns></returns>
        private string GetCodeDays(string text, string stopAt = "1")
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Substring(0, text.IndexOf(stopAt) + 1);
            }

            return string.Empty;
        }

        /// <summary>
        /// get data session from xml for insert to master data
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="grade"></param>
        /// <param name="_lesson"></param>
        /// <param name="IdSchool"></param>
        /// <param name="DaysFromMaster"></param>
        /// <returns></returns>
        public List<SessionSetFromXmlVm> GetSessionSetFromXml(Timetable2019 timetable2019,
                                                                          List<GradePathwayForAscTimeTableResult> grade,
                                                                          List<LessonUploadXMLVm> _lesson,
                                                                          string IdSchool,
                                                                          List<CodeWithIdVm> DaysFromMaster)
        {
            var result = new List<SessionSetFromXmlVm>();
            var daysFromMaster = new List<CodeWithIdVm>();
            var dataDayStructure = (from card in timetable2019.Cards.Card
                                    join p in timetable2019.Periods.Period on card.Period equals p._period into pk
                                    from period in pk.DefaultIfEmpty()
                                    join d in timetable2019.Daysdefs.Daysdef on card.Days equals d.Days into dk
                                    from days in dk.DefaultIfEmpty()
                                        //join lesson in _lesson on card.Lessonid equals lesson.LessonId
                                    select new { Card = card, Period = period, Days = days }).ToList();

            //dataDayStructure = dataDayStructure.Where(x => x.Card.Lessonid == "543632C6B197EB41").ToList();
            foreach (var item in dataDayStructure)
            {                
                var getLesson = _lesson.Where(p => p.LessonIds.Any(x => x == item.Card.Lessonid)).FirstOrDefault();
                var gradepathwaylist = grade.Where(p => p.GradeCode == getLesson?.Grade?.Code && p.GradePathwayClassRooms.Any(x => getLesson.Class.Select(y => y.IdClassFromMaster).ToList().Contains(x.IdGradePathwayClassrom))).ToList();
                if (getLesson != null)
                {
                    //var days = GetCodeDays(item.Days.Days);
                    //var Days = DaysFromMaster.Where(p => p.Code == days).FirstOrDefault();

                    //TimeSpan strat = TimeSpan.Parse(item.Period.Starttime);
                    //TimeSpan end = TimeSpan.Parse(item.Period.Endtime);
                    //TimeSpan time = end - strat;
                    //int minutes = Convert.ToInt32(time.TotalMinutes);
                    ////dataSession.IdFromXml = Guid.NewGuid().ToString();
                    //dataSession.SessionId = item.Period._period;
                    //dataSession.SessionAlias = item.Period.Short;
                    //dataSession.SessionName = item.Period.Name;
                    //dataSession.StartTime = item.Period.Starttime;
                    //dataSession.EndTime = item.Period.Endtime;
                    //dataSession.DaysCode = item.Days.Days;
                    //dataSession.DaysName = item.Days.Name;
                    //dataSession.DaysIdFromMaster = Days != null ? Days.Id : "";
                    //dataSession.DurationInMinute = minutes;
                    //dataSession.IdGradepathway = getLesson.Grade.IdGradePathway;

                    var days = GetCodeDays(item.Days.Days);
                    var Days = DaysFromMaster.Where(p => p.Code == days).FirstOrDefault();

                    TimeSpan strat = TimeSpan.Parse(item.Period.Starttime);
                    TimeSpan end = TimeSpan.Parse(item.Period.Endtime);
                    TimeSpan time = end - strat;
                    int minutes = Convert.ToInt32(time.TotalMinutes);
                    foreach (var GradePathway in gradepathwaylist)
                    {
                        var dataSession = new SessionSetFromXmlVm();
                        dataSession.SessionId = item.Period._period;
                        dataSession.SessionAlias = item.Period.Short;
                        dataSession.SessionName = item.Period.Name;
                        dataSession.StartTime = item.Period.Starttime;
                        dataSession.EndTime = item.Period.Endtime;
                        dataSession.DaysCode = item.Days.Days;
                        dataSession.DaysName = item.Days.Name;
                        dataSession.DaysIdFromMaster = Days != null ? Days.Id : "";
                        dataSession.DurationInMinute = minutes;
                        dataSession.IdGradepathway = GradePathway.IdGradePathway;

                        result.Add(dataSession);
                    }
                }
            }

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(result);

            if (result.Any())
            {
                var session = result.GroupBy(p => new
                {
                    p.SessionId,
                    p.SessionName,
                    p.SessionAlias,
                    p.DaysIdFromMaster,
                    p.StartTime,
                    p.EndTime,
                    p.DaysCode,
                    p.DaysName,
                    p.DurationInMinute,
                    p.IdGradepathway,
                }).Select(p => new SessionSetFromXmlVm
                {
                    IdFromXml = Guid.NewGuid().ToString(),
                    SessionId = p.Key.SessionId,
                    SessionName = p.Key.SessionName,
                    SessionAlias = p.Key.SessionAlias,
                    DaysIdFromMaster = p.Key.DaysIdFromMaster,
                    StartTime = p.Key.StartTime,
                    EndTime = p.Key.EndTime,
                    DaysCode = p.Key.DaysCode,
                    DaysName = p.Key.DaysName,
                    DurationInMinute = p.Key.DurationInMinute,
                    IdGradepathway = p.Key.IdGradepathway,
                }).ToList();

                return session;
            }

            return result;
        }


        /// <summary>
        /// function untuk get class
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="session"></param>
        /// <param name="IdSchool"></param>
        /// <returns></returns>
        public async Task<List<ClassPackageVm>> GetReadAndValidateDataClasss(Timetable2019 timetable2019,
                                                        List<GradePathwayForAscTimeTableResult> session,
                                                        string IdSchool,
                                                        List<GetStudentUploadAscResult> StudentFormMaster,
                                                        List<CheckTeacherForAscTimetableResult> TeacherFromMaster)
        {
            var result = new List<ClassPackageVm>();
            var venueFromMaster = new List<GetVenueForAscTimetableResult>();
            var classPackage = (from classData in timetable2019.Classes.Class
                                join t in timetable2019.Teachers.Teacher on classData.Teacherid equals t.Id into teac
                                from teacher in teac.DefaultIfEmpty()
                                join vn in timetable2019.Classrooms.Classroom on classData.Classroomids equals vn.Id into ven
                                from venue in ven.DefaultIfEmpty()
                                    //where session.Any(z => z.GradeCode == Regex.Replace(classData.Name.Split("-").FirstOrDefault().Substring(0, 2), @"[^\d+]", "") || z.GradeCode == classData.Name)
                                where session.Any(z => z.GradeCode == classData.GetGradeCode() || z.GradeCode == classData.Name)
                                select new { ClassData = classData, Teacher = teacher, Venue = venue }).ToList();

            //group by dulu data class dari xml ambil yg class name nya sama 
            //misallkan ada data class nya sama beda pathway saja maka itu jadikan class yg sama 
            //contoh nya 10A-ipa dan 10A-ips maka ambil 10A saja
            //var classGrouping = classPackage.GroupBy(p => p.ClassData.Name.Split("-").FirstOrDefault()).ToList();
            var classGrouping = classPackage.GroupBy(p => p.ClassData.GetClassName()).ToList();

            if (classGrouping.Count > 0)
            {
                var getCodeVenue = classGrouping.SelectMany(p => p.Where(e=>e.Venue!=null).Select(x => x.Venue.Short)).Distinct().ToList();
                if (getCodeVenue.Count() > 0)
                {
                    venueFromMaster = await _dbContext.Entity<MsVenue>()
                                                      .Include(p => p.Building)
                                                      .Where(p => getCodeVenue.Any(x => x == p.Code) && p.Building.IdSchool == IdSchool)
                                                      .Select(p => new GetVenueForAscTimetableResult
                                                      {
                                                          IdVenue = p.Id,
                                                          Code = p.Code,
                                                          Description = p.Description,
                                                          BuildingCode = p.Building.Code,
                                                          BuildingDescription = p.Building.Description,
                                                      }).ToListAsync();
                }


                var countClass = 0;
                foreach (var item in classGrouping)
                {
                    countClass++;
                    var dataClass = new ClassPackageVm();

                    var classCode = item.Key.Trim();
                    var schoolClass = session.SelectMany(p => p.GradePathwayClassRooms).ToList();
                    var dataClassFromMaster = schoolClass.Where(p => p.ClassRoomCombinationGrade == classCode & p.IdSchool == IdSchool)
                                                               .FirstOrDefault();

                    #region chek Grade
                    var checkGrade = session.Where(p => p.GradePathwayClassRooms.Any(x => x.ClassRoomCombinationGrade == classCode && x.IdSchool == IdSchool)).FirstOrDefault();

                    dataClass.Grade = new GradeClassVM
                    {
                        Id = checkGrade != null ? checkGrade.IdGrade : "",
                        Code = checkGrade != null ? checkGrade.GradeCode : "",
                        Description = checkGrade != null ? checkGrade.GradeDescription : "",
                        DataIsUseInMaster = checkGrade != null ? true : false,
                        IdFromMasterData = checkGrade != null ? checkGrade.IdGrade : "",
                        IdGradePathway = checkGrade != null ? checkGrade.IdGradePathway : "",
                    };
                    #endregion

                    #region proses class
                    dataClass.No = countClass.ToString("D4");
                    dataClass.IdClassFromXMLS = item.Select(p => p.ClassData.Id).ToList();
                    dataClass.IdClassForSave = Guid.NewGuid().ToString();
                    dataClass.Class = new ClassVmInClassData
                    {
                        Id = dataClassFromMaster != null ? dataClassFromMaster.IdGradePathwayClassrom : "",
                        Code = classCode,
                        Description = dataClassFromMaster != null ? dataClass.Grade?.Code + " " + dataClassFromMaster.ClassRoomDescription : "",
                        DataIsUseInMaster = dataClassFromMaster != null ? true : false,
                        ClassCombination = dataClassFromMaster != null ? dataClassFromMaster.ClassRoomCombinationGrade : "",
                        IdFromMasterData = dataClassFromMaster != null ? dataClassFromMaster.IdGradePathwayClassrom : "",
                    };
                    #endregion

                    #region check venue
                    var chekVenue = venueFromMaster.Where(p => p.Code == item.FirstOrDefault().Venue?.Short).FirstOrDefault();

                    dataClass.Venue = new DataModelGeneral
                    {
                        Id = chekVenue != null ? chekVenue.IdVenue : "",
                        Code = chekVenue != null ? chekVenue.Code : item.FirstOrDefault().Venue != null ? item.FirstOrDefault().Venue?.Short : "",
                        Description = chekVenue != null ? chekVenue.Description : item.FirstOrDefault().Venue != null ? item.FirstOrDefault().Venue?.Name : "",
                        DataIsUseInMaster = true,
                        IdFromMasterData = chekVenue != null ? chekVenue.IdVenue : "",
                    };
                    #endregion

                    #region Check data Pathway
                    if (checkGrade != null)
                    {
                        var dataPathwayFromMaster = checkGrade.GradePathwayDetails.ToList();

                        //cek data pathway
                        var getPathway = item.Select(p => p.ClassData.Name).ToList();
                        var pathwayModel = new List<DataModelGeneral>();
                        foreach (var itemPathway in getPathway)
                        {
                            var pathway = itemPathway.Split("-").ToList();
                            if (pathway.Count > 1)
                            {
                                if (dataPathwayFromMaster.Count() > 0)
                                {
                                    var pathwayCode = pathway.LastOrDefault().Trim();
                                    var checkPatway = dataPathwayFromMaster.Where(p => p.PathwayCode == pathwayCode)
                                                                         .FirstOrDefault();

                                    pathwayModel.Add(new DataModelGeneral
                                    {
                                        Id = checkPatway != null ? checkPatway.IdGradePathwayDetail : "",
                                        Code = pathwayCode,
                                        Description = checkPatway != null ? checkPatway.PathwayDescription : "",
                                        DataIsUseInMaster = checkPatway != null ? true : false,
                                        IdFromMasterData = checkPatway != null ? checkPatway.IdGradePathwayDetail : "",
                                    });
                                }

                            }
                            else
                            {
                                if (dataPathwayFromMaster.Count() > 0)
                                {
                                    var checkPatway = dataPathwayFromMaster.Where(p => p.PathwayCode.ToLower() == "no pathway")
                                                                         .FirstOrDefault();

                                    pathwayModel.Add(new DataModelGeneral
                                    {
                                        Id = checkPatway != null ? checkPatway.IdGradePathwayDetail : "",
                                        Code = checkPatway != null ? checkPatway.PathwayCode : "No Pathway",
                                        Description = checkPatway != null ? checkPatway.PathwayDescription : "",
                                        DataIsUseInMaster = checkPatway != null ? true : false,
                                        IdFromMasterData = checkPatway != null ? checkPatway.IdGradePathwayDetail : "",
                                    });
                                }
                            }
                        }
                        pathwayModel = pathwayModel.Distinct().ToList();
                        dataClass.Pathway = pathwayModel;
                    }

                    #endregion

                    #region chek data teacher 
                    //data teacher belum di get dari xml nya 
                    var getTeacherXML = item.Where(p => p.Teacher != null).Select(p => p.Teacher).Distinct().ToList();
                    if (getTeacherXML.Count > 0)
                    {
                        foreach (var itemTeacher in getTeacherXML)
                        {
                            var checkteacher = TeacherFromMaster.Where(p => p.TeacherShortName == itemTeacher.Short ||
                                                                            p.TeacherBinusianId == itemTeacher.Short ||
                                                                            p.TeacherInitialName == itemTeacher.Short).FirstOrDefault();
                            if (checkteacher != null)
                            {
                                dataClass.Teacher.Add(new DataModelGeneral()
                                {
                                    Id = itemTeacher.Id,
                                    Code = checkteacher.TeacherInitialName,
                                    Description = checkteacher.TeacherName,
                                    DataIsUseInMaster = true,
                                    IdFromMasterData = checkteacher.IdTeacher,
                                });
                            }
                        }
                    }

                    #endregion

                    #region Check data student

                    //get data student from xml dan belum cek ke data student 
                    var studentByClassid = timetable2019.Students.Student.Where(p => item.Any(x => x.ClassData.Id == p.Classid)).ToList();

                    var listStudent = new List<StudentInHomeRoom>();
                    foreach (var studentXml in studentByClassid)
                    {
                        var binusianid = studentXml.Name.Split("-").FirstOrDefault().Trim();
                        var checkStudent = StudentFormMaster.Where(p => p.BinusianId == binusianid
                                                                    && p.IdGrade.Any(x => x == dataClass.Grade.IdFromMasterData))
                                                                    .FirstOrDefault();
                        if (checkStudent != null)
                        {
                            listStudent.Add(new StudentInHomeRoom()
                            {
                                IdStudent = checkStudent.IdStudent,
                                StudentName = checkStudent.FullName,
                                Religion = checkStudent.Religion,
                                SubjectReligion = checkStudent.ReligionSubject,
                                Gender = checkStudent.Gender,
                                LastPathway = "",
                                LastYearsRegister = "",
                                NextPathway = "",
                                StudentIsreadyFromMaster = true,
                            });
                        }
                        else
                        {
                            listStudent.Add(new StudentInHomeRoom()
                            {
                                IdStudent = studentXml.Id,
                                StudentName = studentXml.Name,
                                Religion = "",
                                SubjectReligion = "",
                                Gender = "",
                                LastPathway = "",
                                LastYearsRegister = "",
                                NextPathway = "",
                                StudentIsreadyFromMaster = false,
                            });
                        }

                    }

                    dataClass.StudentInHomeRoom = listStudent;
                    dataClass.MaleAndFemale = $"{listStudent.Count(x => x.Gender == "Male")}/{listStudent.Count(x => x.Gender == "Female")}/{listStudent.Count(x => x.Gender != "Male" && x.Gender != "Female")}";
                    dataClass.Reqion = $"{listStudent.Count(x => x.Religion == "Buddhist")}/{listStudent.Count(x => x.Religion == "Catholic")}/" +
                                       $"{listStudent.Count(x => x.Religion == "Christian")}/{listStudent.Count(x => x.Religion == "Islam")}/" +
                                       $"{listStudent.Count(x => x.Religion == "Hindu")}/" +
                                       $"{listStudent.Count(x => string.IsNullOrEmpty(x.Religion) && x.Religion != "Buddhist" && x.Religion != "Catholic" && x.Religion != "Christian" && x.Religion != "Islam" && x.Religion != "Hindu")}";

                    #endregion

                    dataClass.IsDataReadyFromMaster = dataClass.StudentInHomeRoom.TrueForAll(p => p.StudentIsreadyFromMaster) && dataClass.Pathway.TrueForAll(p => p.DataIsUseInMaster == true) && dataClass.Class.DataIsUseInMaster && dataClass.Venue.DataIsUseInMaster;

                    result.Add(dataClass);
                }
            }

            return result;
        }


        /// <summary>
        /// get data lesson dari file xml 
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="session"></param>
        /// <param name="IdSchool"></param>
        /// <param name="classData"></param>
        /// <param name="isGenerateClass"></param>
        /// <param name="gradeCode"></param>
        /// <param name="formatClassId"></param>
        /// <returns></returns>
        public async Task<List<LessonUploadXMLVm>> GetReadAndValidateLesson(Timetable2019 timetable2019,
                                                                            List<GradePathwayForAscTimeTableResult> session,
                                                                            string IdSchool,
                                                                            List<ClassPackageVm> classData,
                                                                            bool isGenerateClass,
                                                                            List<string> gradeCode,
                                                                            string formatClassId,
                                                                            List<CheckTeacherForAscTimetableResult> TeacherFromMaster)
        {
            var result = new List<LessonUploadXMLVm>();
            //get lesson data
            var tempIncrement = new List<TempModelIncrement>();
            var subjectFromMaster = new List<GetSubjectPathwayForAscTimetableResult>();

            var query =
                      (from lesson in timetable2019.Lessons.Lesson
                       join subject in timetable2019.Subjects.Subject on lesson.Subjectid equals subject.Id
                       join weeks in timetable2019.Weeksdefs.Weeksdef on lesson.Weeksdefid equals weeks.Id
                       where lesson.Classids.Split(",").Any(p => classData.Any(x => x.IdClassFromXMLS.Any(s => s == p)))
                       select new { Lesson = lesson, Subject = subject, Weeks = weeks }).ToList();

            //grup by classid ,subject id ,period perweek,period percard ,
            //data lesson di group by 
            var dataXmlLesson = query.GroupBy(p => new { p.Lesson.Classids, p.Lesson.Subjectid }).ToList();

            //if(format inisial teacher){}

            if (dataXmlLesson.Count() > 0)
            {
                var getCodeSubject = dataXmlLesson.Where(p => p.Any(x => x.Subject != null)).SelectMany(p => p.Select(x => x.Subject.Short)).Distinct().ToList();
                var getCodegrade = dataXmlLesson.Where(p => p.Any(x => x.Subject != null)).SelectMany(p => p.Select(x => x.Subject.GetGradeCode())).Distinct().ToList();

                if (getCodeSubject.Count() > 0 && getCodegrade.Count > 0)
                {
                    var getSubject = await _subjectService.Execute.GetSubjectByCodeAndSchool(new GetSubjectPathwayForAscTimetableRequest
                    {
                        SubjectCode = getCodeSubject,
                        IdSchool = IdSchool,
                        GradeCode = getCodegrade,
                    });

                    if (!getSubject.IsSuccess)
                        throw new BadRequestException("Error Subject With Message: " + getSubject.Message);

                    subjectFromMaster = getSubject.Payload;
                }


                var countLesson = 0;
                foreach (var item in dataXmlLesson)
                {
                    var lessonData = new LessonUploadXMLVm();
                    countLesson++;
                    //cek data grade
                    var checkGrade = classData.Where(p => p.IdClassFromXMLS.Any(x => item.Key.Classids.Split(",").Any(o => o == x))).ToList();
                    //end cek data subject
                    //add grade
                    lessonData.Grade = checkGrade != null ? checkGrade.Any(x => !string.IsNullOrEmpty(x.Grade.Code)) ? checkGrade.Where(x => !string.IsNullOrEmpty(x.Grade.Code)).Select(x => x.Grade).FirstOrDefault() : new GradeClassVM() : new GradeClassVM();

                    //cek data subject
                    var checkSubject = subjectFromMaster.Where(p => p.SubjectCode == item.FirstOrDefault().Subject?.Short && p.IdGrade == lessonData.Grade.IdFromMasterData).FirstOrDefault();

                    var subjectStatus = checkSubject != null ? true : false;
                    //end cek data subject

                    lessonData.No = countLesson.ToString("D4");
                    lessonData.IdForSave = Guid.NewGuid().ToString();
                    lessonData.LessonIds = item.Select(p => p.Lesson.Id).ToList();
                    lessonData.Subject = new SubjectLesson()
                    {
                        Id = item.FirstOrDefault().Subject.Id,
                        Code = checkSubject != null ? checkSubject.SubjectCode : item.FirstOrDefault().Subject.Short,
                        Description = checkSubject != null ? checkSubject.SubjectDescription : item.FirstOrDefault().Subject.Name,
                        DataIsUseInMaster = checkSubject != null ? true : false,
                        IdFromMasterData = checkSubject != null ? checkSubject.IdSubject : "",
                        SubjectLevel = checkSubject != null ? checkSubject.SubjectLevel.Select(p => new SubjectLevelLesson
                        {
                            SubjectLevelId = p.IdSubjectLevel,
                            SubjectLevelName = p.SubjectlevelDesc,
                            IsDefault = p.IsDefault,
                        }).ToList() : new List<SubjectLevelLesson>(),
                    };
                    // lessonData.Grade = item.FirstOrDefault().Classname.Grade;
                    lessonData.WeekCode = item.FirstOrDefault().Weeks.Short;
                    lessonData.TotalWeek = (int)item.Sum(p => Convert.ToDouble(p.Lesson.Periodsperweek));

                    //check add class for adding data 
                    #region proses data Class
                    var spliterDataLessonClass = item.Key.Classids.Split(",").ToList();
                    if (spliterDataLessonClass.Count > 0)
                    {
                        if (classData.Count > 0)
                        {
                            foreach (var itemLessonClass in spliterDataLessonClass)
                            {

                                var data = classData.Where(p => p.IdClassFromXMLS.Any(x => x == itemLessonClass)).FirstOrDefault();
                                if (data != null)
                                {
                                    var classLesson = new ClassLesonVm();

                                    classLesson.IdClassFromXmls = data.IdClassFromXMLS;
                                    classLesson.IdClassFromMaster = data.Class.IdFromMasterData;
                                    classLesson.IdClassForSave = data.IdClassForSave;
                                    classLesson.ClassCode = data.Class.Code;
                                    classLesson.ClassDescription = data.Class.Description;
                                    classLesson.ClassCombination = data.Class.ClassCombination;
                                    classLesson.IsClassReadyFromMaster = data.IsDataReadyFromMaster;
                                    classLesson.PathwayLesson = data.Pathway.Select(p => new PathwayLesson
                                    {
                                        IdPathwayFromMaster = p.IdFromMasterData,
                                        PathwayCode = p.Code,
                                        PathwayDescription = p.Description,
                                        IsPathwayReadyFromMaster = p.DataIsUseInMaster,
                                    }).ToList();

                                    classLesson.IsDataReadyFromMaster = data.IsDataReadyFromMaster;

                                    lessonData.Class.Add(classLesson);
                                }
                            }
                        }
                    }

                    lessonData.Class = lessonData.Class.OrderBy(p => p.ClassCode)
                                                       .GroupBy(p => p.ClassCombination)
                                                       .Select(p => new ClassLesonVm
                                                       {
                                                           IdClassFromXmls = p.FirstOrDefault().IdClassFromXmls,
                                                           IdClassFromMaster = p.FirstOrDefault().IdClassFromMaster,
                                                           IdClassForSave = p.FirstOrDefault().IdClassForSave,
                                                           ClassCode = p.FirstOrDefault().ClassCode,
                                                           ClassCombination = p.FirstOrDefault().ClassCombination,
                                                           ClassDescription = p.FirstOrDefault().ClassDescription,
                                                           IsClassReadyFromMaster = p.FirstOrDefault().IsClassReadyFromMaster,
                                                           PathwayLesson = p.FirstOrDefault().PathwayLesson,
                                                           IsDataReadyFromMaster = p.FirstOrDefault().IsDataReadyFromMaster,
                                                       })
                                                       .ToList();

                    #endregion

                    #region Proses Teacher 
                    //get data teacher 
                    var getTeacher = item.Select(p => p.Lesson.Teacherids).ToList();
                    getTeacher = getTeacher.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();

                    if (getTeacher.Count > 0)
                    {

                        foreach (var itemTeacher in getTeacher)
                        {
                            var spliterTeacher = itemTeacher.Split(",").ToList();
                            foreach (var itemTeacherSpliter in spliterTeacher)
                            {
                                var getdataFromXml = timetable2019.Teachers.Teacher.Where(p => p.Id == itemTeacherSpliter).FirstOrDefault();
                                if (getdataFromXml != null)
                                {
                                    //api data teacher
                                    var dataTeacher = TeacherFromMaster.Where(p => p.TeacherShortName == getdataFromXml.Short ||
                                                                       p.TeacherBinusianId == getdataFromXml.Short ||
                                                                       p.TeacherInitialName == getdataFromXml.Short).FirstOrDefault();

                                    var dataTeacherS = new DataModelGeneral();
                                    dataTeacherS.Id = getdataFromXml.Id;
                                    dataTeacherS.Description = dataTeacher != null ? dataTeacher.TeacherName : getdataFromXml.Name;
                                    dataTeacherS.Code = dataTeacher != null ? dataTeacher.TeacherInitialName : getdataFromXml.Short;
                                    dataTeacherS.DataIsUseInMaster = dataTeacher != null ? true : false;
                                    dataTeacherS.IdFromMasterData = dataTeacher != null ? dataTeacher.IdTeacher : "";

                                    lessonData.Teacher.Add(dataTeacherS);
                                }
                            }

                        }
                    }

                    lessonData.Teacher = lessonData.Teacher.OrderBy(p => p.Description).GroupBy(p => p.Id).Select(p => p.FirstOrDefault()).ToList();
                    #endregion

                    #region Proses class Id Lesson
                    //membuat class id dari format
                    if (isGenerateClass)
                    {
                        if (gradeCode.Any(p => p == lessonData.Grade.Code))
                        {
                            var increment = 0;
                            if (formatClassId.Contains("[Subject]") && formatClassId.Contains("AutoIncrement"))
                            {
                                var getIncrement = tempIncrement.Where(p => p.Key == lessonData.Subject.Code).LastOrDefault();
                                if (getIncrement == null)
                                {
                                    increment = 1;
                                    tempIncrement.Add(new TempModelIncrement()
                                    {
                                        Increment = increment,
                                        Key = lessonData.Subject.Code,
                                    });
                                }
                                else
                                {
                                    increment = getIncrement.Increment + 1;
                                    tempIncrement.Add(new TempModelIncrement()
                                    {
                                        Increment = increment,
                                        Key = lessonData.Subject.Code,
                                    });
                                }
                            }

                            lessonData.ClassId = GenerateClassid(formatClassId, increment == 0 ? "" : increment.ToString(),
                                                                 //countLesson,
                                                                 increment,
                                                                 string.Join("", lessonData.Class.Select(p => p.ClassCombination).ToList()),
                                                                 lessonData.Grade.Code,
                                                                 lessonData.Teacher.Count > 0 ? lessonData.Teacher.FirstOrDefault().Code : "",
                                                                 lessonData.Subject.Code);
                        }
                        else
                        {
                            lessonData.ClassId = item.FirstOrDefault().Subject.Name;
                        }
                    }
                    else
                    {
                        lessonData.ClassId = item.FirstOrDefault().Subject.Name;
                    }
                    #endregion

                    lessonData.IsDataReadyFromMaster = subjectStatus && lessonData.Class.TrueForAll(p => p.IsDataReadyFromMaster == true) && lessonData.Teacher.TrueForAll(p => p.DataIsUseInMaster == true);

                    result.Add(lessonData);
                }

                result = result.OrderBy(p => p.ClassId).ToList();
            }
            return result;

        }

        /// <summary>
        /// get data lesson dari file xml 
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="session"></param>
        /// <param name="IdSchool"></param>
        /// <param name="classData"></param>
        /// <param name="isGenerateClass"></param>
        /// <param name="gradeCode"></param>
        /// <param name="formatClassId"></param>
        /// <returns></returns>
        public async Task<List<LessonUploadXMLVm>> GetReadAndValidateLessonInisialTeacher(Timetable2019 timetable2019,
                                                                                            List<GradePathwayForAscTimeTableResult> session,
                                                                                            string IdSchool,
                                                                                            List<ClassPackageVm> classData,
                                                                                            bool isGenerateClass,
                                                                                            List<string> gradeCode,
                                                                                            string formatClassId,
                                                                                            List<CheckTeacherForAscTimetableResult> TeacherFromMaster)
        {
            var result = new List<LessonUploadXMLVm>();
            //get lesson data
            var tempIncrement = new List<TempModelIncrement>();
            var subjectFromMaster = new List<GetSubjectPathwayForAscTimetableResult>();
            var query =
                      (from lesson in timetable2019.Lessons.Lesson
                       join subject in timetable2019.Subjects.Subject on lesson.Subjectid equals subject.Id
                       join weeks in timetable2019.Weeksdefs.Weeksdef on lesson.Weeksdefid equals weeks.Id
                       where lesson.Classids.Split(",").Any(p => classData.Any(x => x.IdClassFromXMLS.Any(s => s == p)))
                       select new { Lesson = lesson, Subject = subject, Weeks = weeks }).ToList();

            //grup by classid ,subject id ,period perweek,period percard ,
            //data lesson di group by 
            var dataXmlLesson = query.OrderBy(e=>e.Lesson.Seminargroup).GroupBy(p => new { p.Lesson.Classids, p.Lesson.Subjectid, p.Lesson.Teacherids }).ToList();
           
            if (dataXmlLesson.Count() > 0)
            {
                var getCodeSubject = dataXmlLesson.Where(p => p.Any(x => x.Subject != null)).SelectMany(p => p.Select(x => x.Subject.Short)).Distinct().ToList();
                var getCodegrade = dataXmlLesson.Where(p => p.Any(x => x.Subject != null)).SelectMany(p => p.Select(x => x.Subject.GetGradeCode())).Distinct().ToList();

                if (getCodeSubject.Count() > 0 && getCodegrade.Count > 0)
                {
                    var getSubject = await _subjectService.Execute.GetSubjectByCodeAndSchool(new GetSubjectPathwayForAscTimetableRequest
                    {
                        SubjectCode = getCodeSubject,
                        IdSchool = IdSchool,
                        GradeCode = getCodegrade,
                    });

                    if (!getSubject.IsSuccess)
                        throw new BadRequestException("Error Subject With Message: " + getSubject.Message);

                    subjectFromMaster = getSubject.Payload;
                }


                var countLesson = 0;
                foreach (var item in dataXmlLesson)
                {
                    var lessonData = new LessonUploadXMLVm();
                    countLesson++;
                    //cek data grade
                    var checkGrade = classData.Where(p => p.IdClassFromXMLS.Any(x => item.Key.Classids.Split(",").Any(o => o == x))).ToList();
                    //end cek data subject
                    //add grade
                    lessonData.Grade = checkGrade != null ? checkGrade.Any(x => !string.IsNullOrEmpty(x.Grade.Code)) ? checkGrade.Where(x => !string.IsNullOrEmpty(x.Grade.Code)).Select(x => x.Grade).FirstOrDefault() : new GradeClassVM() : new GradeClassVM();

                    //cek data subject
                    var checkSubject = subjectFromMaster.Where(p => p.SubjectCode == item.FirstOrDefault().Subject?.Short && p.IdGrade == lessonData.Grade.IdFromMasterData).FirstOrDefault();

                    var subjectStatus = checkSubject != null ? true : false;
                    //end cek data subject

                    lessonData.No = countLesson.ToString("D4");
                    lessonData.IdForSave = Guid.NewGuid().ToString();
                    lessonData.LessonIds = item.Select(p => p.Lesson.Id).ToList();
                    lessonData.SeminarGroupId = item.Select(p => p.Lesson.Seminargroup).Distinct().FirstOrDefault();
                    lessonData.Subject = new SubjectLesson()
                    {
                        Id = item.FirstOrDefault().Subject.Id,
                        Code = checkSubject != null ? checkSubject.SubjectCode : item.FirstOrDefault().Subject.Short,
                        Description = checkSubject != null ? checkSubject.SubjectDescription : item.FirstOrDefault().Subject.Name,
                        DataIsUseInMaster = checkSubject != null ? true : false,
                        IdFromMasterData = checkSubject != null ? checkSubject.IdSubject : "",
                        SubjectLevel = checkSubject != null ? checkSubject.SubjectLevel.Select(p => new SubjectLevelLesson
                        {
                            SubjectLevelId = p.IdSubjectLevel,
                            SubjectLevelName = p.SubjectlevelDesc,
                            IsDefault = p.IsDefault,
                        }).ToList() : new List<SubjectLevelLesson>(),
                    };
                    // lessonData.Grade = item.FirstOrDefault().Classname.Grade;
                    lessonData.WeekCode = item.FirstOrDefault().Weeks.Short;
                    lessonData.TotalWeek = (int)item.Sum(p => Convert.ToDouble(p.Lesson.Periodsperweek));

                    //check add class for adding data 
                    #region proses data Class
                    var spliterDataLessonClass = item.Key.Classids.Split(",").ToList();
                    if (spliterDataLessonClass.Count > 0)
                    {
                        if (classData.Count > 0)
                        {
                            foreach (var itemLessonClass in spliterDataLessonClass)
                            {

                                var data = classData.Where(p => p.IdClassFromXMLS.Any(x => x == itemLessonClass)).FirstOrDefault();
                                if (data != null)
                                {
                                    var classLesson = new ClassLesonVm();

                                    classLesson.IdClassFromXmls = data.IdClassFromXMLS;
                                    classLesson.IdClassFromMaster = data.Class.IdFromMasterData;
                                    classLesson.IdClassForSave = data.IdClassForSave;
                                    classLesson.ClassCode = data.Class.Code;
                                    classLesson.ClassDescription = data.Class.Description;
                                    classLesson.ClassCombination = data.Class.ClassCombination;
                                    classLesson.IsClassReadyFromMaster = data.IsDataReadyFromMaster;
                                    classLesson.PathwayLesson = data.Pathway.Select(p => new PathwayLesson
                                    {
                                        IdPathwayFromMaster = p.IdFromMasterData,
                                        PathwayCode = p.Code,
                                        PathwayDescription = p.Description,
                                        IsPathwayReadyFromMaster = p.DataIsUseInMaster,
                                    }).ToList();

                                    classLesson.IsDataReadyFromMaster = data.IsDataReadyFromMaster;

                                    lessonData.Class.Add(classLesson);
                                }
                            }
                        }
                    }

                    lessonData.Class = lessonData.Class.OrderBy(p => p.ClassCode)
                                                       .GroupBy(p => p.ClassCombination)
                                                       .Select(p => new ClassLesonVm
                                                       {
                                                           IdClassFromXmls = p.FirstOrDefault().IdClassFromXmls,
                                                           IdClassFromMaster = p.FirstOrDefault().IdClassFromMaster,
                                                           IdClassForSave = p.FirstOrDefault().IdClassForSave,
                                                           ClassCode = p.FirstOrDefault().ClassCode,
                                                           ClassCombination = p.FirstOrDefault().ClassCombination,
                                                           ClassDescription = p.FirstOrDefault().ClassDescription,
                                                           IsClassReadyFromMaster = p.FirstOrDefault().IsClassReadyFromMaster,
                                                           PathwayLesson = p.FirstOrDefault().PathwayLesson,
                                                           IsDataReadyFromMaster = p.FirstOrDefault().IsDataReadyFromMaster,
                                                       })
                                                       .ToList();

                    #endregion

                    #region Proses Teacher 
                    //get data teacher 
                    var getTeacher = item.Select(p => p.Lesson.Teacherids).ToList();
                    getTeacher = getTeacher.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();

                    if (getTeacher.Count > 0)
                    {

                        foreach (var itemTeacher in getTeacher)
                        {
                            var spliterTeacher = itemTeacher.Split(",").ToList();
                            foreach (var itemTeacherSpliter in spliterTeacher)
                            {
                                var getdataFromXml = timetable2019.Teachers.Teacher.Where(p => p.Id == itemTeacherSpliter).FirstOrDefault();
                                if (getdataFromXml != null)
                                {
                                    //api data teacher
                                    var dataTeacher = TeacherFromMaster.Where(p => p.TeacherShortName == getdataFromXml.Short ||
                                                                       p.TeacherBinusianId == getdataFromXml.Short ||
                                                                       p.TeacherInitialName == getdataFromXml.Short).FirstOrDefault();

                                    var dataTeacherS = new DataModelGeneral();
                                    dataTeacherS.Id = getdataFromXml.Id;
                                    dataTeacherS.Description = dataTeacher != null ? dataTeacher.TeacherName : getdataFromXml.Name;
                                    dataTeacherS.Code = dataTeacher != null ? dataTeacher.TeacherInitialName : getdataFromXml.Short;
                                    dataTeacherS.DataIsUseInMaster = dataTeacher != null ? true : false;
                                    dataTeacherS.IdFromMasterData = dataTeacher != null ? dataTeacher.IdTeacher : "";

                                    lessonData.Teacher.Add(dataTeacherS);
                                }
                            }

                        }
                    }

                    lessonData.Teacher = lessonData.Teacher.GroupBy(p => p.Id).Select(p => p.FirstOrDefault()).ToList();
                    #endregion

                    #region Proses class Id Lesson
                    //membuat class id dari format
                    if (isGenerateClass)
                    {
                        if (gradeCode.Any(p => p == lessonData.Grade.Code))
                        {
                            var increment = 0;
                            if (formatClassId.Contains("[Subject]") && formatClassId.Contains("AutoIncrement"))
                            {
                                var getIncrement = tempIncrement.Where(p => p.Key == lessonData.Subject.Code).LastOrDefault();
                                if (getIncrement == null)
                                {
                                    increment = 1;
                                    tempIncrement.Add(new TempModelIncrement()
                                    {
                                        Increment = increment,
                                        Key = lessonData.Subject.Code,
                                    });
                                }
                                else
                                {
                                    increment = getIncrement.Increment + 1;
                                    tempIncrement.Add(new TempModelIncrement()
                                    {
                                        Increment = increment,
                                        Key = lessonData.Subject.Code,
                                    });
                                }
                            }

                            lessonData.ClassId = GenerateClassid(formatClassId, increment == 0 ? "" : increment.ToString(),
                                                                 //countLesson,
                                                                 increment,
                                                                 string.Join("", lessonData.Class.Select(p => p.ClassCombination).ToList()),
                                                                 lessonData.Grade.Code,
                                                                 lessonData.Teacher.Count > 0 ? lessonData.Teacher.FirstOrDefault().Code : "",
                                                                 lessonData.Subject.Code);
                        }
                        else
                        {
                            lessonData.ClassId = item.FirstOrDefault().Subject.Name;
                        }
                    }
                    else
                    {
                        lessonData.ClassId = item.FirstOrDefault().Subject.Name;
                    }
                    #endregion

                    lessonData.IsDataReadyFromMaster = subjectStatus && lessonData.Class.TrueForAll(p => p.IsDataReadyFromMaster == true) && lessonData.Teacher.TrueForAll(p => p.DataIsUseInMaster == true);

                    result.Add(lessonData);
                }

                result = result.OrderBy(p => p.ClassId).ToList();
            }
            return result;

        }

        /// <summary>
        /// function unutk get data schedule dari  file xml 
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="_lesson"></param>
        /// <param name="session"></param>
        /// <param name="IdSchool"></param>
        /// <param name="isCreateSessionFromXMl"></param>
        /// <param name="sessionFromXml"></param>
        /// <returns></returns>
        public async Task<List<ScheduleVm>> GetReadAndValidateSchedule(Timetable2019 timetable2019,
                                                                          List<LessonUploadXMLVm> _lesson,
                                                                          List<GetSessionAscTimetableResult> session,
                                                                          string IdSchool,
                                                                          bool isCreateSessionFromXMl,
                                                                          List<SessionSetFromXmlVm> sessionFromXml,
                                                                          List<CodeWithIdVm> DaysFromMaster)
        {
            var result = new List<ScheduleVm>();
            var venueFrommaster = new List<GetVenueForAscTimetableResult>();
            var classSchedule = (from sch in timetable2019.Cards.Card
                                     //join lesson in _lesson on sch.Lessonid equals lesson.LessonId
                                 join period in timetable2019.Periods.Period on sch.Period equals period._period
                                 join daysday in timetable2019.Daysdefs.Daysdef on sch.Days equals daysday.Days
                                 join week in timetable2019.Weeksdefs.Weeksdef on sch.Weeks equals week.Weeks
                                 join c in timetable2019.Classrooms.Classroom on sch.Classroomids.Split(",").FirstOrDefault() equals c.Id into clsRoom
                                 from classrooms in clsRoom.DefaultIfEmpty()
                                 where _lesson.Any(x => x.LessonIds.Any(y => y == sch.Lessonid))
                                 orderby daysday.Days descending
                                 select new { Schedule = sch, ClassRooms = classrooms, period = period, days = daysday, Weeks = week }).ToList();

            //di group by session=period
            var scheduleGroup = classSchedule.GroupBy(p => p.Schedule.Period).OrderBy(p => PadNumbers(p.Key)).ToList();

            if (scheduleGroup.Count() > 0)
            {
                //var getCodeVenue = scheduleGroup.Where(x => x.Any(y => y.ClassRooms != null)).SelectMany(p => p.Select(x => x.ClassRooms.Short)).Distinct().ToList();
                var getCodeVenue = scheduleGroup.SelectMany(p => p.Where(p => p.ClassRooms != null).Select(x => x.ClassRooms.Short)).Distinct().ToList();
                if (getCodeVenue.Count > 0)
                {
                    venueFrommaster = await _dbContext.Entity<MsVenue>()
                                                      .Include(p => p.Building)
                                                      .Where(p => getCodeVenue.Any(x => x == p.Code) && p.Building.IdSchool == IdSchool)
                                                      .Select(p => new GetVenueForAscTimetableResult
                                                      {
                                                          IdVenue = p.Id,
                                                          Code = p.Code,
                                                          Description = p.Description,
                                                          BuildingCode = p.Building.Code,
                                                          BuildingDescription = p.Building.Description,
                                                      }).ToListAsync();
                }

                if (scheduleGroup.Count > 0)
                {
                    foreach (var item in scheduleGroup)
                    {
                        var dataSchedule = new ScheduleVm();
                        dataSchedule.Session = item.Key;

                        var groupBydays = item.GroupBy(p => p.days).ToList();

                        foreach (var itemSchedule in DaysFromMaster.Where(p => p.Code != "000001" && p.Code != "0000001"))
                        {
                            var scheduleValue = new ScheduleValueVM();
                            scheduleValue.ListSchedule = new List<ScheduleDataVM>();
                            GetVenueForAscTimetableResult checkVanue = null;

                            //var daysId = GetCodeDays(itemSchedule.Key.Days);
                            //var getDays = DaysFromMaster.Where(p => p.Code == daysId).FirstOrDefault();
                            scheduleValue.Days = new ScheduleDayVM()
                            {
                                Id = itemSchedule != null ? itemSchedule.Id : "",
                                Code = itemSchedule != null ? itemSchedule.Code : "",
                                Description = itemSchedule != null ? itemSchedule.Description : "",
                                IdFromMasterData = itemSchedule != null ? itemSchedule.Id : "",
                                DataIsUseInMaster = itemSchedule != null ? true : false,
                            };

                            var datas = groupBydays.Where(p => GetCodeDays(p.Key.Days) == itemSchedule.Code).SelectMany(p => p.Where(x => x.Schedule.Period == item.Key).Select(p => p)).ToList();

                            foreach (var listSchedule in datas)
                            {
                                if (listSchedule.ClassRooms != null)
                                {
                                    checkVanue = venueFrommaster.Where(p => p.Code == listSchedule.ClassRooms.Short).FirstOrDefault();

                                }

                                TimeSpan strat = TimeSpan.Parse(listSchedule.period.Starttime);
                                TimeSpan end = TimeSpan.Parse(listSchedule.period.Endtime);
                                TimeSpan time = end - strat;
                                int minutes = time.Minutes;
                                var sessionSetIsready = false;

                                var getLesson = _lesson.Where(p => p.LessonIds.Any(x => x == listSchedule.Schedule.Lessonid)).FirstOrDefault();

                                var IdDataFromMaster = "";
                                var StartTime = "";
                                var EndTime = "";

                                if (!isCreateSessionFromXMl)
                                {
                                    var pathway = getLesson.Class.SelectMany(e => e.PathwayLesson.Select(e => e.PathwayDescription)).Distinct().ToList();

                                    // semua pathway memperlakukan jam yang sama setiap sesinya
                                    var checkDS = session.Where(p => p.Name == listSchedule.period.Name
                                                                     && p.Alias == listSchedule.period.Short
                                                                     && p.SessionId.ToString() == listSchedule.Schedule.Period
                                                                     //&& p.StartTime.ToString(@"h\:mm") == listSchedule.period.Starttime
                                                                     //&& p.EndTime.ToString(@"h\:mm") == listSchedule.period.Endtime
                                                                     && p.DaysCode == itemSchedule.Code
                                                                     //&& p.DurationInMinutes == minutes
                                                                     && p.Grade == getLesson.Grade.Description
                                                                     && pathway.Contains(p.Pathway))
                                                         .FirstOrDefault();

                                    //var checkDS = session.Where(p => p.Name == listSchedule.period.Name
                                    //                                 && p.Alias == listSchedule.period.Short
                                    //                                 && p.SessionId.ToString() == listSchedule.Schedule.Period
                                    //                                 && p.StartTime.ToString(@"h\:mm") == listSchedule.period.Starttime
                                    //                                 && p.EndTime.ToString(@"h\:mm") == listSchedule.period.Endtime
                                    //                                 && p.DaysCode == itemSchedule.Code
                                    //                                 && p.DurationInMinutes == minutes)
                                    //                     .FirstOrDefault();

                                    IdDataFromMaster = checkDS != null ? checkDS.Id : "";
                                    sessionSetIsready = checkDS != null ? true : false;
                                    StartTime = checkDS != null ? checkDS.StartTime.ToString() : "";
                                    EndTime = checkDS != null ? checkDS.EndTime.ToString() : "";
                                }
                                else
                                {
                                    //dari xml 
                                    var chekDSFromXML = sessionFromXml.Where(p => p.DaysCode == listSchedule.days.Days &&
                                                                                 p.SessionId == listSchedule.Schedule.Period &&
                                                                                 p.SessionAlias == listSchedule.period.Short &&
                                                                                 p.SessionName == listSchedule.period.Name &&
                                                                                 p.StartTime == listSchedule.period.Starttime &&
                                                                                 p.EndTime == listSchedule.period.Endtime &&
                                                                                 p.IdGradepathway == getLesson.Grade.IdGradePathway
                                                                                 ).FirstOrDefault();

                                    IdDataFromMaster = chekDSFromXML != null ? chekDSFromXML.IdFromXml : "";
                                    sessionSetIsready = chekDSFromXML != null ? true : false;
                                    StartTime = listSchedule.period.Starttime;
                                    EndTime = listSchedule.period.Endtime;
                                }

                                var LessonAsc = timetable2019.Lessons.Lesson.FirstOrDefault(x => x.Id == listSchedule.Schedule.Lessonid);
                                var teacherIds = timetable2019.Teachers.Teacher.Where(x => LessonAsc.Teacherids.Split(",").Contains(x.Id)).Select(x => x.Id).ToList();

                                var listTeacher = getLesson.Teacher.Where(x => teacherIds.Contains(x.Id)).ToList();

                                var DaysId = scheduleValue.Days.IdFromMasterData;
                                var ClassID = getLesson.ClassId;
                                var LesonId = getLesson.IdForSave;
                                var SubjectId = getLesson.Subject.IdFromMasterData;
                                var LessonIsReadyFromMaster = getLesson.IsDataReadyFromMaster;
                                var Weeks = listSchedule.Weeks.Short;
                                var Terms = listSchedule.Schedule.Terms;
                                var Venue = new DataModelGeneral()
                                {
                                    Id = checkVanue != null ? checkVanue.IdVenue : "",
                                    Code = checkVanue != null ? checkVanue.Code : listSchedule.ClassRooms?.Short,
                                    Description = checkVanue != null ? checkVanue.Description : listSchedule.ClassRooms?.Name,
                                    IdFromMasterData = checkVanue != null ? checkVanue.IdVenue : "",
                                    DataIsUseInMaster = checkVanue != null ? true : false,
                                };

                                var DataIsUseInMaster = sessionSetIsready && getLesson.IsDataReadyFromMaster && Venue.DataIsUseInMaster && scheduleValue.Days.DataIsUseInMaster;

                                foreach (var teacher in listTeacher)
                                {
                                    var modelList = new ScheduleDataVM 
                                    {
                                        IdDataFromMaster = IdDataFromMaster,
                                        StartTime = StartTime,
                                        EndTime = EndTime,
                                        DaysId= DaysId,
                                        ClassID = ClassID,
                                        LesonId = LesonId,
                                        SubjectId = SubjectId,
                                        LessonIsReadyFromMaster = LessonIsReadyFromMaster,
                                        Weeks = Weeks,
                                        Terms = Terms,
                                        Venue = Venue,
                                        DataIsUseInMaster = DataIsUseInMaster,
                                        Teacher = teacher,
                                    };

                                    scheduleValue.ListSchedule.Add(modelList);
                                }

                            }
                            dataSchedule.Schedule.Add(scheduleValue);
                        }
                        result.Add(dataSchedule);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// proses tag enrolment
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="classPackage"></param>
        /// <param name="idSchool"></param>
        /// <param name="StudentFormMaster"></param>
        /// <param name="lessonData"></param>
        /// <returns></returns>
        public EnrollmentDataVM GetReadAndValidateEnrollment(Timetable2019 timetable2019,
                                                                           List<ClassPackageVm> classPackage,
                                                                           string idSchool,
                                                                           List<GetStudentUploadAscResult> StudentFormMaster,
                                                                           List<LessonUploadXMLVm> lessonData)
        {
            var result = new EnrollmentDataVM();
            var EnrolemntStudent = (from stdn in timetable2019.Students.Student
                                        //join classIds in classPackage on stdn.Classid equals classIds.IdClassFromXML //into ck
                                        //from classObj in ck.DefaultIfEmpty()
                                    join subjetStudent in timetable2019.Studentsubjects.Studentsubject on stdn.Id equals subjetStudent.Studentid into stdnSub
                                    from subjectStdn in stdnSub.DefaultIfEmpty()
                                    join s in timetable2019.Subjects.Subject on new { SUB = subjectStdn?.Subjectid } equals new { SUB = s.Id } into sbk
                                    from subject in sbk.DefaultIfEmpty()
                                    where lessonData.Any(x => x.Class.Any(y => y.IdClassFromXmls.Any(z => z == stdn.Classid)))
                                    select new { Student = stdn, SubjectStudent = subjectStdn, Subject = subject }).ToList();

            var getdataEnrolment = EnrolemntStudent.Where(p => p.Subject != null).GroupBy(p => p.Student.Id).ToList();


            if (getdataEnrolment.Count > 0)
            {
                var countEnrolement = 0;
                var getGradeClass = lessonData.Select(p => p.Grade.Code).ToList();
                foreach (var item in getdataEnrolment)
                {

                    countEnrolement++;
                    var dataEnrollment = new EnrollmentVm();
                    dataEnrollment.EnrollmentStudent = new List<EnrollmentStudentVM>();
                    //get student 
                    var binusinId = item.FirstOrDefault().Student.Name.Split("-").FirstOrDefault();
                    var checkStudent = StudentFormMaster.Where(p => p.BinusianId == binusinId)
                                                      .FirstOrDefault();
                    if (checkStudent != null)
                    {
                        //get class
                        var getClass = classPackage.Where(p => p.IdClassFromXMLS.Any(x => x == item.FirstOrDefault().Student.Classid)).FirstOrDefault();

                        dataEnrollment.Student = new StudentEnrollmentVM()
                        {
                            IdFromXml = checkStudent != null ? checkStudent.IdStudent : "",
                            StudentName = checkStudent != null ? checkStudent.FullName : "",
                            BinusianId = checkStudent != null ? checkStudent.BinusianId : "",
                            Religion = checkStudent != null ? checkStudent.Religion : "",
                            DataIsUseInMaster = checkStudent != null ? true : false,
                            Grade = getClass != null ? getClass.Grade : new DataModelGeneral(),
                            Class = getClass != null ? getClass.Class.Code : "",
                            ClassCombination = getClass != null ? getClass.Class.ClassCombination : "",
                        };

                        //get leson 
                        var listSubjectStudent = item.Select(e => e.SubjectStudent).ToList();

                        var seminarGroupIsNull = listSubjectStudent.Where(e => !string.IsNullOrEmpty(e.Seminargroup)).Any();

                        if (seminarGroupIsNull)
                        {
                            foreach (var itemSubjectStudent in listSubjectStudent)
                            {
                                var EnrollmentStudent = lessonData.Where(p => p.Class.Any(x => x.IdClassFromXmls.Any(y => y == item.FirstOrDefault().Student.Classid) && p.SeminarGroupId == itemSubjectStudent.Seminargroup && p.Subject.Id == itemSubjectStudent.Subjectid) &&
                                item.Any(y => y.Subject.Id == p.Subject.Id) && p.IsDataReadyFromMaster)
                                    .Select(p => new EnrollmentStudentVM
                                    {
                                        ClassId = p.ClassId,
                                        LessonId = p.IdForSave,
                                        SubjectID = p.Subject.IdFromMasterData,
                                        SubjectLevelID = GetSubejctLevel(p.Subject.SubjectLevel),
                                        DataIsUseInMaster = p.IsDataReadyFromMaster,
                                        IsDataReadyFromMaster = p.IsDataReadyFromMaster && dataEnrollment.Student.DataIsUseInMaster,
                                    })
                                .ToList();

                                dataEnrollment.EnrollmentStudent.AddRange(EnrollmentStudent);
                            }

                        }
                        else
                        {
                            dataEnrollment.EnrollmentStudent = lessonData.Where(p => p.Class.Any(x => x.IdClassFromXmls.Any(y => y == item.FirstOrDefault().Student.Classid)) &&
                                item.Any(y => y.Subject.Id == p.Subject.Id) && p.IsDataReadyFromMaster)
                                .Select(p => new EnrollmentStudentVM
                                {
                                    ClassId = p.ClassId,
                                    LessonId = p.IdForSave,
                                    SubjectID = p.Subject.IdFromMasterData,
                                    SubjectLevelID = GetSubejctLevel(p.Subject.SubjectLevel),
                                    DataIsUseInMaster = p.IsDataReadyFromMaster,
                                    IsDataReadyFromMaster = p.IsDataReadyFromMaster && dataEnrollment.Student.DataIsUseInMaster,
                                }).ToList();
                        }

                        result.EnrollmentData.Add(dataEnrollment);
                    }

                }


                var colums = result.EnrollmentData.SelectMany(p => p.EnrollmentStudent).GroupBy(p => p.ClassId).ToList();
                var enrolmentColums = new List<ColumsStudentEnrollment>();
                enrolmentColums.AddRange(new List<ColumsStudentEnrollment>
                {
                    new ColumsStudentEnrollment
                    {
                         ColumsName="Student",
                    },
                    new ColumsStudentEnrollment
                    {
                         ColumsName="Religion",
                    }
                });

                foreach (var item in colums)
                {
                    var itemcolums = new ColumsStudentEnrollment();
                    itemcolums.ColumsName = item.Key;
                    enrolmentColums.Add(itemcolums);
                }

                result.EnrollmentSubject = colums.Select(x => new EnrollmentSubjectVm
                {
                    ClassId = x.Key
                }).ToList();

                result.EnrollmentColums = enrolmentColums;
            }

            return result;

        }

        private string GetSubejctLevel(List<SubjectLevelLesson> subjectLevels)
        {
            var result = string.Empty;
            if (subjectLevels.Count > 0)
            {
                var getdata = subjectLevels.Where(p => p.IsDefault).FirstOrDefault();
                if (getdata != null)
                {
                    return getdata.SubjectLevelId;
                }
                else
                {
                    return subjectLevels.Select(p => p.SubjectLevelId).FirstOrDefault();
                }
            }
            return result;
        }


        /// <summary>
        /// read file xml tag card for days structure
        /// </summary>
        /// <param name="timetable2019"></param>
        /// <param name="_lesson"></param>
        /// <param name="session"></param>
        /// <param name="idSchool"></param>
        /// <param name="isCreateSessionFromXMl"></param>
        /// <param name="sessionFromXml"></param>
        /// <returns></returns>
        public List<DayStructureVm> GetReadAndValidateDaysStructure(Timetable2019 timetable2019,
                                                                        List<LessonUploadXMLVm> _lesson,
                                                                        List<GetSessionAscTimetableResult> session,
                                                                        string idSchool,
                                                                        bool isCreateSessionFromXMl,
                                                                        List<SessionSetFromXmlVm> sessionFromXml)
        {
            var result = new List<DayStructureVm>();
            var dataDayStructure = (from card in timetable2019.Cards.Card
                                    join p in timetable2019.Periods.Period on card.Period equals p._period into pk
                                    from period in pk.DefaultIfEmpty()
                                    join d in timetable2019.Daysdefs.Daysdef on card.Days equals d.Days into dk
                                    from days in dk.DefaultIfEmpty()
                                        // join lesson in _lesson on card.Lessonid equals lesson.LessonId
                                    select new { Card = card, Period = period, Days = days }).ToList();

            if (dataDayStructure.Count > 0)
            {
                var countDyStructure = 0;
                foreach (var item in dataDayStructure)
                {
                    var getLesson = _lesson.Where(p => p.LessonIds.Any(x => x == item.Card.Lessonid)).FirstOrDefault();
                    if (getLesson != null)
                    {
                        countDyStructure++;
                        var dataDS = new DayStructureVm();

                        TimeSpan strat = TimeSpan.Parse(item.Period.Starttime);
                        TimeSpan end = TimeSpan.Parse(item.Period.Endtime);
                        TimeSpan time = end - strat;
                        int minutes = time.Minutes;
                        var daysId = GetCodeDays(item.Days.Days);


                        if (!isCreateSessionFromXMl)
                        {
                            var checkDS = session.Where(p => p.Name == item.Period.Name
                                                             && p.Alias == item.Period.Short
                                                             && p.SessionId.ToString() == item.Period._period
                                                             && p.StartTime.ToString(@"h\:mm") == item.Period.Starttime
                                                             && p.EndTime.ToString(@"h\:mm") == item.Period.Endtime
                                                             && p.DaysCode == daysId
                                                             && p.DurationInMinutes == minutes)
                                                 .FirstOrDefault();


                            dataDS.IdSessionFormMaster = checkDS != null ? checkDS.Id : "";
                            dataDS.IsDataReadyFromMaster = checkDS != null ? true : false;
                        }
                        else
                        {
                            //dari xml 
                            var chekDSFromXML = sessionFromXml.Where(p => p.DaysCode == item.Days.Days &&
                                                                         p.SessionId == item.Period._period &&
                                                                         p.SessionName == item.Period.Name &&
                                                                         p.StartTime == item.Period.Starttime &&
                                                                         p.EndTime == item.Period.Endtime
                                                                         ).FirstOrDefault();

                            dataDS.IdSessionFormMaster = chekDSFromXML != null ? chekDSFromXML.IdFromXml : "";
                            dataDS.IsDataReadyFromMaster = chekDSFromXML != null ? true : false;
                        }



                        dataDS.No = countDyStructure.ToString("D4");
                        dataDS.Grade = getLesson != null ? getLesson.Grade.Code : "";
                        dataDS.Pathway = getLesson != null ? string.Join(",", getLesson.Class.SelectMany(x => x.PathwayLesson.Select(y => y.PathwayCode)).Distinct().ToList()) : "";
                        dataDS.SchoolDay = item.Days.Name;
                        dataDS.SessionID = item.Period._period;
                        dataDS.SessionAlias = item.Period.Short;
                        dataDS.SessionName = item.Period.Name;
                        dataDS.StartDate = item.Period.Starttime;
                        dataDS.EndDate = item.Period.Endtime;
                        dataDS.DurationInMinute = minutes.ToString();
                        dataDS.IsDataReadyFromMaster = dataDS.IsDataReadyFromMaster;

                        result.Add(dataDS);
                    }
                }
            }

            return result;

        }

        //helper order by streang 
        public string PadNumbers(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
            }
            return "";
        }

    }

}


