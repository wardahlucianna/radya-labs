using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class GetListClassHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetListClassHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListClassRequest>(new string[] { nameof(GetListClassRequest.IdSchoolGrade), nameof(GetListClassRequest.IdSchoolPathway) });

            var getClass = await _dbContext.Entity<MsGradePathwayClassroom>()
                                .Include(p => p.GradePathway.Grade)
                                .Include(p => p.GradePathway.GradePathwayDetails)
                                .Include(p => p.Classroom)
                                .Where(p => p.GradePathway.IdGrade == param.IdSchoolGrade)
                                .Where(p => p.GradePathway.Id == param.IdSchoolPathway)
                                .Select(p => new GetListClassResult()
                                {
                                    Id = p.IdClassroom,
                                    Code = p.Classroom.Code,
                                    Description = p.Classroom.Description,
                                }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(getClass as object);
        }
    }
}
