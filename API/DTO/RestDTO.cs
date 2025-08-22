using System;

namespace FormBuilderAPI.DTO;

public class RestDTO<T>
{
    public T Data { get; set; } = default!;
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public int RecordCount { get; set; } = 0;
    public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();
}
