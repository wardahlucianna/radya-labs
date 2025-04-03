using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Pathway.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.Pathway
{
    public class CopyPathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopyPathwayHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopyPathwayRequest, CopyPathwayValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyearFrom);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Acadyear"], "Id", body.IdAcadyearFrom));

            var copyDatas = await _dbContext.Entity<MsPathway>().Where(p => p.IdAcademicYear == body.IdAcadyearFrom).ToListAsync();
            if (copyDatas.Count == 0)
                throw new NotFoundException($"Data From Academic year {acadyear.Description} Not found");

            foreach (var item in copyDatas)
            {
                var isExist = await _dbContext.Entity<MsPathway>()
                    .Where(x => x.IdAcademicYear == body.IdAcadyearTo && x.Code.ToLower() == item.Code.ToLower())
                    .FirstOrDefaultAsync();

                if (isExist == null)
                {
                    var pathway = new MsPathway
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcadyearTo,
                        Code = item.Code,
                        Description = item.Description
                    };

                    _dbContext.Entity<MsPathway>().Add(pathway);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
