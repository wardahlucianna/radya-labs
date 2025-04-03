//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using BinusSchool.Common.Exceptions;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Abstractions;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Data.Abstractions;
//using BinusSchool.Data.Api.Extensions;
//using BinusSchool.Data.Api.Scheduling.FnAscTimetable;
//using BinusSchool.Data.Api.Teaching.FnAssignment;
//using BinusSchool.Data.Api.User.FnCommunication;
//using BinusSchool.Data.Configurations;
//using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
//using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
//using BinusSchool.Data.Model.User.FnCommunication.Message;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using BinusSchool.Persistence.SchedulingDb.Entities.School;
//using BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Helpers;
//using BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Validator;
//using Microsoft.Azure.WebJobs;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using SendGrid.Helpers.Mail;

//namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable
//{
//    public class AddAscTimetableHandler
//    {
//#if DEBUG
//        private const string _blobPath = "asc-timetable-debug/add/{name}.json";
//#else
//        private const string _blobPath = "asc-timetable/add/{name}.json";
//#endif

//        private static readonly Lazy<List<string>> _emailTags = new Lazy<List<string>>(new List<string>() { "ASC Timetable" });
//        private static readonly Lazy<List<EmailAddress>> _emailRecipients = new Lazy<List<EmailAddress>>(new List<EmailAddress>()
//        {
//            new EmailAddress("robby@radyalabs.com"),
//            new EmailAddress("ade@radyalabs.com"),
//            new EmailAddress("siti@radyalabs.id"),
//            new EmailAddress("teguh@radyalabs.com")
//        });
//        private static readonly Lazy<List<string>> _ignoredTables = new Lazy<List<string>>(new List<string>()
//        {
//            "TrAsctimetable",
//            "TrAsctimetableLesson",
//            "TrAsctimetableHomeroom",
//            "TrAsctimetableSchedule",
//            "TrAsctimetableDayStructure",
//            "TrAsctimetableEnrollment",
//            "TrAsctimetableProcess",
//            "MsSchedule"
//        });

//        private IDbContextTransaction _transaction;
//        private CancellationToken _cancellationToken;
//        private string _idUser, _idSchool;

//        private readonly ISchedulingDbContext _dbContext;
//        private readonly IStringLocalizer _localizer;
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<AddAscTimetableHandler> _logger;
//        private readonly IApiService<ITeacherPosition> _teacherPosition;
//        private readonly IApiService<IMessage> _messageService;
//        private readonly IApiService<IAscTimetable> _ascService;
//        private readonly HelperSaveXML _helperSave;
//        private readonly INotificationManager _notificationManager;

//        public AddAscTimetableHandler(ISchedulingDbContext dbContext,
//            IStringLocalizer localizer,
//            IConfiguration configuration,
//            ILogger<AddAscTimetableHandler> logger,
//            IApiService<IMessage> messageService,
//            IApiService<ITeacherPosition> teacherPosition,
//            IApiService<IAscTimetable> ascService,
//            INotificationManager notificationManager)
//        {
//            _dbContext = dbContext;
//            _localizer = localizer;
//            _configuration = configuration;
//            _logger = logger;
//            _teacherPosition = teacherPosition;
//            _notificationManager = notificationManager;
//            _messageService = messageService;
//            _ascService = ascService;
//            _helperSave = new HelperSaveXML(_dbContext);
//        }

//        [FunctionName(nameof(AddAscTimetable))]
//        public async Task AddAscTimetable([BlobTrigger(_blobPath)] Stream blobStream,
//            IDictionary<string, string> metadata,
//            string name,
//            CancellationToken cancellationToken)
//        {
//            var body = default(AddDataAscTimeTableAfterUploadRequest);
//            var idSessionSet = default(string);
//            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();
//            var idProcess = string.Empty;
//            _cancellationToken = cancellationToken;

//            try
//            {
//                _logger.LogInformation("[Blob] Processing add Asc Timetable: {0}", name);
//                // throw new Exception("Test exception ASC");
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
//                        body = new JsonSerializer().Deserialize<AddDataAscTimeTableAfterUploadRequest>(jsonReader);
//                        break;
//                    }
//                }

//                // validate body
//                (await new AddAscAfterUploadValidator().ValidateAsync(body, cancellationToken)).EnsureValid(localizer: _localizer);

//                // do save asc timetable
//                _teacherPosition.SetConfigurationFrom(apiConfig);

//                // set job started
//                var startProcess = await _ascService.SetConfigurationFrom(apiConfig).Execute.StartProcess(new StartAscTimetableProcessRequest
//                {
//                    IdSchool = _idSchool,
//                    Grades = body.Lesson.Select(x => x.Grade.IdFromMasterData).Distinct().ToList()
//                });
//                if (!startProcess.IsSuccess)
//                    throw new BadRequestException(startProcess.Message);
//                idProcess = startProcess.Payload;

