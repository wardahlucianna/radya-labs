using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Model.School.FnSubject.SubjectSession;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.SubjectCombination.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination
{
    public class AddSubjectCombinationHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ITeachingDbContext _dbContext;
        //private readonly IApiService<ISubjectSession> _subjectSessionService;

        public AddSubjectCombinationHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
            //_subjectSessionService = subjectSessionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddSubjectCombination, SubjectCombinationValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            //FillConfiguration();

            var idSubjectCombinations = body.SubjectCombination.Select(x => x.Id);
            var subjectCombinations = await _dbContext.Entity<MsSubjectCombination>()
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails)
                .Include(x => x.Subject).ThenInclude(x => x.SubjectSessions)
                .Where(x => idSubjectCombinations.Contains(x.Id))
                .ToListAsync(CancellationToken);

            foreach (var item in body.SubjectCombination)
            {
                var data = subjectCombinations.Find(x => x.Id == item.Id);

                if (data != null)
                {
                    if (item.IsDelete.HasValue && item.IsDelete.Value)
                    {
                        var checkTimetable = data.TimeTablePrefHeader;
                        if (checkTimetable is { CanDelete: true })
                        {
                            data.IsActive = false;
                            _dbContext.Entity<MsSubjectCombination>().Update(data);

                            checkTimetable.IsActive = false;
                            _dbContext.Entity<TrTimeTablePrefHeader>().Update(checkTimetable);

                            foreach (var ttpDetail in checkTimetable.TimetablePrefDetails)
                            {
                                ttpDetail.IsActive = false;
                                _dbContext.Entity<TrTimetablePrefDetail>().Update(ttpDetail);
                            }
                        }
                        else
                        {
                            _dbContext.Entity<MsSubjectCombination>().Update(data);
                        }
                    }
                    else
                    {
                        _dbContext.Entity<MsSubjectCombination>().Update(data);
                    }
                }
                else
                {

                    var check = _dbContext.Entity<MsSubjectCombination>()
                        .Include(x => x.Subject)
                        .ThenInclude(x => x.SubjectSessions).Any(p => p.IdGradePathwayClassroom == item.IdGradeClass && p.IdSubject == item.IdSubject && p.IsActive == true);
                    if (!check)
                    {
                        //var subject = await _subjectSessionService
                        //    .SetConfigurationFrom(ApiConfiguration)
                        //    .Execute.GetSubjectSessions(new GetSubjectSessionRequest { IdSubject = item.IdSubject });
                        var subjects = subjectCombinations.Where(x => x.IdSubject == item.IdSubject).Select(x => x.Subject).FirstOrDefault();
                        var id = Guid.NewGuid().ToString();

                        var dataSubject = new MsSubjectCombination
                        {
                            Id = id,
                            IdGradePathwayClassroom = item.IdGradeClass,
                            IdSubject = item.IdSubject,
                            UserIn = AuthInfo.UserId
                        };
                        _dbContext.Entity<MsSubjectCombination>().Add(dataSubject);

                        var ttpHeader = new TrTimeTablePrefHeader()
                        {
                            Id = id,
                            CanDelete = true,
                            IsMerge = false,
                            Status = false
                        };
                        _dbContext.Entity<TrTimeTablePrefHeader>().Add(ttpHeader);

                        foreach (var subjectSessionResult in subjects.SubjectSessions)
                        {
                            var ttpDetail = new TrTimetablePrefDetail
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdTimetablePrefHeader = id,
                                Count = subjectSessionResult.Content,
                                Length = subjectSessionResult.Length
                            };
                            _dbContext.Entity<TrTimetablePrefDetail>().Add(ttpDetail);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
