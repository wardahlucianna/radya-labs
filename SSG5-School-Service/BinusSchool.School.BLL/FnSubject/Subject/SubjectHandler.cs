using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.School.FnSubject.Subject.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static BinusSchool.Persistence.SchoolDb.SchoolDbContext;

namespace BinusSchool.School.FnSubject.Subject
{
    public class SubjectHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IServiceProvider _provider;
        private readonly ICheckUsage _checkUsageService;
        private readonly ITimetablePreferences _timeTablePreference;

        public SubjectHandler(ISchoolDbContext schoolDbContext, IServiceProvider provider, ICheckUsage checkUsageService, ITimetablePreferences timeTablePreference)
        {
            _dbContext = schoolDbContext;
            _provider = provider;
            _checkUsageService = checkUsageService;
            _timeTablePreference = timeTablePreference;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsSubject>()
                .Include(x => x.SubjectSessions)
                .Include(x => x.SubjectCombinations)
                .Include(x => x.SubjectPathways)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                var checkResult = await _checkUsageService.CheckUsageSubject(data.Id);

                // don't set inactive when row have to-many relation
                if (data.SubjectCombinations.Count != 0 || checkResult.Payload)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsSubject>().Update(data);
                    foreach (var sc in data.SubjectCombinations)
                    {
                        sc.IsActive = false;
                    }
                    foreach (var sc in data.SubjectPathways)
                    {
                        sc.IsActive = false;
                    }
                    foreach (var sc in data.SubjectSessions)
                    {
                        sc.IsActive = false;
                    } 
                    _dbContext.Entity<MsSubjectCombination>().UpdateRange(data.SubjectCombinations);
                    _dbContext.Entity<MsSubjectPathway>().UpdateRange(data.SubjectPathways);
                    _dbContext.Entity<MsSubjectSession>().UpdateRange(data.SubjectSessions);

                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsSubject>()
                .Select(x => new GetSubjectDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    SubjectId = x.SubjectID,
                    MaxSession = x.MaxSession,
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.Grade.Level.AcademicYear.Id,
                        Code = x.Grade.Level.AcademicYear.Code
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code
                    },
                    Department = new CodeWithIdVm
                    {
                        Id = x.Department.Id,
                        Code = x.Department.Code,
                        Description = x.Department.Description
                    },
                    CurriculumType = new CodeWithIdVm
                    {
                        Id = x.Curriculum.Id,
                        Code = x.Curriculum.Code,
                        Description = x.Curriculum.Description
                    },
                    SubjectType = new CodeWithIdVm
                    {
                        Id = x.SubjectType.Id,
                        Code = x.SubjectType.Code,
                        Description = x.SubjectType.Description
                    },
                    // Pathways = x.SubjectPathways.Count != 0
                    //     ? x.SubjectPathways.Select(y => new CodeWithIdVm
                    //     {
                    //         Id = y.GradePathwayDetail.IdPathway,
                    //         Code = y.GradePathwayDetail.Pathway.Code,
                    //         Description = y.GradePathwayDetail.Pathway.Description
                    //     })
                    //     : null,
                    // Pathways = 
                    //  (


                    //             from _subjectPathways in _dbContext.Entity<MsSubjectPathway>()
                    //             join _gradePathwaysDetail in _dbContext.Entity<MsGradePathwayDetail>() on _subjectPathways.IdGradePathwayDetail equals _gradePathwaysDetail.Id
                    //             join _msPathway in _dbContext.Entity<MsPathway>() on _gradePathwaysDetail.IdPathway equals _msPathway.Id
                    //             where
                    //                 _subjectPathways.IdSubject == x.Id
                    //             group _msPathway by new 
                    //             {
                    //                 _msPathway.Id,
                    //                 _msPathway.Code,
                    //                 _msPathway.Description
                    //             }
                    //             into g                           
                    //             select new CodeWithIdVm
                    //             {
                    //                 Id = g.Key.Id,
                    //                 Code = g.Key.Code,
                    //                 Description = g.Key.Description
                    //             }

                    //     ).ToList() ?? null,
                    //  Pathways = 
                    //  (


                    //             from _pathway in _dbContext.Entity<MsPathway>()
                    //             join _gradePathwaysDetail in _dbContext.Entity<MsGradePathwayDetail>() on _pathway.Id equals _gradePathwaysDetail.IdPathway
                    //             join _subjectPathway in _dbContext.Entity<MsSubjectPathway>() on _gradePathwaysDetail.Id equals _subjectPathway.IdGradePathwayDetail
                    //             where
                    //                 _subjectPathway.IdSubject == x.Id
                    //             group _pathway by new 
                    //             {
                    //                 _pathway.Id,
                    //                 _pathway.Code,
                    //                 _pathway.Description
                    //             }
                    //             into g                           
                    //             select new CodeWithIdVm
                    //             {
                    //                 Id = g.Key.Id,
                    //                 Code = g.Key.Code,
                    //                 Description = g.Key.Description
                    //             }

                    //     ).ToList() ?? null,
                    SubjectLevels = x.SubjectMappingSubjectLevels.Count != 0
                        ? x.SubjectMappingSubjectLevels.Select(y => new CodeWithIdVm
                        {
                            Id = y.SubjectLevel.Id,
                            Code = y.SubjectLevel.Code,
                            Description = y.SubjectLevel.Description
                        })
                        : null,
                    Sessions = x.SubjectSessions.Select(y => new SessionCollectionWithId
                    {
                        Id = y.Id,
                        Length = y.Length,
                        Count = y.Content
                    }),
                    School = new GetSchoolResult
                    {
                        Id = x.Grade.Level.AcademicYear.School.Id,
                        Name = x.Grade.Level.AcademicYear.School.Name,
                        Description = x.Grade.Level.AcademicYear.School.Description
                    },
                    Audit = x.GetRawAuditResult2(),
                    IsNeedLessonPlan = x.IsNeedLessonPlan==null?false: x.IsNeedLessonPlan.Value
                })
                .Where(x => x.Id == id).FirstOrDefaultAsync(CancellationToken);
            
            query.Pathways =
            (


                                from _pathway in _dbContext.Entity<MsPathway>()
                                join _gradePathwaysDetail in _dbContext.Entity<MsGradePathwayDetail>() on _pathway.Id equals _gradePathwaysDetail.IdPathway
                                join _subjectPathway in _dbContext.Entity<MsSubjectPathway>() on _gradePathwaysDetail.Id equals _subjectPathway.IdGradePathwayDetail
                                where
                                    _subjectPathway.IdSubject == query.Id
                                group _pathway by new
                                {
                                    _pathway.Id,
                                    _pathway.Code,
                                    _pathway.Description
                                }
                                into g
                                select new CodeWithIdVm
                                {
                                    Id = g.Key.Id,
                                    Code = g.Key.Code,
                                    Description = g.Key.Description
                                }

                        ).ToList() ?? null;
            return Request.CreateApiResult2(query as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            // use GetSubjectHandler.cs instead
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.GetBody<AddSubjectRequest>();
            (await new AddSubjectValidator(_provider).ValidateAsync(body)).EnsureValid();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            // check academic year
            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyear);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Acadyear"], "Id", body.IdAcadyear));

            // check department
            var department = await _dbContext.Entity<MsDepartment>().FindAsync(body.IdDepartment);
            if (department is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Department"], "Id", body.IdDepartment));

            // check curriculum type
            var curType = await _dbContext.Entity<MsCurriculum>().FindAsync(body.IdCurriculumType);
            if (curType is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["CurriculumType"], "Id", body.IdCurriculumType));

            // check subject type
            var subjectType = await _dbContext.Entity<MsSubjectType>().FindAsync(body.IdSubjectType);
            if (subjectType is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SubjectType"], "Id", body.IdSubjectType));

            // check grade
            var reqIdGrades = body.Grades.Select(x => x.IdGrade);
            var existGrades = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Subjects)
                .Where(x => x.Level.IdAcademicYear == body.IdAcadyear && reqIdGrades.Contains(x.Id))
                .ToListAsync(CancellationToken);
            var notExistIdGrades = reqIdGrades.Except(reqIdGrades.Intersect(existGrades.Select(x => x.Id)));
            if (notExistIdGrades.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(", ", notExistIdGrades)));

            // check pathway
            var anyPathway = body.Grades.Any(x => x.IdPathways?.Any() ?? false);
            if (anyPathway)
            {
                var reqIdPathways = body.Grades.SelectMany(x => x.IdPathways);
                var existPathways = await _dbContext.Entity<MsGradePathwayDetail>()
                    .Where(x => reqIdPathways.Contains(x.IdPathway))
                    .Select(x => x.IdPathway)
                    .ToListAsync(CancellationToken);
                var notExistIdPathways = reqIdPathways.Except(reqIdPathways.Intersect(existPathways));
                if (notExistIdPathways.Any())
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Pathway"], "Id", string.Join(", ", notExistIdPathways)));
            }

            // check subject level
            var anySubjectLevel = body.Grades.Any(x => x.IdSubjectLevels?.Any() ?? false);
            if (anySubjectLevel)
            {
                var reqIdSubjectLevels = body.Grades.SelectMany(x => x.IdSubjectLevels);
                var existSubjectlevels = await _dbContext.Entity<MsSubjectLevel>()
                    .Where(x => reqIdSubjectLevels.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(CancellationToken);
                var notExistIdSubjectLevels = reqIdSubjectLevels.Except(reqIdSubjectLevels.Intersect(existSubjectlevels));
                if (notExistIdSubjectLevels.Any())
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SubjectLevel"], "Id", string.Join(", ", notExistIdSubjectLevels)));
            }

            foreach (var grade in body.Grades)
            {
                var subjectId = !string.IsNullOrEmpty(grade.SubjectId) // Grade + Short Subject Name
                    ? grade.SubjectId
                    : existGrades.Find(x => x.Id == grade.IdGrade).Code + body.Code;
                var existGrade = existGrades.Find(x => x.Id == grade.IdGrade);

                if (existGrade.Subjects.Any(x => x.SubjectID == subjectId))
                    throw new BadRequestException(string.Format(Localizer["ExAlreadyHas"], Localizer["Grade"], existGrade.Description, $"{Localizer["Subject"]} {body.Description}"));

                var subject = new MsSubject
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGrade = grade.IdGrade,
                    IdDepartment = body.IdDepartment,
                    IdCurriculum = body.IdCurriculumType,
                    IdSubjectType = body.IdSubjectType,
                    Code = body.Code,
                    Description = body.Description,
                    MaxSession = body.MaxSession,
                    SubjectID = subjectId,
                    IsNeedLessonPlan = body.IsNeedLessonPlan
                };

                _dbContext.Entity<MsSubject>().Add(subject);

                foreach (var session in body.Sessions)
                {
                    var newSession = new MsSubjectSession
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSubject = subject.Id,
                        Length = session.Length,
                        Content = session.Count
                    };
                    _dbContext.Entity<MsSubjectSession>().Add(newSession);
                }

                if (anyPathway)
                {
                    //get list grade pathway detail by id pathway
                    var gradePathwayDetail = await _dbContext.Entity<MsGradePathwayDetail>()
                        .Include(x => x.GradePathway)
                        .Where(x => grade.IdPathways.Contains(x.IdPathway) && x.GradePathway.IdGrade == grade.IdGrade)
                        .ToListAsync(CancellationToken);
                    var pathways = gradePathwayDetail.Select(x => new MsSubjectPathway
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSubject = subject.Id,
                        IdGradePathwayDetail = x.Id
                    });
                    _dbContext.Entity<MsSubjectPathway>().AddRange(pathways);
                }

                if (anySubjectLevel)
                {
                    var subjectLevels = grade.IdSubjectLevels.Select(x => new MsSubjectMappingSubjectLevel
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSubject = subject.Id,
                        IdSubjectLevel = x
                    });
                    _dbContext.Entity<MsSubjectMappingSubjectLevel>().AddRange(subjectLevels);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.GetBody<UpdateSubjectRequest>();
            (await new UpdateSubjectValidator(_provider).ValidateAsync(body)).EnsureValid();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var totalsession = body.Sessions.Sum(p => p.Count * p.Length);
            if (totalsession > body.MaxSession)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotEqual"], Localizer["TotalSession"], Localizer["MaxSession"]));
            }

            var existSubjects = await _dbContext.Entity<MsSubject>()
                .Include(x => x.SubjectSessions)
                .Include(x => x.SubjectPathways)
                .Include(x => x.SubjectMappingSubjectLevels)
                .Include(x => x.SubjectCombinations)
                .Include(x => x.Grade)
                .Where(x => x.Id == body.Id || x.SubjectID == body.SubjectId)
                .ToListAsync(CancellationToken);

            var existSubject = existSubjects.Find(x => x.Id == body.Id);
            if (existSubject is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Subject"], "Id", body.Id));
            //if (existSubject.SubjectCombinations.Count > 0)
            //{
            //    throw new BadRequestException(string.Format("Unable to edit subject because its already in use"));

            //}

            // check if subject id not exist
            if (!string.IsNullOrEmpty(body.SubjectId))
            {
                var exist = existSubjects.Find(x => x.Id != existSubject.Id && x.SubjectID == body.SubjectId && x.IdGrade == existSubject.IdGrade);
                if (exist != null)
                    throw new BadRequestException(string.Format(Localizer["ExAlreadyHas"], Localizer["Grade"], existSubject.Grade.Description, $"{Localizer["Subject"]} {body.Description}"));
            }

            // check department
            var department = await _dbContext.Entity<MsDepartment>().FindAsync(body.IdDepartment);
            if (department is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Department"], "Id", body.IdDepartment));

            // check curriculum type
            var curType = await _dbContext.Entity<MsCurriculum>().FindAsync(body.IdCurriculumType);
            if (curType is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["CurriculumType"], "Id", body.IdCurriculumType));

            // check subject type
            var subjectType = await _dbContext.Entity<MsSubjectType>().FindAsync(body.IdSubjectType);
            if (subjectType is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SubjectType"], "Id", body.IdSubjectType));

            // check pathway
            var anyPathway = body.IdPathways?.Any() ?? false;
            if (anyPathway)
            {
                var existPathways = await _dbContext.Entity<MsGradePathwayDetail>()
                    .Where(x => body.IdPathways.Contains(x.IdPathway) && x.GradePathway.IdGrade == existSubject.IdGrade)
                    .Select(x => x.IdPathway)
                    .ToListAsync(CancellationToken);
                var notExistIdPathways = body.IdPathways.Except(body.IdPathways.Intersect(existPathways));
                if (notExistIdPathways.Any())
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Pathway"], "Id", string.Join(", ", notExistIdPathways)));
            }

            // check subject level
            var anySubjectLevel = body.IdSubjectLevels?.Any() ?? false;
            if (anySubjectLevel)
            {
                var existSubjectlevels = await _dbContext.Entity<MsSubjectLevel>()
                    .Where(x => body.IdSubjectLevels.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(CancellationToken);
                var notExistIdSubjectLevels = body.IdSubjectLevels.Except(body.IdSubjectLevels.Intersect(existSubjectlevels));
                if (notExistIdSubjectLevels.Any())
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SubjectLevel"], "Id", string.Join(", ", notExistIdSubjectLevels)));
            }
            List<TimetableDetailDetailRequest> timetableDetailDetailRequests = new List<TimetableDetailDetailRequest>();
            
            existSubject.IdDepartment = body.IdDepartment;
            existSubject.IdCurriculum = body.IdCurriculumType;
            existSubject.IdSubjectType = body.IdSubjectType;
            existSubject.Code = body.Code;
            existSubject.Description = body.Description;
            existSubject.SubjectID = body.SubjectId ?? existSubject.SubjectID;
            existSubject.MaxSession = body.MaxSession;
            existSubject.IsNeedLessonPlan = body.IsNeedLessonPlan;

            // add or update session detail
            var updatedSessions = new List<MsSubjectSession>();
            foreach (var session in body.Sessions)
            {
                // select existing session detail to update
                var existSession = existSubject.SubjectSessions.FirstOrDefault(x => x.Id == session.Id);

                // create new if not exist
                if (existSession is null)
                {
                    var newSession = new MsSubjectSession
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSubject = existSubject.Id,
                        Length = session.Length,
                        Content = session.Count
                    };
                    _dbContext.Entity<MsSubjectSession>().Add(newSession);
                    var newData = new TimetableDetailDetailRequest
                    {
                        Action = "Add",
                        OldValue = new CountAndLength
                        {
                            Count = 0,
                            Length = 0
                        },
                        NewValue = new CountAndLength
                        {
                            Count = session.Count,
                            Length = session.Length
                        }
                    };
                    timetableDetailDetailRequests.Add(newData);
                }
                else
                {
                    int oldLength, oldContent = 0;
                    oldContent = existSession.Content;
                    oldLength = existSession.Length;
                    updatedSessions.Add(existSession);
                    existSession.Length = session.Length;
                    existSession.Content = session.Count;
                    _dbContext.Entity<MsSubjectSession>().Update(existSession);
                    var updateData = new TimetableDetailDetailRequest
                    {
                        Action = "Edit",
                        OldValue = new CountAndLength
                        {
                            Count = oldContent,
                            Length = oldLength
                        },
                        NewValue = new CountAndLength
                        {
                            Count = session.Count,
                            Length = session.Length
                        }
                    };
                    timetableDetailDetailRequests.Add(updateData);
                }
            }
            // select unupdated session detail and remove it

            var unupdatedSessions = existSubject.SubjectSessions
                .Where(x => x.IsActive) // this will prevent new session being evaluated
                .Except(updatedSessions);
            if (unupdatedSessions.Any())
            {
                foreach (var unupdated in unupdatedSessions)
                {
                    unupdated.IsActive = false;
                    _dbContext.Entity<MsSubjectSession>().Update(unupdated);
                    var deletedData = new TimetableDetailDetailRequest
                    {
                        Action = "Delete",
                        OldValue = new CountAndLength
                        {
                            Count = unupdated.Content,
                            Length = unupdated.Length
                        },
                        NewValue = new CountAndLength
                        {
                            Count = 1,
                            Length = 1
                        }
                    };
                    timetableDetailDetailRequests.Add(deletedData);
                }
            }

            // remove existing subject pathways before add new
            if (existSubject.SubjectPathways.Count != 0)
            {
                foreach (var pathway in existSubject.SubjectPathways)
                {
                    pathway.IsActive = false;
                }

                _dbContext.Entity<MsSubjectPathway>().UpdateRange(existSubject.SubjectPathways);
            }
            if (anyPathway)
            {
                var gradePathwayDetail = await _dbContext.Entity<MsGradePathwayDetail>()
                    .Include(x => x.GradePathway)
                    .Where(x => body.IdPathways.Contains(x.IdPathway) && x.GradePathway.IdGrade == existSubject.IdGrade)
                    .ToListAsync(CancellationToken);
                var pathways = gradePathwayDetail.Select(x => new MsSubjectPathway
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSubject = existSubject.Id,
                    IdGradePathwayDetail = x.Id
                });
                _dbContext.Entity<MsSubjectPathway>().AddRange(pathways);
            }

            // remove existing subject mapping level before add new
            if (existSubject.SubjectMappingSubjectLevels.Count != 0)
            {
                foreach (var subjectLevel in existSubject.SubjectMappingSubjectLevels)
                {
                    subjectLevel.IsActive = false;
                    _dbContext.Entity<MsSubjectMappingSubjectLevel>().Update(subjectLevel);
                }
            }
            if (anySubjectLevel)
            {
                var subjectLevels = body.IdSubjectLevels.Select(x => new MsSubjectMappingSubjectLevel
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSubject = existSubject.Id,
                    IdSubjectLevel = x
                });
                _dbContext.Entity<MsSubjectMappingSubjectLevel>().AddRange(subjectLevels);
            }
            if (existSubject.SubjectCombinations != null && existSubject.SubjectCombinations.Count > 0)
            {
                TimetableDetailRequest timetableDetailRequest = new TimetableDetailRequest
                {
                    IdTimetablePrefHeader = existSubject.SubjectCombinations.Select(x => x.Id).ToList(),
                    DetailRequests = timetableDetailDetailRequests
                };
                var reqString = JsonConvert.SerializeObject(timetableDetailRequest);
                var req = await _timeTablePreference.TimetableDetail(timetableDetailRequest);
                if (req.IsSuccess)
                {
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await Transaction.CommitAsync(CancellationToken);
                    return Request.CreateApiResult2();
                }
                else
                {
                    throw new Exception(req.Message);
                }
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);
            return Request.CreateApiResult2();

        }
    }
}
