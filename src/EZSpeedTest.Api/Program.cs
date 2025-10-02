using EZSpeedTest.Api.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddLoggingAndConfiguration();
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseApi(builder.Environment);

app.Run();

public partial class Program { }
