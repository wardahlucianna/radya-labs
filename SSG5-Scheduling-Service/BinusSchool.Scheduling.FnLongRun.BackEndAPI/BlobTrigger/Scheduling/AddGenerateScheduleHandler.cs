//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using BinusSchool.Common.Exceptions;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Abstractions;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Common.Utils;
//using BinusSchool.Data.Abstractions;
//using BinusSchool.Data.Api.Extensions;
//using BinusSchool.Data.Api.Scheduling.FnSchedule;
//using BinusSchool.Data.Api.User.FnCommunication;
//using BinusSchool.Data.Configurations;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
//using BinusSchool.Data.Model.User.FnCommunication.Message;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using BinusSchool.Persistence.SchedulingDb.Entities.School;
//using BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling.Validator;
//using Microsoft.Azure.WebJobs;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using SendGrid.Helpers.Mail;

//namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling
//{
//    // TODO: drop this class after migration to durable function
//    public class AddGenerateScheduleHandler
//    {
//#if DEBUG
//        private const string _blobPath = "generate-schedule-debug/add/{name}.json";
//#else
//        private const string _blobPath = "generate-schedule/add/{name}.json";
//#endif
//        //private const string _blobPath = "generate-schedule-debug/add/{name}.json";

//        private IDbContextTransaction _transaction;
//        private CancellationToken _cancellationToken;
//        private string _idUser, _idSchool;

//        private readonly ISchedulingDbContext _dbContext;
//        private readonly ILogger<AddGenerateScheduleHandler> _logger;
//        private readonly IApiService<IMessage> _messageService;
//        private readonly IApiService<IGenerateSchedule> _generateScheduleService;
//        private readonly IStringLocalizer _localizer;
//        private readonly IConfiguration _configuration;
//        private readonly INotificationManager _notificationManager;

//        public AddGenerateScheduleHandler(
//            ISchedulingDbContext dbContext,
//            ILogger<AddGenerateScheduleHandler> logger,
//            IApiService<IMessage> messageService,
//            IApiService<IGenerateSchedule> generateScheduleService,
//            IStringLocalizer localizer,
//            IConfiguration configuration,
//            INotificationManager notificationManager)
//        {
//            _dbContext = dbContext;
//            _logger = logger;
//            _messageService = messageService;
//            _generateScheduleService = generateScheduleService;
//            _localizer = localizer;
//            _configuration = configuration;
//            _notificationManager = notificationManager;
//        }

//        [FunctionName(nameof(AddGenerateSchedule))]
//        public async Task AddGenerateSchedule([BlobTrigger(_blobPath)] Stream blobStream,
//            IDictionary<string, string> metadata,
//            string name,
//            [Queue("notification-ays-longrun")] ICollector<string> collector,
//            CancellationToken cancellationToken)
//        {
//            var body = default(AddGenerateScheduleRequest);
//            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();
//            var idProcess = string.Empty;
//            _cancellationToken = cancellationToken;

//            try
//            {
//                _logger.LogInformation("[Blob] Processing add Add Generate Schedule: {0}", name);

//                if (!metadata.TryGetValue("idUser", out _idUser))
//                    throw new ArgumentException(nameof(_idUser));
//                if (!metadata.TryGetValue("idSchool", out _idSchool))
//                    throw new ArgumentException(nameof(_idSchool));

//                using var blobStreamReader = new StreamReader(blobStream);
//                using var jsonReader = new JsonTextReader(blobStreamReader);

//                while (await jsonReader.ReadAsync(cancellationToken))
//                {
//                    if (jsonReader.TokenType == JsonToken.StartObject)
//                    {
//                        body = new JsonSerializer().Deserialize<AddGenerateScheduleRequest>(jsonReader);
//                        break;
//                    }
//                }

//                // validate body
//                (await new AddGenerateScheduleValidator().ValidateAsync(body, cancellationToken)).EnsureValid(localizer: _localizer);

