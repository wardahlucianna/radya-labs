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
    public class DeleteDivisionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteDivisionHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DeleteSchoolClassRoomDivisionRequest>(nameof(DeleteSchoolClassRoomDivisionRequest.IdClassRoomDivision));
            var ids = new[] { param.IdClassRoomDivision }.AsEnumerable();
            var datas = await _dbContext.Entity<MsClassroomDivision>()
                .Include(x => x.PathwayClassroom)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                //harus cek data ke function time table preferences
                //if (getData.Division.TimetablePreferencesDetails.Count > 0 && getData.IdSchoolAcadyearLevelGradePathwayClassroom == getData.SchoolDivision.TimetablePreferencesDetails.FirstOrDefault().TimetablePreferenceHeader.SubjectCombination.IdGradeClass)
                //{
                //    throw new BadRequestException(string.Format(_localizer["ExAlreadyUse"], getData.SchoolDivision.Description));
                //}

                // // don't set inactive when row have to-many relation
                // if (data.Venues.Count != 0)
                // {
                //     datas.Remove(data);
                //     undeleted.AlreadyUse ??= new Dictionary<string, string>();
                //     undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                // }
                // else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsClassroomDivision>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
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
