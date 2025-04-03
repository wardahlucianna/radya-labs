using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling
{
    public class AddGenerateScheduleV2Handler
    {
#if DEBUG
        private const string _blobPath = "generate-schedule-debug/add/{name}.json";
#else
        private const string _blobPath = "generate-schedule/add/{name}.json";
#endif
        //private const string _blobPath = "generate-schedule-debug/add/{name}.json";

        private IDbContextTransaction _transaction;
        private CancellationToken _cancellationToken;
        private string _idUser, _idSchool;

        private readonly ISchedulingDbContext _dbContext;
        private readonly ILogger<AddGenerateScheduleV2Handler> _logger;
        private readonly IMessage _messageService;
        private readonly IGenerateSchedule _generateScheduleService;
        private readonly IStringLocalizer _localizer;
        private readonly IConfiguration _configuration;
        private readonly INotificationManager _notificationManager;

        public AddGenerateScheduleV2Handler(
            ISchedulingDbContext dbContext,
            ILogger<AddGenerateScheduleV2Handler> logger,
            IMessage messageService,
            IGenerateSchedule generateScheduleService,
            IStringLocalizer localizer,
            IConfiguration configuration,
            INotificationManager notificationManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _messageService = messageService;
            _generateScheduleService = generateScheduleService;
            _localizer = localizer;
            _configuration = configuration;
            _notificationManager = notificationManager;
        }

        [FunctionName(nameof(AddGenerateScheduleV2))]
        public async Task AddGenerateScheduleV2([BlobTrigger(_blobPath)] Stream blobStream,
            IDictionary<string, string> metadata,
            string name,
            [Queue("notification-ays-longrun")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var body = default(AddGenerateScheduleRequest);
            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();
            var idProcess = string.Empty;
            _cancellationToken = cancellationToken;

            try
            {
                _logger.LogInformation("[Blob] Processing add Add Generate Schedule: {0}", name);

#if DEBUG
                _idUser = "SASEMARANG";
                _idSchool = "4";
#else
                if (!metadata.TryGetValue("idUser", out _idUser))
                    throw new ArgumentException(nameof(_idUser));
                if (!metadata.TryGetValue("idSchool", out _idSchool))
                    throw new ArgumentException(nameof(_idSchool));
#endif

                using var blobStreamReader = new StreamReader(blobStream);
                using var jsonReader = new JsonTextReader(blobStreamReader);

                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        body = new JsonSerializer().Deserialize<AddGenerateScheduleRequest>(jsonReader);
                        break;
                    }
                }

                // validate body
                (await new AddGenerateScheduleValidator().ValidateAsync(body, cancellationToken)).EnsureValid(localizer: _localizer);

                // set job started
                var startProcess = await _generateScheduleService.StartProcess(new StartGeneratedScheduleProcessRequest
                {
                    IdSchool = _idSchool,
                    Grades = body.Grades.Select(x => x.GradeId).Distinct().ToList(),
                    Version = 2
                });
                if (!startProcess.IsSuccess)
                    throw new BadRequestException(startProcess.Message);
                idProcess = startProcess.Payload;

                var grades = body.Grades.Select(x => x.GradeId).Distinct().ToList();

                var startPeriod = body.Grades.Select(x => x.StartPeriod).Distinct().FirstOrDefault();
                var endPeriod = body.Grades.Select(x => x.EndPeriod).Distinct().FirstOrDefault();

                var periods = await _dbContext.Entity<MsPeriod>()
                                .Where(x => grades.Contains(x.IdGrade))
                                .Select(e => new
                                {
                                    e.AttendanceStartDate,
                                    e.AttendanceEndDate,
                                    e.Semester
                                })
                                .ToListAsync(cancellationToken);

                var listDays = await _dbContext.Entity<LtDay>()
                                .Select(e => new
                                {
                                    e.Id,
                                    e.Description
                                })
                                .ToListAsync(cancellationToken);

                var listGrade = await _dbContext.Entity<MsGrade>()
                                        .Include(e => e.Level).ThenInclude(e => e.AcademicYear)
                                        .Where(x => grades.Contains(x.Id))
                                        .Select(e => new
                                        {
                                            e.Id,
                                            e.Description,
                                            e.IdLevel,
                                            e.Level.IdAcademicYear
                                        })
                                        .ToListAsync(cancellationToken);

                #region ungenerated
                var getIdLesson = body.Grades.SelectMany(e => e.Students.SelectMany(f => f.Lessons.Select(g => g.LessonId))).Distinct().ToList();
                var getIdSession = body.Grades.SelectMany(e => e.Students.SelectMany(f => f.Lessons.Select(g => g.IdSession))).Distinct().ToList();
                var getScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                        .Include(e => e.GeneratedScheduleGrade).ThenInclude(e => e.GeneratedSchedule)
                                        .IgnoreQueryFilters()
                                        .Where(x => grades.Contains(x.IdGrade)
                                            && x.IsGenerated
                                            && getIdLesson.Contains(x.IdLesson)
                                            && getIdSession.Contains(x.IdSession)
                                            && x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAsctimetable
                                            && x.ScheduleDate.Date >= startPeriod.Date && x.ScheduleDate.Date <= endPeriod.Date)
                                        .ToListAsync(cancellationToken);

                if (getScheduleLesson.Any())
                {
                    var IdGeneratedSchedule = getScheduleLesson.Select(e => e.GeneratedScheduleGrade.GeneratedSchedule.Id).Distinct().ToList();

                    foreach (var itemIdGeneratedSchedule in IdGeneratedSchedule)
                    {
                        var listScheduleLesson = getScheduleLesson.Where(e => e.GeneratedScheduleGrade.IdGeneratedSchedule == itemIdGeneratedSchedule).ToList();

                        listScheduleLesson.ForEach(e =>
                                                {
                                                    e.IsActive = false;
                                                    e.IsGenerated = false;
                                                });

                        _dbContext.Entity<MsScheduleLesson>().UpdateRange(getScheduleLesson);

                        var listIdScheduleLesson = listScheduleLesson.Select(e => e.Id).ToList();

                        var listGeneratedScheduleGrade = await _dbContext.Entity<TrGeneratedScheduleGrade>()
                                      .Include(e => e.ScheduleLessons)
                                      .Include(e => e.GeneratedSchedule)
                                      .Where(x =>x.IdGeneratedSchedule==itemIdGeneratedSchedule && x.IdGeneratedSchedule== itemIdGeneratedSchedule)
                                      .ToListAsync(cancellationToken);

                        var isRevoveSchedule = true;
                        foreach(var GeneratedScheduleGrade in listGeneratedScheduleGrade)
                        {
                            var listIdGeneratedScheduleGrade = GeneratedScheduleGrade.ScheduleLessons.Where(e=> !listIdScheduleLesson.Contains(e.Id)).ToList();

                            if (!listIdGeneratedScheduleGrade.Any())
                            {
                                GeneratedScheduleGrade.IsActive=false;
                                _dbContext.Entity<TrGeneratedScheduleGrade>().Update(GeneratedScheduleGrade);
                            }
                            else
                            {
                                isRevoveSchedule = false;
                            }
                        }

                        if (isRevoveSchedule)
                        {
                            var GeneratedSchedule = listGeneratedScheduleGrade.Select(e=>e.GeneratedSchedule).FirstOrDefault();
                            GeneratedSchedule.IsActive = true;
                            _dbContext.Entity<TrGeneratedSchedule>().UpdateRange(GeneratedSchedule);
                        }
                    }
                }



                #endregion

                if (body.Type == "grade")
                {
                    foreach (var grade in body.Grades)
                    {
                        var lessons = grade.Students.FirstOrDefault().Lessons.GroupBy(x => new
                        {
                            x.WeekId,
                            x.ScheduleDate,
                            x.ClassId,
                            x.LessonId,
                            x.VenueId,
                            x.VenueName,
                            x.IdSubject,
                            x.SubjectName,
                            x.SessionID,
                            x.IdSession,
                            x.StartTime,
                            x.EndTime,
                            x.DaysofWeek,
                        })
                        .Select(x => new GenerateScheduleLessonVM
                        {
                            WeekId = x.Key.WeekId,
                            ScheduleDate = x.Key.ScheduleDate,
                            ClassId = x.Key.ClassId,
                            LessonId = x.Key.LessonId,
                            VenueId = x.Key.VenueId,
                            VenueName = x.Key.VenueName,
                            IdSubject = x.Key.IdSubject,
                            SubjectName = x.Key.SubjectName,
                            SessionID = x.Key.SessionID,
                            IdSession = x.Key.IdSession,
                            StartTime = x.Key.StartTime,
                            EndTime = x.Key.EndTime,
                            DaysofWeek = x.Key.DaysofWeek,
                        })
                        .ToList();
                        _logger.LogWarning($"Lesson Has Been Grouping{JsonConvert.SerializeObject(lessons)}");
                    }

                    _transaction = await _dbContext.BeginTransactionAsync(_cancellationToken, System.Data.IsolationLevel.ReadUncommitted);

                    var generateSchedule = new TrGeneratedSchedule();
                    generateSchedule.Id = Guid.NewGuid().ToString();
                    generateSchedule.IdAscTimetable = body.IdAsctimetable;
                    _dbContext.Entity<TrGeneratedSchedule>().Add(generateSchedule);

                    foreach (var grade in body.Grades)
                    {
                        var _grade = listGrade
                                        .Where(x => x.Id == grade.GradeId)
                                        .FirstOrDefault();

                        #region add TrGeneratedScheduleGrade
                        var trGeneratedScheduleGrade = new TrGeneratedScheduleGrade();
                        trGeneratedScheduleGrade.Id = Guid.NewGuid().ToString();
                        trGeneratedScheduleGrade.IdGeneratedSchedule = generateSchedule.Id;
                        trGeneratedScheduleGrade.IdGrade = grade.GradeId;
                        trGeneratedScheduleGrade.StartPeriod = grade.StartPeriod;
                        trGeneratedScheduleGrade.EndPeriod = grade.EndPeriod;
                        _dbContext.Entity<TrGeneratedScheduleGrade>().Add(trGeneratedScheduleGrade);
                        _logger.LogWarning($"Total Student In Grade {_grade.Description} = {grade.Students.Count}");
                        #endregion

                        #region add MsScheduleLesson
                        var GetBodyLesson = grade.Students.SelectMany(e => e.Lessons).ToList();
                        foreach (var lesson in GetBodyLesson)
                        {

                            var semesterByDate = periods.Where(x => lesson.ScheduleDate.Date >= x.AttendanceStartDate.Date && lesson.ScheduleDate.Date <= x.AttendanceEndDate.Date).Select(x => x.Semester).FirstOrDefault();
                            var idDay = listDays.Where(e => e.Description == lesson.DaysofWeek).Select(e => e.Id).FirstOrDefault();
                            _dbContext.Entity<MsScheduleLesson>().Add(new MsScheduleLesson
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdWeek = lesson.WeekId,
                                IdVenue = lesson.VenueId,
                                VenueName = lesson.VenueName,
                                ClassID = lesson.ClassId,
                                IdLesson = lesson.LessonId,
                                IsGenerated = true,
                                ScheduleDate = lesson.ScheduleDate,
                                StartTime = TimeSpan.Parse(lesson.StartTime.ToString()),
                                EndTime = TimeSpan.Parse(lesson.EndTime.ToString()),
                                IdSession = lesson.IdSession,
                                SessionID = lesson.SessionID.ToString(),
                                IdSubject = lesson.IdSubject,
                                SubjectName = lesson.SubjectName,
                                DaysOfWeek = lesson.DaysofWeek,
                                IdAcademicYear = _grade.IdAcademicYear,
                                IdGrade = _grade.Id,
                                IdLevel = _grade.IdLevel,
                                IdDay = idDay,
                                IdGeneratedScheduleGrade = trGeneratedScheduleGrade.Id,
                            });
                        }
                        #endregion

                        await _dbContext.SaveChangesAsync(_cancellationToken);
                    }
                }

                await _transaction.CommitAsync(_cancellationToken);

                //set process job finished
                _ = await _generateScheduleService.FinishProcess(new FinishGeneratedScheduleProcessRequest
                {
                    IdProcess = idProcess
                });

                // send message to user that save asc timetable
                _ = _messageService
                    .AddMessage(new AddMessageRequest
                    {
                        Type = UserMessageType.GenerateSchedule,
                        IdSender = Guid.Empty.ToString(),
                        Recepients = new List<string> { _idUser },
                        Subject = "Generated Schedule Success",
                        Content = $"Generated Schedule Success.",
                        Attachments = new List<MessageAttachment>(), // attachments can't be null
                        IsSetSenderAsSchool = true,
                        IsDraft = false,
                        IsAllowReply = false,
                        IsMarkAsPinned = false
                    });

                // send notification
                var queueMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS4")
                {
                    IdRecipients = new[] { _idUser },
                    KeyValues = new Dictionary<string, object> { { "date", DateTimeUtil.ServerTime } }
                });
                collector.Add(queueMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Blob] " + ex.Message);
                _transaction?.Rollback();

                //set process job finished if started
                if (!string.IsNullOrEmpty(idProcess))
                    _ = await _generateScheduleService.FinishProcess(new FinishGeneratedScheduleProcessRequest
                    {
                        IdProcess = idProcess
                    });

                var subjectMessage = $"Generated Schedule Failed";

                //send message to user that save asc timetable
                _ = _messageService
                    .AddMessage(new AddMessageRequest
                    {
                        Type = UserMessageType.GenerateSchedule,
                        IdSender = Guid.Empty.ToString(),
                        Recepients = new List<string> { _idUser },
                        Subject = subjectMessage,
                        Content = $"Generate Schedule Failed, please contact IT Admin or re-upload.",
                        Attachments = new List<MessageAttachment>(), // attachments can't be null
                        IsSetSenderAsSchool = true,
                        IsDraft = false,
                        IsAllowReply = false,
                        IsMarkAsPinned = false
                    });

                // send notification
                var queueMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS5")
                {
                    IdRecipients = new[] { _idUser },
                    KeyValues = new Dictionary<string, object> { { "date", DateTimeUtil.ServerTime } }
                });
                collector.Add(queueMessage);

                // send error notification to dev
                var queueErrorMessage = JsonConvert.SerializeObject(new NotificationQueue(_idSchool, "AYS0")
                {
                    KeyValues = new Dictionary<string, object>
                    {
                        { "exMessage", ex.Message },
                        { "exStackTrace", ex.StackTrace },
                        { "exInnerMessage", ex.InnerException?.Message },
                        { "exInnerStackTrace", ex.InnerException?.StackTrace }
                    }
                });
                collector.Add(queueErrorMessage);

                // rethrow the exception
                throw;
            }
        }
    }


}
