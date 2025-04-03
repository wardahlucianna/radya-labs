using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.AttendanceDb.Abstractions
{
    public interface IAttendanceDbContext : IAppDbContext<IAttendanceEntity>
    {
        void AttachEntity(object entity);
        
        void DetachChanges();
    }
}
