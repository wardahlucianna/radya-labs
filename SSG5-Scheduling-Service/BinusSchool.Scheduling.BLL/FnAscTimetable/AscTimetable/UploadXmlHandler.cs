using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel.XML2012Type;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Helpers;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class UploadXmlHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IApiService<ISubject> _subjectService;
        private readonly IApiService<IStudent> _studentServices;
        private readonly IApiService<IStaff> _staffServices;
        public UploadXmlHandler(ISchedulingDbContext dbContext,
            IApiService<ISubject> subjectService,
            IApiService<IStudent> studentServices,
            IApiService<IStaff> staffServices)
        {
            _dbContext = dbContext;
            _subjectService = subjectService;
            _studentServices = studentServices;
            _staffServices = staffServices;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            _subjectService.SetConfigurationFrom(ApiConfiguration);
            _studentServices.SetConfigurationFrom(ApiConfiguration);
            _staffServices.SetConfigurationFrom(ApiConfiguration);
            var _helper = new HelperXML(_dbContext, _subjectService);

            var check = await _helper.CheckVersionXmlHelpers(Request);
            if (string.IsNullOrWhiteSpace(check))
            {
                throw new BadRequestException("Version For XML file Not Suport for this system!");
            }
            else if (check != "aSc Timetables 2012 XML")
            {
                throw new BadRequestException("Version For XML file Not Suport for this system!");
            }

            var result = new UploadXmlFileResult();
            var body = await Request.ValidateBodyForm<AscTimeTableUploadXmlRequest, UploadXmlValidator>();

#if DEBUG
            //body.CodeGradeForAutomaticGenerateClassId = new List<string>()
            //{
            //    "43ac9e54-1e95-4061-96ec-3dd1acd66717"
            //};
            //body.IdGradepathwayforCreateSession = new List<string>()
            //{
            //    "d33e7fc1-a155-4aad-8f66-5c71913fd87e"
            //};
#endif

            var file = Request.Form.Files["file"];
            var contents = "";

            ///Read file XML 
            using (StreamReader streamReader = new StreamReader(file.OpenReadStream()))
            {
                contents = await streamReader.ReadToEndAsync();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Timetable2019));
            StringReader stringReader = new StringReader(contents);
            var _ascTimetable = ((Timetable2019)serializer.Deserialize(stringReader));
            var data = await ProccesXMLFormat(_ascTimetable, body, _helper);
            result = data;

            return Request.CreateApiResult2(result as object);
        }

        /// <summary>
        /// eksekusi xml format aSc Timetables 2012 XML
        /// </summary>
        /// <returns></returns>
        public async Task<UploadXmlFileResult> ProccesXMLFormat(Timetable2019 timetable2019, AscTimeTableUploadXmlRequest model, HelperXML _helper)
        {

            var resultData = new UploadXmlFileResult();
            var session = new List<GetSessionAscTimetableResult>();
            resultData.Name = model.Name;
            resultData.IdSchool = model.IdSchool;
            resultData.IdSessionSet = model.IdSessionSet;
            resultData.IsCreateSessionSetFromXml = model.IsCreateSessionSetFromXml;
            resultData.IdGradepathwayforCreateSession = model.IdGradepathwayforCreateSession;
            resultData.SessionSetName = model.SessionSetName;
            resultData.IdSchoolAcademicyears = model.IdSchoolAcademicyears;
            resultData.FormatIdClass = model.FormatIdClass;
            resultData.CodeGradeForAutomaticGenerateClassId = model.CodeGradeForAutomaticGenerateClassId;
            resultData.AutomaticGenerateClassId = model.AutomaticGenerateClassId;

            var gradeCode = new List<string>();
            if (resultData.AutomaticGenerateClassId)
            {
                gradeCode = await _dbContext.Entity<MsGrade>()
                                            .Where(p => resultData.CodeGradeForAutomaticGenerateClassId.Any(x => x == p.Id))
                                            .Select(p => p.Code).ToListAsync(CancellationToken);
            }

            var gradeByGradePathway = new List<GradePathwayForAscTimeTableResult>();

            if (!model.IsCreateSessionSetFromXml)
            {
                session = await _dbContext.Entity<MsSession>()
                                          .Include(p => p.GradePathway).ThenInclude(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway)
                                          .Include(p => p.GradePathway).ThenInclude(p => p.Grade).Include(p => p.Day)
                                          .Where(p => p.IdSessionSet == model.IdSessionSet)
                                          .Select(p => new GetSessionAscTimetableResult
                                          {
                                              Id = p.Id,
                                              Name = p.Name,
                                              Alias = p.Alias,
                                              DaysCode = p.Day.Code,
                                              DaysName = p.Day.Description,
                                              SessionId = p.SessionID,
                                              StartTime = p.StartTime,
                                              EndTime = p.EndTime,
                                              DurationInMinutes = p.DurationInMinutes,
                                              Pathway = string.Join("-", p.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description)),
                                              Grade = p.GradePathway.Grade.Description,
                                          }).ToListAsync();
            }

            //get grdae class pathway mapping 
            if (model.IsCreateSessionSetFromXml)
            {
                gradeByGradePathway = await _dbContext.Entity<MsGradePathway>()
                                                 .Include(p => p.Sessions).ThenInclude(p => p.Day)
                                                 .Include(p => p.Grade.Level.AcademicYear)
                                                 .Include(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway.AcademicYear)
                                                 .Include(p => p.MsGradePathwayClassrooms).ThenInclude(p => p.Classroom)
                                                 .Include(p => p.MsGradePathwayClassrooms).ThenInclude(p => p.MsGradePathway.Grade)
                                                 .Where(p => model.IdGradepathwayforCreateSession.Any(x => x == p.Id) && p.Grade.Level.IdAcademicYear == model.IdSchoolAcademicyears)
                                                 .Select(p => new GradePathwayForAscTimeTableResult
                                                 {
                                                     IdGradePathway = p.Id,
                                                     IdGrade = p.Grade.Id,
                                                     GradeCode = p.Grade.Code,
                                                     GradeDescription = p.Grade.Description,
                                                     GradePathwayClassRooms = p.MsGradePathwayClassrooms.Count() > 0 ? p.MsGradePathwayClassrooms.Select(x => new GradePathwayClassRoom
                                                     {
                                                         IdClassRoom = x.Classroom.Id,
                                                         IdGradePathwayClassrom = x.Id,
                                                         ClassRoomCode = x.Classroom.Code,
                                                         ClassRoomDescription = x.Classroom.Description,
                                                         ClassRoomCombinationGrade = x.MsGradePathway.Grade.Code + x.Classroom.Code,
                                                         IdSchool = x.Classroom.IdSchool,
                                                     }).ToList() : new List<GradePathwayClassRoom>(),
                                                     GradePathwayDetails = p.GradePathwayDetails.Count() > 0 ? p.GradePathwayDetails.Select(x => new GradePathwayDetail
                                                     {
                                                         IdGradePathwayDetail = x.Id,
                                                         IdPathway = x.Pathway.Id,
                                                         PathwayCode = x.Pathway.Code,
                                                         PathwayDescription = x.Pathway.Description,
                                                         IdSchool = x.Pathway.AcademicYear.IdSchool,
                                                     }).ToList() : new List<GradePathwayDetail>(),
                                                 })
                                                 .ToListAsync();

            }
            else
            {
                gradeByGradePathway = await _dbContext.Entity<MsGradePathway>()
                                                 .Include(p => p.Sessions).ThenInclude(p => p.Day)
                                                 .Include(p => p.Grade.Level.AcademicYear)
                                                 .Include(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway.AcademicYear)
                                                 .Include(p => p.MsGradePathwayClassrooms).ThenInclude(p => p.Classroom)
                                                 .Include(p => p.MsGradePathwayClassrooms).ThenInclude(p => p.MsGradePathway.Grade)
                                                 .Where(p => p.Sessions.Any(x => x.IdSessionSet == model.IdSessionSet) && p.Grade.Level.IdAcademicYear == model.IdSchoolAcademicyears)
                                                 .Select(p => new GradePathwayForAscTimeTableResult
                                                 {
                                                     IdGradePathway = p.Id,
                                                     IdGrade = p.Grade.Id,
                                                     GradeCode = p.Grade.Code,
                                                     GradeDescription = p.Grade.Description,
                                                     GradePathwayClassRooms = p.MsGradePathwayClassrooms.Count() > 0 ? p.MsGradePathwayClassrooms.Select(x => new GradePathwayClassRoom
                                                     {
                                                         IdClassRoom = x.Classroom.Id,
                                                         IdGradePathwayClassrom = x.Id,
                                                         ClassRoomCode = x.Classroom.Code,
                                                         ClassRoomDescription = x.Classroom.Description,
                                                         ClassRoomCombinationGrade = x.MsGradePathway.Grade.Code + x.Classroom.Code,
                                                         IdSchool = x.Classroom.IdSchool,
                                                     }).ToList() : new List<GradePathwayClassRoom> { new GradePathwayClassRoom {
                                                         IdClassRoom = null,
                                                         IdGradePathwayClassrom = null,
                                                         ClassRoomCode = null,
                                                         ClassRoomDescription = null,
                                                         ClassRoomCombinationGrade =  p.Grade.Code,
                                                         IdSchool = p.Grade.Level.AcademicYear.IdSchool,
                                                     } },
                                                     GradePathwayDetails = p.GradePathwayDetails.Count() > 0 ? p.GradePathwayDetails.Select(x => new GradePathwayDetail
                                                     {
                                                         IdGradePathwayDetail = x.Id,
                                                         IdPathway = x.Pathway.Id,
                                                         PathwayCode = x.Pathway.Code,
                                                         PathwayDescription = x.Pathway.Description,
                                                         IdSchool = x.Pathway.AcademicYear.IdSchool,
                                                     }).ToList() : new List<GradePathwayDetail>(),
                                                 })
                                              .ToListAsync();

                resultData.IdGradepathwayforCreateSession = gradeByGradePathway.Select(p => p.IdGradePathway).ToList();
            }


            //get all student 
            var dataStudent = new List<GetStudentUploadAscResult>();
            if (timetable2019.Students.Student.Any())
            {
                var getBinusianidInTag = timetable2019.Students.Student.Select(p => p.Name.Split("-").FirstOrDefault()).ToList();

                var getStudent = await _studentServices.Execute.GetStudentForUploadXML(new GetStudentUploadAscRequest
                {
                    BinusianId = getBinusianidInTag,
                    IdSchool = model.IdSchool,
                });

                if (!getStudent.IsSuccess)
                    throw new BadRequestException("Error Student With Message :" + getStudent.Message);

                dataStudent = getStudent.Payload.ToList();
            }


            //get all teacher 
            var getteacherTag = timetable2019.Teachers.Teacher.Select(p => p.Short).ToList();
            var getTeacher = await _staffServices.Execute.GetTeacherFoUploadAsc(new CheckTeacherForAscTimetableRequest
            {
                ShortName = getteacherTag,
                IdSchool = model.IdSchool,
            });

            if (!getTeacher.IsSuccess)
                throw new BadRequestException("Error Teacher With Message :" + getTeacher.Message);

            var days = await _dbContext.Entity<LtDay>()
                                       .Select(x => new CodeWithIdVm
                                       {
                                           Id = x.Id,
                                           Code = x.Code,
                                           Description = x.Description
                                       }).ToListAsync(CancellationToken);


            //class
            resultData.Class = await _helper.GetReadAndValidateDataClasss(timetable2019,
                                                    gradeByGradePathway,
                                                    model.IdSchool,
                                                    dataStudent,
                                                    getTeacher.Payload.ToList());

            //lesson
            if (!string.IsNullOrEmpty(model.FormatIdClass))
            {
                if (model.FormatIdClass.Contains("[InitialTeacher]"))
                {
                    resultData.Lesson = await _helper.GetReadAndValidateLessonInisialTeacher(timetable2019,
                                                                   gradeByGradePathway,
                                                                   model.IdSchool,
                                                                   resultData.Class,
                                                                   model.AutomaticGenerateClassId,
                                                                   gradeCode,
                                                                   model.FormatIdClass,
                                                                   getTeacher.Payload.ToList());
                }
                else
                {
                    resultData.Lesson = await _helper.GetReadAndValidateLesson(timetable2019,
                                                                   gradeByGradePathway,
                                                                   model.IdSchool,
                                                                   resultData.Class,
                                                                   model.AutomaticGenerateClassId,
                                                                   gradeCode,
                                                                   model.FormatIdClass,
                                                                   getTeacher.Payload.ToList());
                }
            }
            else
            {
                resultData.Lesson = await _helper.GetReadAndValidateLesson(timetable2019,
                                                               gradeByGradePathway,
                                                               model.IdSchool,
                                                               resultData.Class,
                                                               model.AutomaticGenerateClassId,
                                                               gradeCode,
                                                               model.FormatIdClass,
                                                               getTeacher.Payload.ToList());
            }

           
            //get data session for add in database
            if (model.IsCreateSessionSetFromXml)
                resultData.SessionSetFromXml = _helper.GetSessionSetFromXml(timetable2019, gradeByGradePathway, resultData.Lesson, model.IdSchool, days);


            //schedule
            resultData.Schedule = await _helper.GetReadAndValidateSchedule(timetable2019,
                                                                    resultData.Lesson,
                                                                    session,
                                                                    model.IdSchool,
                                                                    model.IsCreateSessionSetFromXml,
                                                                    resultData.SessionSetFromXml,
                                                                    days);

            //enrolemnt
            resultData.Enrollment = _helper.GetReadAndValidateEnrollment(timetable2019,
                                                                       resultData.Class,
                                                                       model.IdSchool,
                                                                       dataStudent,
                                                                       resultData.Lesson);
            //daystruecture
            if (model.IsCreateSessionSetFromXml)
            {
                resultData.DayStructure = _helper.GetReadAndValidateDaysStructure(timetable2019,
                                                                           resultData.Lesson,
                                                                           session,
                                                                           model.IdSchool,
                                                                           model.IsCreateSessionSetFromXml,
                                                                           resultData.SessionSetFromXml);
            }
            else
            {
                resultData.DayStructure = session.Select(p => new DayStructureVm
                {
                    No = p.Id,
                    Grade = p.Grade,
                    Pathway = p.Pathway,
                    //IdLesson = "",
                    SchoolDay = p.DaysName,
                    SchoolCode = model.IdSchool,
                    SessionName = p.Name,
                    SessionID = p.SessionId.ToString(),
                    SessionAlias = p.Alias,
                    DurationInMinute = p.DurationInMinutes.ToString(),
                    StartDate = p.StartTime.ToString(),
                    EndDate = p.EndTime.ToString(),
                    IdSessionFormMaster = p.Id,
                    IsDataReadyFromMaster = true,
                }).ToList();
            }

            return resultData;

        }

    }
}