//                // set job started
//                var startProcess = await _generateScheduleService.SetConfigurationFrom(apiConfig).Execute.StartProcess(new StartGeneratedScheduleProcessRequest
//                {
//                    IdSchool = _idSchool,
//                    Grades = body.Grades.Select(x => x.GradeId).Distinct().ToList()
//                });
//                if (!startProcess.IsSuccess)
//                    throw new BadRequestException(startProcess.Message);
//                idProcess = startProcess.Payload;

//                var grades = body.Grades.Select(x => x.GradeId).Distinct();
//                var periods = await _dbContext.Entity<MsPeriod>().Where(x => grades.Contains(x.IdGrade)).ToListAsync(cancellationToken);
//                List<HomeroomByStudentAndLesson> homeroomByStudentAndLessons = new List<HomeroomByStudentAndLesson>();
//                if (body.Type == "grade")
//                {
//                    var studentInGrades = await _dbContext.Entity<MsHomeroomStudent>()
//                    .Include(x => x.Homeroom)
//                    .Include(x => x.Student)
//                    .Where(x => grades.Contains(x.Homeroom.IdGrade))
//                    .GroupBy(x => new
//                    {
//                        x.IdStudent,
//                        x.Homeroom.IdGrade
//                    })
//                    .Select(x => new
//                    {
//                        StudentId = x.Key.IdStudent,
//                        GradeId = x.Key.IdGrade
//                    })
//                    .ToListAsync(cancellationToken);
//                    var studentIds = studentInGrades.Select(x => x.StudentId).ToList();
//                    homeroomByStudentAndLessons = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
//                    .Include(x => x.HomeroomStudent)
//                        .ThenInclude(x => x.Student)
//                    .Include(x => x.Lesson)
//                    .Where(x => studentIds.Contains(x.HomeroomStudent.IdStudent))
//                    .Select(x => new HomeroomByStudentAndLesson
//                    {
//                        IdLesson = x.IdLesson,
//                        IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
//                        IdHomeroom = x.HomeroomStudent.IdHomeroom,
//                        HomeroomName = string.Format("{0}{1}", x.HomeroomStudent.Homeroom.Grade.Code, x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code),
//                        Semester = x.HomeroomStudent.Homeroom.Semester,
//                        ClassIdGenerated = x.Lesson.ClassIdGenerated,
//                        IdStudent = x.HomeroomStudent.IdStudent
//                    })
//                    .ToListAsync();
//                    foreach (var grade in body.Grades)
//                    {
//                        var students = studentInGrades
//                            .Where(x => x.GradeId == grade.GradeId)
//                            .Where(x => x.StudentId != "RL000")
//                            .Select(x => new GenerateScheduleStudentVM
//                            {
//                                StudentId = x.StudentId,
//                                Lessons = new List<GenerateScheduleLessonVM>()
//                            }).ToList();

