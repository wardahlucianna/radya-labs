using System;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper
{
    public static class BookingVenueAndEquipmentValidationHelper
    {
        public static bool HasPassedScheduleDateTime(DateTime today, DateTime scheduleDate, TimeSpan startTime)
        {
            var scheduleDatetime = scheduleDate.Add(startTime);
            return today > scheduleDatetime;
        }

        public static bool HasPassedRulesCutoff(DateTime today, DateTime scheduleDate, int? maxDayBooking, TimeSpan? maxTimeBooking)
        {
            var cutoffDateTime = scheduleDate
                                     .AddDays(-(double?)maxDayBooking ?? 0)
                                     .Add(maxTimeBooking.HasValue ? maxTimeBooking.Value : new TimeSpan(23, 59, 59));
            return today > cutoffDateTime;
        }

        public static bool IsCreatedByOrForLoggedUser(string idLoggedUser, string createdBy, string createdFor)
        {
            return idLoggedUser == createdBy || idLoggedUser == createdFor;
        }

        public static bool HasCanOverrideAndAllSuperAccess(bool canOverride, bool allSuperAccess)
        {
            return (canOverride && allSuperAccess);
        }

        public static bool IsEditApprovalValid(int status, bool isOverlapping)
        {
            if (status == (int)VenueApprovalStatus.WaitingForApproval) return false;
            if (status == (int)VenueApprovalStatus.Approved) return false;
            if (status == (int)VenueApprovalStatus.Rejected) return false;
            if (status == (int)VenueApprovalStatus.Canceled) return false;
            if (isOverlapping) return false;

            // No Need Approval
            return true;
        }

        public static bool IsDeleteApprovalValid(int status, bool isOverlapping)
        {
            if (status == (int)VenueApprovalStatus.Rejected) return false;
            if (status == (int)VenueApprovalStatus.Canceled) return false;
            if (isOverlapping) return false;

            // No Need Approval, Waiting Approval, Approved
            return true;
        }

        public static bool IsAdminApprovalValid(int status)
        {
            if (status == (int)VenueApprovalStatus.WaitingForApproval) return false;
            if (status == (int)VenueApprovalStatus.Rejected) return false;
            if (status == (int)VenueApprovalStatus.Canceled) return false;

            // No Need Approval, Approved, IsOverlapping (doesn't matter true/false)
            return true;
        }

        public static bool ValidateEdit(BookingVenueAndEquipmentValidationParams parameter)
        {
            if (HasPassedScheduleDateTime(parameter.Today, parameter.ScheduleDate, parameter.StartTime))
                return false;

            if (!parameter.AllSuperAccess && HasPassedRulesCutoff(parameter.Today, parameter.ScheduleDate, parameter.MaxDayBooking, parameter.MaxTimeBooking))
                return false;

            if (!parameter.CanOverride && !IsCreatedByOrForLoggedUser(parameter.IdLoggedUser, parameter.CreatedBy, parameter.CreatedFor))
                return false;

            if (HasCanOverrideAndAllSuperAccess(parameter.CanOverride, parameter.AllSuperAccess))
                return IsAdminApprovalValid(parameter.ApprovalStatus);

            if (!parameter.CheckApprovalStatus)
                return true;

            return IsEditApprovalValid(parameter.ApprovalStatus, parameter.IsOverlapping);
        }

        public static bool ValidateDelete(BookingVenueAndEquipmentValidationParams parameter)
        {
            if (HasPassedScheduleDateTime(parameter.Today, parameter.ScheduleDate, parameter.StartTime))
                return false;

            if (!parameter.AllSuperAccess && HasPassedRulesCutoff(parameter.Today, parameter.ScheduleDate, parameter.MaxDayBooking, parameter.MaxTimeBooking))
                return false;

            if (!parameter.CanOverride && !IsCreatedByOrForLoggedUser(parameter.IdLoggedUser, parameter.CreatedBy, parameter.CreatedFor))
                return false;

            if (HasCanOverrideAndAllSuperAccess(parameter.CanOverride, parameter.AllSuperAccess))
                return IsAdminApprovalValid(parameter.ApprovalStatus);

            if (!parameter.CheckApprovalStatus)
                return true;

            return IsDeleteApprovalValid(parameter.ApprovalStatus, parameter.IsOverlapping);
        }
    }
}