//                var semseter = new Dictionary<string, List<int>>();
//                _transaction = await _dbContext.BeginTransactionAsync(cancellationToken, System.Data.IsolationLevel.ReadUncommitted);

//                var gradeID = body.Class.Where(p => p.IsDataReadyFromMaster).Select(p => p.Grade).GroupBy(p => p.IdFromMasterData).Select(p => p.FirstOrDefault()).ToList();

//                var gradeIds = gradeID.Select(x => x.IdFromMasterData).ToList();
//                var gradeSemesters = await _dbContext.Entity<MsPeriod>()
//                                                         .Where(x => gradeIds.Contains(x.IdGrade))
//                                                         .Select(x => new { idGrade = x.IdGrade, semester = x.Semester })
//                                                         .ToListAsync(cancellationToken);
//                foreach (var item in gradeID)
//                {
//                    if (!gradeSemesters.Any(x => x.idGrade == item.IdFromMasterData))
//                        throw new BadRequestException($"Semester for Grade {item.Code} not found ,Please add in Period Setting");

//                    semseter.Add(item.IdFromMasterData, gradeSemesters.Where(x => x.idGrade == item.IdFromMasterData).Select(x => x.semester).Distinct().ToList());
//                }

//                var getposition = await _teacherPosition.Execute.GetPositionCAforAsc(new Data.Model.Teaching.FnAssignment.TeacherPosition.GetCAForAscTimetableRequest
//                {
//                    IdSchool = body.IdSchool
//                });

//                if (!getposition.IsSuccess)
//                    throw new BadRequestException(getposition.Message);

//                if (string.IsNullOrWhiteSpace(getposition.Payload))
//                    throw new BadRequestException("Teacher's Position for Class Advisor is not ready. Please create data to fix it");

//                var getdataAsc = await _dbContext.Entity<TrAscTimetable>().Where(p => p.Name.ToLower() == body.Name.ToLower()).FirstOrDefaultAsync();
//                if (getdataAsc != null)
//                    throw new BadRequestException($"Asc time table {body.Name} Already use");

//                if (body.SaveHomeroom == false && body.SaveHomeroomStudent)
//                {
//                    if (!await ValidateSaveStudentHomeRoom(body))
//                        throw new BadRequestException("Homerooom not found from data for save student ");
//                }

//                if (body.SaveLesson == false && body.SaveStudentEnrolemnt)
//                {
//                    if (!await ValidateSaveStudentEnrollment(body))
//                        throw new BadRequestException("Lesson not found from data for save student Enrollment");
//                }

//                if (body.SaveHomeroom == false && body.SaveLesson == false && body.SaveSchedule)
//                {
//                    if (!await ValidateSaveSchedule(body))
//                        throw new BadRequestException("Lesson Or Homeroom Not found from data if not saved");
//                }

//                var ascTimetableId = "";
//                if (body.IsCreateSessionSetFromXml)
//                {
//                    var sessionActionTime = DateTime.Now;
//                    _logger.LogInformation("[Trace] Processing Create Session Set and Session");
//                    idSessionSet = await CreateAscSessionSetAndSession(body.SessionSetFromXml, body.IdSchool, body.SessionSetName);
//                    _logger.LogInformation("[Trace] Processed Create Session Set and Session ({0})", (DateTime.Now - sessionActionTime).ToString(@"hh\:mm\:ss"));
//                    //var idSessionset = "16105ea9-a793-4306-94a5-307cd7d9011a";
//                    ascTimetableId = await AddAscTimeTable(new TrAscTimetable()
//                    {
//                        Name = body.Name,
//                        IdSessionSet = idSessionSet,
//                        IdSchool = body.IdSchool,
//                        IdAcademicYear = body.IdSchoolAcademicyears,
//                        FormatClassName = !string.IsNullOrWhiteSpace(body.FormatIdClass) ? body.FormatIdClass : "",
//                        GradeCodeForGenerateClassId = JsonConvert.SerializeObject(body.CodeGradeForAutomaticGenerateClassId),
//                        IdGradepathwayforParticipan = JsonConvert.SerializeObject(body.IdGradepathwayforCreateSession),
//                        IsAutomaticGenerateClassId = body.AutomaticGenerateClassId,
//                    });
//                }
//                else
//                {
//                    ascTimetableId = await AddAscTimeTable(new TrAscTimetable()
//                    {
//                        Name = body.Name,
//                        IdSessionSet = body.IdSessionSet,
//                        IdSchool = body.IdSchool,
//                        IdAcademicYear = body.IdSchoolAcademicyears,
//                        FormatClassName = !string.IsNullOrWhiteSpace(body.FormatIdClass) ? body.FormatIdClass : "",
//                        GradeCodeForGenerateClassId = JsonConvert.SerializeObject(body.CodeGradeForAutomaticGenerateClassId),
//                        IdGradepathwayforParticipan = JsonConvert.SerializeObject(body.IdGradepathwayforCreateSession),
//                        IsAutomaticGenerateClassId = body.AutomaticGenerateClassId,
//                    });
//                }