//                        var lessons = grade.Students.FirstOrDefault().Lessons.GroupBy(x => new
//                        {
//                            x.WeekId,
//                            x.StartPeriode,
//                            x.EndPeriode,
//                            x.ScheduleDate,
//                            x.ClassId,
//                            x.LessonId,
//                            x.VenueId,
//                            x.VenueName,
//                            x.TeacherName,
//                            x.TeacherId,
//                            x.IdSubject,
//                            x.SubjectName,
//                            x.SessionID,
//                            x.IdSession,
//                            x.StartTime,
//                            x.EndTime,
//                            x.DaysofWeek,
//                            x.IdHomeroom,
//                            x.HomeroomName
//                        })
//                        .Select(x => new GenerateScheduleLessonVM
//                        {
//                            WeekId = x.Key.WeekId,
//                            StartPeriode = x.Key.StartPeriode,
//                            EndPeriode = x.Key.EndPeriode,
//                            ScheduleDate = x.Key.ScheduleDate,
//                            ClassId = x.Key.ClassId,
//                            LessonId = x.Key.LessonId,
//                            VenueId = x.Key.VenueId,
//                            VenueName = x.Key.VenueName,
//                            TeacherName = x.Key.TeacherName,
//                            TeacherId = x.Key.TeacherId,
//                            IdSubject = x.Key.IdSubject,
//                            SubjectName = x.Key.SubjectName,
//                            SessionID = x.Key.SessionID,
//                            IdSession = x.Key.IdSession,
//                            StartTime = x.Key.StartTime,
//                            EndTime = x.Key.EndTime,
//                            DaysofWeek = x.Key.DaysofWeek,
//                            IdHomeroom = x.Key.IdHomeroom,
//                            HomeroomName = x.Key.HomeroomName
//                        })
//                        .ToList();
//                        _logger.LogWarning($"Lesson Has Been Grouping{JsonConvert.SerializeObject(lessons)}");
//                        foreach (var studentEnrollment in students)
//                        {
//                            var enrollmentStudent = homeroomByStudentAndLessons
//                            .Where(x => x.IdStudent == studentEnrollment.StudentId)
//                            .Select(x => x.IdLesson)
//                            .ToList();
//                            var lessonId = lessons.GroupBy(x => x.LessonId).Select(x => x.Key).ToList();
//                            var finalLessonId = lessonId.Intersect(enrollmentStudent).ToList();
//                            studentEnrollment.Lessons = lessons.Where(x => finalLessonId.Any(y => y == x.LessonId)).ToList();
//                            _logger.LogWarning($"Class Id Will Be Generated in this time {string.Join(",", studentEnrollment.Lessons.Select(x => x.ClassId).ToList())}");
//                        }
//                        grade.Students = students;
//                    }

//                    _transaction = await _dbContext.BeginTransactionAsync(_cancellationToken, System.Data.IsolationLevel.ReadUncommitted);
//                    var generateSchedule = new TrGeneratedSchedule();
//                    generateSchedule.Id = Guid.NewGuid().ToString();
//                    generateSchedule.IdAscTimetable = body.IdAsctimetable;
//                    _dbContext.Entity<TrGeneratedSchedule>().Add(generateSchedule);
//                    foreach (var grade in body.Grades)
//                    {
//                        var _grade = await _dbContext.Entity<MsGrade>().Where(x => x.Id == grade.GradeId).FirstOrDefaultAsync(cancellationToken);
//                        var trGeneratedScheduleGrade = new TrGeneratedScheduleGrade();
//                        trGeneratedScheduleGrade.Id = Guid.NewGuid().ToString();
//                        trGeneratedScheduleGrade.IdGeneratedSchedule = generateSchedule.Id;
//                        trGeneratedScheduleGrade.IdGrade = grade.GradeId;
//                        trGeneratedScheduleGrade.StartPeriod = grade.StartPeriod;
//                        trGeneratedScheduleGrade.EndPeriod = grade.EndPeriod;
//                        _dbContext.Entity<TrGeneratedScheduleGrade>().Add(trGeneratedScheduleGrade);


