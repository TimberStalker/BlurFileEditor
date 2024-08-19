using BlurFileEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils
{
    public class PagedEnumerableDataSource<T> : ObservableObject
    {
        T[] cache = Array.Empty<T>();
        public int Length => cache.Length;
        public int TotalPages => cache.Length / Length;
        int pageSize = 20;
        public int PageSize
        {
            get => pageSize;
            set => SetProperty(ref pageSize, value);
        }
        int currentPage;
        public int CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, Math.Clamp(currentPage, 0, TotalPages-1));
        }
        public PagedEnumerableDataSource()
        {
            
        }

        public void SetSource(IEnumerable<T> source) 
        {
            cache = source.ToArray();
        }
    }
}
