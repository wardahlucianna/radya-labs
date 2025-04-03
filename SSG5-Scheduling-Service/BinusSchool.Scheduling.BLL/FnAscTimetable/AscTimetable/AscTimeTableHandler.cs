using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class AscTimeTableHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IApiService<IAcademicYear> _academicYears;
        private readonly IApiService<IMetadata> _metadataService;
        private readonly IApiService<ISession> _sessionService;
        private readonly IApiService<ISessionSet> _sessionSetService;
        private readonly IApiService<IGrade> _gradePathway;
        private readonly IApiService<IDay> _dayservices;
        private readonly IApiService<IStudent> _studentServices;
        private readonly IApiService<IStaff> _staffServices;
        private readonly IApiService<IGrade> _gradeService;
        private readonly IApiService<IVenue> _venueServices;

        public AscTimeTableHandler(ISchedulingDbContext dbContext,
            IApiService<IAcademicYear> academicYears,
            IApiService<IMetadata> metadataService,
            IApiService<ISession> sessionService,
            IApiService<IGrade> gradePathway,
            IApiService<IDay> dayservices,
            IApiService<IStudent> studentServices,
            IApiService<IStaff> staffServices,
            IApiService<ISessionSet> sessionSetService,
            IApiService<IGrade> gradeService,
            IApiService<IVenue> venueServices)
        {
            _dbContext = dbContext;
            _academicYears = academicYears;
            _metadataService = metadataService;
            _sessionService = sessionService;
            _gradePathway = gradePathway;
            _dayservices = dayservices;
            _studentServices = studentServices;
            _staffServices = staffServices;
            _sessionSetService = sessionSetService;
            _gradeService = gradeService;
            _venueServices = venueServices;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            FillConfiguration();
            _academicYears.SetConfigurationFrom(ApiConfiguration);
            _metadataService.SetConfigurationFrom(ApiConfiguration);
            _sessionService.SetConfigurationFrom(ApiConfiguration);
            _gradePathway.SetConfigurationFrom(ApiConfiguration);
            _dayservices.SetConfigurationFrom(ApiConfiguration);
            _staffServices.SetConfigurationFrom(ApiConfiguration);
            _studentServices.SetConfigurationFrom(ApiConfiguration);
            _venueServices.SetConfigurationFrom(ApiConfiguration);

            var dataChek = await _dbContext.Entity<TrAscTimetable>()
                .Include(x => x.AscTimetableHomerooms).ThenInclude(p => p.Homeroom).ThenInclude(p => p.HomeroomPathways)
                .Include(x => x.AscTimetableHomerooms).ThenInclude(p => p.Homeroom).ThenInclude(p => p.HomeroomStudents)
                .Include(x => x.AscTimetableHomerooms).ThenInclude(p => p.Homeroom).ThenInclude(p => p.HomeroomTeachers)
                .Where(x => x.Id == id).AsNoTracking().FirstOrDefaultAsync();

            if (dataChek == null)
                throw new BadRequestException("Asc Time table Not Found");


            var getLesson = await _dbContext.Entity<MsLesson>()
                                          .Include(p => p.AscTimetableLessons)
                                          .Include(p => p.LessonTeachers)
                                          .Include(p => p.Subject)
                                          .Include(p => p.LessonPathways).ThenInclude(p => p.HomeroomPathway.Homeroom)
                                          .Where(p => p.AscTimetableLessons.Any(x => x.IdAscTimetable == id) &&
                                                     p.Semester == 1)
                                          .ToListAsync();

            var IdmasterLesson = getLesson.Select(p => new
            {
                subject = p.IdSubject,
                teacher = p.LessonTeachers.Select(x => x.IdUser).ToList()
            }).ToList();

            var idmasterHomeroome = dataChek.AscTimetableHomerooms.Select(p => new
            {
                grade = p.Homeroom.IdGrade,
                Venue = p.Homeroom.IdVenue,
                gradeClassroom = p.Homeroom.IdGradePathwayClassRoom,
                gradepathway = p.Homeroom.IdGradePathway,
                gradePathwayDetail = p.Homeroom.HomeroomPathways.Select(x => x.IdGradePathwayDetail).ToList(),
                student = p.Homeroom.HomeroomStudents.Select(x => x.IdStudent).ToList(),
                teacher = p.Homeroom.HomeroomTeachers.Select(x => x.IdBinusian).ToList(),

            }).ToList();

            var idmasterSchedule = _dbContext.Entity<TrAscTimetableSchedule>()
                                              .Include(p => p.Schedule)
                                              .Where(p => p.IdAscTimetable == id)
                                              .Select(p => new
                                              {
                                                  session = p.Schedule.IdSession,
                                                  venue = p.Schedule.IdVenue,
                                              }).ToList();

            var teacher = IdmasterLesson.SelectMany(p => p.teacher).ToList();
            teacher.AddRange(idmasterHomeroome.SelectMany(p => p.teacher).ToList());

            teacher = teacher.GroupBy(p => p).Select(p => p.Key).ToList();


            var grade = idmasterHomeroome.Where(p => p.grade != null).Select(p => p.grade).ToList();
            var venue = idmasterHomeroome.Where(p => p.Venue != null).Select(p => p.Venue).ToList();
            venue.AddRange(idmasterSchedule.Where(p => p.venue != null).Select(p => p.venue).ToList());
            venue = venue.GroupBy(p => p).Select(p => p.Key).ToList();


            var idSubject = IdmasterLesson.Where(p => p.subject != null).Select(p => p.subject).GroupBy(p => p).Select(p => p.Key).ToList();

            var metadata = new GetMetadataRequest();
            metadata.Acadyears = new List<string>() { dataChek.IdAcademicYear };
            metadata.Grades = grade;
            metadata.Venues = venue;
            // metadata.Subjects = idSubject;

            //metadata
            var formMetadata = await _metadataService.Execute.GetMetadata(metadata);

            //var subject = _dbContext.Entity<MsSubject>().Where(p => idSubject.Any(x => x == p.Id)).Select(p => new ItemValueVm
            //{
            //    Id = p.Id,
            //    Description = p.Description,
            //}).ToList();

            //if (subject.Any())
            //{
            //    formMetadata.Payload.Subjects = subject;
            //}

            //sessions
            var getIDsession = idmasterSchedule.GroupBy(p => p.session).SelectMany(p => p.Select(x => x.session)).ToList();
            var session = await _sessionService.Execute.GetSessionListForHelperAsc(new GetSessionBySessionSetRequest()
            {
                IdSessionSet = dataChek.IdSessionSet,
            });

            //gradeoathway
            var gradepathway = await _gradePathway.Execute.GetGradeListByGradePathwayIds(new GetGradepathwayForXMLRequest()
            {
                IdAcademicyear = dataChek.IdAcademicYear,
                IdGradePathway = idmasterHomeroome.Select(p => p.gradepathway).ToList(),
            });

            var getVenue = await _venueServices.Execute.GetVenues(new GetVenueRequest
            {
                Ids = venue,
                IdSchool = new List<string> { dataChek.IdSchool },
                Return = 0,
                Page = 1,
                Size = 1000,
            });

            var getDataStudent = new List<GetStudentUploadAscResult>();
            var getStudent = await _studentServices.Execute.GetStudentForUploadXML(new GetStudentUploadAscRequest
            {
                BinusianId = idmasterHomeroome.SelectMany(p => p.student).Distinct().ToList(),
                IdSchool = dataChek.IdSchool,
            });

            //if (!getStudent.IsSuccess)
            //    throw new BadRequestException("Error Student With Message :" + getStudent.Message);

            getDataStudent = getStudent.Payload?.ToList();

            //get all teacher 
            var getDataTeacher = new List<CheckTeacherForAscTimetableResult>();
            var getTeacher = await _staffServices.Execute.GetTeacherFoUploadAsc(new CheckTeacherForAscTimetableRequest
            {
                ShortName = teacher,
            });

            //if (!getTeacher.IsSuccess)
            //    throw new BadRequestException("Error Teacher With Message :" + getTeacher.Message);
            getDataTeacher = getTeacher.Payload?.ToList();

            var getDyas = await _dayservices.Execute.GetDays(new CollectionSchoolRequest
            {
                GetAll = true,
                IdSchool = new string[] { dataChek.IdSchool }
            });

            var data = new AscTimetableGetDetailResult();
            data.IdAscTimeTableName = dataChek.Id;
            data.AscTimeTableName = dataChek.Name;
            data.IdSessionSet = dataChek.IdSessionSet;
            data.AutomaticGenerateClassId = dataChek.IsAutomaticGenerateClassId;
            data.IdSchool = dataChek.IdSchool;
            data.IdSchoolAcademicyears = dataChek.IdAcademicYear;
            data.Name = dataChek.Name;
            data.Participan = GetPathway(gradepathway.Payload?.ToList() ?? new List<GradePathwayForAscTimeTableResult>(), dataChek.IdGradePathwayForParticipant);
            data.Acadedmicyears = formMetadata.Payload.Acadyears.FirstOrDefault().Description;
            data.FormatIdClass = dataChek.FormatClassName.Replace("%", "");
            data.ClassIdExampleValue = getLesson.FirstOrDefault()?.ClassIdGenerated;
            data.Lesson = GetdataMasterLesson(getLesson, formMetadata.Payload, gradepathway.Payload?.ToList() ?? new List<GradePathwayForAscTimeTableResult>(), getDataTeacher);
            data.Class = dataChek.AscTimetableHomerooms.GetClassData(gradepathway.Payload?.ToList() ?? new List<GradePathwayForAscTimeTableResult>(),
                                                       getDataTeacher,
                                                       getDataStudent,
                                                       getVenue.Payload.ToList());
            data.Schedule = GetdataSchedule(dataChek.Id, session.Payload.ToList(),
                                                                getDyas.Payload.OrderByDescending(x => x.Code).ToList(),
                                                                getDataTeacher,
                                                                getVenue.Payload.ToList());
            data.Enrollment = GetEnrolemnt(dataChek.Id, getDataStudent, formMetadata.Payload, gradepathway.Payload?.ToList() ?? new List<GradePathwayForAscTimeTableResult>());
            data.DayStructure = session.Payload.Select(p => new DayStructureVm
            {
                Grade = p.Grade,
                Pathway = p.Pathway,
                StartDate = p.StartTime.ToString(@"h\:mm"),
                EndDate = p.EndTime.ToString(@"h\:mm"),
                SessionAlias = p.Alias,
                SessionName = p.Name,
                IdSessionFormMaster = p.Id,
                SchoolDay = p.DaysName,
                DurationInMinute = p.DurationInMinutes.ToString(),
                IdSessionFormXml = p.Id,
                IsDataReadyFromMaster = true,
                SchoolCode = p.DaysCode
            }).OrderByDescending(x => x.SchoolCode).ToList();


            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<AscTimetableGetListRequest>(nameof(AscTimetableGetListRequest.IdSchool));
            var columns = new[] { "acadyear", "scheduleName", "sessionSetName", "participant" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "idAcademicYears" },
                { columns[1], "name" },
                { columns[2], "idSessionSet" }
            };
            var predicate = PredicateBuilder.Create<TrAscTimetable>(x => x.IdSchool == param.IdSchool);
            if (!string.IsNullOrEmpty(param.IdSchoolAcademicyears))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdSchoolAcademicyears);
            if (!string.IsNullOrEmpty(param.IdSchoolLevel))
                predicate = predicate.And(x => x.AscTimetableHomerooms.Any(x => x.Homeroom.Grade.IdLevel == param.IdSchoolLevel));
            if (!string.IsNullOrEmpty(param.IdSchoolGrade))
                predicate = predicate.And(x => x.AscTimetableHomerooms.Any(x => x.Homeroom.IdGrade == param.IdSchoolGrade));
            if (!string.IsNullOrEmpty(param.IdSessionSet))
                predicate = predicate.And(x => x.IdSessionSet == param.IdSessionSet);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Name, param.SearchPattern()));

            var query = _dbContext.Entity<TrAscTimetable>()
                .Include(x => x.AcademicYear)
                .Include(x => x.SessionSet)
                .Include(x => x.AscTimetableHomerooms).ThenInclude(x => x.Homeroom).ThenInclude(x => x.Grade)
                .Where(predicate);

            if (param.OrderBy != columns[3])
                query = query.OrderByDynamic(param, aliasColumns);
            else
            {
                if (param.OrderType == OrderType.Asc)
                    query = query.OrderBy(x => x.AscTimetableHomerooms.Count());
                else
                    query = query.OrderByDescending(x => x.AscTimetableHomerooms.Count());
            }


            int count = default;
            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Name))
                    .ToListAsync(CancellationToken);
            else
            {
                var data = await query
                    .SetPagination(param)
                    .Select(x => new AscTimetableGetListResult
                    {
                        Id = x.Id,
                        Academicyears = x.AcademicYear.Description,
                        ScheduleName = x.Name,
                        TotalParticipans = x.AscTimetableHomerooms.Where(x=>x.Homeroom.Semester == 1).Count().ToString() + " Class",
                        SessionSetName = x.SessionSet.Description,
                        Description = x.GradeCodeForGenerateClassId
                    })
                    .ToListAsync(CancellationToken);
                items = data;
                if (string.IsNullOrEmpty(param.IdSchoolLevel))
                {
                    count = param.CanCountWithoutFetchDb(items.Count)
                        ? items.Count
                        : await query.Select(x => x.Id).CountAsync(CancellationToken);
                }
            }

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        #region Helper Details private 
        private EnrollmentDataVM GetEnrolemnt(string ascId,
                                                  List<GetStudentUploadAscResult> Student,
                                                  GetMetadataResult metadata,
                                                  List<GradePathwayForAscTimeTableResult> gradePathway)
        {
            var result = new EnrollmentDataVM();
            var getData = _dbContext.Entity<TrAscTimetableEnrollment>()
                .Include(p => p.HomeroomStudentEnrollment)
                    .ThenInclude(p => p.HomeroomStudent)
                    .ThenInclude(p => p.Homeroom)
                .Where(p => p.HomeroomStudentEnrollment != null && p.IdAscTimetable == ascId)
                .ToList();

            var datagroup = getData.Where(p => p.HomeroomStudentEnrollment.HomeroomStudent.Homeroom.Semester == 1).GroupBy(p => p.HomeroomStudentEnrollment.HomeroomStudent.IdStudent).ToList();
            foreach (var item in datagroup)
            {
                var getGrade = gradePathway.Where(p => p.IdGradePathway == item.FirstOrDefault().HomeroomStudentEnrollment.HomeroomStudent.Homeroom.IdGradePathway).FirstOrDefault();
                var classsroom = gradePathway.SelectMany(p => p.GradePathwayClassRooms).Where(p => p.IdGradePathwayClassrom == item.FirstOrDefault().HomeroomStudentEnrollment.HomeroomStudent.Homeroom.IdGradePathwayClassRoom).FirstOrDefault();

                var dataEnrollment = new EnrollmentVm();
                dataEnrollment.EnrollmentStudent = new List<EnrollmentStudentVM>();

                dataEnrollment.Student = Student.Where(p => p.BinusianId == item.FirstOrDefault().HomeroomStudentEnrollment.HomeroomStudent.IdStudent).Select(p => new StudentEnrollmentVM
                {
                    BinusianId = p.BinusianId,
                    StudentName = p.FullName,
                    Religion = p.Religion,
                    DataIsUseInMaster = true,
                    Class = classsroom != null ? classsroom.ClassRoomCode : "",
                    ClassCombination = classsroom != null ? classsroom.ClassRoomCombinationGrade : "",
                    Grade = new DataModelGeneral
                    {
                        Id = getGrade.IdGrade,
                        Code = getGrade.GradeCode,
                        Description = getGrade.GradeDescription,
                    },

                }).FirstOrDefault();

                dataEnrollment.EnrollmentStudent = item
                       .Select(p => new EnrollmentStudentVM
                       {
                           ClassId = p?.HomeroomStudentEnrollment?.Lesson?.ClassIdGenerated,
                           LessonId = p?.HomeroomStudentEnrollment?.IdLesson,
                           SubjectID = p?.HomeroomStudentEnrollment?.IdSubject,
                           SubjectLevelID = p?.HomeroomStudentEnrollment?.IdSubjectLevel,
                       }).ToList();
                result.EnrollmentData.Add(dataEnrollment);
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

            return result;
        }

        private List<ScheduleVm> GetdataSchedule(string ascId,
                                                  List<GetSessionAscTimetableResult> session,
                                                  List<CodeWithIdVm> Days,
                                                  List<CheckTeacherForAscTimetableResult> teacher,
                                                  List<GetVenueResult> venue)
        {
            var result = new List<ScheduleVm>();
            var getData = _dbContext.Entity<TrAscTimetableSchedule>()
                .Include(p => p.Schedule)
                    .ThenInclude(p => p.WeekVarianDetail.Week)
                .Include(p => p.Schedule)
                    .ThenInclude(p => p.Lesson)
                .Where(p => p.Schedule.Semester == 1 && !p.IsFromMaster && p.IdAscTimetable == ascId)
                .ToList();

            var query = from sch in getData
                        join period in session on sch.Schedule.IdSession equals period.Id
                        join daysday in Days.Where(p => p.Code != "000001" && p.Code != "0000001") on sch.Schedule.IdDay equals daysday.Id
                        orderby daysday.Code ascending
                        select new { Schedule = sch, session = period, days = daysday };

            var scheduleGroup = query.GroupBy(p => p.session.SessionId).OrderBy(p => p.Key).ToList();


            foreach (var item in scheduleGroup)
            {
                var data = new ScheduleVm();
                data.Session = item.Key.ToString();
                foreach (var itemSchedule in Days.Where(p => p.Code != "000001" && p.Code != "0000001"))
                {
                    var scheduleValue = new ScheduleValueVM();
                    scheduleValue.ListSchedule = new List<ScheduleDataVM>();
                    scheduleValue.Days = new ScheduleDayVM()
                    {
                        Id = itemSchedule != null ? itemSchedule.Id : "",
                        Code = itemSchedule != null ? itemSchedule.Code : "",
                        Description = itemSchedule != null ? itemSchedule.Description : "",
                        IdFromMasterData = itemSchedule != null ? itemSchedule.Id : "",
                        DataIsUseInMaster = itemSchedule != null ? true : false,
                    };

                    var datas = item.GroupBy(p => p.days).Where(p => p.Key.Id == itemSchedule.Id).SelectMany(p => p.Select(p => p)).ToList();

                    foreach (var listSchedule in datas)
                    {
                        var modelList = new ScheduleDataVM();
                        modelList.LessonIsReadyFromMaster = true;
                        modelList.DataIsUseInMaster = true;
                        modelList.StartTime = listSchedule.session.StartTime.ToString(@"h\:mm");
                        modelList.EndTime = listSchedule.session.EndTime.ToString(@"h\:mm");
                        modelList.ClassID = listSchedule.Schedule.Schedule.Lesson.ClassIdGenerated;
                        if (teacher != null && teacher.Any())
                        {
                            modelList.Teacher = teacher.Where(p => p.IdTeacher == listSchedule.Schedule.Schedule.IdUser).Select(p => new DataModelGeneral
                            {
                                Id = p.IdTeacher,
                                Code = p.TeacherInitialName,
                                Description = p.TeacherName,
                                IdFromMasterData = p.IdTeacher,
                                DataIsUseInMaster = true,
                            }).FirstOrDefault();
                        }

                        modelList.Weeks = listSchedule.Schedule.Schedule.WeekVarianDetail.Week.Description;
                        modelList.Venue = venue.Where(p => p.Id == listSchedule.Schedule.Schedule.IdVenue).Select(p => new DataModelGeneral
                        {
                            Id = p.Id,
                            Code = p.Code,
                            Description = p.Description,
                            DataIsUseInMaster = true,
                        }).FirstOrDefault();

                        scheduleValue.ListSchedule.Add(modelList);
                    }
                    data.Schedule.Add(scheduleValue);
                }
                result.Add(data);
            }
            return result;
        }

        private List<LessonUploadXMLVm> GetdataMasterLesson(List<MsLesson> dataleson,
                                       GetMetadataResult metadata,
                                       List<GradePathwayForAscTimeTableResult> gradePathway,
                                       List<CheckTeacherForAscTimetableResult> teacher)
        {
            var result = new List<LessonUploadXMLVm>();
            var datas = dataleson.OrderBy(p => p.ClassIdGenerated);
            foreach (var item in datas)
            {
                var data = new LessonUploadXMLVm();
                data.LessonIds = new List<string>() { item.Id };
                data.Grade = gradePathway.Where(p => p.IdGrade == item.IdGrade).Select(p => new GradeClassVM
                {
                    IdFromMasterData = p.IdGrade,
                    Code = p.GradeCode,
                    Description = p.GradeDescription,

                }).FirstOrDefault();
                data.Subject = metadata.Subjects.Where(p => p.Id == item.IdSubject).Select(p => new SubjectLesson
                {
                    Id = p.Id,
                    Description = p.Description,
                }).FirstOrDefault();

                data.Subject = new SubjectLesson
                {
                    Id = item.Subject?.Id,
                    Description = item.Subject?.Description,
                };
                data.ClassId = item.ClassIdGenerated;
                data.TotalWeek = item.TotalPerWeek;

                if (item.LessonPathways.Any())
                {
                    var _class = new List<ClassLesonVm>();
                    foreach (var classLesson in item.LessonPathways)
                    {
                        var getdataGrade = gradePathway.Where(p => p.IdGradePathway == classLesson.HomeroomPathway.Homeroom.IdGradePathway).ToList();
                        var getClassData = getdataGrade.SelectMany(p => p.GradePathwayClassRooms).ToList();
                        var _pathway = new List<PathwayLesson>();
                        foreach (var itemPathway in getdataGrade.SelectMany(p => p.GradePathwayDetails))
                        {
                            _pathway.Add(new PathwayLesson
                            {
                                PathwayCode = itemPathway.PathwayCode,
                                PathwayDescription = itemPathway.PathwayDescription,
                            });
                        }

                        var getclass = getClassData.Where(p => p.IdGradePathwayClassrom == classLesson.HomeroomPathway.Homeroom.IdGradePathwayClassRoom).ToList();
                        foreach (var itemClass in getclass)
                        {
                            _class.Add(new ClassLesonVm
                            {
                                IdClassForSave = itemClass.IdClassRoom,
                                IdClassFromMaster = itemClass.IdClassRoom,
                                ClassCode = itemClass.ClassRoomCode,
                                ClassCombination = itemClass.ClassRoomCombinationGrade,
                                ClassDescription = itemClass.ClassRoomCode,
                                PathwayLesson = _pathway.Any() ? _pathway : new List<PathwayLesson>(),
                            });
                        }
                    }

                    if (_class.Any())
                    {
                        data.Class = _class.OrderBy(p => p.ClassCode).GroupBy(p => p.IdClassFromMaster)
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
                                            }).ToList();
                    }

                }


                if (teacher != null && teacher.Any())
                {
                    data.Teacher = teacher.OrderBy(p => p.TeacherName).Where(p => item.LessonTeachers.Any(x => x.IdUser == p.IdTeacher)).Select(p => new DataModelGeneral
                    {
                        Id = p.IdTeacher,
                        Code = p.TeacherInitialName,
                        Description = p.TeacherName,
                    }).ToList();
                }

                result.Add(data);
            }
            return result;
        }



        private List<ParticipanVm> GetPathway(List<GradePathwayForAscTimeTableResult> gradePathway, string idGradePathway)
        {
            var get = new List<ParticipanVm>();
            var getPathway = JsonConvert.DeserializeObject<List<string>>(idGradePathway);
            if (gradePathway != null)
            {
                get = gradePathway.Where(p => getPathway.Any(x => x == p.IdGradePathway)).Select(p => new ParticipanVm
                {
                    IdGradePathway = p.IdGradePathway,
                    Grade = p.GradeCode,
                    Pathway = string.Join(", ", p.GradePathwayDetails.Select(p => p.PathwayCode).Distinct().ToList())
                }).ToList();

            }
            return get;
        }

        #endregion


    }

    public static class Helper
    {
        #region HelperDetail



        public static List<ClassPackageVm> GetClassData(this ICollection<TrAscTimetableHomeroom> homerooms,
                                                        List<GradePathwayForAscTimeTableResult> gradePathway,
                                                        List<CheckTeacherForAscTimetableResult> teacher,
                                                        List<GetStudentUploadAscResult> Student,
                                                        List<GetVenueResult> venue)
        {
            var result = new List<ClassPackageVm>();
            if (gradePathway == null)
                return result;

            var classes = gradePathway.SelectMany(p => p.GradePathwayClassRooms).ToList();
            var pathway = gradePathway.SelectMany(p => p.GradePathwayDetails).ToList();
            foreach (var item in homerooms.Where(p => p.Homeroom.Semester == 1))
            {
                var data = new ClassPackageVm();
                data.Grade = gradePathway.Where(p => p.IdGrade == item.Homeroom.IdGrade).Select(p => new GradeClassVM()
                {
                    Id = p.IdGrade,
                    Code = p.GradeCode,
                    Description = p.GradeDescription,
                }).FirstOrDefault();
                if (teacher != null && teacher.Any())
                {
                    data.Teacher = teacher.Where(p => item.Homeroom.HomeroomTeachers.Any(x => x.IdBinusian == p.IdTeacher)).Select(p => new DataModelGeneral
                    {
                        Id = p.IdTeacher,
                        Code = p.TeacherInitialName,
                        Description = p.TeacherName,
                    }).ToList();
                }

                data.Class = classes.Where(p => p.IdGradePathwayClassrom == item.Homeroom.IdGradePathwayClassRoom).Select(p => new ClassVmInClassData
                {
                    Id = p.IdGradePathwayClassrom,
                    Code = p.ClassRoomCode,
                    Description = data.Grade?.Code + " " + p.ClassRoomDescription,
                    ClassCombination = p.ClassRoomCombinationGrade,
                    DataIsUseInMaster = true,
                }).FirstOrDefault();
                data.Pathway = pathway.Where(p => item.Homeroom.HomeroomPathways.Any(x => x.IdGradePathwayDetail == p.IdGradePathwayDetail)).Select(p => new DataModelGeneral
                {
                    Id = p.IdGradePathwayDetail,
                    Code = p.PathwayCode,
                    Description = p.PathwayDescription,
                    DataIsUseInMaster = true,
                }).ToList();
                data.Venue = venue.Where(p => p.Id == item.Homeroom.IdVenue).Select(p => new DataModelGeneral
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    DataIsUseInMaster = true,
                }).FirstOrDefault();
                if (item.Homeroom.HomeroomStudents.Any())
                {
                    data.StudentInHomeRoom = Student.Where(p => item.Homeroom.HomeroomStudents.Any(x => x.IdStudent == p.BinusianId)).Select(p => new StudentInHomeRoom
                    {

                        IdStudent = p.BinusianId,
                        StudentName = p.FullName,
                        Gender = p.Gender,
                        Religion = p.Religion,
                        SubjectReligion = p.ReligionSubject,

                    }).ToList();
                }


                data.MaleAndFemale = $"{data.StudentInHomeRoom.Count(x => x.Gender == "Male")}/{data.StudentInHomeRoom.Count(x => x.Gender == "Female")}/{data.StudentInHomeRoom.Count(x => x.Gender != "Male" && x.Gender != "Female")}";
                data.Reqion = $"{data.StudentInHomeRoom.Count(x => x.Religion == "Buddhist")}/{data.StudentInHomeRoom.Count(x => x.Religion == "Catholic")}/" +
                                   $"{data.StudentInHomeRoom.Count(x => x.Religion == "Christian")}/{data.StudentInHomeRoom.Count(x => x.Religion == "Islam")}/" +
                                   $"{data.StudentInHomeRoom.Count(x => x.Religion == "Hindu")}/" +
                                   $"{data.StudentInHomeRoom.Count(x => x.Religion != "Buddhist" && x.Religion != "Catholic" && x.Religion != "Christian" && x.Religion != "Islam" && x.Religion != "Hindu")}";

                result.Add(data);
            }
            return result;
        }


        #endregion
    }

}
