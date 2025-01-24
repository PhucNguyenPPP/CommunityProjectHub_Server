using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Paging
{
    public class PagedList<T> : List<T>
    {
        public int? CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int? PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public PagedList(IQueryable<T> items, int count, int? pageNumber, int? pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)(pageSize ?? 1));  // Avoid division by 0 if pageSize is null

            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int? pageNumber, int? pageSize)
        {
            // Assign default values if null
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;

            var count = source.Count();
            var items = source.Skip((currentPage - 1) * currentPageSize).Take(currentPageSize);

            return new PagedList<T>(items, count, currentPage, currentPageSize);
        }
    }
}
