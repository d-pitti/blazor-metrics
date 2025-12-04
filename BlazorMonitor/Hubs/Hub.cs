using BlazorMonitor.Models;
using Microsoft.AspNetCore.SignalR;

namespace BlazorMonitor.Hubs;
public class HardwareHub : Hub
{
       public async Task SendData(List<GaugeProperty> SensorsDataList)
    {

        await Clients.All.SendAsync("Client", SensorsDataList);
        // Uncomment to see data in console
        // Console.WriteLine($"{SensorsDataList.Count}");
        // foreach(var d in SensorsDataList){
        //         Console.WriteLine($"{d.Name} : {d.Value}");
        //     }
    }
}



