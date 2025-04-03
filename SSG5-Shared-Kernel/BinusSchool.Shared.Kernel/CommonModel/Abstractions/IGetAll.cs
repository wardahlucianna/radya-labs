namespace BinusSchool.Common.Model.Abstractions
{
    public interface IGetAll : IPagination
    {
        /// <summary>
        /// Get all items, will override <see cref="Page"/> & <see cref="Size"/> params
        /// </summary>
        bool? GetAll { get; set; }

        /// <summary>
        /// When param getAll is true, return count from items, or
        /// dont execute query when page is 1 & items count less than pagination size
        /// </summary>
        bool CanCountWithoutFetchDb(int itemsCount);
    }
}