using CrawData.Data;
using CrawData.CronJob;
using CrawData.Service;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

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
        path: Path.Combine("Logs", "log_.txt"), // Specify the directory and log file name
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day, //Log will create new files every day
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

//add dbcontext
builder.Services.AddDbContext<DataContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
      throw new InvalidOperationException("Connection String is not found"));
});

//add hosts
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
    options.ExcludedHosts.Add("example.com");
    options.ExcludedHosts.Add("www.example.com");
});


builder.Services.AddHttpClient();
builder.Services.AddScoped<SCrawl>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
