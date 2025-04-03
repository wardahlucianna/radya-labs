﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselorLevelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetCounselorLevelHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselorLevelRequest>();

            var query = await (from counselor in _dbContext.Entity<MsCounselor>()
                               join counselorGrade in _dbContext.Entity<MsCounselorGrade>() on counselor.Id equals counselorGrade.IdCounselor
                               join grade in _dbContext.Entity<MsGrade>() on counselorGrade.IdGrade equals grade.Id
                               join level in _dbContext.Entity<MsLevel>() on grade.IdLevel equals level.Id
                               where counselor.IdUser == param.IdUser
                               select new GetCounselorLevelResult
                               {
                                   Id = level.Id,
                                   Description = level.Description,
                                   Code = level.Code
                               }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
