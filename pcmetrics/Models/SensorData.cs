using Microsoft.VisualBasic;

namespace PCMetrics.Models;

public class SensorData
{
    public string? Name { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public double MaxValue { get; set; }
    public string? hardwareType { get; set; }

    public SensorData()
    { }

}

