using System.Collections.Generic;

namespace LogManager.BLL.Utilities
{
    public class PaginatedList<T> : List<T>
    {
        public PaginatedList(
            IEnumerable<T> items, 
            int page, 
            int pageSize, 
            int pageTotalCount,
            int minPage,
            int maxPage)
        {
            
            this.Page = page;
            this.PageSize = pageSize;
            this.PageTotalCount = pageTotalCount;
            this.MinPage = minPage;
            this.MaxPage = maxPage;
            this.AddRange(items);
        }

        public int Page { get; }

        public int PageSize { get; }

        public int MinPage { get; }
        
        public int MaxPage { get; }

        public int PageTotalCount { get; }

        public string SortField { get; set; }

        public bool IsDescending { get; set; }

        public string SearchText { get; set; }

        public bool HasPreviousPage => this.Page > 1;

        public bool HasNextPage => this.Page < this.PageTotalCount;
    }
}
