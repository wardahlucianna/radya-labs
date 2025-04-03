using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradePathwayForAscTimetableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradePathwayForAscTimetableHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradepathwayForXMLRequest>(nameof(GetGradepathwayForXMLRequest.IdGradePathway));
            var gradeByGradePathway = await _dbContext.Entity<MsGradePathway>()
                                                 .Include(p => p.Sessions)
                                                      .ThenInclude(p => p.Day)
                                                  .Include(p => p.Grade.Level.AcademicYear)
                                                 .Include(p => p.GradePathwayDetails)
                                                    .ThenInclude(p => p.Pathway.AcademicYear)
                                                 .Include(p => p.GradePathwayClassrooms)
                                                    .ThenInclude(p => p.Classroom)
                                                 .Include(p => p.GradePathwayClassrooms)
                                                    .ThenInclude(p => p.GradePathway.Grade)
                                                 .Where(p => param.IdGradePathway.Any(x => x == p.Id) && p.Grade.Level.IdAcademicYear==param.IdAcademicyear)
                                                 .OrderBy(p=> p.Grade.OrderNumber)
                                                 .Select(p=> new GradePathwayForAscTimeTableResult 
                                                 {
                                                    IdGradePathway=p.Id,
                                                    IdGrade=p.Grade.Id,
                                                    GradeCode=p.Grade.Code,
                                                    GradeDescription=p.Grade.Description,
                                                    GradePathwayClassRooms=p.GradePathwayClassrooms.Count()>0?p.GradePathwayClassrooms.Select(x=> new GradePathwayClassRoom 
                                                    {
                                                      IdClassRoom=x.Classroom.Id,
                                                      IdGradePathwayClassrom=x.Id,
                                                      ClassRoomCode=x.Classroom.Code,
                                                      ClassRoomDescription=x.Classroom.Description,
                                                      ClassRoomCombinationGrade=x.GradePathway.Grade.Code+x.Classroom.Code,
                                                      IdSchool=x.Classroom.IdSchool,
                                                    }).ToList():new List<GradePathwayClassRoom>(),
                                                    GradePathwayDetails=p.GradePathwayDetails.Count()>0?p.GradePathwayDetails.Select(x=> new GradePathwayDetail 
                                                    {
                                                        IdGradePathwayDetail=x.Id,
                                                        IdPathway=x.Pathway.Id,
                                                        PathwayCode=x.Pathway.Code,
                                                        PathwayDescription=x.Pathway.Description,
                                                        IdSchool=x.Pathway.AcademicYear.IdSchool,
                                                    }).ToList():new List<GradePathwayDetail>(),
                                                 })
                                                 .ToListAsync();

            return Request.CreateApiResult2(gradeByGradePathway as object);
        }
    }
}