//                        _logger.LogWarning($"Total Student In Grade {_grade.Description} = {grade.Students.Count}");
//                        foreach (var student in grade.Students)
//                        {
//                            var getClassIDs = student.Lessons.Select(p => p.ClassId).Distinct().ToList();
//                            await SetToDatagenerateFalse(getClassIDs, grade.StartPeriod, grade.EndPeriod, student.StudentId);
//                            var generatedScheduleStudent = await _dbContext.Entity<TrGeneratedScheduleStudent>().Where(x => x.IdStudent == student.StudentId).FirstOrDefaultAsync();
//                            if (generatedScheduleStudent == null)
//                            {
//                                generatedScheduleStudent = new TrGeneratedScheduleStudent();
//                                generatedScheduleStudent.Id = Guid.NewGuid().ToString();
//                                generatedScheduleStudent.IdGenerateScheduleGrade = trGeneratedScheduleGrade.Id;
//                                generatedScheduleStudent.IdStudent = student.StudentId;
//                                _dbContext.Entity<TrGeneratedScheduleStudent>().Add(generatedScheduleStudent);
//                            }
//                            var HomeroomByStudentAndLesson = homeroomByStudentAndLessons
//                            .Where(x => x.IdGrade == trGeneratedScheduleGrade.IdGrade)
//                            .Where(x => x.IdStudent == student.StudentId)
//                            .ToList();
//                            _logger.LogWarning($"Generate Schedule For Student {student.StudentId}");
//                            foreach (var lesson in student.Lessons)
//                            {
//                                var semesterByDate = periods.Where(x => lesson.ScheduleDate.Date >= x.AttendanceStartDate.Date && lesson.ScheduleDate.Date <= x.AttendanceEndDate.Date).Select(x => x.Semester).FirstOrDefault();
//                                var _lesson = new TrGeneratedScheduleLesson();
//                                _lesson.Id = Guid.NewGuid().ToString();
//                                _lesson.IdGeneratedScheduleStudent = generatedScheduleStudent.Id;
//                                _lesson.IdWeek = lesson.WeekId;
//                                _lesson.IdUser = lesson.TeacherId;
//                                _lesson.TeacherName = lesson.TeacherName;
//                                _lesson.IdVenue = lesson.VenueId;
//                                _lesson.VenueName = lesson.VenueName;
//                                _lesson.ClassID = lesson.ClassId;
//                                _lesson.IdLesson = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate && x.ClassIdGenerated == lesson.ClassId).FirstOrDefault()?.IdLesson ?? lesson.LessonId;
//                                _lesson.IsGenerated = true;
//                                _lesson.ScheduleDate = lesson.ScheduleDate;
//                                _lesson.StartPeriod = lesson.StartPeriode;
//                                _lesson.EndPeriod = lesson.EndPeriode;
//                                _lesson.StartTime = TimeSpan.Parse(lesson.StartTime.ToString());
//                                _lesson.EndTime = TimeSpan.Parse(lesson.EndTime.ToString());
//                                _lesson.IdSession = lesson.IdSession;
//                                _lesson.SessionID = lesson.SessionID.ToString();
//                                _lesson.IdSubject = lesson.IdSubject;
//                                _lesson.SubjectName = lesson.SubjectName;
//                                _lesson.DaysOfWeek = lesson.DaysofWeek;
//                                _lesson.IdHomeroom = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate).FirstOrDefault()?.IdHomeroom ?? lesson.IdHomeroom;
//                                _lesson.HomeroomName = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate).FirstOrDefault()?.HomeroomName ?? lesson.HomeroomName;
//                                _dbContext.Entity<TrGeneratedScheduleLesson>().Add(_lesson);
//                            }
//                            await _dbContext.SaveChangesAsync(_cancellationToken);
//                        }
//                    }
//                }
//                else
//                {
//                    var listStudents = body.Grades.SelectMany(x => x.Students).ToList();
//                    var studentIds = listStudents.Select(x => x.StudentId).ToList();
//                    homeroomByStudentAndLessons = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
//                    .Include(x => x.HomeroomStudent)
//                        .ThenInclude(x => x.Student)
//                    .Include(x => x.Lesson)
//                    .Where(x => studentIds.Contains(x.HomeroomStudent.IdStudent))
//                    .Select(x => new HomeroomByStudentAndLesson
//                    {
//                        IdLesson = x.IdLesson,
//                        IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
//                        IdHomeroom = x.HomeroomStudent.IdHomeroom,
//                        HomeroomName = string.Format("{0}{1}", x.HomeroomStudent.Homeroom.Grade.Code, x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code),
//                        Semester = x.HomeroomStudent.Homeroom.Semester,
//                        ClassIdGenerated = x.Lesson.ClassIdGenerated,
//                        IdStudent = x.HomeroomStudent.IdStudent
//                    }).ToListAsync(cancellationToken);