//                await SaveDatas(body, ascTimetableId, semseter, getposition.Payload);

//                //set process job finished immediatelly
//                var process = await _dbContext.Entity<TrAscTimetableProcess>()
//                                          .Where(x => x.Id == idProcess)
//                                          .SingleOrDefaultAsync();
//                if (process is null)
//                    throw new NotFoundException("process is not found");

//                process.FinishedAt = DateTime.Now;
//                _dbContext.Entity<TrAscTimetableProcess>().Update(process);

//                await _dbContext.SaveChangesAsync(_ignoredTables.Value, cancellationToken);
//                await _transaction.CommitAsync(cancellationToken);

//                // send message to user that save asc timetable
//                _ = _messageService
//                    .SetConfigurationFrom(apiConfig)
//                    .Execute.AddMessage(new AddMessageRequest
//                    {
//                        Type = UserMessageType.AscTimetable,
//                        IdSender = Guid.Empty.ToString(),
//                        Recepients = new List<string> { _idUser },
//                        Subject = "Upload ASC Timetable Success",
//                        Content = $"Upload ASC Timetable {body.Name} Success.",
//                        Attachments = new List<MessageAttachment>(), // attachments can't be null
//                        IsSetSenderAsSchool = true,
//                        IsDraft = false,
//                        IsAllowReply = false,
//                        IsMarkAsPinned = false
//                    });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[Blob] " + ex.Message);
//                try
//                {
//                    _transaction?.Rollback();
//                }
//                catch { }

//                //set process job finished if started
//                if (!string.IsNullOrEmpty(idProcess))
//                    _ = await _ascService.SetConfigurationFrom(apiConfig).Execute.FinishProcess(new FinishAscTimetableProcessRequest
//                    {
//                        IdProcess = idProcess
//                    });

//                var subjectMessage = $"Upload ASC Timetable {body?.Name} Failed";

//                // send message to user that save asc timetable
//                _ = _messageService
//                    .SetConfigurationFrom(apiConfig)
//                    .Execute.AddMessage(new AddMessageRequest
//                    {
//                        Type = UserMessageType.AscTimetable,
//                        IdSender = Guid.Empty.ToString(),
//                        Recepients = new List<string> { _idUser },
//                        Subject = subjectMessage,
//                        Content = $"Upload ASC Timetable {body?.Name} Failed, please contact IT Admin or re-upload.",
//                        Attachments = new List<MessageAttachment>(), // attachments can't be null
//                        IsSetSenderAsSchool = true,
//                        IsDraft = false,
//                        IsAllowReply = false,
//                        IsMarkAsPinned = false
//                    });

//                // send email to developers
//                var emailMessage = new SendGridMessage
//                {
//                    Subject = subjectMessage,
//                    PlainTextContent =
//                        $"Message: {ex.Message} {Environment.NewLine}Stack Trace: {ex.StackTrace} {Environment.NewLine}{Environment.NewLine}" +
//                        $"Inner Message: {ex.InnerException?.Message} {Environment.NewLine}Inner Stack Trace: {ex.InnerException?.StackTrace}",
//                    Categories = _emailTags.Value,
//                };
//                emailMessage.AddTos(_emailRecipients.Value);
//                _ = _notificationManager.SendEmail(emailMessage);

//                // rethrow the exception
//                throw;
//            }
//        }

//        /// <summary>
//        /// create session dan session set dari xml 
//        /// </summary>
//        /// <param name="sessionsData"></param>
//        /// <param name="idSchool"></param>
//        /// <param name="sessionSetname"></param>
//        /// <returns></returns>
//        private async Task<string> CreateAscSessionSetAndSession(List<SessionSetFromXmlVm> sessionsData, string idSchool, string sessionSetname)
//        {
//            string idSessionSet = Guid.NewGuid().ToString();
//            await CreateSessionAndSessionSetInDbSchedule(sessionsData, idSchool, sessionSetname, idSessionSet);
//            return idSessionSet;
//        }

//        /// <summary>
//        /// create session dan session set dari xml ke db lokal scheduler
//        /// </summary>
//        /// <param name="sessionsData"></param>
//        /// <param name="idSchool"></param>
//        /// <param name="sessionSetname"></param>
//        /// <param name="idSessionSetFromDbSChool"></param>
//        /// <returns></returns>
//        private async Task CreateSessionAndSessionSetInDbSchedule(List<SessionSetFromXmlVm> sessionsData, string idSchool, string sessionSetname, string idSessionSetFromDbSChool)
//        {
//            if (!string.IsNullOrWhiteSpace(idSessionSetFromDbSChool))
//            {
//                if (!await _dbContext.Entity<MsSessionSet>().AnyAsync(x => x.Id == idSessionSetFromDbSChool))
//                {
//                    var sessionSet = new MsSessionSet();
//                    sessionSet.Id = idSessionSetFromDbSChool;
//                    sessionSet.IdSchool = idSchool;
//                    sessionSet.Code = sessionSetname;
//                    sessionSet.Description = sessionSetname;

