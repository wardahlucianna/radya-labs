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
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher 
{
    public class GetDetailEntryDemeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailEntryDemeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailMeritTeacherRequest>();
            var predicate = PredicateBuilder.Create<TrEntryDemeritStudent>(x => x.IdHomeroomStudent == param.IdHomeroomStudent);

            string[] _columns = { "Date", "DeciplineName", "Point", "Note", "CreateBy", "UpdateDate", "Status" };

            var IsHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
               .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
               .Any(e => e.IdBinusian == param.IdUser && e.IdHomeroom == param.IdHomeroom && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
               e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id == param.IdLevel &&
               e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id == param.IdGrade);


            var MeritDemeritComponentSetting = await _dbContext.Entity<MsMeritDemeritComponentSetting>()
                                                    .Where(e => e.IdGrade == param.IdGrade)
                                                    .SingleOrDefaultAsync(CancellationToken);

            var query = (from EntryDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                         join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                         join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                         join user in _dbContext.Entity<MsUser>() on EntryDemeritStudent.DemeritUserCreate equals user.Id
                         where EntryDemeritStudent.IdHomeroomStudent == param.IdHomeroomStudent
                         && ((EntryDemeritStudent.RequestType == RequestType.Create && EntryDemeritStudent.Status == "Approved") || (EntryDemeritStudent.RequestType == RequestType.Delete && EntryDemeritStudent.Status != "Decline"))
                         select new
                         {
                             Id = EntryDemeritStudent.Id,
                             Date = EntryDemeritStudent.DateDemerit,
                             DeciplineName = MeritDemeritMapping.DisciplineName,
                             Point = EntryDemeritStudent.Point.ToString(),
                             Note = EntryDemeritStudent.RequestType == RequestType.Create ? EntryDemeritStudent.Note : EntryDemeritStudent.RequestReason,
                             CreateBy = user.Id + " - " + user.DisplayName,
                             UpdateDate = EntryDemeritStudent.DateUp,
                             Status = EntryDemeritStudent.RequestType == RequestType.Delete ? EntryDemeritStudent.Status : "",
                             LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                             LevelOfInfractionSort = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction + "-",
                             IsDisabledDelete = IsHomeroomTeacher
                                                ? EntryDemeritStudent.RequestType == RequestType.Delete || EntryDemeritStudent.IsDeleted ? true : false
                                                : true,
                             IsDisabledEdit = EntryDemeritStudent.DemeritUserCreate == param.IdUser ? false : true,
                         }).OrderBy(e => e.LevelOfInfractionSort);

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

            GetDetailDemeritTeacherResult itemsDemerit = new GetDetailDemeritTeacherResult();
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
                      LevelOfInfraction = e.LevelOfInfraction,
                  })
                  .ToList();

                itemsDemerit.Demerit = items;
                itemsDemerit.IsShowDemerit = MeritDemeritComponentSetting == null ? true : MeritDemeritComponentSetting.IsUseDemeritSystem;
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
                       LevelOfInfraction = e.LevelOfInfraction,
                   })
                   .ToList();

                itemsDemerit.Demerit = items;
                itemsDemerit.IsShowDemerit = MeritDemeritComponentSetting == null ? true : MeritDemeritComponentSetting.IsUseDemeritSystem;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsDemerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
