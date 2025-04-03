using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using EasyCaching.Core;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetExperienceToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStorageManager _storageManager;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly ISchool _schoolService;

        public GetExperienceToPdfHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExperienceToPdfRequest>();
            var predicate = PredicateBuilder.Create<TrExperience>(x => param.IdAcademicYear.Contains(x.IdAcademicYear));

            if (RoleConstant.Student == param.Role)
            {
                predicate = predicate.And(e => e.HomeroomStudent.IdStudent == param.IdUser);
            }
            else if (RoleConstant.Parent == param.Role)
            {
                var IdSibling = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(e => e.IdStudent == param.IdUser.Substring(1))
                            .Select(e => e.Id)
                            .FirstOrDefaultAsync(CancellationToken);

                var IdStudent = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(e => e.Id == IdSibling && e.IdStudent == param.IdStudent)
                            .Select(e => e.IdStudent)
                            .FirstOrDefaultAsync(CancellationToken);

                predicate = predicate.And(e => e.HomeroomStudent.IdStudent == param.IdStudent && (e.Status == ExperienceStatus.Approved || e.Status == ExperienceStatus.Completed));
            }
            else if (RoleConstant.Staff == param.Role)
            {
                var queryExperience = _dbContext.Entity<TrExperience>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                .Where(e => param.IdAcademicYear.Contains(e.IdAcademicYear));

                var getIdAyByCas = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.HomeroomStudent)
                              .Include(e => e.CasAdvisor)
                              .Where(e => e.CasAdvisor.IdUserCAS == param.IdUser && param.IdAcademicYear.Contains(e.CasAdvisor.IdAcademicYear) && e.HomeroomStudent.IdStudent == param.IdStudent)
                              .Select(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear)
                              .ToListAsync(CancellationToken);

                var listIdExperience = await queryExperience
                                        .Where(e => getIdAyByCas.Contains(e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear) && e.HomeroomStudent.IdStudent == param.IdStudent)
                                        .Select(e => e.Id)
                                        .Distinct().ToListAsync(CancellationToken);

                var listIdExperienceByExperience = await queryExperience
                           .Where(e => e.IdUserSupervisor == param.IdUser && e.HomeroomStudent.IdStudent == param.IdStudent)
                           .Select(e => e.Id)
                           .Distinct().ToListAsync(CancellationToken);

                listIdExperience.AddRange(listIdExperienceByExperience);

                predicate = predicate.And(x => listIdExperience.Contains(x.Id));
            }

            var GetExperience = await _dbContext.Entity<TrExperience>()
                            .Include(e => e.HomeroomStudent)
                            .Where(predicate)
                            .Select(e => new GetExperienceResult
                            {
                                Id = e.Id,
                                IdStudent = e.HomeroomStudent.Student.IdBinusian,
                                Student = e.HomeroomStudent.Student.FirstName + (e.HomeroomStudent.Student.MiddleName == null ? "" : " " + e.HomeroomStudent.Student.MiddleName) + (e.HomeroomStudent.Student.LastName == null ? "" : " " + e.HomeroomStudent.Student.LastName),
                                ExperienceName = e.ExperienceName,
                                Organizer = e.Organizer,
                                Description = e.Description,
                                Contribution = e.ContributionOrganizer,
                                ActivityDate = e.StartDate.ToString("dd MMM yyy") + " - " + e.EndDate.ToString("dd MMM yyy"),
                                LearningOutcome = e.TrExperienceLearnings.Select(e => e.LearningOutcome.LearningOutcomeName).ToList(),
                                Evidances = e.TrEvidences.Select(f => new GetEvidance
                                {
                                    Id = f.Id,
                                    Student = e.HomeroomStudent.Student.FirstName + (e.HomeroomStudent.Student.MiddleName == null ? "" : " " + e.HomeroomStudent.Student.MiddleName) + (e.HomeroomStudent.Student.LastName == null ? "" : " " + e.HomeroomStudent.Student.LastName),
                                    Date = f.DateUp == null ? Convert.ToDateTime(f.DateIn).ToString("dd MMM yyy HH:mm") : Convert.ToDateTime(f.DateUp).ToString("dd MMM yyy HH:mm"),
                                    Type = f.EvidencesType.ToString(),
                                    Value = f.EvidencesValue,
                                    Link = f.Url,
                                    Attachment = f.TrEvidencesAttachments.Select(g => g.Url).ToList(),
                                    LearningOutcome = f.TrEvidenceLearnings.Select(g => g.LearningOutcome.LearningOutcomeName).ToList(),
                                    IsComment = param.IsComment,
                                    Commens = !param.IsComment?default:f.TrEvidencesComments.Select(g => new GetComment
                                    {
                                        Id = g.Id,
                                        IdUserComment = g.IdUserComment,
                                        NameComment = g.UserComment.DisplayName,
                                        Date = g.DateUp == null ? Convert.ToDateTime(g.DateIn).ToString("dd MMM yyy HH:mm") : Convert.ToDateTime(g.DateUp).ToString("dd MMM yyy HH:mm"),
                                        Comment = g.Comment,
                                    }).ToList(),
                                }).ToList(),
                            })
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(GetExperience as object);
        }
    }
}
