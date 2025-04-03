using BinusSchool.Common.Model;
using BinusSchool.Persistence.Abstractions;

namespace BinusSchool.Persistence.Extensions
{
    public static class EntityExtension
    {
        public static AuditableResult GetRawAuditResult2<T>(this T entity)
            where T : IAuditable
        {
            return new AuditableResult
            {
                DateIn = entity.DateIn,
                DateUp = entity.DateUp,
                UserIn = entity.UserIn is null ? null : new AuditableUser(entity.UserIn),
                UserUp = entity.UserUp is null ? null : new AuditableUser(entity.UserUp)
            };
        }
    }
}
