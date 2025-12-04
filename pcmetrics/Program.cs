using Microsoft.AspNetCore.SignalR;
using WorkerService;
using Microsoft.AspNetCore.ResponseCompression;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSignalR();
builder.Services.AddRouting();
builder.Services.AddSingleton<IHostedService, Worker>();

builder.Services.AddResponseCompression(opts =>
{
   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       [ "application/octet-stream" ]);
});

var app = builder.Build();

app.UseRouting();


app.Run("http://**USE LOCAL IP**:5000");