//                    _transaction = await _dbContext.BeginTransactionAsync(_cancellationToken, System.Data.IsolationLevel.ReadUncommitted);
//                    var generateSchedule = new TrGeneratedSchedule();
//                    generateSchedule.Id = Guid.NewGuid().ToString();
//                    generateSchedule.IdAscTimetable = body.IdAsctimetable;
//                    _dbContext.Entity<TrGeneratedSchedule>().Add(generateSchedule);
//                    foreach (var grade in body.Grades)
//                    {
//                        var _grade = await _dbContext.Entity<MsGrade>().Where(x => x.Id == grade.GradeId).FirstOrDefaultAsync(cancellationToken);
//                        var trGeneratedScheduleGrade = new TrGeneratedScheduleGrade();
//                        trGeneratedScheduleGrade.Id = Guid.NewGuid().ToString();
//                        trGeneratedScheduleGrade.IdGeneratedSchedule = generateSchedule.Id;
//                        trGeneratedScheduleGrade.IdGrade = grade.GradeId;
//                        trGeneratedScheduleGrade.StartPeriod = grade.StartPeriod;
//                        trGeneratedScheduleGrade.EndPeriod = grade.EndPeriod;
//                        _dbContext.Entity<TrGeneratedScheduleGrade>().Add(trGeneratedScheduleGrade);
//                        _logger.LogWarning($"Total Student In Grade {_grade.Description} = {grade.Students.Count}");
//                        foreach (var student in grade.Students)
//                        {
//                            var getClassIDs = student.Lessons.Select(p => p.ClassId).Distinct().ToList();
//                            await SetToDatagenerateFalse(getClassIDs, grade.StartPeriod, grade.EndPeriod, student.StudentId);
//                            var generatedScheduleStudent = await _dbContext.Entity<TrGeneratedScheduleStudent>().Where(x => x.IdStudent == student.StudentId).FirstOrDefaultAsync();
//                            if (generatedScheduleStudent == null)
//                            {
//                                generatedScheduleStudent = new TrGeneratedScheduleStudent();
//                                generatedScheduleStudent.Id = Guid.NewGuid().ToString();
//                                generatedScheduleStudent.IdGenerateScheduleGrade = trGeneratedScheduleGrade.Id;
//                                generatedScheduleStudent.IdStudent = student.StudentId;
//                                _dbContext.Entity<TrGeneratedScheduleStudent>().Add(generatedScheduleStudent);
//                            }
//                            var HomeroomByStudentAndLesson = homeroomByStudentAndLessons
//                            .Where(x => x.IdGrade == trGeneratedScheduleGrade.IdGrade)
//                            .Where(x => x.IdStudent == student.StudentId)
//                            .ToList();
//                            _logger.LogWarning($"Generate Schedule For Student {student.StudentId}");
//                            var lessonsGenerated = student.Lessons.GroupBy(x => new
//                            {
//                                x.WeekId,
//                                x.StartPeriode,
//                                x.EndPeriode,
//                                x.ScheduleDate,
//                                x.ClassId,
//                                x.LessonId,
//                                x.VenueId,
//                                x.VenueName,
//                                x.TeacherName,
//                                x.TeacherId,
//                                x.IdSubject,
//                                x.SubjectName,
//                                x.SessionID,
//                                x.IdSession,
//                                x.StartTime,
//                                x.EndTime,
//                                x.DaysofWeek,
//                                x.IdHomeroom,
//                                x.HomeroomName
//                            })
//                            .Select(x => new GenerateScheduleLessonVM
//                            {
//                                WeekId = x.Key.WeekId,
//                                StartPeriode = x.Key.StartPeriode,
//                                EndPeriode = x.Key.EndPeriode,
//                                ScheduleDate = x.Key.ScheduleDate,
//                                ClassId = x.Key.ClassId,
//                                LessonId = x.Key.LessonId,
//                                VenueId = x.Key.VenueId,
//                                VenueName = x.Key.VenueName,
//                                TeacherName = x.Key.TeacherName,
//                                TeacherId = x.Key.TeacherId,
//                                IdSubject = x.Key.IdSubject,
//                                SubjectName = x.Key.SubjectName,
//                                SessionID = x.Key.SessionID,
//                                IdSession = x.Key.IdSession,
//                                StartTime = x.Key.StartTime,
//                                EndTime = x.Key.EndTime,
//                                DaysofWeek = x.Key.DaysofWeek,
//                                IdHomeroom = x.Key.IdHomeroom,
//                                HomeroomName = x.Key.HomeroomName
//                            })
//                            .ToList();
//                            foreach (var lesson in lessonsGenerated)
//                            {
//                                var semesterByDate = periods.Where(x => lesson.ScheduleDate.Date >= x.AttendanceStartDate.Date && lesson.ScheduleDate.Date <= x.AttendanceEndDate.Date).Select(x => x.Semester).FirstOrDefault();
//                                var _lesson = new TrGeneratedScheduleLesson();
//                                _lesson.Id = Guid.NewGuid().ToString();
//                                _lesson.IdGeneratedScheduleStudent = generatedScheduleStudent.Id;
//                                _lesson.IdWeek = lesson.WeekId;
//                                _lesson.IdUser = lesson.TeacherId;
//                                _lesson.TeacherName = lesson.TeacherName;
//                                _lesson.IdVenue = lesson.VenueId;
//                                _lesson.VenueName = lesson.VenueName;
//                                _lesson.ClassID = lesson.ClassId;
//                                _lesson.IdLesson = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate && x.ClassIdGenerated == lesson.ClassId).FirstOrDefault()?.IdLesson ?? lesson.LessonId;
//                                _lesson.IsGenerated = true;
//                                _lesson.ScheduleDate = lesson.ScheduleDate;
//                                _lesson.StartPeriod = lesson.StartPeriode;
//                                _lesson.EndPeriod = lesson.EndPeriode;
//                                _lesson.StartTime = TimeSpan.Parse(lesson.StartTime.ToString());
//                                _lesson.EndTime = TimeSpan.Parse(lesson.EndTime.ToString());
//                                _lesson.IdSession = lesson.IdSession;
//                                _lesson.SessionID = lesson.SessionID.ToString();
//                                _lesson.IdSubject = lesson.IdSubject;
//                                _lesson.SubjectName = lesson.SubjectName;
//                                _lesson.DaysOfWeek = lesson.DaysofWeek;
//                                _lesson.IdHomeroom = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate).FirstOrDefault()?.IdHomeroom ?? lesson.IdHomeroom;
//                                _lesson.HomeroomName = HomeroomByStudentAndLesson.Where(x => x.Semester == semesterByDate).FirstOrDefault()?.HomeroomName ?? lesson.HomeroomName;
//                                _dbContext.Entity<TrGeneratedScheduleLesson>().Add(_lesson);
//                            }
//                            await _dbContext.SaveChangesAsync(_cancellationToken);
//                        }
//                    }
//                }


