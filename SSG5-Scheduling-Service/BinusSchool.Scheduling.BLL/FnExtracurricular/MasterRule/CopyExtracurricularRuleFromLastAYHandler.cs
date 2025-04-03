using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule
{
    public class CopyExtracurricularRuleFromLastAYHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopyExtracurricularRuleFromLastAYHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopyExtracurricularRuleFromLastAYRequest, CopyExtracurricularRuleFromLastAYValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getCurrentAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                    .Where(a => a.Id == body.IdAcademicYear)
                    .FirstOrDefaultAsync(CancellationToken);

                var getPrevAcademicYear = int.Parse(getCurrentAcademicYear.Code) - 1;

                var prevAcademicYear = getPrevAcademicYear.ToString();

                if (body.Semester == 1 && body.IsBothSemester == false)
                {
                    var getExtracurricularRule = _dbContext.Entity<MsExtracurricularRule>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.AcademicYear.Code == prevAcademicYear
                            && a.Semester == 2);

                    foreach (var items in body.IdExtracurricularRule)
                    {
                        var getIdExtracurricularRule = getExtracurricularRule.Where(b => b.Id == items).FirstOrDefault();

                        var copyExtracurricularRule = new MsExtracurricularRule
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = body.IdAcademicYear,
                            Semester = body.Semester,
                            Name = getIdExtracurricularRule.Name,
                            MinEffectives = getIdExtracurricularRule.MinEffectives,
                            MaxEffectives = getIdExtracurricularRule.MaxEffectives,
                            RegistrationStartDate = getIdExtracurricularRule.RegistrationStartDate,
                            RegistrationEndDate = getIdExtracurricularRule.RegistrationEndDate,
                            Status = getIdExtracurricularRule.Status,
                            DueDayInvoice = getIdExtracurricularRule.DueDayInvoice,
                            ReviewDate = getIdExtracurricularRule.ReviewDate
                        };

                        _dbContext.Entity<MsExtracurricularRule>().Add(copyExtracurricularRule);
                    }
                }
                else if (body.Semester == 2 && body.IsBothSemester == false)
                {
                    var getExtracurricularRule = _dbContext.Entity<MsExtracurricularRule>()
                        .Where(a => a.IdAcademicYear == body.IdAcademicYear
                            && a.Semester == 1);

                    foreach (var items in body.IdExtracurricularRule)
                    {
                        var getIdExtracurricularRule = getExtracurricularRule.Where(b => b.Id == items).FirstOrDefault();

                        var copyExtracurricularRule = new MsExtracurricularRule
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = body.IdAcademicYear,
                            Semester = body.Semester,
                            Name = getIdExtracurricularRule.Name,
                            MinEffectives = getIdExtracurricularRule.MinEffectives,
                            MaxEffectives = getIdExtracurricularRule.MaxEffectives,
                            RegistrationStartDate = getIdExtracurricularRule.RegistrationStartDate,
                            RegistrationEndDate = getIdExtracurricularRule.RegistrationEndDate,
                            Status = getIdExtracurricularRule.Status,
                            DueDayInvoice = getIdExtracurricularRule.DueDayInvoice,
                            ReviewDate = getIdExtracurricularRule.ReviewDate
                        };

                        _dbContext.Entity<MsExtracurricularRule>().Add(copyExtracurricularRule);
                    }
                }
                else if (body.IsBothSemester == true)
                {
                    var getExtracurricularRule = _dbContext.Entity<MsExtracurricularRule>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.AcademicYear.Code == prevAcademicYear);

                    for (int i = 1; i <= 2; i++)
                    {
                        foreach (var items in body.IdExtracurricularRule)
                        {
                            var getIdExtracurricularRule = getExtracurricularRule.Where(b => b.Semester == 2 && b.Id == items).FirstOrDefault();

                            var copyExtracurricularRule = new MsExtracurricularRule
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdAcademicYear = body.IdAcademicYear,
                                Semester = i,
                                Name = getIdExtracurricularRule.Name,
                                MinEffectives = getIdExtracurricularRule.MinEffectives,
                                MaxEffectives = getIdExtracurricularRule.MaxEffectives,
                                RegistrationStartDate = getIdExtracurricularRule.RegistrationStartDate,
                                RegistrationEndDate = getIdExtracurricularRule.RegistrationEndDate,
                                Status = getIdExtracurricularRule.Status,
                                DueDayInvoice = getIdExtracurricularRule.DueDayInvoice,
                                ReviewDate = getIdExtracurricularRule.ReviewDate
                            };

                            _dbContext.Entity<MsExtracurricularRule>().Add(copyExtracurricularRule);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();

                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
