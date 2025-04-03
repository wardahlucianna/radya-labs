using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using BinusSchool.Document.FnDocument.BLPGroup.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.BLPGroup
{
    public class BLPGroupHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public BLPGroupHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetBLPGroups = await _dbContext.Entity<LtBLPGroup>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetBLPGroups.ForEach(x => x.IsActive = false);

            _dbContext.Entity<LtBLPGroup>().UpdateRange(GetBLPGroups);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var returnResult = await _dbContext.Entity<LtBLPGroup>()
                .Include(x => x.Level)
                    .ThenInclude(x => x.AcademicYear)
                .Where(x => x.Id == id)
                .Select(x => new GetBLPGroupDetailResult { 
                    AcademicYear = new ItemValueVm
                    {
                        Id = x.Level.IdAcademicYear,
                        Description = x.Level.AcademicYear.Description
                    },
                    Level = new ItemValueVm
                    {
                        Id = x.Level.Id,
                        Description = x.Level.Description
                    },
                    GroupName = x.GroupName

                })
                .FirstOrDefaultAsync();

            if (returnResult is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["BLP Group"], "Id", id));

            return Request.CreateApiResult2(returnResult as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBLPGroupRequest>(nameof(GetBLPGroupRequest.IdLevel));

            IReadOnlyList<IItemValueVm> returnResult;

            returnResult = await _dbContext.Entity<LtBLPGroup>()
                                            .Where(a => a.IdLevel == param.IdLevel)
                                            .Select(a => new GetBLPGroupResult
                                            {
                                                Id = a.Id,
                                                Description = a.GroupName
                                            })
                                            .OrderBy(a => a.Description)
                                            .ToListAsync();

            return Request.CreateApiResult2(returnResult);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            try
            {
                var body = await Request.ValidateBody<SaveBLPGroupRequest, SaveBLPGroupValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExist = await _dbContext.Entity<LtBLPGroup>()
                    .Where(x => x.IdLevel == body.IdLevel)
                    .ToListAsync(CancellationToken);

                if(isExist != null)
                {
                    var checkName = isExist.Where(x => x.GroupName.Contains(body.GroupName)).FirstOrDefault();
                    if (checkName != null)
                        throw new BadRequestException("Group Name already exist in IdLevel");
                }

                var setNewBLPGroup = new LtBLPGroup
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                    GroupName = body.GroupName
                };

                _dbContext.Entity<LtBLPGroup>().Add(setNewBLPGroup);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateBLPGroupRequest, UpdateBLPGroupValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExist = await _dbContext.Entity<LtBLPGroup>()
                    .Where(x => x.Id == body.IdBLPGroup)
                    .FirstOrDefaultAsync(CancellationToken);

                if (isExist != null)
                {
                    if (isExist.IdLevel == body.IdLevel && isExist.GroupName.ToLower().Trim() == body.GroupName.ToLower().Trim())
                    {
                        throw new BadRequestException("Group Name already exist in IdLevel");
                    }
                    else
                    {
                        isExist.IdLevel = body.IdLevel;
                        isExist.GroupName = body.GroupName;

                        _dbContext.Entity<LtBLPGroup>().Update(isExist);
                    }
                }
                else
                {
                    throw new BadRequestException("Group not Found");
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
