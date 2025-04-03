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
    public class GetDetailExemplaryCategorySettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailExemplaryCategorySettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailExemplaryCategorySettingsRequest>(
                            nameof(GetDetailExemplaryCategorySettingsRequest.IdExemplaryCharacter));

            var item = await _dbContext.Entity<LtExemplaryCategory>()
                            .Where(x => x.Id == param.IdExemplaryCharacter)
                            .Select(x => new GetDetailExemplaryCategorySettingsResult
                            {
                                IdExemplaryCategory = x.Id,
                                ShortDesc = x.ShortDesc,
                                LongDesc = x.LongDesc,
                                CurrentStatus = x.CurrentStatus
                            })
                            .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }
    }
}
