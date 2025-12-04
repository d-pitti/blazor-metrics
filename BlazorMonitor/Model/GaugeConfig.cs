namespace BlazorMonitor.Models;
public class GaugeProperty
{
    public string? Name { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public string? hardwareType { get; set; }

    // More properties can be added later
    // public double RangeFrom { get; set; }
    // public double RangeTo { get; set; }
    // public string? RangeColor { get; set; }
}