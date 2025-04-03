using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class UpdateStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateStudentParticipantHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateStudentParticipantRequest, UpdateStudentParticipantValidator>();

            var idGrade = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(hs => hs.Homeroom)
                                .Where(x => x.Homeroom.Id == param.IdHomeroom)
                                .Select(x => x.Homeroom.IdGrade)
                                .FirstOrDefaultAsync(CancellationToken);

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                                                    .Where(x => x.IdGrade == idGrade &&
                                                                x.IdStudent == param.IdStudent)
                                                    .ToListAsync(CancellationToken);

            var updatedExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                                                    .Where(x => x.IdGrade == idGrade &&
                                                                x.IdStudent == param.IdStudent &&
                                                                x.IdExtracurricular == param.IdExtracurricular)
                                                    .FirstOrDefaultAsync(CancellationToken);

            if(getExtracurricularParticipant != null)
            {
                #region no longer using priority
                //// get max priority of student extracurricular
                //int updatedExtracurricularPriority = updatedExtracurricularParticipant.Priority;

                //int maxPriority = getExtracurricularParticipant.Select(x => x.Priority).OrderByDescending(priority => priority).FirstOrDefault();


                //foreach (var extracurricular in getExtracurricularParticipant)
                //{
                //    if (extracurricular.IdExtracurricular != updatedExtracurricularParticipant.IdExtracurricular)
                //    {
                //        // minus one all priority above the updated extracurricular participant
                //        if (extracurricular.Priority > updatedExtracurricularPriority)
                //        {
                //            var changePriorityExtracurricular = new MsExtracurricularParticipant();
                //            changePriorityExtracurricular = extracurricular;

                //            changePriorityExtracurricular.Priority -= 1;
                //            _dbContext.Entity<MsExtracurricularParticipant>().Update(changePriorityExtracurricular);
                //        }
                //    }
                //}
                #endregion

                updatedExtracurricularParticipant.Status = param.Status;
                //updatedExtracurricularParticipant.Priority = maxPriority;
                _dbContext.Entity<MsExtracurricularParticipant>().Update(updatedExtracurricularParticipant);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            else
            {
                throw new BadRequestException("Student extracurricular not found");
            }

            return Request.CreateApiResult2();
        }
    }
}
