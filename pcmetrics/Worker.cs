using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.SignalR.Client;
using PCMetrics.Models;

namespace WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Computer? _computer;
    public List<SensorData> SensorsDataList = new List<SensorData>();
    private HubConnection? _hubConnection;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };
        _computer.Open();
        _computer.Accept(new UpdateVisitor());

        foreach (IHardware hardware in _computer.Hardware)
        {
            Console.WriteLine("Hardware: {0}", hardware.Name);

            foreach (ISensor sensor in hardware.Sensors)
            {

                Console.WriteLine("\tSensor Found: {0}, value: {1}", sensor.Name, sensor.Value);

            }
        }

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://**VM IP ADDRESS**:80/hardwarehub")
                .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
                .WithAutomaticReconnect()
                .Build();

        _logger.LogInformation("Worker Service started. Waiting for Blazor App...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Blazor App is up and running! Proceeding with worker tasks.");
                // Proceed with the main logic of your worker service
                break; // Exit the waiting loop
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // The Blazor app's SignalR Hub is not yet available
                 _logger.LogWarning("Blazor app's SignalR Hub not found. Retrying in 2 seconds...");
                 await Task.Delay(2000, stoppingToken); // Wait for 5 seconds before retrying
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blazor App not ready yet. Retrying in 2 seconds...");
                await Task.Delay(2000, stoppingToken); // Wait for 5 seconds before retrying
            }

        }

        while (!stoppingToken.IsCancellationRequested)
        {

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // Update the hardware data
            if (_computer == null)
            {
                _logger.LogWarning("Computer instance is not initialized.");
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            _computer.Accept(new UpdateVisitor());

            SensorsDataList.Clear();
            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (ISensor sensor in hardware.Sensors)
                {

                    if (sensor.SensorType == SensorType.Data || sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core Max")
                    || sensor.Name.Contains("Core Average") || sensor.Name.Contains("GPU Core") || sensor.Name.Contains("Memory Used")
                    || sensor.Name.Contains("Memory Total") || sensor.Name.Contains("Memory Free") || sensor.Name.Contains("Memory Available"))
                    {

                        int oldValue = (int)(sensor.Value ?? 0);
                        int value = ConvertFormat(sensor.SensorType, oldValue);
                        var unitString = GetUnitString(sensor.SensorType, oldValue);
                        SensorsDataList.Add(new SensorData
                        {
                            Name = sensor.Name,
                            Value = value,
                            Unit = unitString,
                            MaxValue = (int)sensor.Max!.Value,
                            hardwareType = hardware.Name
                        }
                        );

                    }



                }
            }
            // Uncomment to see data and add to output to see in worker logs
            // List<string> sensorsList = SensorsDataList
            //     .Select(s => $"{s.Name}: {s.Value}")
            //     .ToList();

            foreach (var d in SensorsDataList)
            {
                _logger.LogInformation($"{d.Name} : {d.Value}{d.Unit}");
                _logger.LogInformation($"{d.MaxValue}");
                Console.WriteLine();
            }
            var data = SensorsDataList;

            await _hubConnection.InvokeAsync("SendData", data);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);


        }

        if (_computer != null)
        {
            _computer.Close();
        }

    }

    private static string GetUnitString(SensorType sensorType, int value)
    {
        switch (sensorType)
        {
            case SensorType.Temperature:
                return " °C";
            case SensorType.SmallData:
                if (value > 1024)
                {
                    return " GB";
                }
                else
                {
                    return " MB";
                }
            case SensorType.Voltage:
                return " V";
            case SensorType.Fan:
                return " RPM";
            case SensorType.Control:
                return " %"; // Fan control percentage
            case SensorType.Load:
                return " %";
            case SensorType.Level: // Drive wear level, etc
                return " %";
            case SensorType.Power:
                return " W";
            case SensorType.Clock:
                return " MHz";
            case SensorType.Data:
                return " GB"; // Depends on context, e.g., memory used
            case SensorType.Flow:
                return " L/h";
            case SensorType.Factor:
                return ":1";
            case SensorType.Frequency:
                return " Hz";
            case SensorType.Throughput:
                return " MB/s"; // Depends on context, e.g., disk read/write
            default:
                return "";
        }
    }

    public static int ConvertFormat(SensorType type, int sensorValue)
    {
        // Converter MB to GB for SmallData sensors if value exceeds 1024 MB
        if (type == SensorType.SmallData && sensorValue > 1024)
        {

            return sensorValue / 1024;
        }
        else
        {
            return sensorValue;
        }

    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}

public class UpdateVisitor : IVisitor
{
    // IVisitor implementation to update hardware
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware)
            subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}


