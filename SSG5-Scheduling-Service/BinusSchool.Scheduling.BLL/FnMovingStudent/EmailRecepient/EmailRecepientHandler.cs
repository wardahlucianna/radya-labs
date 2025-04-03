using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnMovingStudent.EmailRecepient.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingSubject.EmailRecepient
{
    public class EmailRecepientHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public EmailRecepientHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetEmailRecepientRequest>();
            var predicate = PredicateBuilder.Create<MsEmailRecepient>(x => x.Type == param.Type && x.Role.IdSchool==param.IdSchool);

            var query = _dbContext.Entity<MsEmailRecepient>()
                .Include(x => x.Role).ThenInclude(e=>e.RoleGroup)
                .Include(x => x.TeacherPosition)
                .Include(x => x.Staff)
                .Where(predicate)
                .Select(x => new 
                {
                    id = x.Id,
                    role = new ItemValueVm
                    {
                        Id = x.Role.Id,
                        Description = x.Role.Description
                    },
                    roleGroup = new ItemValueVm
                    {
                        Id = x.Role.RoleGroup.Id,
                        Description = x.Role.RoleGroup.Description
                    },
                    teacherPosition = new ItemValueVm
                    {
                        Id = x.TeacherPosition.Id,
                        Description = x.TeacherPosition.Description
                    },
                    staff = new ItemValueVm
                    {
                        Id = x.Staff.IdBinusian,
                        Description = (string.IsNullOrEmpty(x.Staff.FirstName)?"": x.Staff.FirstName) + (string.IsNullOrEmpty(x.Staff.LastName) ? "" : " "+x.Staff.LastName)
                    },
                    isCC = x.IsCC
                });

            var listRole = await _dbContext.Entity<LtRole>()
                .Where(x => x.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                .Include(e => e.Position)
                .Where(x => x.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            var listRolePosition = await _dbContext.Entity<TrRolePosition>()
               .Include(e => e.Role)
               .Where(x => x.Role.IdSchool == param.IdSchool)
               .ToListAsync(CancellationToken);

            List<EmailRecepients> listTo = new List<EmailRecepients>();
            if (TypeEmailRecepient.MovingStudentEnrollment==param.Type)
            {
                var roleST = listRole
                .Where(x => x.Code == PositionConstant.SubjectTeacher)
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Description
                }).FirstOrDefault();

                var positionST = listTeacherPosition
               .Where(x => x.Code == PositionConstant.SubjectTeacher && x.Position.Code == PositionConstant.SubjectTeacher)
               .Select(x => new ItemValueVm
               {
                   Id = x.Id,
                   Description = x.Description
               }).FirstOrDefault();

                listTo.Add(new EmailRecepients
                {
                    role = roleST,
                    teacherPosition = positionST
                });
            }
            else if (TypeEmailRecepient.MovingStudentHomeroom == param.Type)
            {
                var positionCA = listTeacherPosition
               .Where(x => x.Position.Code == PositionConstant.ClassAdvisor)
               .Select(x => new ItemValueVm
               {
                   Id = x.Id,
                   Description = x.Description
               }).ToList();

                foreach(var item in positionCA)
                {
                    var roleCA = listRolePosition
                        .Where(x => x.IdTeacherPosition == item.Id)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Role.Id,
                            Description = x.Role.Description
                        }).FirstOrDefault();

                    if (roleCA == null)
                        continue;

                    listTo.Add(new EmailRecepients
                    {
                        role = roleCA,
                        teacherPosition = item
                    });
                }
            }
            else
            {
                var _listTo = query
                        .Where(e => !e.isCC)
                        .Select(e => new EmailRecepients
                        {
                            role = e.role,
                            staff = e.staff,
                            teacherPosition = e.teacherPosition,
                            roleGroup = e.roleGroup
                        })
                        .Distinct().ToList();

                listTo.AddRange(_listTo);
            }


            List<GetEmailRecepientResult> result = new List<GetEmailRecepientResult>()
            {
                new GetEmailRecepientResult
                {
                    tos = listTo,
                    ccs = query
                        .Where(e => e.isCC)
                        .Select(e => new EmailRecepients
                        {
                            role = e.role,
                            staff = e.staff,
                            teacherPosition = e.teacherPosition,
                            roleGroup = e.roleGroup
                        })
                        .Distinct().ToList(),
                }
            };

            IReadOnlyList<IItemValueVm> items = result;

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddEmailRecepientRequest, AddEmailRecepientValidator>();

            var listEmail = await _dbContext.Entity<MsEmailRecepient>()
                .Include(x => x.Role)
                .Include(x => x.TeacherPosition)
                .Include(x => x.Staff)
                .Where(x=>x.Type == body.Type && x.Role.IdSchool==body.IdSchool)
                .ToListAsync(CancellationToken);

            listEmail.ForEach(e=>e.IsActive = false);
            _dbContext.Entity<MsEmailRecepient>().UpdateRange(listEmail);

            foreach(var itemTo in body.Tos)
            {
                var AddEmailRecepient = new MsEmailRecepient
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = itemTo.IdRole,
                    IdBinusian = itemTo.IdBinusian,
                    IdTeacherPosition = itemTo.IdTeacherPosition,
                    Type = body.Type,
                    IsCC = false
                };
                _dbContext.Entity<MsEmailRecepient>().Add(AddEmailRecepient);
            }

            foreach (var itemTo in body.Ccs)
            {
                var AddEmailRecepient = new MsEmailRecepient
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = itemTo.IdRole,
                    IdBinusian = itemTo.IdBinusian,
                    IdTeacherPosition = itemTo.IdTeacherPosition,
                    Type = body.Type,
                    IsCC = true
                };
                _dbContext.Entity<MsEmailRecepient>().Add(AddEmailRecepient);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
