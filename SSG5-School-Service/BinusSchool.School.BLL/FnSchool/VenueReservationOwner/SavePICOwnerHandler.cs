using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.VenueReservationOwner.Validator;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.VenueReservationOwner
{ 
    public class SavePICOwnerHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SavePICOwnerHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SavePicOwnerRequest, SavePICValidator>();

            try
            {
                var validData = 0;
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var email in param.PICEmail)
                {
                    string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                    bool validEmail = Regex.IsMatch(email.OwnerEmail, pattern);

                    if (validEmail == false)
                    {
                        throw new Exception("Invalid Email");
                    }
                }

                if (string.IsNullOrEmpty(param.IdReservationOwner)) //Insert new data
                {
                    //Insert Owner Data

                    var checkData = await _dbContext.Entity<MsReservationOwner>()
                        .Where(x => x.OwnerName == param.OwnerName &&
                                    x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (checkData != null)
                    {
                        throw new Exception("PIC data already exists");
                    }

                    var data = new MsReservationOwner
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        OwnerName = param.OwnerName,
                        IsPICVenue = param.IsPICVenue,
                        IsPICEquipment = param.IsPICEquipment
                    };

                    _dbContext.Entity<MsReservationOwner>().Add(data);

                    //Insert Owner Email

                    foreach (var item in param.PICEmail)
                    {
                        var existEmail = await _dbContext.Entity<MsReservationOwnerEmail>()
                            .Where(x => x.OwnerEmail == item.OwnerEmail &&
                                        x.IdReservationOwner == param.IdReservationOwner)
                            .FirstOrDefaultAsync(CancellationToken);

                        if (existEmail != null)
                        {
                            throw new Exception("Email is already exist");
                        }

                        var Email = new MsReservationOwnerEmail
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdReservationOwner = data.Id,
                            OwnerEmail = item.OwnerEmail,
                            IsOwnerEmailTo = item.IsOwnerEmailTo,
                            IsOwnerEmailCC = item.IsOwnerEmailCC,
                            IsOwnerEmailBCC = item.IsOwnerEmailBCC
                        };

                        if (item.IsOwnerEmailTo != false)
                        {
                            validData += 1;
                        }
                        _dbContext.Entity<MsReservationOwnerEmail>().Add(Email);
                    }

                    if (validData == 0)
                    {
                        throw new Exception("Need 'To' Email");
                    }

                }
                else //update data
                {
                    var dataOwner = await _dbContext.Entity<MsReservationOwner>()
                        .Where(x => x.Id == param.IdReservationOwner)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (dataOwner == null)
                    {
                        throw new Exception("Data not found");
                    }

                    var dupName = await _dbContext.Entity<MsReservationOwner>()
                        .Where(x => x.OwnerName == param.OwnerName && 
                                    x.Id != param.IdReservationOwner)
                        .AnyAsync(CancellationToken);

                    if (dupName)
                    {
                        throw new Exception("Name is already exist");
                    }

                    //Insert data to History

                    var historyData = new HMsReservationOwner
                    {
                        IdHMsReservationOwner = Guid.NewGuid().ToString(),
                        IdReservationOwner = dataOwner.Id,
                        OwnerName = dataOwner.OwnerName,
                        IsPICVenue = dataOwner.IsPICVenue,
                        IsPICEquipment = dataOwner.IsPICEquipment,
                        IdSchool = dataOwner.IdSchool
                    };

                    //Insert Email to History

                    var historyEmails = new List<HMsReservationOwnerEmail>();

                    var emailOwner = await _dbContext.Entity<MsReservationOwnerEmail>()
                        .Where(x => x.IdReservationOwner == param.IdReservationOwner)
                        .ToListAsync(CancellationToken);    

                    foreach (var item in emailOwner)
                    {
                        var historyEmail = new HMsReservationOwnerEmail
                        {
                            IdHMsReservationOwnerEmail = Guid.NewGuid().ToString(),
                            IdReservationOwnerEmail = item.Id,
                            IdReservationOwner = item.IdReservationOwner,
                            OwnerEmail = item.OwnerEmail,
                            IsOwnerEmailTo = item.IsOwnerEmailTo,
                            IsOwnerEmailBCC = item.IsOwnerEmailBCC,
                            IsOwnerEmailCC = item.IsOwnerEmailCC
                        };
                        historyEmails.Add(historyEmail);
                    }

                    _dbContext.Entity<HMsReservationOwner>().Add(historyData);
                    _dbContext.Entity<HMsReservationOwnerEmail>().AddRange(historyEmails);

                    //Update Owner Data

                    dataOwner.OwnerName = param.OwnerName;
                    dataOwner.IsPICVenue = param.IsPICVenue;
                    dataOwner.IsPICEquipment = param.IsPICEquipment;

                    _dbContext.Entity<MsReservationOwner>().Update(dataOwner);

                    //Update Owner Email

                    var existingEmails = await _dbContext.Entity<MsReservationOwnerEmail>()
                        .Where(x => x.IdReservationOwner == dataOwner.Id)
                        .ToListAsync(CancellationToken);

                    existingEmails.ForEach(x => x.IsActive = false);    

                    foreach (var item in param.PICEmail)
                    {
                        var Email = new MsReservationOwnerEmail
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdReservationOwner = param.IdReservationOwner,
                            OwnerEmail = item.OwnerEmail,
                            IsOwnerEmailTo = item.IsOwnerEmailTo,
                            IsOwnerEmailCC = item.IsOwnerEmailCC,
                            IsOwnerEmailBCC = item.IsOwnerEmailBCC
                        };
                        _dbContext.Entity<MsReservationOwnerEmail>().Add(Email);
                    }

                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }
    }
}
