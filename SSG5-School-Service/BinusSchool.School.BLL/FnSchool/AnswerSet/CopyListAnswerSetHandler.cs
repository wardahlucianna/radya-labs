using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using BinusSchool.Persistence.SchoolDb;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AnswerSet
{
    public class CopyListAnswerSetHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public CopyListAnswerSetHandler(
            DbContextOptions<SchoolDbContext> options)
        {
            _dbContext = new SchoolDbContext(options); 
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<CopyListAnswerSetRequest>();

            var getIdCopyAnswerSet = param.ListAnswerSets.Select(x => x.Id).ToList();
            if(!getIdCopyAnswerSet.Any())
                throw new NotFoundException("List Answer Set can not be empty.");

            var dataOptions = _dbContext.Entity<MsAnswerSetOption>().Where(x => getIdCopyAnswerSet.Any(y => y == x.IdAnswerSet)).ToList();
            
            foreach (var data in param.ListAnswerSets)
            {
                var AnswerSetId = Guid.NewGuid().ToString();
                var answerSet = new MsAnswerSet
                {
                    Id = AnswerSetId,
                    IdAcademicYear = param.IdAcademicYear,
                    AnswerSetName = data.AnswerSetName
                };
                _dbContext.Entity<MsAnswerSet>().Add(answerSet);

                var AnswerSetOptions = dataOptions.Where(x => x.IdAnswerSet == data.Id).ToList();
                var order = 0;
                foreach (var answerSetOption in AnswerSetOptions)
                {
                    var setOption = new MsAnswerSetOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAnswerSet = AnswerSetId,
                        AnswerSetOptionName = answerSetOption.AnswerSetOptionName,
                        Order = order
                    };
                    order++;
                    _dbContext.Entity<MsAnswerSetOption>().Add(setOption);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();

        }
    }
}
