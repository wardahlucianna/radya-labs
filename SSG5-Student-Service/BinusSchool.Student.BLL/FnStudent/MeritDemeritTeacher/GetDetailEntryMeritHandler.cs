using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailEntryMeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailMeritTeacherRequest>();
            var predicate = PredicateBuilder.Create<TrEntryMeritStudent>(x => x.IdHomeroomStudent == param.IdHomeroomStudent);

            string[] _columns = { "Date", "DeciplineName", "Point", "Note", "CreateBy", "UpdateDate", "Status" };

            var PointStudent = await (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() 
                                      join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                      join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                      join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                      join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                      join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                      join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                      join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                      join Point in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals Point.IdHomeroomStudent into JoinedPoint
                                      from Point in JoinedPoint.DefaultIfEmpty()
                                      join LevelInfraction in _dbContext.Entity<MsLevelOfInteraction>() on Point.IdLevelOfInteraction equals LevelInfraction.Id into JoinedLevel
                                      from LevelInfraction in JoinedLevel.DefaultIfEmpty()
                                      join ParentLevelInfraction in _dbContext.Entity<MsLevelOfInteraction>() on LevelInfraction.IdParentLevelOfInteraction equals ParentLevelInfraction.Id into JoinedParentLevelInfraction
                                      from ParentLevelInfraction in JoinedParentLevelInfraction.DefaultIfEmpty()
                                      join Sanction in _dbContext.Entity<MsSanctionMapping>() on Point.IdSanctionMapping equals Sanction.Id into JoinedSanction
                                      from Sanction in JoinedSanction.DefaultIfEmpty()
                                      where HomeroomStudent.Id == param.IdHomeroomStudent && Level.IdAcademicYear == param.IdAcademicYear
                                      select new
                                      {
                                          Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                          Homeroom = Level.Code + " - " + Grade.Code + Classroom.Code,
                                          TotalDemerit = Point.DemeritPoint,
                                          TotalMerit = Point.MeritPoint,
                                          LevelOfInteraction = LevelInfraction==null
                                            ?""
                                            : LevelInfraction.IdParentLevelOfInteraction==null
                                                ?LevelInfraction.NameLevelOfInteraction
                                                :ParentLevelInfraction.NameLevelOfInteraction+LevelInfraction.NameLevelOfInteraction,
                                          Sanction = Sanction.SanctionName
                                      }
                                    ).SingleOrDefaultAsync(CancellationToken);

            if (PointStudent==null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["StudentPoint"], "Id", param.IdHomeroomStudent));


            var IsHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                .Include(e=>e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                .Any(e => e.IdBinusian == param.IdUser && e.IdHomeroom == param.IdHomeroom && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear &&
                e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id == param.IdLevel &&
                e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id == param.IdGrade);

            var query = (from EntryMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                         join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                         join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join user in _dbContext.Entity<MsUser>() on EntryMeritStudent.MeritUserCreate equals user.Id
                         where EntryMeritStudent.IdHomeroomStudent == param.IdHomeroomStudent
                         && ((EntryMeritStudent.RequestType == RequestType.Create && EntryMeritStudent.Status == "Approved") || (EntryMeritStudent.RequestType == RequestType.Delete && EntryMeritStudent.Status != "Decline")) && Level.IdAcademicYear == param.IdAcademicYear
                         select new
                         {
                             Id = EntryMeritStudent.Id,
                             Date = EntryMeritStudent.DateMerit,
                             DeciplineName = MeritDemeritMapping.DisciplineName,
                             Point = EntryMeritStudent.Point.ToString(),
                             Note = EntryMeritStudent.RequestType == RequestType.Create ? EntryMeritStudent.Note : EntryMeritStudent.RequestReason,
                             CreateBy = user.Id + " - " + user.DisplayName,
                             UpdateDate = EntryMeritStudent.DateUp,
                             Status = EntryMeritStudent.RequestType == RequestType.Delete ? EntryMeritStudent.Status : "",
                             IsDisabledDelete = IsHomeroomTeacher
                                                ? EntryMeritStudent.RequestType == RequestType.Delete || EntryMeritStudent.IsDeleted ? true : false
                                                : true,
                             IsDisabledEdit = EntryMeritStudent.MeritUserCreate == param.IdUser && EntryMeritStudent.Status == "Approved" && EntryMeritStudent.Type == EntryMeritStudentType.Merit ? false : true
                         });

            //ordering
            switch (param.OrderBy)
            {
                case "Date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date);
                    break;
                case "DeciplineName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.DeciplineName)
                        : query.OrderBy(x => x.DeciplineName);
                    break;
                case "Point":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Point)
                        : query.OrderBy(x => x.Point);
                    break;
                case "Note":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Note)
                        : query.OrderBy(x => x.Note);
                    break;
                case "CreateBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CreateBy)
                        : query.OrderBy(x => x.CreateBy);
                    break;
                case "UpdateDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UpdateDate)
                        : query.OrderBy(x => x.UpdateDate);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            GetDetailMeritTeacherResult itemsMerit = new GetDetailMeritTeacherResult();
            List<DetailMeritDemeritTeacher> items = new List<DetailMeritDemeritTeacher>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                   .Select(e => new DetailMeritDemeritTeacher
                   {
                       Id = e.Id,
                       Date = e.Date,
                       DeciplineName = e.DeciplineName,
                       Point = e.Point,
                       Note = e.Note,
                       CreateBy = e.CreateBy,
                       UpdateDate = e.UpdateDate,
                       Status = e.Status,
                       IsDisabledDelete = e.IsDisabledDelete,
                   })
                   .ToList();

                itemsMerit.Merit = items;
                itemsMerit.Student = PointStudent.Student;
                itemsMerit.Homeroom = PointStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent.LevelOfInteraction;
                itemsMerit.Sanction = PointStudent.Sanction;
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(e => new DetailMeritDemeritTeacher
                    {
                        Id = e.Id,
                        Date = e.Date,
                        DeciplineName = e.DeciplineName,
                        Point = e.Point,
                        Note = e.Note,
                        CreateBy = e.CreateBy,
                        UpdateDate = e.UpdateDate,
                        Status = e.Status,
                        IsDisabledDelete = e.IsDisabledDelete,
                    })
                    .ToList();

                itemsMerit.Merit = items;
                itemsMerit.Student = PointStudent.Student;
                itemsMerit.Homeroom = PointStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent.LevelOfInteraction;
                itemsMerit.Sanction = PointStudent.Sanction;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsMerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));


        }
    }
}
