using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class UpdateStudentExtracurricularPriorityHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public UpdateStudentExtracurricularPriorityHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateStudentExtracurricularPriorityRequest, UpdateStudentExtracurricularPriorityValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {

                #region no longer using extracurricular rule for this module
                // get max effective number for the grade
                //var extracurricularRule = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                //                                    .Include(ergm => ergm.ExtracurricularRule)
                //                                    .Where(x => x.IdGrade == param.IdGrade &&
                //                                                x.ExtracurricularRule.Status == true)
                //                                    .FirstOrDefault();
                #endregion

                // get number of student extracurricular
                var studentExtracurricularCount = _dbContext.Entity<MsExtracurricularParticipant>()
                                                    .Include(ep => ep.Extracurricular)
                                                    .Where(x => x.Status == true &&
                                                                x.IdGrade == param.IdGrade &&
                                                                x.Extracurricular.Semester == param.Semester &&
                                                                x.IdStudent == param.IdStudent)
                                                    .Count();

                #region not using priorty anymore
                //var extracurricularPriorityListParam = param.ExtracurricularPriorityList;

                //// validate if each priority in param cannot have same value of priority
                //var countPriorityParam = extracurricularPriorityListParam.Select(x => x.Priority).Distinct().Count();

                //if (countPriorityParam != extracurricularPriorityListParam.Count())
                //{
                //    throw new BadRequestException("Each extracurriculars must have different priority");
                //}
                //else
                //{
                //    foreach (var extracurricularParam in extracurricularPriorityListParam)
                //    {

                //        // validate priority cannot exceed the maxEffectiveCount
                //        if (extracurricularParam.Priority > studentExtracurricularCount)
                //        {
                //            throw new BadRequestException("Priority cannot have greater value than max number of student extracurriculars");
                //        }
                //        else
                //        {
                //            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                //                                    .Include(ep => ep.Extracurricular)
                //                                    .Where(x => x.Status == true &&
                //                                                x.IdGrade == param.IdGrade &&
                //                                                x.Extracurricular.Semester == param.Semester &&
                //                                                x.Extracurricular.Id == extracurricularParam.IdExtracurricular &&
                //                                                x.IdStudent == param.IdStudent)
                //                                    .FirstOrDefaultAsync(CancellationToken);

                //            // cannot change to primary extracurricular if the ShowScoreRC is false
                //            if (getExtracurricularParticipant.Extracurricular.ShowScoreRC == false && extracurricularParam.Priority == 1)
                //            {
                //                throw new BadRequestException("Cannot change to primary extracurricular because this extracurricular does not need scoring in the report card");
                //            }
                //            else
                //            {
                //                getExtracurricularParticipant.Priority = extracurricularParam.Priority;
                //                _dbContext.Entity<MsExtracurricularParticipant>().Update(getExtracurricularParticipant);
                //            }
                //        }
                //    }

                //    await _dbContext.SaveChangesAsync(CancellationToken);
                //    await _transaction.CommitAsync(CancellationToken);
                //}
                #endregion

                var extracurricularPrimaryListParam = param.ExtracurricularPrimaryList;

                foreach (var extracurricularParam in extracurricularPrimaryListParam)
                {

                    var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                                            .Include(ep => ep.Extracurricular)
                                            .Where(x => x.Status == true &&
                                                        x.IdGrade == param.IdGrade &&
                                                        x.Extracurricular.Semester == param.Semester &&
                                                        x.Extracurricular.Id == extracurricularParam.IdExtracurricular &&
                                                        x.IdStudent == param.IdStudent)
                                            .FirstOrDefaultAsync(CancellationToken);

                    getExtracurricularParticipant.IsPrimary = extracurricularParam.IsPrimary;
                    _dbContext.Entity<MsExtracurricularParticipant>().Update(getExtracurricularParticipant);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
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

            return Request.CreateApiResult2();
        }
    }
}
