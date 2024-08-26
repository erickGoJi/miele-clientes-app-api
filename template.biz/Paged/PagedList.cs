using System.Collections.Generic;

namespace template.biz.Paged
{
    public class PagedList<T>
    {
        public int TotalRows { get; set; }
        public IList<T> Paged { get; set; }

        public PagedList()
        {
            TotalRows = 0;
            Paged = new List<T>();
        }

        public PagedList(int totalRows, IList<T> result)
        {
            TotalRows = totalRows;
            Paged = result;
        }
    }
}