//                    _dbContext.Entity<MsSessionSet>().Add(sessionSet);

//                    foreach (var item in sessionsData)
//                    {
//                        var param = new MsSession
//                        {
//                            Id = item.IdFromXml,
//                            IdSessionSet = idSessionSetFromDbSChool,
//                            IdDay = item.DaysIdFromMaster,
//                            IdGradePathway = item.IdGradepathway,
//                            SessionID = int.Parse(item.SessionId),
//                            Name = item.SessionName,
//                            Alias = item.SessionAlias,
//                            DurationInMinutes = item.DurationInMinute,
//                            StartTime = TimeSpan.Parse(item.StartTime),
//                            EndTime = TimeSpan.Parse(item.EndTime),
//                            UserIn = _idUser
//                        };

//                        _dbContext.Entity<MsSession>().Add(param);
//                    }

//                    await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                }
//            }

//        }

//        /// <summary>
//        /// add asc time table 
//        /// </summary>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        private async Task<string> AddAscTimeTable(TrAscTimetable data)
//        {
//            data.Id = Guid.NewGuid().ToString();
//            await _dbContext.Entity<TrAscTimetable>().AddAsync(data, _cancellationToken);

//            return data.Id;
//        }

//        /// <summary>
//        /// simpand data semua dari file xml semuanya 
//        /// </summary>
//        /// <param name="model"></param>
//        /// <param name="ascTimetableId"></param>
//        /// <param name="Semesters"></param>
//        /// <returns></returns>
//        private async Task<string> SaveDatas(AddDataAscTimeTableAfterUploadRequest model, string ascTimetableId, Dictionary<string, List<int>> Semesters, string CaPosition)
//        {
//            #region Proses simpan data home room 
//            var homeroomActionTime = DateTime.Now;
//            _logger.LogInformation("[Trace] Processing Save Homeroom");
//            foreach (var itemHomeRoom in model.Class)
//            {
//                var datasemester = new List<int>();
//                if (Semesters.TryGetValue(itemHomeRoom.Grade.IdFromMasterData, out datasemester))
//                {
//                    //pilihan mekanisme simpan kalo true maka data nya harus di simpan 
//                    if (model.SaveHomeroom)
//                    {
//                        //insert data to db apabila pilihan nya adalah save home rom=om
//                        //kalo home rome teacher nya ga ada maka jangan di save jadi kan kosong 
//                        //kalodata home nya ada tapi tidak ada di master jangan di save juga 

//                        //cek data terlebih dahulu yg mau di insert
//                        var getDataHomerooms = await _dbContext.Entity<MsHomeroom>()
//                                                               .Include(p => p.HomeroomPathways)
//                                                               .Include(p => p.HomeroomStudents).ThenInclude(p => p.HomeroomStudentEnrollments).ThenInclude(p => p.AscTimetableEnrollments)
//                                                               .Include(p => p.HomeroomTeachers)
//                                                               .Where(p => p.IdAcademicYear == model.IdSchoolAcademicyears &&
//                                                                           p.IdGrade == itemHomeRoom.Grade.IdFromMasterData &&
//                                                                           p.IdGradePathwayClassRoom == itemHomeRoom.Class.IdFromMasterData)
//                                                               .ToListAsync(_cancellationToken);

//                        foreach (var semester in datasemester)
//                        {
//                            //cek data terlebih dahulu yg mau di insert
//                            var getDataHomerome = getDataHomerooms.FirstOrDefault(x => x.Semester == semester);

//                            //kalo ada maka lakukan update data
//                            if (getDataHomerome != null)
//                            {

//                                var homeromestudent = new List<MsHomeroomStudent>();

//                                //update data home rome ,student enrolment and teacher assgnmn
//                                homeromestudent = _helperSave.UpdateDataHomeRoom(getDataHomerome, itemHomeRoom, semester, CaPosition, model.SaveHomeroomStudent);

//                                //save home room
//                                _helperSave.SaveHomerromAsc(ascTimetableId, getDataHomerome.Id);
//                            }
//                            else
//                            {
//                                //kalo tidak ada berarti bikin data baru berdasarkan data xml
//                                var homeRome = _helperSave.SaveHomeRoomData(itemHomeRoom, semester, model.IdSchoolAcademicyears, CaPosition);

//                                //save data home rome pathway
//                                var idHomeromepathway = _helperSave.SaveHomeRoomPathway(itemHomeRoom.Pathway, homeRome);

