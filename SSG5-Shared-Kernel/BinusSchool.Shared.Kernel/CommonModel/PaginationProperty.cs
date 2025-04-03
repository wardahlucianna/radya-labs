namespace BinusSchool.Common.Model
{
    public class PaginationProperty : Pagination 
    {
        public int TotalPage => TotalItem % Size == 0 
            ? (int)(TotalItem / Size) 
            : ((int)TotalItem / Size) + 1;
        public long TotalItem { get; set; }
    }
}