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
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class CourseworkHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public CourseworkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        private string GetTeacherName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.FirstName + " " + dataTeacher.LastName;
        }

        private string GetSeenByName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsUser>().Where(x => x.Id == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.DisplayName;
        }

        private string GetDisplaName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsUser>().Where(x => x.Id == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _dbContext.Entity<TrCourseworkAnecdotalStudent>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var data = await _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Include(x => x.AcademicYear)
                .Include(x => x.Attachments)
                .Select(x => new GetDetailCourseworkResult
                {
                    Id = x.Id,
                    UOI = new CodeWithIdVm
                    {
                        Id = x.IdUIO,
                        Code = x.IdUIO,
                        Description = x.UOI.Name
                    },
                    Content = x.Content,
                    NotifyParentStudent = x.NotifyParentStudent,
                    Type = x.Type,
                    Attachments = x.Attachments != null ? x.Attachments.Select(y => new CourseworkAttachment
                    {
                        Id = y.Id,
                        IdCourseworkAnecdotalStudent = y.IdCourseworkAnecdotalStudent,
                        Url = y.Url,
                        Filename = y.FileName,
                        Filetype = y.FileType,
                        Filesize = y.FileSize,
                        OriginalFilename = y.OriginalName
                    }).ToList() : null,
                    AcademicYear = new ItemValueVm(x.IdAcademicYear,x.AcademicYear.Description),
                    Semester = x.Semester
                })
                .SingleOrDefaultAsync(x => x.Id == id, CancellationToken);

            var items = new GetDetailCourseworkResult
            {
                Id = data.Id,
                UOI = new CodeWithIdVm
                {
                    Id = data.UOI.Id,
                    Code = data.UOI.Code,
                    Description = data.UOI.Description
                },
                Content = data.Content,
                NotifyParentStudent = data.NotifyParentStudent,
                Attachments = data.Attachments
            };

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetListCourseworkRequest>();

            var predicate = PredicateBuilder.Create<TrCourseworkAnecdotalStudent>(x => true);
            
            if(param.IdAcademicYear != null)
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if(param.Semester != null)
                predicate = predicate.And(x => x.Semester == param.Semester);

            if(param.Type != null)
                predicate = predicate.And(x => x.Type == param.Type);

            if (!string.IsNullOrWhiteSpace(param.IdStudent))
                predicate = predicate.And(x => x.IdStudent == param.IdStudent);
            
            var query = _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Include(x => x.Attachments)
                .Include(x => x.Comments)
                .Include(x => x.Seens)
                .Include(x => x.UOI)
                .Where(predicate)
                .OrderByDescending(x => x.DateIn);

            List<CourseworkAttachment> listAttachments = new List<CourseworkAttachment>();

            List<GetListCourseworkResult> result = new List<GetListCourseworkResult>();

            List<CourseworkSeenBy> listSeenBy = new List<CourseworkSeenBy>();

            result = await query.Select(x => new GetListCourseworkResult
            {
                Id = x.Id,
                TeacherName = GetTeacherName(x.UserIn),
                BinusianID = x.UserIn,
                Date = x.DateUp == null ? x.DateIn : x.DateUp,
                StatusUpdated = x.DateUp == null ? false : true,
                UOIName = x.IdUIO,
                Content = x.Content,
                Attachments = x.Attachments != null ? x.Attachments.Select(y => new CourseworkAttachment{
                    Id = y.Id,
                    IdCourseworkAnecdotalStudent = y.IdCourseworkAnecdotalStudent,
                    Url = y.Url,
                    Filename = y.FileName,
                    Filetype = y.FileType,
                    Filesize = y.FileSize,
                    OriginalFilename = y.OriginalName
                }).ToList() : null,
                SeenBy = x.Seens != null ? x.Seens.Select(y => new CourseworkSeenBy{
                    Id = y.Id,
                    Fullname = y.Id
                }).ToList(): null,
                Comments = x.Comments != null ? x.Comments.Select(y => new CourseworkComments{
                    BinusianID = y.IdUserComment,
                    Fullname = y.Id,
                    Date = y.DateUp == null ? y.DateIn : y.DateUp,
                    Comment = y.Comment,
                    CanEdit = y.IdUserComment == param.IdUser,
                    CanDelete = y.IdUserComment == param.IdUser
                }).ToList() : null,
                CanEdit = x.UserIn == param.IdUser,
                CanDelete = x.UserIn == param.IdUser

            }).ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = result.Select(x => new GetListCourseworkResult
                {
                    Id = x.Id,
                    TeacherName = x.TeacherName,
                    BinusianID = x.BinusianID,
                    Date = x.Date,
                    StatusUpdated = x.StatusUpdated,
                    UOIName = x.UOIName,
                    Content = x.Content,
                    Attachments = listAttachments,
                    SeenBy = listSeenBy,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }
            else
            {
                items = result
                .SetPagination(param)
                .Select(x => new GetListCourseworkResult
                {
                    Id = x.Id,
                    TeacherName = x.TeacherName,
                    BinusianID = x.BinusianID,
                    Date = x.Date,
                    StatusUpdated = x.StatusUpdated,
                    UOIName = x.UOIName,
                    Content = x.Content,
                    Attachments = listAttachments,
                    SeenBy = listSeenBy,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddCourseworkRequest, AddCourseworkValidator>();

            if(body.Type == 0)
            {
                if(body.IdUOI == null)
                    throw new BadRequestException("Id UOI cannot empty");
            }

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var IdCoursework = Guid.NewGuid().ToString();

            var CourseworkAnecdotal = new TrCourseworkAnecdotalStudent
            {
                Id = IdCoursework,
                IdStudent = body.IdStudent,
                IdUIO = body.IdUOI,
                Content = body.Content,
                NotifyParentStudent = body.NotifyParentStudent,
                Type = body.Type,
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester
            };

            _dbContext.Entity<TrCourseworkAnecdotalStudent>().Add(CourseworkAnecdotal);

            if (body.Attachments != null)
            {
                if(body.Attachments.Count > 0)
                {
                    await _dbContext.Entity<TrCourseworkAnecdotalAttachment>().AddRangeAsync(body.Attachments.Select(x => new TrCourseworkAnecdotalAttachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdCourseworkAnecdotalStudent = IdCoursework,
                                Url = x.Url,
                                FileName = x.Filename,
                                FileType = x.Filetype,
                                FileSize = x.Filesize,
                                OriginalName = x.OriginalFilename
                            }), CancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            await ProcessEmailAndNotif(body, IdCoursework);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateCourseworkRequest, UpdateCourseworkValidator>();

            if(body.Type == 0)
            {
                if(body.IdUOI == null)
                    throw new BadRequestException("Id UOI cannot empty");
            }

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id"], "Id", body.Id));

            data.IdUIO = body.IdUOI;
            data.Content = body.Content;
            data.NotifyParentStudent = body.NotifyParentStudent;
            data.IdAcademicYear = body.IdAcademicYear;
            data.Semester = body.Semester;

            _dbContext.Entity<TrCourseworkAnecdotalStudent>().Update(data);

            var dataAttachment = await _dbContext.Entity<TrCourseworkAnecdotalAttachment>()
                .Where(x => x.IdCourseworkAnecdotalStudent == body.Id)
                .ToListAsync(CancellationToken);

            foreach (var da in dataAttachment)
                    {
                        da.IsActive = false;

                        _dbContext.Entity<TrCourseworkAnecdotalAttachment>().Update(da);
                    }


            if (body.Attachments != null)
            {
                if(body.Attachments.Count > 0)
                {
                    await _dbContext.Entity<TrCourseworkAnecdotalAttachment>().AddRangeAsync(body.Attachments.Select(x => new TrCourseworkAnecdotalAttachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdCourseworkAnecdotalStudent = body.Id,
                                Url = x.Url,
                                FileName = x.Filename,
                                FileType = x.Filetype,
                                FileSize = x.Filesize,
                                OriginalName = x.OriginalFilename
                            }), CancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task ProcessEmailAndNotif(AddCourseworkRequest param, string IdCoursework)
        {
            var GetParent = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                            .Where(x => x.IdStudent == param.IdStudent)
                            .Select(y => new DataParentCourseworkAnecdotal
                            {
                                EmailParent = y.Parent != null ? y.Parent.PersonalEmailAddress : "",
                                ParentName = y.Parent != null ? !string.IsNullOrEmpty(y.Parent.FirstName) ? y.Parent.FirstName : y.Parent.LastName : "",
                            })
                            .ToListAsync(CancellationToken);

            var TeacherName = await _dbContext.Entity<MsUser>()
                        .Where(x => x.Id == param.IdUser)
                        .Select(x => x.DisplayName
                        )
                        .FirstOrDefaultAsync(CancellationToken);

           
            var data = await _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                    .Include(x => x.AcademicYear)
                    .Include(x => x.Attachments)
                    .Include(x => x.Student).ThenInclude(x=> x.School)
                    .Select(x => new EmailCourseworkResult
                    {
                        Id = x.Id,
                        UOI = new CodeWithIdVm
                        {
                            Id = x.IdUIO,
                            Code = x.IdUIO,
                            Description = x.UOI.Name
                        },
                        Content = x.Content,
                        IdUser = param.IdUser,
                        TeacherName = TeacherName,
                        DataParents = GetParent,
                        IdStudent = param.IdStudent,
                        SchoolName = x.Student.School.Name,
                        IdAcademicYear = x.IdAcademicYear,
                        Semester = x.Semester,
                        Date = x.DateIn.Value,
                        NotifyParentStudent = x.NotifyParentStudent,
                        Type = x.Type,
                        ForStudent = true                        
                    })
                    .SingleOrDefaultAsync(x => x.Id == IdCoursework, CancellationToken);

            if (data.NotifyParentStudent)
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                if (KeyValues.ContainsKey("GetDataCourseworkAnecdotalEmail"))
                {
                    KeyValues.Remove("GetDataCourseworkAnecdotalEmail");
                }

                paramTemplateNotification.Add("GetDataCourseworkAnecdotalEmail", data);

                IEnumerable<string> IdRecipients = new string[] { param.IdStudent };

                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var scenario = string.Empty;
                    if (param.Type == 0)
                    {
                        scenario = "PAR1";
                    }
                    else
                    {
                        scenario = "PAR2";
                    }
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, scenario)
                    {
                        IdRecipients = IdRecipients,
                        KeyValues = paramTemplateNotification
                    });
                    collector.Add(message);


                    paramTemplateNotification = new Dictionary<string, object>();
                    data.ForStudent = false;
                    paramTemplateNotification.Add("GetDataCourseworkAnecdotalEmail", data);

                    var message2 = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, scenario)
                    {
                        IdRecipients = IdRecipients,
                        KeyValues = paramTemplateNotification
                    });
                    collector.Add(message2);
                }
            }
        }
    }
}
