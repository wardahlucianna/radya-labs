using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Constants;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using NPOI.SS.Formula.Functions;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using Microsoft.Azure.WebJobs;
using NPOI.POIFS.FileSystem;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using NPOI.Util;
using NPOI.HPSF;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.OData;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDownloadLessonPlanSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _containerPath;
        private readonly string _documentStorageConnection;

        public GetDownloadLessonPlanSummaryHandler(ITeachingDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;

            _containerPath = "lesson-plan-download";
#if DEBUG
            _documentStorageConnection = configuration["ConnectionStrings:Teaching:AccountStorageDocument"];
            //_documentStorageConnection = "UseDevelopmentStorage=true";
#else 
            _documentStorageConnection = configuration["ConnectionStrings:Teaching:AccountStorageDocument"];
#endif
        }

        #region config create container
        public void CreateContainerIfNotExists(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_documentStorageConnection);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            var perm = new BlobContainerPermissions();
            perm.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(perm);

            blobContainer.CreateIfNotExistsAsync();
        }
        #endregion

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDownloadLessonPlanSummaryRequest>(nameof(GetDownloadLessonPlanSummaryRequest));

            var listLessonByUser = new List<LessonByUser>();
            var listGradeSubject = new List<LessonByGradeSubject>();
            var listSubjectGrade = new List<LessonBySubjectGrade>();
            var listSubjectGradeNew = new List<LessonBySubjectGradeNew>();
            string zipFileName = string.Empty;
            string blobUrl;

            CreateContainerIfNotExists(_containerPath);

            listLessonByUser = await GetLessonByUser(_dbContext, CancellationToken, param.IdUser, param.IdAcademicYear, param.PositionCode);

            if (param.IsGrade)
            {
                zipFileName = $"Lesson_Plan_Summary_By_Grade_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.zip";
            }

            if (param.IsSubject)
            {
                zipFileName = $"Lesson_Plan_Summary_By_Subject_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.zip";
            }

            try
            {
                var zipBlobClient = new BlockBlobClient(_documentStorageConnection, "lesson-plan-download", zipFileName);
                await using var zipFileStream = await zipBlobClient.OpenWriteAsync(true,
                options: new BlockBlobOpenWriteOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/zip"
                    }
                }, cancellationToken: CancellationToken);

                await using var zipFileOutputStream = new ZipOutputStream(zipFileStream)
                {
                    IsStreamOwner = false
                };

                const int level = 0;
                zipFileOutputStream.SetLevel(0);

                if (param.IsSubject)
                {
                    var listSubject = listLessonByUser.Where(x => param.IdSubject.Select(x => x.ToLower()).Contains(x.SubjectDescription.ToLower())).Select(x => x.SubjectDescription).Distinct().ToList();

                    foreach (var subject in listSubject)
                    {
                        var subjectGradeNew = new LessonBySubjectGradeNew();
                        var grade = new List<string>();

                        subjectGradeNew.SubjectDescription = subject;
                        var GradeSubject = listLessonByUser.Where(x => x.SubjectDescription.ToLower() == subject.ToLower());

                        foreach (var idGradeItem in GradeSubject)
                        {
                            grade.Add(idGradeItem.IdGrade);
                        }
                        subjectGradeNew.Grade = grade.Distinct().ToList();

                        listSubjectGradeNew.Add(subjectGradeNew);
                    }

                    foreach (var subject in listSubjectGradeNew)
                    {
                        var subjectName = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Description == subject.SubjectDescription);

                        foreach (var idGrade in subject.Grade)
                        {
                            var gradeName = await _dbContext.Entity<MsGrade>().FirstOrDefaultAsync(x => x.Id == idGrade);
                            var idLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                                    .Include(x => x.Lesson)
                                                        .ThenInclude(x => x.Subject)
                                                    .Where(x => x.Lesson.Subject.Description == subject.SubjectDescription && x.Lesson.IdGrade == idGrade)
                                                    .Select(x => x.Id).ToListAsync(CancellationToken);

                            var lessonDocument = await _dbContext.Entity<TrLessonPlanDocument>()
                                                    .Include(x => x.LessonPlan)
                                                    .Where(x => idLessonTeacher.Contains(x.LessonPlan.IdLessonTeacher) && x.Status.ToLower() != "unsubmitted")
                                                    .Select(x => new TempFile
                                                    {
                                                        FileName = x.Filename,
                                                        PathFile = x.PathFile
                                                    }).Distinct().ToListAsync(CancellationToken);

                            if (lessonDocument.Count > 0)
                            {
                                foreach (var file in lessonDocument)
                                {
                                    try
                                    {
                                        var blockBlobClient = new BlockBlobClient(_documentStorageConnection, "document-file-reference", file.FileName);
                                        var properties = await blockBlobClient.GetPropertiesAsync(cancellationToken: CancellationToken);

                                        var zipEntry = new ZipEntry($"{subjectName.Description}/{gradeName.Description}/{file.FileName}")
                                        {
                                            Size = properties.Value.ContentLength
                                        };

                                        zipFileOutputStream.PutNextEntry(zipEntry);
                                        await blockBlobClient.DownloadToAsync(zipFileOutputStream, CancellationToken);
                                        zipFileOutputStream.CloseEntry();
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                var zipEntry = new ZipEntry($"{subjectName.Description}/{gradeName.Description}/")
                                {
                                    Size = 0,
                                };

                                zipFileOutputStream.PutNextEntry(zipEntry);
                                zipFileOutputStream.CloseEntry();
                            }

                        }
                    }
                }

                if (param.IsGrade)
                {
                    var listGrade = listLessonByUser.Where(x => param.IdGrade.Contains(x.IdGrade)).Select(x => x.IdGrade).Distinct().ToList();
                    foreach (var grade in listGrade)
                    {
                        var gradeSubject = new LessonByGradeSubject();
                        var subjects = new List<string>();

                        gradeSubject.IdGrade = grade;
                        var lessonByUserSubject = listLessonByUser.Where(x => x.IdGrade == grade);

                        foreach (var idSubjectItem in lessonByUserSubject)
                        {
                            subjects.Add(idSubjectItem.IdSubject);
                        }
                        gradeSubject.Subject = subjects.Distinct().ToList();
                        listGradeSubject.Add(gradeSubject);
                    }

                    foreach (var grade in listGradeSubject)
                    {
                        var gradeName = await _dbContext.Entity<MsGrade>().FirstOrDefaultAsync(x => x.Id == grade.IdGrade);

                        foreach (var idSubject in grade.Subject)
                        {
                            var subjectName = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Id == idSubject);

                            var idLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                                    .Include(x => x.Lesson)
                                                    .Where(x => x.Lesson.IdSubject == idSubject)
                                                    .Select(x => x.Id).ToListAsync(CancellationToken);

                            var lessonDocument = await _dbContext.Entity<TrLessonPlanDocument>()
                                                    .Include(x => x.LessonPlan)
                                                    .Where(x => idLessonTeacher.Contains(x.LessonPlan.IdLessonTeacher) && x.Status.ToLower() != "unsubmitted")
                                                    .Select(x => new TempFile
                                                    {
                                                        FileName = x.Filename,
                                                        PathFile = x.PathFile
                                                    }).Distinct().ToListAsync(CancellationToken);

                            if (lessonDocument.Count > 0)
                            {
                                foreach (var file in lessonDocument)
                                {
                                    try
                                    {
                                        var blockBlobClient = new BlockBlobClient(_documentStorageConnection, "document-file-reference", file.FileName);
                                        var properties = await blockBlobClient.GetPropertiesAsync(cancellationToken: CancellationToken);

                                        var zipEntry = new ZipEntry($"{gradeName.Description}/{subjectName.Description}/{file.FileName}")
                                        {
                                            Size = properties.Value.ContentLength
                                        };

                                        zipFileOutputStream.PutNextEntry(zipEntry);
                                        await blockBlobClient.DownloadToAsync(zipFileOutputStream, CancellationToken);
                                        zipFileOutputStream.CloseEntry();
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                var zipEntry = new ZipEntry($"{gradeName.Description}/{subjectName.Description}/")
                                {
                                    Size = 0
                                };

                                zipFileOutputStream.PutNextEntry(zipEntry);
                                zipFileOutputStream.CloseEntry();
                            }
                        }
                    }
                }

                blobUrl = zipBlobClient.Uri.AbsoluteUri;

                #region send email download lesson plan summary
                var idUsers = new List<string>();
                idUsers.Add(param.IdUser);

                var userEmailDownload = new UrlDownload
                {
                    IdUser = idUsers,
                    Url = blobUrl
                };

                if (KeyValues.ContainsKey("EmailDownloadLessonPlanSummary"))
                {
                    KeyValues.Remove("EmailDownloadLessonPlanSummary");
                }
                KeyValues.Add("EmailDownloadLessonPlanSummary", userEmailDownload);

                await LP5NotificationEmail(KeyValues, AuthInfo);
                #endregion
            }
            catch (Exception ex)
            {
                var blockBlobClient = new BlockBlobClient(_documentStorageConnection, "lesson-plan-download", zipFileName);
                await blockBlobClient.DeleteIfExistsAsync(cancellationToken: CancellationToken);

                #region send email failed download lesson plan summary
                var idUsers = new List<string>();
                idUsers.Add(param.IdUser);

                var userEmailDownload = new UrlDownload
                {
                    IdUser = idUsers,
                    Url = ""
                };

                if (KeyValues.ContainsKey("EmailDownloadLessonPlanSummary"))
                {
                    KeyValues.Remove("EmailDownloadLessonPlanSummary");
                }
                KeyValues.Add("EmailDownloadLessonPlanSummary", userEmailDownload);

                await LP6NotificationEmail(KeyValues, AuthInfo);
                #endregion

                throw;
            }

            return Request.CreateApiResult2(blobUrl as object);
        }

        public static async Task<List<LessonByUser>> GetLessonByUser(ITeachingDbContext _dbContext, System.Threading.CancellationToken CancellationToken, string IdUser, string IdAcademicYear, string PositionCode)
        {
            List<LessonByUser> listLessonByUser = new List<LessonByUser>();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                                   .Where(x => x.Id == IdAcademicYear)
                                   .Select(e => e.IdSchool)
                                   .FirstOrDefaultAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                       .Include(e => e.Position)
                                       .Where(x => x.IdSchool == idSchool)
                                       .Select(e => new
                                       {
                                           Id = e.Id,
                                           PositionCode = e.Position.Code,
                                       })
                                       .ToListAsync(CancellationToken);

            #region CA
            var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
               .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
               .Where(x => x.IdBinusian == IdUser && x.Homeroom.IdAcademicYear == IdAcademicYear)
               .Select(e => new
               {
                   e.IdHomeroom,
                   PositionCode = e.TeacherPosition.Position.Code
               })
               .Distinct().ToListAsync(CancellationToken);

            var listIdHomeroom = listHomeroomTeacher.Select(e => e.IdHomeroom).ToList();

            var listLessonByCa = await _dbContext.Entity<MsLessonPathway>()
               .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
               .Include(e => e.HomeroomPathway)
               .Where(x => listIdHomeroom.Contains(x.HomeroomPathway.IdHomeroom))
               .Select(e => new
               {
                   IdLevel = e.Lesson.Grade.IdLevel,
                   IdGrade = e.Lesson.IdGrade,
                   IdLesson = e.IdLesson,
                   IdHomeroom = e.HomeroomPathway.IdHomeroom
               })
               .Distinct().ToListAsync(CancellationToken);

            foreach (var itemHomeroomTeacher in listHomeroomTeacher)
            {
                var listLessonCaByHomeroom = listLessonByCa
                    .Where(e => e.IdHomeroom == itemHomeroomTeacher.IdHomeroom)
                    .Select(e => new LessonByUser
                    {
                        IdLevel = e.IdLevel,
                        IdGrade = e.IdGrade,
                        IdLesson = e.IdLesson,
                        PositionCode = itemHomeroomTeacher.PositionCode
                    })
                    .ToList();
                listLessonByUser.AddRange(listLessonCaByHomeroom);
            }

            foreach (var item in listLessonByUser)
            {
                var subject = await _dbContext.Entity<MsLesson>().Include(x => x.Subject).Where(x => x.Id == item.IdLesson).FirstOrDefaultAsync();
                item.IdSubject = subject.IdSubject;
                item.SubjectDescription = subject.Subject.Description;
            }
            #endregion

            #region ST
            var positionCodeBySubjectTeacher = listTeacherPosition
                                                    .Where(e => e.PositionCode == PositionConstant.SubjectTeacher)
                                                    .Select(e => e.PositionCode)
                                                    .ToList();

            var listLessonBySt = await _dbContext.Entity<MsLessonTeacher>()
                                    .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                   .Where(x => x.IdUser == IdUser && x.Lesson.IdAcademicYear == IdAcademicYear)
                                   .Select(e => new LessonByUser
                                   {
                                       IdGrade = e.Lesson.IdGrade,
                                       IdLevel = e.Lesson.Grade.IdLevel,
                                       IdLesson = e.IdLesson,
                                   })
                                   .Distinct()
                                   .ToListAsync(CancellationToken);

            foreach (var itemPositionCode in positionCodeBySubjectTeacher)
            {
                listLessonBySt.ForEach(d => d.PositionCode = itemPositionCode);
                listLessonByUser.AddRange(listLessonBySt);
            }

            foreach (var item in listLessonByUser)
            {
                var subject = await _dbContext.Entity<MsLesson>().Include(x => x.Subject).Where(x => x.Id == item.IdLesson).FirstOrDefaultAsync();
                item.IdSubject = subject.IdSubject;
                item.SubjectDescription = subject.Subject.Description;
            }

            #endregion

            #region non teaching load
            var listTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                 .Where(x => x.IdUser == IdUser && x.MsNonTeachingLoad.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            var listLesson = await _dbContext.Entity<MsLesson>()
                                .Include(e => e.Grade).ThenInclude(e => e.Level)
                                .Where(x => x.Grade.Level.IdAcademicYear == IdAcademicYear)
                                .Select(e => new
                                {
                                    IdLevel = e.Grade.IdLevel,
                                    IdGrade = e.IdGrade,
                                    IdLesson = e.Id,
                                    IdSubject = e.IdSubject
                                })
                                .ToListAsync(CancellationToken);

            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.Grades)
                                 .Where(x => x.Level.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            foreach (var item in listTeacherNonTeaching)
            {
                var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);

                if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
                {
                    var getDepartmentLevelbyIdLevel = listDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id).ToList();

                    foreach (var itemDepartement in getDepartmentLevelbyIdLevel)
                    {
                        var listGrade = itemDepartement.Level.Grades.ToList();

                        foreach (var itemGrade in listGrade)
                        {
                            var listLessonByIdGarde = listLesson.Where(e => e.IdGrade == itemGrade.Id).ToList();

                            foreach (var itemLesson in listLessonByIdGarde)
                            {
                                var subjectDesciption = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Id == itemLesson.IdSubject, CancellationToken);

                                LessonByUser newSubjectTeacher = new LessonByUser
                                {
                                    IdGrade = itemLesson.IdGrade,
                                    IdLevel = itemLesson.IdLevel,
                                    IdLesson = itemLesson.IdLesson,
                                    IdSubject = itemLesson.IdSubject,
                                    SubjectDescription = subjectDesciption.Description,
                                    PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                };

                                listLessonByUser.Add(newSubjectTeacher);
                            }
                        }
                    }

                }
                else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                {
                    var listLessonByIdSubject = listLesson.Where(e => e.IdSubject == _SubjectPosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdSubject)
                    {
                        var subjectDesciption = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Id == itemSubject.IdSubject, CancellationToken);

                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            IdSubject = itemSubject.IdSubject,
                            SubjectDescription = subjectDesciption.Description,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }

                }
                else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                {
                    var listLessonByIdGrade = listLesson.Where(e => e.IdGrade == _GradePosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdGrade)
                    {
                        var subjectDesciption = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Id == itemSubject.IdSubject, CancellationToken);

                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            IdSubject = itemSubject.IdSubject,
                            SubjectDescription = subjectDesciption.Description,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }
                }
                else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                {
                    var listLessonByIdLevel = listLesson.Where(e => e.IdLevel == _LevelPosition.Id).ToList();

                    foreach (var itemSubject in listLessonByIdLevel)
                    {
                        var subjectDesciption = await _dbContext.Entity<MsSubject>().FirstOrDefaultAsync(x => x.Id == itemSubject.IdSubject, CancellationToken);

                        LessonByUser newSubjectTeacher = new LessonByUser
                        {
                            IdGrade = itemSubject.IdGrade,
                            IdLevel = itemSubject.IdLevel,
                            IdLesson = itemSubject.IdLesson,
                            IdSubject = itemSubject.IdSubject,
                            SubjectDescription = subjectDesciption.Description,
                            PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                        };

                        listLessonByUser.Add(newSubjectTeacher);
                    }
                }

            }
            #endregion

            var listLessonByPositionCode = listLessonByUser.ToList();
            if (!string.IsNullOrEmpty(PositionCode))
            {
                listLessonByPositionCode = listLessonByUser.Where(e => e.PositionCode == PositionCode).ToList();
            }

            return listLessonByPositionCode.ToList();
        }

        private async Task LP5NotificationEmail(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDownloadLessonPlanSummary").Value;
            var emailDownload = JsonConvert.DeserializeObject<UrlDownload>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "LP5")
                {
                    IdRecipients = emailDownload.IdUser.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        private async Task LP6NotificationEmail(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDownloadLessonPlanSummary").Value;
            var emailDownload = JsonConvert.DeserializeObject<UrlDownload>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "LP6")
                {
                    IdRecipients = emailDownload.IdUser.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        public class LessonBySubjectGrade
        {
            public string IdSubject { get; set; }
            public List<string> Grade { get; set; }
        }

        public class LessonBySubjectGradeNew
        {
            public string SubjectDescription { get; set; }
            public List<string> Grade { get; set; }
        }

        public class LessonByGradeSubject
        {
            public string IdGrade { get; set; }
            public List<string> Subject { get; set; }
        }

        public class LessonByUser
        {
            public string IdLevel { get; set; }
            public string IdGrade { get; set; }
            public string IdLesson { get; set; }
            public string IdSubject { get; set; }
            public string SubjectDescription { get; set; }
            public string PositionCode { get; set; }
        }

        public class TempFile
        {
            public string IdLessonDocument { get; set; }
            public string FileName { get; set; }
            public string PathFile { get; set; }
        }

        public class UrlDownload
        {
            public List<string> IdUser { get; set; }
            public string Url { get; set; }
        }
    }
}
