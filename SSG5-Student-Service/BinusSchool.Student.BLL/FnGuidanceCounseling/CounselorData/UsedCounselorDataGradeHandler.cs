using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnGuidanceCounseling.CounselorData.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData
{
    public class UsedCounselorDataGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public UsedCounselorDataGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            string id = Request.RouteValues["id"].ToString();

            var dataCouncelorGrade = await _dbContext.Entity<MsCounselorGrade>()
                .Where(x => x.IdCounselor == id && x.IsActive)
                .Select(x => x.IdGrade)
                .Distinct()
                .ToListAsync(CancellationToken);

            var datas = await _dbContext.Entity<MsCounselorGrade>()
                .Where(x => !dataCouncelorGrade.Contains(x.IdGrade) && x.IsActive )
                .Select(x => x.IdGrade)
                .Distinct()
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(datas as object);
        }
    }
}