//                                //kalo lebih dari satu maka simpan satu saja 
//                                var homeromestudent = _helperSave.SaveHomeRoomStudent(itemHomeRoom.StudentInHomeRoom, homeRome, semester, model.SaveHomeroomStudent);

//                                //save home room 
//                                _helperSave.SaveHomerromAsc(ascTimetableId, homeRome);
//                            }

//                            await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                        }
//                    }
//                    else
//                    {
//                        //save data dari berdasarkan data dari data master 
//                        var getDataHomerooms = await _dbContext.Entity<MsHomeroom>()
//                                                               .Include(p => p.HomeroomPathways)
//                                                               .Include(p => p.HomeroomStudents)
//                                                               .Include(p => p.HomeroomTeachers)
//                                                               .Where(p => p.IdAcademicYear == model.IdSchoolAcademicyears &&
//                                                                           p.IdGrade == itemHomeRoom.Grade.IdFromMasterData &&
//                                                                           p.IdGradePathwayClassRoom == itemHomeRoom.Class.IdFromMasterData)
//                                                               .ToListAsync(_cancellationToken);

//                        //ambil data dari db apabila data home rome nya tidak di save 
//                        foreach (var semester in datasemester)
//                        {
//                            var getDataHomerome = getDataHomerooms.FirstOrDefault(x => x.Semester == semester);
//                            if (getDataHomerome != null)
//                            {
//                                // homeRoom.Add(getDataHomerome);
//                                //save home room 
//                                _helperSave.SaveHomerromAsc(ascTimetableId, getDataHomerome.Id);
//                                //await SaveLesson(model, ascTimetableId, itemHomeRoom.IdClassForSave, semester, getDataHomerome.HomeroomPathways.Select(p => p.Id).ToList(), getDataHomerome.HomeroomStudents.ToList());

//                                await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                                _logger.LogInformation("[Trace] Processed Save Homeroom ({0})", (DateTime.Now - homeroomActionTime).ToString(@"hh\:mm\:ss"));
//                            }
//                        }

//                    }
//                }

//            }
//            _logger.LogInformation("[Trace] Processed Save Homeroom ({0})", (DateTime.Now - homeroomActionTime).ToString(@"hh\:mm\:ss"));
//            #endregion

//            #region Proses simpan data Lesson
//            var lessonActionTime = DateTime.Now;
//            _logger.LogInformation("[Trace] Processing Save Lesson");
//            //proses data lesson 
//            var weekVarian = await _dbContext.Entity<MsWeekVariantDetail>()
//                                             .Include(p => p.Week)
//                                             .ToListAsync(_cancellationToken);

//            var getGradeLesson = model.Lesson.Select(p => p.Grade).GroupBy(p => p.IdFromMasterData).Select(p => p.Key).ToList();
//            foreach (var item in getGradeLesson)
//            {
//                var getdataPathwaysFromXml = model.Lesson.SelectMany(p => p.Class.SelectMany(x => x.PathwayLesson.Select(y => y.IdPathwayFromMaster))).ToList();
//                var getidClassRoomsFromXml = model.Lesson.SelectMany(x => x.Class.Select(p => p.IdClassFromMaster)).ToList();
//                var getHomeroomPathwaysFromDB = await _dbContext.Entity<MsHomeroomPathway>()
//                                                               .Include(p => p.Homeroom).ThenInclude(p => p.HomeroomStudents)
//                                                               .Where(p => getdataPathwaysFromXml.Any(x => x == p.IdGradePathwayDetail) &&
//                                                                           getidClassRoomsFromXml.Any(x => x == p.Homeroom.IdGradePathwayClassRoom) &&
//                                                                           p.Homeroom.IdGrade == item)
//                                                               .ToListAsync();

//                var classIdFromXml = model.Lesson.Select(p => p.ClassId).ToList();
//                var subjectFromXml = model.Lesson.Select(p => p.Subject.IdFromMasterData).ToList();
//                var getLessonFromDB = await _dbContext.Entity<MsLesson>()
//                                                      .Include(p => p.LessonPathways)
//                                                      .Include(p => p.LessonTeachers)
//                                                      .Where(p => classIdFromXml.Any(x => x == p.ClassIdGenerated) &&
//                                                                  subjectFromXml.Any(x => x == p.IdSubject) &&
//                                                                  p.IdGrade == item &&
//                                                                  p.IdAcademicYear == model.IdSchoolAcademicyears)
//                                                      .ToListAsync();

//                var datasemester = new List<int>();
//                if (Semesters.TryGetValue(item, out datasemester))
//                {
//                    foreach (var itemSemester in datasemester)
//                    {
//                        await SaveLessonNew(model, ascTimetableId, itemSemester, item, getLessonFromDB, getHomeroomPathwaysFromDB, weekVarian);
//                    }
//                }
//            }
//            _logger.LogInformation("[Trace] Processed Save Lesson ({0})", (DateTime.Now - lessonActionTime).ToString(@"hh\:mm\:ss"));
//            #endregion

