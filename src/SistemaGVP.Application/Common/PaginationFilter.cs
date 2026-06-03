namespace SistemaGVP.Application.Common;

/// <summary>
/// Filtro de paginación para consultas.
/// </summary>
public class PaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SearchTerm { get; set; }

    public PaginationFilter() { }

    public PaginationFilter(int pageNumber, int pageSize, string? searchTerm = null)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize > 100 ? 100 : pageSize < 1 ? 25 : pageSize;
        SearchTerm = searchTerm;
    }
}
