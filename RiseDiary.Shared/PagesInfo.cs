namespace RiseDiary.Shared;

public sealed class PagesInfo
{
    public static PagesInfo GetPagesInfo(int totalItems, int currentPage = 1, int pageSize = 10, int maxPages = 10)
    {
        if (pageSize <= 0) pageSize = 10;

        var totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);

        if (currentPage < 1)
        {
            currentPage = 1;
        }
        else if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        int startPage, endPage;
        if (totalPages <= maxPages)
        {
            startPage = 1;
            endPage = totalPages;
        }
        else
        {
            var maxPagesBeforeCurrentPage = (int)Math.Floor((decimal)maxPages / 2);
            var maxPagesAfterCurrentPage = (int)Math.Ceiling((decimal)maxPages / 2) - 1;
            if (currentPage <= maxPagesBeforeCurrentPage)
            {
                startPage = 1;
                endPage = maxPages;
            }
            else if (currentPage + maxPagesAfterCurrentPage >= totalPages)
            {
                startPage = totalPages - maxPages + 1;
                endPage = totalPages;
            }
            else
            {
                startPage = currentPage - maxPagesBeforeCurrentPage;
                endPage = currentPage + maxPagesAfterCurrentPage;
            }
        }

        var startIndex = (currentPage - 1) * pageSize;
        var endIndex = Math.Min(startIndex + pageSize - 1, totalItems - 1);

        var pages = Enumerable.Range(startPage, (endPage + 1) - startPage);

        return new PagesInfo
        {
            TotalItems = totalItems,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages,
            StartPage = startPage,
            EndPage = endPage,
            StartIndex = startIndex,
            EndIndex = endIndex,
            Pages = pages
        };
    }

    public int TotalItems { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public int StartPage { get; set; }

    public int EndPage { get; set; }

    public int StartIndex { get; set; }

    public int EndIndex { get; set; }

    public IEnumerable<int> Pages { get; set; } = Enumerable.Empty<int>();
}