//                await _transaction.CommitAsync(_cancellationToken);

//                //set process job finished
//                _ = await _generateScheduleService.SetConfigurationFrom(apiConfig).Execute.FinishProcess(new FinishGeneratedScheduleProcessRequest
//                {
//                    IdProcess = idProcess
//                });

//                // send message to user that save asc timetable
//                _ = _messageService
//                    .SetConfigurationFrom(apiConfig)
//                    .Execute.AddMessage(new AddMessageRequest
//                    {
//                        Type = UserMessageType.GenerateSchedule,
//                        IdSender = Guid.Empty.ToString(),
//                        Recepients = new List<string> { _idUser },
//                        Subject = "Generated Schedule Success",
//                        Content = $"Generated Schedule Success.",
//                        Attachments = new List<MessageAttachment>(), // attachments can't be null
//                        IsSetSenderAsSchool = true,
//                        IsDraft = false,
//                        IsAllowReply = false,
//                        IsMarkAsPinned = false
//                    });

//                // send notification
//                var queueMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS4")
//                {
//                    IdRecipients = new[] { _idUser },
//                    KeyValues = new Dictionary<string, object> { { "date", DateTimeUtil.ServerTime } }
//                });
//                collector.Add(queueMessage);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[Blob] " + ex.Message);
//                _transaction?.Rollback();

