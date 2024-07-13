using CrawData.Data;
using CrawData.CronJob;
using CrawData.Service;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using CrawData.Responses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure Serilog

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine("Logs", $"log_{DateTime.Now:ddMMyyyy}.txt"), // Log file with current day's date
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day, // Log will create new files every day
        rollOnFileSizeLimit: true)
    .CreateLogger();
// Add Quartz services
builder.Services.AddQuartz(q =>
{
    // Use a Scoped Job Factory to inject dependencies into your job
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Create a key for the job
    var jobKey = new JobKey("CrawlDataJob");
    q.AddJob<CrawlDataJob>(opts => opts.WithIdentity(jobKey));

    // Create a trigger for the job
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("CrawlDataJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(30)
            .RepeatForever())
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddDbContext<DataContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
      throw new InvalidOperationException("Connection String is not found"));
});


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpClient();
builder.Services.AddScoped<SCrawl>();
builder.Services.AddScoped<ResponseListPaper>();
builder.Services.AddScoped<ResponsePaper>();
builder.Services.AddScoped<ResponseListPaperWithType>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
