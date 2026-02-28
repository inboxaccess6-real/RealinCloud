using Microsoft.AspNetCore.Components;

namespace RealEstate.Admin.Components.Data;

public class DataColumn<TItem>
{
    public string Header { get; set; } = string.Empty;
    public Func<TItem, object?> ValueSelector { get; set; } = _ => null;
    public string? Width { get; set; }
    public string? CssClass { get; set; }
    public RenderFragment<TItem>? Template { get; set; }
}
