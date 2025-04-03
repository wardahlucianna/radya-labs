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
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator;
using FluentEmail.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using NPOI.HPSF;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class ReportStudentToGcHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ReportStudentToGcHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var gcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                                        .Include(e=>e.GcReportStudentGrade).ThenInclude(e=>e.GcReportStudentLevel)
                                        .Include(e=>e.AcademicYear)
                                        .Include(e=>e.Student)
                                        .Where(e => e.Id == id)
                                        .FirstOrDefaultAsync(CancellationToken);

            TrGcReportStudentLevel getGcReportStudentLevel = default;
            if (gcReportStudent.IdGcReportStudentGrade != null)
            {
                getGcReportStudentLevel = await _dbContext.Entity<TrGcReportStudentLevel>()
                               .Include(e => e.GcReportStudentGrades).ThenInclude(e => e.GcReportStudents).ThenInclude(e => e.Student)
                               .Include(e => e.GcReportStudentGrades).ThenInclude(e => e.Grade)
                               .Include(e => e.Level).ThenInclude(e => e.MsAcademicYear)
                               .Where(e => e.Id == gcReportStudent.GcReportStudentGrade.IdGcReportStudentLevel)
                               .FirstOrDefaultAsync(CancellationToken);
            }
           

            List<string> listIdStudent = new List<string>();
            if (getGcReportStudentLevel != null)

                listIdStudent = getGcReportStudentLevel.GcReportStudentGrades.SelectMany(e => e.GcReportStudents.Select(f => f.IdStudent)).ToList();
            else
                listIdStudent.Add(gcReportStudent.IdStudent);
         

            var listClassroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Where(e=>e.Homeroom.Grade.MsLevel.IdAcademicYear== gcReportStudent.IdAcademicYear 
                                                && listIdStudent.Contains(e.IdStudent))
                                        .GroupBy(e => new
                                        {
                                            IdStudent = e.IdStudent,
                                            ClassroomCode = e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                            IdLevel = e.Homeroom.Grade.IdLevel,
                                            Level = e.Homeroom.Grade.MsLevel.Description,
                                            LevelCode = e.Homeroom.Grade.MsLevel.Code,
                                            IdGrade = e.Homeroom.Grade.Id,
                                            Grade = e.Homeroom.Grade.Description,
                                            GradeCode = e.Homeroom.Grade.Code,
                                        })
                                        .Select(e => new
                                        {
                                            IdStudent = e.Key.IdStudent,
                                            ClassroomCode = e.Key.ClassroomCode,
                                            IdLevel = e.Key.IdLevel,
                                            Level = e.Key.Level,
                                            LevelCode = e.Key.LevelCode,
                                            IdGrade = e.Key.IdGrade,
                                            Grade = e.Key.Grade,
                                            GradeCode = e.Key.GradeCode,
                                        })
                                        .ToListAsync(CancellationToken);

            var Result = new GetDetailReportStudentToGcResult();

            if (getGcReportStudentLevel != null)
            {
                Result = new GetDetailReportStudentToGcResult
                {
                    Id = getGcReportStudentLevel.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = getGcReportStudentLevel.Level.MsAcademicYear.Id,
                        Code = getGcReportStudentLevel.Level.MsAcademicYear.Code,
                        Description = getGcReportStudentLevel.Level.MsAcademicYear.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = getGcReportStudentLevel.Level.Id,
                        Code = getGcReportStudentLevel.Level.Code,
                        Description = getGcReportStudentLevel.Level.Description
                    },
                    Grades = getGcReportStudentLevel.GcReportStudentGrades.Select(f => new CodeWithIdVm
                    {
                        Id = f.IdGrade,
                        Code = f.Grade.Code,
                        Description = f.Grade.Description
                    }).ToList(),
                    Students = getGcReportStudentLevel.GcReportStudentGrades.SelectMany(f => f.GcReportStudents.Select(g => new GetDetailReportStudent
                    {
                        Id = g.IdStudent,
                        IdGcReportStudent = gcReportStudent.Id,
                        FullName = NameUtil.GenerateFullName(g.Student.FirstName, g.Student.MiddleName, g.Student.LastName),
                        BinusianId = g.Student.IdBinusian,
                        IdGrade = f.Grade.Id,
                        Grade = f.Grade.Code,
                        Level = getGcReportStudentLevel.Level.Code,
                        Homeroom = listClassroomStudent.Where(x => x.IdStudent == g.IdStudent).Select(x => x.ClassroomCode).FirstOrDefault(),
                        Note = g.Note,
                    })).ToList(),
                    Date = getGcReportStudentLevel.GcReportStudentGrades.SelectMany(f => f.GcReportStudents.Select(g => g.Date)).FirstOrDefault(),
                };
            }
            else
            {
                var getHomeroomStudent = listClassroomStudent.FirstOrDefault();


                Result = new GetDetailReportStudentToGcResult
                {
                    Id = null,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = gcReportStudent.AcademicYear.Id,
                        Code = gcReportStudent.AcademicYear.Code,
                        Description = gcReportStudent.AcademicYear.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = getHomeroomStudent.IdLevel,
                        Code = getHomeroomStudent.LevelCode,
                        Description = getHomeroomStudent.Level
                    },
                    Grades = new List<CodeWithIdVm>()
                    {
                        new CodeWithIdVm
                        {
                            Id = getHomeroomStudent.IdGrade,
                            Code = getHomeroomStudent.GradeCode,
                            Description = getHomeroomStudent.Grade
                        }
                    },
                    Students = new List<GetDetailReportStudent>()
                    {
                        new GetDetailReportStudent
                        {
                            Id = gcReportStudent.IdStudent,
                            IdGcReportStudent = gcReportStudent.Id,
                            FullName = NameUtil.GenerateFullName(gcReportStudent.Student.FirstName, gcReportStudent.Student.MiddleName, gcReportStudent.Student.LastName),
                            BinusianId = gcReportStudent.Student.IdBinusian,
                            IdGrade = getHomeroomStudent.IdGrade,
                            Grade = getHomeroomStudent.GradeCode,
                            Level = getHomeroomStudent.Level,
                            Homeroom = listClassroomStudent.Where(x => x.IdStudent == gcReportStudent.IdStudent).Select(x => x.ClassroomCode).FirstOrDefault(),
                            Note = gcReportStudent.Note,
                        }
                    },
                    Date = gcReportStudent.Date,
                };
            }
                
            return Request.CreateApiResult2(Result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetReportStudentToGcRequest>();
            string[] _columns = { "AcademicYear", "StudentName", "BinusanId", "ReportStudentDate", "ReportStudentNote" };

            if (param.EndDate < param.StartDate)
                throw new BadRequestException("Report student to GC with start date: " + param.StartDate + " and end date: " + param.EndDate + " are wrong number.");

            var predicate = PredicateBuilder.Create<TrGcReportStudent>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdUserReport == param.IdUser);

            var query = _dbContext.Entity<TrGcReportStudent>()
                .Include(e=>e.Student).ThenInclude(e=>e.StudentGrades)
                .Include(e=>e.Student).ThenInclude(e=>e.MsHomeroomStudents).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
                .Include(e=>e.Student).ThenInclude(e=>e.MsHomeroomStudents).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.MsGradePathwayClassroom).ThenInclude(e=>e.Classroom)
                .Where(predicate)
                .Select(e => new
                {
                    e.Id,
                    e.IsRead,
                    Level = e.Student.MsHomeroomStudents
                            .Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == e.IdAcademicYear)
                            .Select(e => new CodeWithIdVm
                            {
                                Id = e.Homeroom.Grade.IdLevel,
                                Code = e.Homeroom.Grade.MsLevel.Code,
                                Description = e.Homeroom.Grade.MsLevel.Description
                            })
                            .FirstOrDefault(),
                    Grade = e.Student.MsHomeroomStudents
                            .Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == e.IdAcademicYear)
                            .Select(e => new CodeWithIdVm
                            {
                                Id = e.Homeroom.Grade.Id,
                                Code = e.Homeroom.Grade.Code,
                                Description = e.Homeroom.Grade.Description
                            })
                            .FirstOrDefault(),
                    ClassHomeroom = e.Student.MsHomeroomStudents
                            .Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == e.IdAcademicYear)
                            .Select(e => new CodeWithIdVm
                            {
                                Id = e.Homeroom.Id,
                                Code = e.Homeroom.Grade.Code,
                                Description = e.Homeroom.MsGradePathwayClassroom.Classroom.Description
                            })
                            .FirstOrDefault(),
                    StudentPhoto = new CodeWithIdVm
                    {
                        Id = e.Student.StudentPhotos.OrderByDescending(x => x.DateUp).FirstOrDefault().Id,
                        Code = e.Student.StudentPhotos.OrderByDescending(x => x.DateUp).FirstOrDefault().FileName,
                        Description = e.Student.StudentPhotos.OrderByDescending(x => x.DateUp).FirstOrDefault().FilePath
                    },
                    AcademicYear = e.Student.MsHomeroomStudents
                            .Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == e.IdAcademicYear)
                            .Select(e => e.Homeroom.Grade.MsLevel.MsAcademicYear.Description)
                            .FirstOrDefault(),
                    StudentName = (e.Student.FirstName == null ? "" : e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                    BinusanId = e.IdStudent,
                    ReportStudentDate = e.Date,
                    ReportStudentNote = e.Note,
                });

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                query = query.Where(x => x.ReportStudentDate.Date >= Convert.ToDateTime(param.StartDate).Date && x.ReportStudentDate.Date <= Convert.ToDateTime(param.EndDate).Date);

            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.Grade.Id==param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.Level.Id == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.ClassHomeroom.Id == param.IdHomeroom);

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.BinusanId.Contains(param.Search)
                || x.StudentName.Contains(param.Search)
                || x.Level.Description.Contains(param.Search)
                || x.Grade.Description.Contains(param.Search)
                || x.ClassHomeroom.Code.Contains(param.Search) || x.ClassHomeroom.Description.Contains(param.Search));

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusanId)
                        : query.OrderBy(x => x.BinusanId);
                    break;
                case "ReportStudentDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentDate)
                        : query.OrderBy(x => x.ReportStudentDate);
                    break;
                case "ReportStudentNote":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentNote)
                        : query.OrderBy(x => x.ReportStudentNote);
                    break;
                case "Read":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IsRead)
                        : query.OrderBy(x => x.IsRead);
                    break;
                default:
                    query = query.OrderByDynamic(param);
                    break;

            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcResult
                {
                    Id = x.Id,
                    Level = x.Level,
                    Grade = x.Grade,
                    ClassHomeroom = x.ClassHomeroom,
                    StudentPhoto = x.StudentPhoto,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcResult
                {
                    Id = x.Id,
                    Level = x.Level,
                    Grade = x.Grade,
                    ClassHomeroom = x.ClassHomeroom,
                    StudentPhoto = x.StudentPhoto,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddReportStudentToGcRequest, AddReportStudentToGcValidator>();

            var listCounselorsGrade = await _dbContext.Entity<MsCounselorGrade>()
                        .Include(e => e.Counselor)
                        .Where(e => e.Counselor.IdAcademicYear == body.IdAcademicYear
                            && body.Students.Select(y => y.IdGrade).Distinct().Contains(e.IdGrade))
                        .ToListAsync(CancellationToken);

            var listGcReportStudentLevel = await _dbContext.Entity<TrGcReportStudentLevel>()
                                   .Include(e => e.GcReportStudentGrades).ThenInclude(e=>e.GcReportStudents)
                                   .Where(x => x.Level.IdAcademicYear == body.IdAcademicYear && x.Id == body.IdGcReportStudentLevel)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(CancellationToken);

            var listIdStudent = body.Students.Select(y => y.IdStudent).Distinct().ToList();
            var listIdGcReportStudent = body.Students.Select(y => y.IdGcReportStudent).Distinct().ToList();

            var listGcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                                   .Where(x => listIdGcReportStudent.Contains(x.Id))
                                   .AsNoTracking()
                                   .ToListAsync(CancellationToken);

            List<TrGcReportStudentGrade> listGcReportGrade = new List<TrGcReportStudentGrade>();
            var listIdGcReportStudentsRemove = new List<string>();
            #region Remove
            if (listGcReportStudentLevel!=null)
            {
                listGcReportStudent = listGcReportStudentLevel.GcReportStudentGrades.SelectMany(e => e.GcReportStudents).ToList();
                listGcReportGrade = listGcReportStudentLevel.GcReportStudentGrades.ToList();

                if (listGcReportStudentLevel.IdLevel != body.IdLevel)
                {
                    listGcReportStudentLevel.IsActive = false;
                    _dbContext.Entity<TrGcReportStudentLevel>().Update(listGcReportStudentLevel);

                    listGcReportGrade.ForEach(e=>e.IsActive = false);
                    _dbContext.Entity<TrGcReportStudentGrade>().UpdateRange(listGcReportGrade);

                    listGcReportStudent.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrGcReportStudent>().UpdateRange(listGcReportStudent);
                    listIdGcReportStudentsRemove.AddRange(listGcReportStudent.Select(e => e.Id).ToList());
                }
                else
                {
                    var removeGcReportGrade = listGcReportGrade.Where(e=>!body.IdGrades.Contains(e.IdGrade)).ToList();
                    removeGcReportGrade.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrGcReportStudentGrade>().UpdateRange(removeGcReportGrade);

                    //romove student by student
                    var removeGcReportStudent = listGcReportStudentLevel.GcReportStudentGrades
                                                .SelectMany(e => e.GcReportStudents)
                                                .Where(e => !listIdStudent.Contains(e.IdStudent) || !body.IdGrades.Contains(e.GcReportStudentGrade.IdGrade))
                                                .ToList();
                    removeGcReportStudent.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrGcReportStudent>().UpdateRange(removeGcReportStudent);
                    listIdGcReportStudentsRemove.AddRange(removeGcReportStudent.Select(e => e.Id).ToList());
                }
            }
            #endregion

            #region Add level and grade
            var newGcReportStudentLevel = new TrGcReportStudentLevel();
            if (listGcReportStudentLevel == null)
            {
                newGcReportStudentLevel = new TrGcReportStudentLevel
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                };
                _dbContext.Entity<TrGcReportStudentLevel>().Add(newGcReportStudentLevel);
            }
            else if (listGcReportStudentLevel.IdLevel != body.IdLevel)
            {
                newGcReportStudentLevel = new TrGcReportStudentLevel
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                };
                _dbContext.Entity<TrGcReportStudentLevel>().Add(newGcReportStudentLevel);
            }
            else
            {
                newGcReportStudentLevel = listGcReportStudentLevel;
            }

            List<TrGcReportStudentGrade> listNewGcReportStudentGrade = new List<TrGcReportStudentGrade>();
            listNewGcReportStudentGrade.AddRange(listGcReportGrade);

            foreach (var itemGrade in body.IdGrades)
            {
                var exsisGrade = listGcReportGrade.Where(e => e.IdGrade == itemGrade).Any();

                if (!exsisGrade)
                {
                    var newGcReportStudentGrade = new TrGcReportStudentGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGrade = itemGrade,
                        IdGcReportStudentLevel = newGcReportStudentLevel.Id,
                        IsActive = true
                    };
                    listNewGcReportStudentGrade.Add(newGcReportStudentGrade);
                    _dbContext.Entity<TrGcReportStudentGrade>().Add(newGcReportStudentGrade);
                }
            }
            #endregion

            var listIdGcReportStudentsCreate = new List<string>();
            var listGcReportStudentsUpdate = new List<GC2NotificationRequest>();
            foreach (var student in body.Students)
            {
                if (!listCounselorsGrade.Any(x => x.IdGrade == student.IdGrade))
                    throw new BadRequestException("Counsellor with id user: " + body.IdUserReport + " is not found.");

                var gcReportStudentByIdStudent = listGcReportStudent.Where(x => x.IdStudent == student.IdStudent).FirstOrDefault();

                if (gcReportStudentByIdStudent != null)
                {
                    listGcReportStudentsUpdate.Add(new GC2NotificationRequest
                    {
                        Id = gcReportStudentByIdStudent.Id,
                        oldDate = gcReportStudentByIdStudent.Date,
                        oldNote = gcReportStudentByIdStudent.Note,
                    });

                    var IdGcReportStudentGrade = "";
                    if (gcReportStudentByIdStudent.IdGcReportStudentGrade == null)
                        IdGcReportStudentGrade = listNewGcReportStudentGrade
                                                    .Where(e => e.IdGrade == student.IdGrade && e.IsActive).Select(e => e.Id).FirstOrDefault();
                    else
                        IdGcReportStudentGrade = gcReportStudentByIdStudent.IdGcReportStudentGrade;

                    gcReportStudentByIdStudent.IdUserCounselor = listCounselorsGrade.FirstOrDefault(x => x.IdGrade == student.IdGrade).Counselor.IdUser;
                    gcReportStudentByIdStudent.IdUserReport = body.IdUserReport;
                    gcReportStudentByIdStudent.Date = body.Date;
                    gcReportStudentByIdStudent.Note = student.Note;
                    gcReportStudentByIdStudent.IdGcReportStudentGrade = IdGcReportStudentGrade;
;
                    _dbContext.Entity<TrGcReportStudent>().Update(gcReportStudentByIdStudent);
                    
                }
                else
                {
                    var IdGcReportStudentGrade = listNewGcReportStudentGrade.Where(e => e.IdGrade == student.IdGrade && e.IsActive).Select(e => e.Id).FirstOrDefault();
                    var newGcReportStudent = new TrGcReportStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcademicYear,
                        IdStudent = student.IdStudent,
                        IdUserCounselor = listCounselorsGrade.FirstOrDefault(x => x.IdGrade == student.IdGrade).Counselor.IdUser,
                        IdUserReport = body.IdUserReport,
                        Date = body.Date,
                        IdGcReportStudentGrade = IdGcReportStudentGrade,
                        Note = student.Note,
                        IsRead = false,
                    };
                    _dbContext.Entity<TrGcReportStudent>().Add(newGcReportStudent);

                    listIdGcReportStudentsCreate.Add(newGcReportStudent.Id);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            if(!listGcReportStudentsUpdate.Any())
                await GC1Notification(listIdGcReportStudentsCreate);
            else
                await GC2Notification(listGcReportStudentsUpdate, listIdGcReportStudentsRemove, listIdGcReportStudentsCreate);

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        private async Task GC1Notification(List<string> listIdGcReportStudents)
        {
            var dataGcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                .Include(x => x.Student)
                .Include(x => x.UserConsellor)
                .Include(x => x.UserReport)
                .Include(x => x.AcademicYear).ThenInclude(e=>e.MsSchool)
                .Where(x => listIdGcReportStudents.Contains(x.Id))
                .ToListAsync(CancellationToken);

            var listIdRecipients = dataGcReportStudent.Select(e=>e.IdUserCounselor).Distinct().ToList();

            var listEmailGc1 = dataGcReportStudent
                        .Select(e => new Gc1Result
                        {
                            AcademicYear = e.AcademicYear.Description,
                            StudentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                            BinussianId = e.Student.IdBinusian,
                            ReportedBy = e.UserReport.DisplayName,
                            Date = e.Date.ToShortDateString(),
                            Note = e.Note,
                        })
                        .ToList();

            if (KeyValues.ContainsKey("EmailGc1"))
            {
                KeyValues.Remove("EmailGc1");
            }
            KeyValues.Add("EmailGc1", listEmailGc1);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "GC1")
                {
                    IdRecipients = listIdRecipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        private async Task GC2Notification(List<GC2NotificationRequest> listGcReportStudents, List<string> listIdGcReportStudentsRemove, List<string> listIdGcReportStudentsCreate)
        {
            var listIdGcReportStudents = listGcReportStudents.Select(e => e.Id).ToList();
            var dataGcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
            .Include(x => x.Student)
            .Include(x => x.UserConsellor)
            .Include(x => x.UserReport)
            .Include(x => x.AcademicYear).ThenInclude(e=>e.MsSchool)
            .Where(x => listIdGcReportStudents.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var listIdGcRemoveAndAdd = listIdGcReportStudentsRemove.Union(listIdGcReportStudentsCreate).ToList();

            var dataGcReportStudentRemoveAndAdd = await _dbContext.Entity<TrGcReportStudent>()
            .Include(x => x.Student)
            .Include(x => x.UserConsellor)
            .Include(x => x.UserReport)
            .Include(x => x.AcademicYear).ThenInclude(e => e.MsSchool)
            .IgnoreQueryFilters()
            .Where(x => listIdGcRemoveAndAdd.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var dataGcReportStudentRemove = dataGcReportStudentRemoveAndAdd
                                               .Where(x => listIdGcReportStudentsRemove.Contains(x.Id))
                                               .ToList();

            var dataGcReportStudentAdd = dataGcReportStudentRemoveAndAdd
                                               .Where(x => listIdGcReportStudentsCreate.Contains(x.Id))
                                               .ToList();

            var listIdRecipients = dataGcReportStudent.Select(e => e.IdUserCounselor).Distinct().ToList();

            Gc2Result listEmailGc1 = new Gc2Result();
            listEmailGc1.DataNew = dataGcReportStudent.Union(dataGcReportStudentAdd)
                        .Select(e => new Gc1Result
                        {
                            AcademicYear = e.AcademicYear.Description,
                            StudentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                            BinussianId = e.Student.IdBinusian,
                            ReportedBy = e.UserReport.DisplayName,
                            Date = e.Date.ToShortDateString(),
                            Note = e.Note,
                        })
                        .ToList();

            listEmailGc1.DataOld = dataGcReportStudent.Union(dataGcReportStudentRemove)
                        .Select(e => new Gc1Result
                        {
                            AcademicYear = e.AcademicYear.Description,
                            StudentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                            BinussianId = e.Student.IdBinusian,
                            ReportedBy = e.UserReport.DisplayName,
                            Date = listGcReportStudents.Where(f => f.Id == e.Id).Any() 
                                    ? listGcReportStudents.Where(f => f.Id == e.Id).Select(e => e.oldDate.ToShortDateString()).FirstOrDefault()
                                    : e.Date.ToShortDateString(),
                            Note = listGcReportStudents.Where(f => f.Id == e.Id).Any()
                                    ? listGcReportStudents.Where(f => f.Id == e.Id).Select(e => e.oldNote).FirstOrDefault()
                                    : e.Note,
                        })
                        .ToList();

            if (KeyValues.ContainsKey("EmailGc2"))
            {
                KeyValues.Remove("EmailGc2");
            }
            KeyValues.Add("EmailGc2", listEmailGc1);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "GC2")
                {
                    IdRecipients = listIdRecipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
    }
}
