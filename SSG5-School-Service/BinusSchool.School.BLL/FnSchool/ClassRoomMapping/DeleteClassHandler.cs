using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class DeleteClassHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public DeleteClassHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DeleteSchoolClassRoomMappingRequest>(nameof(DeleteSchoolClassRoomMappingRequest.IdGradePathwayClassroom));
            var ids = new[] { param.IdGradePathwayClassroom }.AsEnumerable();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var datas = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.SubjectCombinations)
                .Include(x => x.ClassroomDivisions)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                // don't set inactive when row have to-many relation
                if (data.SubjectCombinations.Count != 0)
                {
                    datas.Remove(data);
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Classroom.Description ?? data.Classroom.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsGradePathwayClassroom>().Update(data);
                }

                // set inactive division
                if (data.ClassroomDivisions.Count != 0)
                {
                    foreach (var division in data.ClassroomDivisions)
                    {
                        division.IsActive = false;
                        _dbContext.Entity<MsClassroomDivision>().Update(division);
                    }
                }
            }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2(errors: undeleted.AsErrors());
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
