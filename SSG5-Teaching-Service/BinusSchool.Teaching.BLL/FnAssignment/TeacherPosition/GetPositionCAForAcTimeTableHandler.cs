using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition
{
    public class GetPositionCAForAcTimeTableHanlder : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        public GetPositionCAForAcTimeTableHanlder(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCAForAscTimetableRequest>(nameof(GetCAForAscTimetableRequest.IdSchool));
            var result = "";
            var getdata = await _dbContext.Entity<MsTeacherPosition>().Where(p => p.IdSchool == param.IdSchool && p.IdPosition == "5").FirstOrDefaultAsync();
            if (getdata!=null) 
            {
                result = getdata.Id;
            }
            return Request.CreateApiResult2(result as object);
        }
    }
}
