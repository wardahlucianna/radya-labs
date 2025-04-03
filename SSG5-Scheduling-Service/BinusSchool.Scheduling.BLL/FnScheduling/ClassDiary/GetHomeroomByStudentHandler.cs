using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetHomeroomByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomByStudentHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomByStudentRequest>();

            var ReturnResult = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                        .Include(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                        .Where(e=>e.Homeroom.IdAcademicYear==param.AcademicYearId
                                            && e.Homeroom.IdGrade==param.GradeId
                                            && e.IdStudent==param.UserId
                                            && e.Homeroom.Semester==param.Semester)
                                        .Select(e=>new ItemValueVm()
                                        {
                                            Id = e.Homeroom.Id,
                                            Description = (e.Homeroom.Grade.Code)+ (e.Homeroom.GradePathwayClassroom.Classroom.Code)
                                        }).Distinct().ToListAsync(CancellationToken);
            
            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