//            return string.Empty;
//        }


//        /// <summary>
//        /// Proses data lesson
//        /// </summary>
//        /// <param name="model"></param>
//        /// <param name="ascTimetableId"></param>
//        /// <param name="Semester"></param>
//        /// <param name="idGrade"></param>
//        /// <returns></returns>
//        public async Task<string> SaveLessonNew(AddDataAscTimeTableAfterUploadRequest model,
//                                                 string ascTimetableId,
//                                                 int Semester,
//                                                 string idGrade,
//                                                 List<MsLesson> lessonFromDB,
//                                                 List<MsHomeroomPathway> homeroomPathwayFromDb,
//                                                 List<MsWeekVariantDetail> weekvarianDetail)
//        {

//            foreach (var itemLesson in model.Lesson.Where(p => p.Grade.IdFromMasterData == idGrade))
//            {
//                var getdataPathway = itemLesson.Class.SelectMany(p => p.PathwayLesson.Select(x => x.IdPathwayFromMaster)).ToList();
//                var getidClassRoom = itemLesson.Class.Select(p => p.IdClassFromMaster).ToList();

//                //get data homeroom pathway
//                var getdataHomeroomPathway = homeroomPathwayFromDb.Where(p => getdataPathway.Any(x => x == p.IdGradePathwayDetail) &&
//                                                                       getidClassRoom.Any(x => x == p.Homeroom.IdGradePathwayClassRoom) &&
//                                                                       p.Homeroom.IdGrade == idGrade &&
//                                                                       p.Homeroom.Semester == Semester)
//                                                           .ToList();

//                var homeroomStudent = getdataHomeroomPathway.SelectMany(p => p.Homeroom.HomeroomStudents).ToList();


//                if (model.SaveLesson)
//                {
//                    var getData = lessonFromDB.Where(p => p.ClassIdGenerated == itemLesson.ClassId &&
//                                                              p.Semester == Semester &&
//                                                              p.IdGrade == itemLesson.Grade.IdFromMasterData &&
//                                                              p.IdSubject == itemLesson.Subject.IdFromMasterData &&
//                                                              p.IdAcademicYear == model.IdSchoolAcademicyears)
//                                                  .FirstOrDefault();


//                    if (getData != null)
//                    {
//                        //update data lesson apa bila sudah ada datanya 

//                        _helperSave.UpdateLesson(getData, itemLesson, getdataHomeroomPathway);
//                        _helperSave.SaveLessonAsc(ascTimetableId, getData.Id);
//                        await SaveSchedule(model, ascTimetableId, getData.ClassIdGenerated, Semester, getData.Id, getData.IdSubject, getData.IdWeekVariant, new List<string>(), false, weekvarianDetail);
//                        SaveEnrollment(model, ascTimetableId, getData.ClassIdGenerated, Semester, getData.Id, itemLesson.IdForSave, homeroomStudent);

//                        await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                    }
//                    else
//                    {
//                        var dataLesson = _helperSave.SimpanLesson(itemLesson, model.IdSchoolAcademicyears, model.FormatIdClass, getdataHomeroomPathway, Semester);
//                        _helperSave.SaveLessonAsc(ascTimetableId, dataLesson.Id);
//                        await SaveSchedule(model, ascTimetableId, itemLesson.ClassId, Semester, dataLesson.Id, dataLesson.IdSubject, dataLesson.IdWeekVariant, new List<string>(), false, weekvarianDetail);
//                        SaveEnrollment(model, ascTimetableId, itemLesson.ClassId, Semester, dataLesson.Id, itemLesson.IdForSave, homeroomStudent);

//                        await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                    }


//                }
//                else
//                {
//                    //insert data pake id dari database lesson yg sudah ada
//                    var getData = lessonFromDB.Where(p => p.ClassIdGenerated == itemLesson.ClassId &&
//                                                             p.Semester == Semester &&
//                                                             p.IdGrade == itemLesson.Grade.IdFromMasterData &&
//                                                             p.IdSubject == itemLesson.Subject.IdFromMasterData &&
//                                                             p.IdAcademicYear == model.IdSchoolAcademicyears)
//                                                 .FirstOrDefault();

//                    if (getData != null)
//                    {
//                        _helperSave.SaveLessonAsc(ascTimetableId, getData.Id);
//                        var teacherLesson = getData.LessonTeachers.Select(p => p.IdUser).ToList();
//                        await SaveSchedule(model, ascTimetableId, itemLesson.ClassId, Semester, getData.Id, getData.IdSubject, getData.IdWeekVariant, teacherLesson, true, weekvarianDetail);
//                        SaveEnrollment(model, ascTimetableId, itemLesson.ClassId, Semester, getData.Id, itemLesson.IdForSave, homeroomStudent);

//                        await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
//                    }

