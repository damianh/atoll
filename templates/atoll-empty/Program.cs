using Atoll.Middleware.Server.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtoll(options =>
{
    options.Assemblies.Add(typeof(Program).Assembly);
});

var app = builder.Build();

app.UseAtoll();

app.Run();
