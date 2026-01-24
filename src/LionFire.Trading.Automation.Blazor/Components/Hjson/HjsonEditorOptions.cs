namespace LionFire.Trading.Automation.Blazor.Components.Hjson;

/// <summary>
/// Options for the HJSON editor component.
/// </summary>
public class HjsonEditorOptions
{
    /// <summary>
    /// Height of the editor (CSS value, e.g., "400px", "100%").
    /// </summary>
    public string Height { get; set; } = "400px";

    /// <summary>
    /// Whether the editor is read-only.
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Placeholder text when empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Number of rows for the textarea (if using simple mode).
    /// </summary>
    public int Rows { get; set; } = 20;
}
