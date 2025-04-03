using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnAscTimetable;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Helpers;
using BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable
{
    // TODO: drop this class after migration to durable function
    public class UpdateAscTimetableHandler
    {
#if DEBUG
        private const string _blobPath = "asc-timetable-debug/update/{name}.json";
#else
        private const string _blobPath = "asc-timetable/update/{name}.json";
#endif

        private static readonly Lazy<List<string>> _emailTags = new Lazy<List<string>>(new List<string>() { "ASC Timetable" });
        private static readonly Lazy<List<EmailAddress>> _emailRecipients = new Lazy<List<EmailAddress>>(new List<EmailAddress>()
        {
            // new EmailAddress("taufik@radyalabs.id"),
            // new EmailAddress("cilla.azzahra@radyalabs.id"),
            // new EmailAddress("siti@radyalabs.id"),
            // new EmailAddress("tri@radyalabs.id"),
            // new EmailAddress("mianti.juliansa@binus.edu"),
            // new EmailAddress("indra.gunawan@binus.edu")
            new EmailAddress("itdevschool@binus.edu")
        });
        private static readonly Lazy<List<string>> _ignoredTables = new Lazy<List<string>>(new List<string>()
        {
            "TrAsctimetable",
            "TrAsctimetableLesson",
            "TrAsctimetableHomeroom",
            "TrAsctimetableSchedule",
            "TrAsctimetableDayStructure",
            "TrAsctimetableEnrollment",
            "TrAsctimetableProcess",
            "MsSchedule"
        });

        private IDbContextTransaction _transaction;
        private CancellationToken _cancellationToken;
        private string _idUser, _idSchool;

        private readonly ISchedulingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UpdateAscTimetableHandler> _logger;
        private readonly IApiService<ITeacherPosition> _teacherPosition;
        private readonly IApiService<IMessage> _messageService;
        private readonly IApiService<IAscTimetable> _ascService;
        private readonly HelperSaveXML _helperSave;
        private readonly INotificationManager _notificationManager;

        public UpdateAscTimetableHandler(ISchedulingDbContext dbContext,
            IStringLocalizer localizer,
            IConfiguration configuration,
            ILogger<UpdateAscTimetableHandler> logger,
            IApiService<IMessage> messageService,
            IApiService<ITeacherPosition> teacherPosition,
            IApiService<IAscTimetable> ascService,
            INotificationManager notificationManager)
        {
            _dbContext = dbContext;
            _localizer = localizer;
            _configuration = configuration;
            _logger = logger;
            _teacherPosition = teacherPosition;
            _notificationManager = notificationManager;
            _messageService = messageService;
            _ascService = ascService;
            _helperSave = new HelperSaveXML(_dbContext);
        }

        [FunctionName(nameof(UpdateAscTimetable))]
        public async Task UpdateAscTimetable([BlobTrigger(_blobPath)] Stream blobStream,
            IDictionary<string, string> metadata,
            string name,
            CancellationToken cancellationToken)
        {
            var body = default(AddReUploadXmlRequest);
            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();
            var idProcess = string.Empty;
            _cancellationToken = cancellationToken;

            try
            {
                _logger.LogInformation("[Blob] Processing update Asc Timetable: {0}", name);

#if DEBUG
                _idUser = "SASERPONG";
                _idSchool = "2";
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
                        body = new JsonSerializer().Deserialize<AddReUploadXmlRequest>(jsonReader);
                        break;
                    }
                }

                // validate body
                (await new AddReUploadXmlValidator().ValidateAsync(body, cancellationToken)).EnsureValid(localizer: _localizer);

                // do save asc timetable
                _teacherPosition.SetConfigurationFrom(apiConfig);

                // set job started
                var startProcess = await _ascService.SetConfigurationFrom(apiConfig).Execute.StartProcess(new StartAscTimetableProcessRequest
                {
                    IdSchool = _idSchool,
                    Grades = body.Lesson.Select(x => x.Grade.IdFromMasterData).Distinct().ToList()
                });
                if (!startProcess.IsSuccess)
                    throw new BadRequestException(startProcess.Message.Replace("upload", "re upload"));
                idProcess = startProcess.Payload;

                var semseter = new Dictionary<string, List<int>>();
                _transaction = await _dbContext.BeginTransactionAsync(cancellationToken, System.Data.IsolationLevel.ReadUncommitted);

                //get garde
                var gradeID = body.Class.Where(p => p.IsDataReadyFromMaster).Select(p => p.Grade).GroupBy(p => p.IdFromMasterData).Select(p => p.FirstOrDefault()).ToList();
                var gradeIds = gradeID.Select(x => x.IdFromMasterData).ToList();
                var gradeSemesters = await _dbContext.Entity<MsPeriod>()
                                                         .Where(x => gradeIds.Contains(x.IdGrade))
                                                         .Select(x => new { idGrade = x.IdGrade, semester = x.Semester })
                                                         .ToListAsync(cancellationToken);
                foreach (var item in gradeID)
                {
                    if (!gradeSemesters.Any(x => x.idGrade == item.IdFromMasterData))
                        throw new BadRequestException($"Semester for Grade {item.Code} not found ,Please add in Period Setting");

                    semseter.Add(item.IdFromMasterData, gradeSemesters.Where(x => x.idGrade == item.IdFromMasterData).Select(x => x.semester).Distinct().ToList());
                }

                var getDataAsc = await _dbContext.Entity<TrAscTimetable>()
                                                 .Where(p => p.Id == body.IdSchool)
                                                 .FirstOrDefaultAsync(cancellationToken);
                if (getDataAsc != null)
                {
                    getDataAsc.AscTimetableHomerooms = await _dbContext.Entity<TrAscTimetableHomeroom>()
                                                                       .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers)
                                                                       .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomPathways)
                                                                       .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomStudents)
                                                                       .Where(x => x.IdAscTimetable == getDataAsc.Id)
                                                                       .ToListAsync(cancellationToken);

                    getDataAsc.AscTimetableLessons = await _dbContext.Entity<TrAscTimetableLesson>()
                                                                     .Include(x => x.Lesson).ThenInclude(x => x.LessonTeachers)
                                                                     .Include(x => x.Lesson).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.HomeroomPathway)
                                                                     .Where(x => x.IdAscTimetable == getDataAsc.Id)
                                                                     .ToListAsync(cancellationToken);
                }

                var getposition = await _teacherPosition.Execute.GetPositionCAforAsc(new Data.Model.Teaching.FnAssignment.TeacherPosition.GetCAForAscTimetableRequest
                {
                    IdSchool = getDataAsc.IdSchool
                });

                if (!getposition.IsSuccess)
                    throw new BadRequestException(getposition.Message);


                var getLessonId = GetIdLesson(body, getDataAsc, semseter);
                RemoveEnrolment(getDataAsc.Id, getLessonId);
                RemoveSchedule(getDataAsc.Id, getLessonId);
                await _dbContext.SaveChangesAsync(_ignoredTables.Value, cancellationToken);

                await UpdateData(body, getDataAsc, semseter, getposition.Payload);

                //set process job finished immediatelly
                var process = await _dbContext.Entity<TrAscTimetableProcess>()
                                          .Where(x => x.Id == idProcess)
                                          .SingleOrDefaultAsync();
                if (process is null)
                    throw new NotFoundException("process is not found");

                process.FinishedAt = DateTime.Now;
                _dbContext.Entity<TrAscTimetableProcess>().Update(process);

                await _dbContext.SaveChangesAsync(_ignoredTables.Value, cancellationToken);
                await _transaction.CommitAsync(cancellationToken);


                // send message to user that save asc timetable
                _ = _messageService
                    .SetConfigurationFrom(apiConfig)
                    .Execute.AddMessage(new AddMessageRequest
                    {
                        Type = UserMessageType.AscTimetable,
                        IdSender = Guid.Empty.ToString(),
                        Recepients = new List<string> { _idUser },
                        Subject = "Re-Upload ASC Timetable Success",
                        Content = $"Re-Upload ASC Timetable {body.Name} Success.",
                        Attachments = new List<MessageAttachment>(), // attachments can't be null
                        IsSetSenderAsSchool = true,
                        IsDraft = false,
                        IsAllowReply = false,
                        IsMarkAsPinned = false
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Blob] " + ex.Message);
                try
                {
                    _transaction?.Rollback();
                }
                catch { }

                //set process job finished if started
                if (!string.IsNullOrEmpty(idProcess))
                    _ = await _ascService.SetConfigurationFrom(apiConfig).Execute.FinishProcess(new FinishAscTimetableProcessRequest
                    {
                        IdProcess = idProcess
                    });


                var subjectMessage = $"Re-Upload ASC Timetable {body?.Name} Failed";

                // send message to user that save asc timetable
                _ = _messageService
                    .SetConfigurationFrom(apiConfig)
                    .Execute.AddMessage(new AddMessageRequest
                    {
                        Type = UserMessageType.AscTimetable,
                        IdSender = Guid.Empty.ToString(),
                        Recepients = new List<string> { _idUser },
                        Subject = subjectMessage,
                        Content = $"Re-Upload ASC Timetable {body?.Name} Failed, please contact IT Admin or re-upload.",
                        Attachments = new List<MessageAttachment>(), // attachments can't be null
                        IsSetSenderAsSchool = true,
                        IsDraft = false,
                        IsAllowReply = false,
                        IsMarkAsPinned = false
                    });

                // send email to developers
                var emailMessage = new SendGridMessage
                {
                    Subject = subjectMessage,
                    PlainTextContent =
                        $"Message: {ex.Message} {Environment.NewLine}Stack Trace: {ex.StackTrace} {Environment.NewLine}{Environment.NewLine}" +
                        $"Inner Message: {ex.InnerException?.Message} {Environment.NewLine}Inner Stack Trace: {ex.InnerException?.StackTrace}",
                    Categories = _emailTags.Value,
                };
                emailMessage.AddTos(_emailRecipients.Value);
                _ = _notificationManager.SendEmail(emailMessage);

                // rethrow the exception
                throw;
            }
        }

        public string SaveScheduleAsc(string ascTimetableId, string idSchedule)
        {
            var simpn = new TrAscTimetableSchedule();
            simpn.Id = Guid.NewGuid().ToString();
            simpn.IdAscTimetable = ascTimetableId;
            simpn.IdSchedule = idSchedule;

            _dbContext.Entity<TrAscTimetableSchedule>().Add(simpn);

            return string.Empty;
        }

        public string SaveEnrolmentAsc(string ascTimetableId, string idHomeroomestudent)
        {
            var simpn = new TrAscTimetableEnrollment();
            simpn.Id = Guid.NewGuid().ToString();
            simpn.IdAscTimetable = ascTimetableId;
            simpn.IdHomeroomStudentEnrollment = idHomeroomestudent;

            _dbContext.Entity<TrAscTimetableEnrollment>().Add(simpn);

            return string.Empty;
        }


        public string RemoveSchedule(string idAscTimeTable, List<string> idLesson)
        {
            var getdata = _dbContext.Entity<TrAscTimetableSchedule>()
                                   .Include(p => p.Schedule)
                                   .Where(p => p.IdAscTimetable == idAscTimeTable && p.IsFromMaster == false &&
                                              idLesson.Any(x => x == p.Schedule.IdLesson)).ToList();

            foreach (var item in getdata)
            {
                item.IsActive = false;
                _dbContext.Entity<TrAscTimetableSchedule>().Update(item);

                var dataSchedule = item.Schedule;
                dataSchedule.IsActive = false;
                _dbContext.Entity<MsSchedule>().Update(dataSchedule);
            }

            return string.Empty;
        }

        public string RemoveEnrolment(string idAscTimeTable, List<string> idLesson)
        {
            var getdata = _dbContext.Entity<TrAscTimetableEnrollment>()
                                    .Include(p => p.HomeroomStudentEnrollment)
                                    .Where(p => p.IdAscTimetable == idAscTimeTable && p.HomeroomStudentEnrollment.IsFromMaster == false &&
                                               idLesson.Any(x => x == p.HomeroomStudentEnrollment.IdLesson)).ToList();
            foreach (var item in getdata)
            {
                item.IsActive = false;
                _dbContext.Entity<TrAscTimetableEnrollment>().Update(item);

                var dataEnrolment = item.HomeroomStudentEnrollment;
                dataEnrolment.IsActive = false;
                _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(dataEnrolment);
            }

            return string.Empty;
        }

        private List<string> GetIdLesson(AddReUploadXmlRequest model, TrAscTimetable ascTimetable, Dictionary<string, List<int>> Semesters)
        {
            var result = new List<string>();
            foreach (var itemHomeRoom in model.Class)
            {
                var datasemester = new List<int>();
                if (Semesters.TryGetValue(itemHomeRoom.Grade.IdFromMasterData, out datasemester))
                {
                    foreach (var semester in datasemester)
                    {
                        var getdata = ascTimetable.AscTimetableHomerooms.Where(p => p.Homeroom != null).Where(p => p.Homeroom.IdGrade == itemHomeRoom.Grade.IdFromMasterData &&
                                                                              p.Homeroom.IdAcademicYear == ascTimetable.IdAcademicYear &&
                                                                              p.Homeroom.Semester == semester &&
                                                                              p.Homeroom.IdGradePathway == itemHomeRoom.Grade.IdGradePathway &&
                                                                              p.Homeroom.IdGradePathwayClassRoom == itemHomeRoom.Class.IdFromMasterData)
                                                                   .FirstOrDefault();
                        if (getdata != null)
                        {
                            var listLesson = model.Lesson.Where(p => p.Class.Any(x => x.IdClassForSave == itemHomeRoom.IdClassForSave)).ToList();
                            foreach (var itemLesson in listLesson)
                            {
                                var getdatalesson = ascTimetable.AscTimetableLessons.Where(p => p.Lesson != null).Where(p => p.Lesson.ClassIdGenerated == itemLesson.ClassId &&
                                                                                       p.Lesson.IdAcademicYear == ascTimetable.IdAcademicYear &&
                                                                                       p.Lesson.Semester == semester &&
                                                                                       p.Lesson.IdSubject == itemLesson.Subject.IdFromMasterData &&
                                                                                       p.Lesson.IdGrade == itemLesson.Grade.IdFromMasterData)
                                                                            .FirstOrDefault();
                                if (getdatalesson != null)
                                {
                                    result.Add(getdatalesson.Lesson.Id);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// update data re upload xml 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ascTimetable"></param>
        /// <param name="Semesters"></param>
        /// <param name="CaPosition"></param>
        /// <returns></returns>
        private async Task UpdateData(AddReUploadXmlRequest model, TrAscTimetable ascTimetable, Dictionary<string, List<int>> Semesters, string CaPosition)
        {
            #region Proses update data Home room
            var homeroomActionTime = DateTime.Now;
            _logger.LogInformation("[Trace] Processing Save Homeroom");

            //get data dari database untuk di update 
            var listdataUpdate = new List<TrAscTimetableHomeroom>();
            var HomeRooms = ascTimetable.AscTimetableHomerooms.Where(p => p.Homeroom != null).ToList();
            var grade = model.Class.Select(p => p.Grade.Id).ToArray();
            var gradePathway = model.Class.Select(p => p.Grade.IdGradePathway).ToArray();
            var classmd = model.Class.Select(p => p.Class.IdFromMasterData).ToArray();

            var getDataInUpdate = HomeRooms.Where(p => grade.Contains(p.Homeroom.IdGrade) &&
                                                    gradePathway.Contains(p.Homeroom.IdGradePathway) &&
                                                    p.Homeroom.IdAcademicYear == ascTimetable.IdAcademicYear &&
                                                    classmd.Contains(p.Homeroom.IdGradePathwayClassRoom)).ToList();


            //var dataUnutkDihapusDarimappingAsc = HomeRooms.Where(p => !grade.Contains(p.Homeroom.IdGrade) &&
            //                                       !gradePathway.Contains(p.Homeroom.IdGradePathway) &&
            //                                       p.Homeroom.IdAcademicYear == ascTimetable.IdAcademicYear &&
            //                                       !classmd.Contains(p.Homeroom.IdGradePathwayClassRoom)).ToList();

            //get data dari xml unutk di insert
            var listdataInsert = new List<ClassPackageVm>();
            var getdataUnutkInsert = model.Class.ToArray().Where(p => ascTimetable.AscTimetableHomerooms.ToArray()
                                                           .All(x => x.Homeroom.IdGrade != p.Grade.IdFromMasterData &&
                                                                     x.Homeroom.IdGradePathway != p.Grade.IdGradePathway &&
                                                                     x.Homeroom.IdGradePathwayClassRoom != p.Class.IdFromMasterData))
                                                           .ToList();



            foreach (var itemHomeRoom in model.Class)
            {
                var datasemester = new List<int>();
                if (Semesters.TryGetValue(itemHomeRoom.Grade.IdFromMasterData, out datasemester))
                {
                    foreach (var semester in datasemester)
                    {
                        #region proses update data Home room

                        if (getDataInUpdate.Count > 0)
                        {
                            //proses yg terjadi apabila terdapat data yang memang harus di update setelah hasil compare di atas
                            var prosesUpdate = getDataInUpdate.Where(p => p.Homeroom.IdGrade == itemHomeRoom.Grade.IdFromMasterData &&
                                                                                  p.Homeroom.IdAcademicYear == ascTimetable.IdAcademicYear &&
                                                                                  p.Homeroom.Semester == semester &&
                                                                                  p.Homeroom.IdGradePathway == itemHomeRoom.Grade.IdGradePathway &&
                                                                                  p.Homeroom.IdGradePathwayClassRoom == itemHomeRoom.Class.IdFromMasterData)
                                                                       .FirstOrDefault();
                            if (prosesUpdate != null)
                            {

                                var homeromestudent = new List<MsHomeroomStudent>();
                                var homerome = prosesUpdate.Homeroom;

                                homeromestudent = _helperSave.UpdateDataHomeRoom(homerome, itemHomeRoom, semester, CaPosition);
                            }
                        }
                        #endregion

                        #region Proses Insert data dari xml 
                        //kalo data setelah di kompare dan ada data yg unutk di insert maka lakukan proses insert data home rome 
                        if (getdataUnutkInsert.Count > 0)
                        {
                            //get data home rome dari data list yg insert berdasarkan data looping 
                            var procesInsert = getdataUnutkInsert.FirstOrDefault(p => p == itemHomeRoom);
                            if (procesInsert != null)
                            {
                                //cek data terlebih dahulu yg mau di insert
                                //ada atau tidak di table master nya 
                                var getDataHomerome = await _dbContext.Entity<MsHomeroom>()
                                       .Include(p => p.HomeroomPathways)
                                       .Include(p => p.HomeroomStudents).ThenInclude(p => p.HomeroomStudentEnrollments).ThenInclude(p => p.AscTimetableEnrollments)
                                       .Include(p => p.HomeroomTeachers)
                                       .Where(p => p.IdAcademicYear == model.IdSchoolAcademicyears &&
                                                  p.IdGrade == itemHomeRoom.Grade.IdFromMasterData &&
                                                  p.IdGradePathwayClassRoom == itemHomeRoom.Class.IdFromMasterData &&
                                                  p.Semester == semester).FirstOrDefaultAsync(_cancellationToken);

                                if (getDataHomerome != null)
                                {
                                    //kalu ada maka gunakan data yg sudah ada dan update data teacher nya sesuai dari xml 
                                    var homeromestudent = new List<MsHomeroomStudent>();

                                    homeromestudent = _helperSave.UpdateDataHomeRoom(getDataHomerome, itemHomeRoom, semester, CaPosition);

                                    //add data homerome tadi ke asc time table yg di re upload xml 
                                    _helperSave.SaveHomerromAsc(ascTimetable.Id, getDataHomerome.Id);
                                }
                                else
                                {
                                    //kalo tidak ada berarti bikin data baru berdasarkan data xml
                                    var homeRome = _helperSave.SaveHomeRoomData(itemHomeRoom, semester, model.IdSchoolAcademicyears, CaPosition);

                                    //save data home rome pathway
                                    var idHomeromepathway = _helperSave.SaveHomeRoomPathway(itemHomeRoom.Pathway, homeRome);

                                    //kalo lebih dari satu maka simpan satu saja 
                                    var homeromestudent = _helperSave.SaveHomeRoomStudent(itemHomeRoom.StudentInHomeRoom, homeRome, semester);

                                    _helperSave.SaveHomerromAsc(ascTimetable.Id, homeRome);
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
            _logger.LogInformation("[Trace] Processed Save Homeroom ({0})", (DateTime.Now - homeroomActionTime).ToString(@"hh\:mm\:ss"));

            #endregion

            #region Proses Update Data Lesson 
            var lessonActionTime = DateTime.Now;
            _logger.LogInformation("[Trace] Processing Save Lesson");

            var getGradeLesson = model.Lesson.Select(p => p.Grade).GroupBy(p => p.IdFromMasterData).Select(p => p.Key).ToList();
            var processedIdLessons = new List<string>();
            foreach (var item in getGradeLesson)
            {
                var datasemester = new List<int>();
                if (Semesters.TryGetValue(item, out datasemester))
                {
                    foreach (var itemSemester in datasemester)
                    {
                        await UpdateDataLesson(model, ascTimetable, itemSemester, item, processedIdLessons);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(_ignoredTables.Value, _cancellationToken);
            _logger.LogInformation("[Trace] Processed Save Lesson ({0})", (DateTime.Now - lessonActionTime).ToString(@"hh\:mm\:ss"));
            #endregion
        }

        private async Task UpdateDataLesson(AddReUploadXmlRequest model, TrAscTimetable ascTimetable, int Semester, string idGrade, List<string> processedIdLessons)
        {
            var lesonXML = model.Lesson.Where(p => p.Grade.IdFromMasterData == idGrade).ToArray();

            var classids = lesonXML.Select(p => p.ClassId).Distinct().ToArray();
            var subject = lesonXML.Select(p => p.Subject.IdFromMasterData).Distinct().ToArray();
            var grade = lesonXML.Select(p => p.Grade.IdFromMasterData).Distinct().ToList();




            //var lessonInDatabase = ascTimetable.AscTimetableLessons.Where(p => p.Lesson != null).Where(p => p.Lesson.LessonPathways.All(x => x.HomeroomPathway.IdHomeroom == homeRoomid)).ToList();
            var lessonInDatabase = ascTimetable.AscTimetableLessons.Where(p => p.Lesson != null).Where(p => p.Lesson.IdGrade == idGrade && p.Lesson.Semester == Semester).ToList();

            var classidsdb = lessonInDatabase.Select(p => p.Lesson.ClassIdGenerated).Distinct().ToArray();
            var subjectdb = lessonInDatabase.Select(p => p.Lesson.IdSubject).Distinct().ToArray();
            var gradedb = lessonInDatabase.Select(p => p.Lesson.IdGrade).Distinct().ToArray();

            //getdata lesson yg dari existing unutk di hapus 

            var deletedDataLesson = lessonInDatabase.Where(p => classids.Any(x => x != p.Lesson.ClassIdGenerated) &&
                                                              subject.Any(x => x != p.Lesson.IdSubject) &&
                                                              grade.Any(x => x != p.Lesson.IdGrade))
                                                  .ToList();


            //get data unutk di update data nya 
            var dataUpdateLesson = lessonInDatabase.Where(p => classids.Any(x => x == p.Lesson.ClassIdGenerated) &&
                                                                subject.Any(x => x == p.Lesson.IdSubject) &&
                                                                grade.Any(x => x == p.Lesson.IdGrade))
                                                               .ToList();



            //get data unutk di insert ke lesson 
            var dataForInsert = new List<LessonUploadXMLVm>();
            if (lessonInDatabase.Count > 0)
            {
                dataForInsert = lesonXML.Where(p => classidsdb.Any(x => x != p.ClassId) &&
                                                    subjectdb.Any(x => x != p.Subject.IdFromMasterData) &&
                                                    gradedb.Any(x => x != p.Grade.IdFromMasterData)).ToList();
            }
            else
            {
                dataForInsert = lesonXML.ToList();
            }


            //get data unutk di insert ke lesson 

            foreach (var itemLesson in lesonXML)
            {
                if (itemLesson.ClassId == "9Hin.A")
                {

                }
                //get data homeroom pathway
                var getdataPathway = itemLesson.Class.SelectMany(p => p.PathwayLesson.Select(x => x.IdPathwayFromMaster)).ToList();
                var getidClassRoom = itemLesson.Class.Select(p => p.IdClassFromMaster).ToList();

                //get data homeroom pathway
                var getdataHomeroomPathway = await _dbContext.Entity<MsHomeroomPathway>()
                                                           .Include(p => p.Homeroom).ThenInclude(p => p.HomeroomStudents)
                                                           .Include(p => p.Homeroom).ThenInclude(p => p.AscTimetableHomerooms)
                                                           .Where(p => getdataPathway.Any(x => x == p.IdGradePathwayDetail) &&
                                                                       getidClassRoom.Any(x => x == p.Homeroom.IdGradePathwayClassRoom) &&
                                                                       p.Homeroom.IdGrade == idGrade &&
                                                                       p.Homeroom.Semester == Semester &&
                                                                       p.Homeroom.AscTimetableHomerooms.Any(x => x.IdAscTimetable == ascTimetable.Id))
                                                           .ToListAsync(_cancellationToken);

                var homeroomStudent = getdataHomeroomPathway.SelectMany(p => p.Homeroom.HomeroomStudents).ToList();

                #region Proses insert lesson 

                if (dataForInsert.Count > 0)
                {
                    var procesInsert = dataForInsert.FirstOrDefault(p => p == itemLesson);
                    if (procesInsert != null)
                    {
                        var getdata = await _dbContext.Entity<MsLesson>()
                                                      .Include(p => p.LessonPathways)
                                                      .Include(p => p.LessonTeachers)
                                                      .Where(p => p.ClassIdGenerated == itemLesson.ClassId &&
                                                                  p.Semester == Semester &&
                                                                  p.IdGrade == itemLesson.Grade.IdFromMasterData &&
                                                                  p.IdSubject == itemLesson.Subject.IdFromMasterData &&
                                                                  p.IdAcademicYear == model.IdSchoolAcademicyears)
                                                      .FirstOrDefaultAsync(_cancellationToken);



                        if (getdata != null)
                        {

                            //update data lesson apa bila sudah ada datanya dan belum di update
                            if (!processedIdLessons.Contains(getdata.Id))
                                _helperSave.UpdateLesson(getdata, itemLesson, getdataHomeroomPathway);

                            _helperSave.SaveLessonAsc(ascTimetable.Id, getdata.Id);

                            await UpdateDataSchedule(model, ascTimetable, Semester, getdata.Id, getdata.ClassIdGenerated, getdata.IdWeekVariant, itemLesson.IdForSave);
                            UpdateEnrollment(model, ascTimetable, getdata.ClassIdGenerated, Semester, getdata.Id, itemLesson.IdForSave, homeroomStudent);

                            processedIdLessons.Add(getdata.Id);
                        }
                        else
                        {

                            var dataLesson = _helperSave.SimpanLesson(itemLesson, ascTimetable.IdAcademicYear, ascTimetable.FormatClassName, getdataHomeroomPathway, Semester);
                            _helperSave.SaveLessonAsc(ascTimetable.Id, dataLesson.Id);
                            await UpdateDataSchedule(model, ascTimetable, Semester, dataLesson.Id, dataLesson.ClassIdGenerated, dataLesson.IdWeekVariant, itemLesson.IdForSave);
                            UpdateEnrollment(model, ascTimetable, dataLesson.ClassIdGenerated, Semester, dataLesson.Id, itemLesson.IdForSave, homeroomStudent);

                            processedIdLessons.Add(dataLesson.Id);
                        }
                    }
                }

                #endregion

                #region Proses Update data
                if (dataUpdateLesson.Count > 0)
                {
                    var getdata = dataUpdateLesson.Where(p => p.Lesson.ClassIdGenerated == itemLesson.ClassId &&
                                                                           p.Lesson.IdAcademicYear == ascTimetable.IdAcademicYear &&
                                                                           p.Lesson.Semester == Semester &&
                                                                           p.Lesson.IdSubject == itemLesson.Subject.IdFromMasterData &&
                                                                           p.Lesson.IdGrade == itemLesson.Grade.IdFromMasterData)
                                                                   .FirstOrDefault();



                    if (getdata != null)
                    {
                        var lesson = getdata.Lesson;

                        if (!processedIdLessons.Contains(lesson.Id))
                            _helperSave.UpdateLesson(lesson, itemLesson, getdataHomeroomPathway);

                        await UpdateDataSchedule(model, ascTimetable, Semester, getdata.Lesson.Id, getdata.Lesson.ClassIdGenerated, getdata.Lesson.IdWeekVariant, itemLesson.IdForSave);
                        UpdateEnrollment(model, ascTimetable, getdata.Lesson.ClassIdGenerated, Semester, getdata.Lesson.Id, itemLesson.IdForSave, homeroomStudent);

                        processedIdLessons.Add(lesson.Id);
                    }
                }
                #endregion

                #region Remove Data Lesson
                if (deletedDataLesson.Count > 0)
                {
                    foreach (var item in deletedDataLesson)
                    {
                        item.IsActive = false;
                        _dbContext.Entity<TrAscTimetableLesson>().Update(item);
                    }
                }
                #endregion

            }
        }

        private async Task<string> UpdateDataSchedule(AddReUploadXmlRequest model, TrAscTimetable ascTimetable, int Semester, string idLesson, string Classid, string idWeekVariant, string lesonFromXml)
        {
            var getData = model.Schedule.SelectMany(schedule => schedule.Schedule.SelectMany(x => x.ListSchedule.Where(p => p.ClassID == Classid && p.LesonId == lesonFromXml)), (schedule, data) => new { schedule, data }).ToList();

            foreach (var item in getData)
            {
                var getWeekvariandetail = new MsWeekVariantDetail();
                getWeekvariandetail = await _dbContext.Entity<MsWeekVariantDetail>()
                                                         .Include(p => p.Week).Where(p => p.Week.Code == item.data.Weeks && p.IdWeekVariant == idWeekVariant)
                                                         .FirstOrDefaultAsync(_cancellationToken);
                if (getWeekvariandetail is null)
                {
                    getWeekvariandetail = await _dbContext.Entity<MsWeekVariantDetail>()
                                                        .Include(p => p.Week)
                                                        .Where(p => p.Week.Code == item.data.Weeks)
                                                        .FirstOrDefaultAsync(_cancellationToken);
                }

                //insert kalo blm ada datanya 
                var dataSchedule = new MsSchedule();
                dataSchedule.Id = Guid.NewGuid().ToString();
                dataSchedule.IdVenue = item.data.Venue.IdFromMasterData;
                dataSchedule.IdDay = item.data.DaysId;
                dataSchedule.IdLesson = idLesson;
                dataSchedule.IdSession = item.data.IdDataFromMaster;
                dataSchedule.IdUser = item.data.Teacher.IdFromMasterData;
                //dataSchedule.SessionNo = itemSession.Session;
                dataSchedule.IdWeekVarianDetail = getWeekvariandetail?.Id;
                dataSchedule.IdWeek = getWeekvariandetail?.IdWeek;
                dataSchedule.Semester = Semester;

                _dbContext.Entity<MsSchedule>().Add(dataSchedule);

                SaveScheduleAsc(ascTimetable.Id, dataSchedule.Id);

                ////insert kalo blm ada datanya 
                //var query = itemSchedule.Schedule.Where(x => x.ListSchedule.Any(p => p.ClassID == Classid)).SelectMany(schedule => schedule.ListSchedule, (schedule, data) => new { schedule, data }).ToList();
                //foreach (var item in query)
                //{


                //}

            }
            return string.Empty;
        }

        private string UpdateEnrollment(AddReUploadXmlRequest model, TrAscTimetable ascTimetable, string classId, int Semester, string lessonId, string lessonIdFromXMl, List<MsHomeroomStudent> homeromestudent)
        {

            foreach (var itemEnroll in model.Enrollment.EnrollmentData.Where(p => p.EnrollmentStudent.Any(x => x.ClassId == classId && x.LessonId == lessonIdFromXMl)))
            {

                var getStudentHomeroom = homeromestudent.FirstOrDefault(p => p.IdStudent == itemEnroll.Student.BinusianId);

                foreach (var item in itemEnroll.EnrollmentStudent.Where(p => p.ClassId == classId && p.LessonId == lessonIdFromXMl))
                {

                    if (item.IsDataReadyFromMaster)
                    {
                        //insert kalo blm ada datanya 
                        if (getStudentHomeroom != null)
                        {
                            var dataEnrollemnt = new MsHomeroomStudentEnrollment();
                            dataEnrollemnt.Id = Guid.NewGuid().ToString();
                            dataEnrollemnt.IdHomeroomStudent = getStudentHomeroom.Id;
                            dataEnrollemnt.IdLesson = lessonId;
                            //dataEnrollemnt.Semester = Semester;
                            dataEnrollemnt.IdSubject = item.SubjectID;
                            dataEnrollemnt.IdSubjectLevel = item.SubjectLevelID == "" ? null : item.SubjectLevelID; 

                            _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(dataEnrollemnt);

                            SaveEnrolmentAsc(ascTimetable.Id, dataEnrollemnt.Id);
                        }
                    }
                }

            }

            return string.Empty;
        }
    }
}
