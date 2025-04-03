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
    public class CheckUsageTermHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public CheckUsageTermHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("id", out var id))
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], Localizer["Term"], "Id", id));

            var query = await _dbContext.Entity<MsForm>()
                .Where(x => EF.Functions.Like(x.IdPeriod, $"%{id}%"))
                .Select(x => x.IdPeriod)
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2((query != null) as object);
        }
    }
}