//                //set process job finished if started
//                if (!string.IsNullOrEmpty(idProcess))
//                    _ = await _generateScheduleService.SetConfigurationFrom(apiConfig).Execute.FinishProcess(new FinishGeneratedScheduleProcessRequest
//                    {
//                        IdProcess = idProcess
//                    });

//                var subjectMessage = $"Generated Schedule Failed";

//                // send message to user that save asc timetable
//                _ = _messageService
//                    .SetConfigurationFrom(apiConfig)
//                    .Execute.AddMessage(new AddMessageRequest
//                    {
//                        Type = UserMessageType.GenerateSchedule,
//                        IdSender = Guid.Empty.ToString(),
//                        Recepients = new List<string> { _idUser },
//                        Subject = subjectMessage,
//                        Content = $"Generate Schedule Failed, please contact IT Admin or re-upload.",
//                        Attachments = new List<MessageAttachment>(), // attachments can't be null
//                        IsSetSenderAsSchool = true,
//                        IsDraft = false,
//                        IsAllowReply = false,
//                        IsMarkAsPinned = false
//                    });

//                // send notification
//                var queueMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS5")
//                {
//                    IdRecipients = new[] { _idUser },
//                    KeyValues = new Dictionary<string, object> { { "date", DateTimeUtil.ServerTime } }
//                });
//                collector.Add(queueMessage);

//                // send error notification to dev
//                var queueErrorMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS0")
//                {
//                    KeyValues = new Dictionary<string, object>
//                    {
//                        { "exMessage", ex.Message },
//                        { "exStackTrace", ex.StackTrace },
//                        { "exInnerMessage", ex.InnerException?.Message },
//                        { "exInnerStackTrace", ex.InnerException?.StackTrace }
//                    }
//                });
//                collector.Add(queueErrorMessage);

//                // rethrow the exception
//                throw;
//            }
//        }

//        /// <summary>
//        /// set data generate schedule sebelum nya memnjadi is generate false
//        /// </summary>
//        /// <param name="classId"></param>
//        /// <param name="idGrade"></param>
//        /// <param name="idAsctimetable"></param>
//        /// <param name="startPeriod"></param>
//        /// <param name="endPeriod"></param>
//        /// <returns></returns>
//        private async Task SetToDatagenerateFalse(List<string> classId, DateTime startPeriod, DateTime endPeriod, string IdStudent)
//        {
//            var getUpdate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
//                                   .Include(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule)
//                                   .Where(p =>
//                                   classId.Any(x => x == p.ClassID)
//                                   && p.IsGenerated
//                                   //&& p.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == idGrade
//                                   && p.ScheduleDate.Date >= startPeriod.Date
//                                   && p.ScheduleDate.Date <= endPeriod.Date
//                                   && p.GeneratedScheduleStudent.IdStudent == IdStudent
//                                   )
//                                   .ToListAsync();

//            if (getUpdate.Any())
//            {
//                foreach (var setItemfalse in getUpdate)
//                {
//                    setItemfalse.IsGenerated = false;
//                    setItemfalse.IsActive = false;
//                    _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
//                }
//            }
//        }


//    }

//    public class HomeroomByStudentAndLesson
//    {
//        public string IdStudent { get; set; }
//        public string IdLesson { get; set; }
//        public string IdGrade { get; set; }
//        public string IdHomeroom { get; set; }
//        public string HomeroomName { get; set; }
//        public int Semester { get; set; }
//        public string ClassIdGenerated { get; set; }
//    }
//}
