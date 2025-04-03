using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.CheckUsage
{
    public class CheckUsageSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public CheckUsageSubjectHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("id", out var id))
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], Localizer["Subject"], "Id", id));

            var query = await _dbContext.Entity<MsForm>()
                .Where(x => EF.Functions.Like(x.IdSubject, $"%{id}%"))
                .Select(x => x.IdSubject)
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2((query != null) as object);
        }
    }
}