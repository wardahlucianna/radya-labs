using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentParticipantHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentParticipantRequest>(nameof(GetStudentParticipantRequest.IdEvent),nameof(GetStudentParticipantRequest.IdActivity));

            var dataStudent = await (from te in _dbContext.Entity<TrEvent>()
                                join teaa in _dbContext.Entity<TrEventActivityAward>() on te.Id equals teaa.IdEventActivity
                                join ma in _dbContext.Entity<MsAward>() on teaa.IdAward equals ma.Id
                                join mhs in _dbContext.Entity<MsHomeroomStudent>() on teaa.IdHomeroomStudent equals mhs.Id
                                join s in _dbContext.Entity<MsStudent>() on mhs.IdStudent equals s.Id
                                join sg in _dbContext.Entity<MsStudentGrade>() on mhs.IdStudent equals sg.IdStudent
                                join g in _dbContext.Entity<MsGrade>() on sg.IdGrade equals g.Id
                                join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                join h in _dbContext.Entity<MsHomeroom>() on g.Id equals h.IdGrade
                                join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                                join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                
                                select new GetStudentParticipantResult
                                {
                                    Fullname = s.FirstName + " " + s.LastName,
                                    BinusianID = s.Id,
                                    Grade = g.Description,
                                    Homeroom = g.Code + c.Code,
                                    Award = ma.Description
                                }).ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.Search))
                dataStudent = dataStudent.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower())).ToList();
                
                switch(param.OrderBy)
                {
                    case "fullname":
                        dataStudent = param.OrderType == OrderType.Desc 
                            ? dataStudent.OrderByDescending(x => x.Fullname).ToList()
                            : dataStudent.OrderBy(x => x.Fullname).ToList();
                        break;
                    case "binusianid":
                        dataStudent = param.OrderType == OrderType.Desc 
                            ? dataStudent.OrderByDescending(x => x.BinusianID).ToList()
                            : dataStudent.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "grade":
                        dataStudent = param.OrderType == OrderType.Desc 
                            ? dataStudent.OrderByDescending(x => x.Grade).ToList()
                            : dataStudent.OrderBy(x => x.Grade).ToList();
                        break;
                    case "homeroom":
                        dataStudent = param.OrderType == OrderType.Desc 
                            ? dataStudent.OrderByDescending(x => x.Homeroom).ToList()
                            : dataStudent.OrderBy(x => x.Homeroom).ToList();
                        break;
                    case "award":
                        dataStudent = param.OrderType == OrderType.Desc 
                            ? dataStudent.OrderByDescending(x => x.Award).ToList()
                            : dataStudent.OrderBy(x => x.Award).ToList();
                        break;
                };

                var dataUserPagination = dataStudent
                .SetPagination(param)
                .Select(x => new GetStudentParticipantResult
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    Award = x.Award
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                ? dataUserPagination.Count 
                : dataStudent.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));

        }
    }
}
