using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.SubjectSession;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSubject.SubjectSession
{
    public class GetSubjectSessionHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetSubjectSessionRequest.IdSubject)
        });
        
        private readonly ISchoolDbContext _dbContext;

        public GetSubjectSessionHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectSessionRequest>(_requiredParams.Value);
            var results = await _dbContext.Entity<MsSubjectSession>()
                .Where(x => x.IdSubject == param.IdSubject)
                .Select(x => new GetSubjectSessionResult
                {
                    Id = x.Id,
                    Count = x.Content,
                    Length = x.Length
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(results as object);
        }
    }
}
