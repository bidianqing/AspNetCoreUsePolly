using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult((response) =>
    {
        var content =  response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return false;
    })
    .WaitAndRetryAsync(new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    });

builder.Services.AddHttpClient("localhost")
    .AddPolicyHandler(retryPolicy);
    //.AddTransientHttpErrorPolicy(builder =>
    //{
    //    return builder.WaitAndRetryAsync(new[]
    //    {
    //        TimeSpan.FromSeconds(1),
    //        TimeSpan.FromSeconds(5),
    //        TimeSpan.FromSeconds(10),
    //        TimeSpan.FromSeconds(20)
    //    }, async (delegateResult, timespan, retryAttempt, context) =>
    //    {
    //        Console.WriteLine(retryAttempt);
    //        Console.WriteLine(timespan);
    //        string content = await delegateResult.Result.Content.ReadAsStringAsync();
    //        Console.WriteLine(content);
    //    });
    //});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