//                }
//            }

//            return string.Empty;
//        }


//        public async Task<string> SaveSchedule(AddDataAscTimeTableAfterUploadRequest model,
//                                               string ascTimetableId,
//                                               string classId,
//                                               int Semester,
//                                               string lessonId,
//                                               string subjectId,
//                                               string idWeekVariant,
//                                               List<string> teacherLesson,
//                                               bool chekTeacherLesson,
//                                               List<MsWeekVariantDetail> weekvarianDetailFromDb)
//        {
//            if (model.SaveSchedule)
//            {
//                var getData = model.Schedule.SelectMany(schedule => schedule.Schedule.SelectMany(x => x.ListSchedule.Where(p => p.ClassID == classId && p.SubjectId == subjectId)), (schedule, data) => new { schedule, data }).ToList();

//                foreach (var item in getData)
//                {
//                    var getWeekvariandetail = new MsWeekVariantDetail();
//                    getWeekvariandetail = weekvarianDetailFromDb.FirstOrDefault(p => p.Week.Code == item.data.Weeks && p.IdWeekVariant == idWeekVariant);
//                    if (getWeekvariandetail is null)
//                    {
//                        getWeekvariandetail = weekvarianDetailFromDb.FirstOrDefault(p => p.Week.Code == item.data.Weeks);
//                    }

//                    //chek dulu teacher nya kalo simpan schedule tanpa simpan lesson ketika add asc
//                    //simpan kalo ada teacher nya di leson abaikan kalo ga ada teacher nya 
//                    if (!string.IsNullOrEmpty(item.data.IdDataFromMaster))
//                    {
//                        if (await _dbContext.Entity<MsSession>().AnyAsync(x => x.Id == item.data.IdDataFromMaster))
//                        {
//                            if (chekTeacherLesson)
//                            {
//                                if (teacherLesson.Any(p => p == item.data.Teacher.IdFromMasterData))
//                                {
//                                    //insert kalo blm ada datanya 
//                                    var dataSchedule = new MsSchedule();
//                                    dataSchedule.Id = Guid.NewGuid().ToString();
//                                    dataSchedule.IdVenue = item.data.Venue.IdFromMasterData;
//                                    dataSchedule.IdDay = item.data.DaysId;
//                                    dataSchedule.IdLesson = lessonId;
//                                    dataSchedule.IdSession = item.data.IdDataFromMaster;
//                                    dataSchedule.IdUser = item.data.Teacher.IdFromMasterData;
//                                    //dataSchedule.SessionNo = itemSession.Session;
//                                    dataSchedule.IdWeekVarianDetail = getWeekvariandetail?.Id;
//                                    dataSchedule.Semester = Semester;

//                                    _dbContext.Entity<MsSchedule>().Add(dataSchedule);

//                                    SaveScheduleAsc(ascTimetableId, dataSchedule.Id);
//                                }
//                            }
//                            else
//                            {
//                                //insert kalo blm ada datanya 
//                                var dataSchedule = new MsSchedule();
//                                dataSchedule.Id = Guid.NewGuid().ToString();
//                                dataSchedule.IdVenue = item.data.Venue.IdFromMasterData;
//                                dataSchedule.IdDay = item.data.DaysId;
//                                dataSchedule.IdLesson = lessonId;
//                                dataSchedule.IdSession = item.data.IdDataFromMaster;
//                                dataSchedule.IdUser = item.data.Teacher.IdFromMasterData;
//                                //dataSchedule.SessionNo = itemSession.Session;
//                                dataSchedule.IdWeekVarianDetail = getWeekvariandetail?.Id;
//                                dataSchedule.Semester = Semester;

//                                _dbContext.Entity<MsSchedule>().Add(dataSchedule);

//                                SaveScheduleAsc(ascTimetableId, dataSchedule.Id);
//                            }
//                        }
//                    }

//                    //insert kalo blm ada datanya 
//                    //var query = itemSession.Schedule.SelectMany(schedule => schedule.ListSchedule, (schedule, data) => new { schedule, data }).ToList();
//                    //foreach (var item in query)
//                    //{


//                    //}

//                }
//            }

//            return string.Empty;
//        }


