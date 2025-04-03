using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherPositionInfo
{
    public class GetTeacherPositionByUserIdHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        public GetTeacherPositionByUserIdHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetTeacherPositionByUserIDRequest>();

            var query = _dbContext.Entity<TrNonTeachingLoad>()
                        .Include(x => x.MsNonTeachingLoad)
                        .Include(x => x.MsNonTeachingLoad.TeacherPosition)
                        .Include(x => x.MsNonTeachingLoad.TeacherPosition.Position)
                        .OrderBy(x => x.MsNonTeachingLoad.TeacherPosition.Position.PositionOrder)
                        .Where(x => x.IdUser == param.UserId
                                    && x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
                        .Select(x => new GetTeacherPositionByUserIDResult
                        {
                            Data = x.Data,
                            IdTeacherPosition = x.MsNonTeachingLoad.TeacherPosition.Id,
                            PositionName = x.MsNonTeachingLoad.TeacherPosition.Position.Description,
                            PositionShortName = x.MsNonTeachingLoad.TeacherPosition.Position.Code
                        }).FirstOrDefault();

            return Request.CreateApiResult2(query as object);
        }

    }
}
