using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class GetGradeAndClassByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGradeAndClassByStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomByStudentRequest>(
                nameof(GetHomeroomByStudentRequest.Ids), nameof(GetHomeroomByStudentRequest.IdGrades));

            //var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => param.IdGrades.Contains(x.MsGradePathway.IdGrade));
            //var crMapResult = await _dbContext.Entity<MsGradePathwayClassroom>()
            //    .Include(x => x.Classroom)
            //    .Include(x => x.MsGradePathway).ThenInclude(x => x.GradePathwayDetails)
            //    .Where(predicate)
            //    .Select(x => new GetClassroomMapByGradeResult
            //    {
            //        Id = x.Id,
            //        Code = x.Classroom.Code,
            //        Description = x.Classroom.Description,
            //        Formatted = $"{x.MsGradePathway.Grade.Code}{x.Classroom.Code}",
            //        Grade = new CodeWithIdVm
            //        {
            //            Id = x.MsGradePathway.IdGrade,
            //            Code = x.MsGradePathway.Grade.Code,
            //            Description = x.MsGradePathway.Grade.Description
            //        },
            //        Pathway = new ClassroomMapPathway
            //        {
            //            Id = x.MsGradePathway.Id,
            //            PathwayDetails = x.MsGradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
            //            {
            //                Id = y.Id,
            //                Code = y.Pathway.Code,
            //                Description = y.Pathway.Description
            //            })
            //        },
            //        Class = new CodeWithIdVm
            //        {
            //            Id = x.Classroom.Id,
            //            Code = x.Classroom.Code,
            //            Description = x.Classroom.Description
            //        }
            //    })
            //    .OrderBy(x => x.Grade.Code).ThenBy(x => x.Code).ToListAsync();
            //var hrStudents = await _dbContext.Entity<MsHomeroomStudent>()
            //    .Include(x => x.Homeroom)
            //    .Where(x => param.Ids.Contains(x.IdStudent) && param.IdGrades.Contains(x.Homeroom.IdGrade))
            //    .ToListAsync(CancellationToken);

            var homerooms = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .ThenInclude(x => x.GradePathwayClassroom)
                .ThenInclude(x => x.Classroom)
                .Include(x => x.Homeroom)
                .ThenInclude(x => x.Grade)
                .Where(x => param.Ids.Contains(x.IdStudent) && param.IdGrades.Contains(x.Homeroom.IdGrade))
                .Select(x => new GetGradeAndClassByStudentResult
                {
                    IdStudent = x.IdStudent,
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Homeroom.IdGrade,
                        Code = x.Homeroom.Grade.Code,
                        Description = x.Homeroom.Grade.Description
                    },
                    Classroom = new CodeWithIdVm
                    {
                        Id = x.Homeroom.Id,
                        Code = x.Homeroom.GradePathwayClassroom.Classroom.Code,
                        Description = x.Homeroom.GradePathwayClassroom.Classroom.Description
                    }
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(homerooms as object);
        }
    }
}