//        public string SaveEnrollment(AddDataAscTimeTableAfterUploadRequest model, string ascTimetableId, string classId, int Semester, string lessonId, string lessonIdFromXMl, List<MsHomeroomStudent> student)
//        {
//            if (model.SaveStudentEnrolemnt)
//            {
//                foreach (var itemEnroll in model.Enrollment.EnrollmentData.Where(p => p.EnrollmentStudent.Any(x => x.ClassId == classId && x.LessonId == lessonIdFromXMl)))
//                {
//                    var getStudentHomeroom = student.FirstOrDefault(p => p.IdStudent == itemEnroll.Student.BinusianId);
//                    foreach (var item in itemEnroll.EnrollmentStudent.Where(p => p.ClassId == classId && p.LessonId == lessonIdFromXMl))
//                    {
//                        if (item.IsDataReadyFromMaster)
//                        {
//                            //insert kalo blm ada datanya 
//                            if (getStudentHomeroom != null)
//                            {
//                                var dataEnrollemnt = new MsHomeroomStudentEnrollment();
//                                dataEnrollemnt.Id = Guid.NewGuid().ToString();
//                                dataEnrollemnt.IdHomeroomStudent = getStudentHomeroom.Id;
//                                dataEnrollemnt.IdLesson = lessonId;
//                                dataEnrollemnt.Semester = Semester;
//                                dataEnrollemnt.IdSubject = item.SubjectID;
//                                dataEnrollemnt.IdSubjectLevel = item.SubjectLevelID;

//                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(dataEnrollemnt);

//                                SaveEnrolmentAsc(ascTimetableId, dataEnrollemnt.Id);
//                            }
//                        }
//                    }

//                }

//            }

//            return string.Empty;
//        }


//        /// <summary>
//        /// simpan data schedule asc
//        /// </summary>
//        /// <param name="ascTimetableId"></param>
//        /// <param name="idSchedule"></param>
//        /// <returns></returns>
//        public string SaveScheduleAsc(string ascTimetableId, string idSchedule)
//        {
//            var simpn = new TrAscTimetableSchedule();
//            simpn.Id = Guid.NewGuid().ToString();
//            simpn.IdAscTimetable = ascTimetableId;
//            simpn.IdSchedule = idSchedule;

//            _dbContext.Entity<TrAscTimetableSchedule>().Add(simpn);

//            return string.Empty;
//        }

//        /// <summary>
//        /// simpan data enrollment asc
//        /// </summary>
//        /// <param name="ascTimetableId"></param>
//        /// <param name="idHomeroomestudent"></param>
//        /// <returns></returns>
//        public string SaveEnrolmentAsc(string ascTimetableId, string idHomeroomestudent)
//        {
//            var simpn = new TrAscTimetableEnrollment();
//            simpn.Id = Guid.NewGuid().ToString();
//            simpn.IdAscTimetable = ascTimetableId;
//            simpn.IdHomeroomStudentEnrollment = idHomeroomestudent;

//            _dbContext.Entity<TrAscTimetableEnrollment>().Add(simpn);

//            return string.Empty;
//        }

//        /// <summary>
//        /// validate save schedule tanpa save lesson dan home room
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        private async Task<bool> ValidateSaveSchedule(AddDataAscTimeTableAfterUploadRequest model)
//        {
//            if (!await ValidateSaveStudentHomeRoom(model))
//            {
//                return false;
//            }


//            if (!await ValidateSaveStudentEnrollment(model))
//            {
//                return false;
//            }

//            return true;
//        }

//        /// <summary>
//        /// validasi save student home room tanpa simpan homeroom
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        private async Task<bool> ValidateSaveStudentHomeRoom(AddDataAscTimeTableAfterUploadRequest model)
//        {
//            var getGrade = model.Class.Select(p => p.Grade.IdFromMasterData).ToList();
//            var getPthwayClassRoom = model.Class.Select(p => p.Class.IdFromMasterData).ToList();
//            var getDataHomerome = await _dbContext.Entity<MsHomeroom>()
//                                   .Include(p => p.HomeroomPathways)
//                                   .Include(p => p.HomeroomStudents)
//                                   .Include(p => p.HomeroomTeachers)
//                                   .Where(p => p.IdAcademicYear == model.IdSchoolAcademicyears &&
//                                              getGrade.Any(x => x == p.IdGrade) &&
//                                              getPthwayClassRoom.Any(x => x == p.IdGradePathwayClassRoom))
//                                   .ToListAsync(_cancellationToken);

//            if (!getDataHomerome.Any())
//            {
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        /// validasi student enrolment kalo save tanpa lesson disimpan juga 
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        private async Task<bool> ValidateSaveStudentEnrollment(AddDataAscTimeTableAfterUploadRequest model)
//        {
//            var classIds = model.Lesson.Select(p => p.ClassId).ToList();
//            var gradeLesson = model.Lesson.Select(p => p.Grade.IdFromMasterData).Distinct().ToList();
//            var getDataLesson = await _dbContext.Entity<MsLesson>()
//                                                     .Include(p => p.LessonPathways)
//                                                     .Include(p => p.LessonTeachers)
//                                                     .Where(p => classIds.Any(x => x == p.ClassIdGenerated) &&
//                                                                 gradeLesson.Any(x => x == p.IdGrade) &&
//                                                                 p.IdAcademicYear == model.IdSchoolAcademicyears)
//                                                     .ToListAsync(_cancellationToken);

//            if (!getDataLesson.Any())
//            {
//                return false;
//            }
//            return true;
//        }
//    }
//}
