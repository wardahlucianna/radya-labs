using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class GetExemplaryCategoryUsedHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetExemplaryCategoryUsedHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetExemplaryCategoryUsedRequest>(
                            nameof(GetExemplaryCategoryUsedRequest.IdSchool));

            var ExemplaryCategoryList = await _dbContext.Entity<LtExemplaryValue>()
                            .Include(x => x.TrExemplaryValues)
                            .Where(x => x.IdSchool == param.IdSchool
                            && x.TrExemplaryValues.Count() > 0)
                            .Select(x => new GetListExemplaryValueSettingsResult
                            {
                                IdExemplaryValue = x.Id,
                                ShortDesc = x.ShortDesc,
                                LongDesc = x.LongDesc,
                                OrderNumber = x.OrderNumber,
                                CurrentStatus = x.CurrentStatus
                            })
                            .Distinct()
                            .OrderBy(x => x.OrderNumber)
                            .ThenBy(x => x.ShortDesc)
                            .ToListAsync();


            return Request.CreateApiResult2(ExemplaryCategoryList as object);
        }
    }
}
