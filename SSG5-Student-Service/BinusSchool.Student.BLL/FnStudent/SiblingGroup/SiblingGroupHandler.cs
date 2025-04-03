using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Common.Model.Information;
using BinusSchool.Student.FnStudent.SiblingGroup.Validator;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.SiblingGroup
{
    public class SiblingGroupHandler: FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbSiblingGroupContext;
        public SiblingGroupHandler(IStudentDbContext dbSiblingGroupContext)
        {
            _dbSiblingGroupContext = dbSiblingGroupContext;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            /*var body = await Request.ValidateBody<AddSiblingGroupRequest, AddSiblingGroupValidator>();
            Transaction = await _dbSiblingGroupContext.BeginTransactionAsync(CancellationToken);

            var datas = await _dbSiblingGroupContext.Entity<MsSiblingGroup>()
                .Where(x => ids.Any(y => y == x.Id) && x.IdStudent == body.IdStudent)
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                    data.IsActive = false;
                _dbSiblingGroupContext.Entity<MsSiblingGroup>().Update(data);
            }

            await _dbSiblingGroupContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());*/
            throw new NotImplementedException();

        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            //var param = Request.ValidateParams<GetSiblingGroupRequest>(nameof(GetSiblingGroupRequest.IdStudent));
            var query = await _dbSiblingGroupContext.Entity<MsSiblingGroup>()
                        .Where(x => x.Id == id)
                        .Select(x => new GetSiblingGroupDetailResult
                        {
                            Id = x.Id,
                            IdStudent = x.IdStudent,
                            Description = "Sibling Group",
                            Audit = x.GetRawAuditResult2()
                        })
                        .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var query = _dbSiblingGroupContext.Entity<MsSiblingGroup>();
            IReadOnlyList<IItemValueVm> items;
            items = await query
                    .Select(x => new GetSiblingGroupResult
                    {
                        Id = x.Id,
                        IdStudent = x.IdStudent,
                        Description = "Sibling Group",
                        Audit = x.GetRawAuditResult2()
                    })
                    .ToListAsync(CancellationToken);

            var count = param.GetAll == true
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);
            
            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }
            
        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            /*var body = await Request.ValidateBody<AddSiblingGroupRequest, AddSiblingGroupValidator>();
            Transaction = await _dbSiblingGroupContext.BeginTransactionAsync(CancellationToken);

            //var siblingGroup = await _dbSiblingGroupContext.Entity<MsSiblingGroup>().FindAsync(body.IdSiblingGroup);
            //if (siblingGroup is null)
                //throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ID Student"], "Id", body.IdStudent));

            var isExist = await _dbSiblingGroupContext.Entity<MsSiblingGroup>()
                .Where(x => x.IdStudent == body.IdStudent ) //&& x.Id == body.IdSiblingGroup
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.IdStudent} already exist");

            var param = new MsSiblingGroup
            {
                IdStudent = body.IdStudent,
                Id = body.IdSiblingGroup,
                UserIn = AuthInfo.UserId,
                DateIn = DateTime.Now
            };

            _dbSiblingGroupContext.Entity<MsSiblingGroup>().Add(param);

            await _dbSiblingGroupContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();*/
            throw new NotImplementedException();
        }
        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
