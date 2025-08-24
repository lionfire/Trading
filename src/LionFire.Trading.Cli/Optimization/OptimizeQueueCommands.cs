using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Oakton;
using Spectre.Console;
using System.Text.Json;
using Humanizer;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire;
using LionFire.Trading.Automation;
using LionFire.Trading;

#region Input Classes

public class OptimizeQueueAddInput : CommonTradingInput
{
    [Description("Priority for the job (lower number = higher priority, default: 5)")]
    public int Priority { get; set; } = 5;

    [Description("JSON file containing optimization parameters")]
    public string? ConfigFile { get; set; }

    [Description("Bot type to optimize")]
    public string? BotType { get; set; }

    [Description("Output job ID as JSON")]
    public bool JsonFlag { get; set; }
}

public class OptimizeQueueListInput
{
    [Description("Filter by status (queued, running, completed, failed, cancelled)")]
    public string? Status { get; set; }

    [Description("Maximum number of jobs to show")]
    public int Limit { get; set; } = 50;

    [Description("Output as JSON")]
    public bool JsonFlag { get; set; }

    [Description("Show only queue summary")]
    public bool SummaryFlag { get; set; }
}

public class OptimizeQueueCancelInput
{
    [Description("Job ID to cancel")]
    public string JobId { get; set; } = string.Empty;

    [Description("Output result as JSON")]
    public bool JsonFlag { get; set; }
}

public class OptimizeQueueStatusInput
{
    [Description("Job ID to check status")]
    public string JobId { get; set; } = string.Empty;

    [Description("Output as JSON")]
    public bool JsonFlag { get; set; }
}

#endregion

#region Command Classes

[Area("optimize")]
[Description("Add optimization job to queue", Name = "queue add")]
public class OptimizeQueueAddCommand : OaktonAsyncCommand<OptimizeQueueAddInput>
{
    public override async Task<bool> Execute(OptimizeQueueAddInput input)
    {
        try
        {
            var host = input.BuildHost();
            var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
            var queueGrain = grainFactory.GetGrain<IOptimizationQueueGrain>("global");

            // Create or load parameters
            PMultiSim parameters;
            if (!string.IsNullOrEmpty(input.ConfigFile))
            {
                if (!File.Exists(input.ConfigFile))
                {
                    AnsiConsole.MarkupLine($"[red]Config file not found: {input.ConfigFile}[/]");
                    return false;
                }

                var configJson = await File.ReadAllTextAsync(input.ConfigFile);
                parameters = JsonSerializer.Deserialize<PMultiSim>(configJson) 
                    ?? throw new InvalidOperationException("Failed to deserialize config file");
            }
            else
            {
                // Create basic parameters from command line
                parameters = new PMultiSim
                {
                    ExchangeSymbolTimeFrame = new(
                        input.ExchangeFlag ?? "Binance",
                        input.ExchangeAreaFlag ?? "futures", 
                        input.Symbol ?? "BTCUSDT",
                        TimeFrame.Parse(input.IntervalFlag ?? "h1")
                    ),
                    Start = input.FromFlag,
                    EndExclusive = input.ToFlag
                };

                if (!string.IsNullOrEmpty(input.BotType))
                {
                    // This would need to be enhanced to support bot type resolution
                    AnsiConsole.MarkupLine("[yellow]Bot type specification from CLI not yet implemented. Use --config-file instead.[/]");
                }
            }

            var parametersJson = JsonSerializer.Serialize(parameters);
            var job = await queueGrain.EnqueueJobAsync(parametersJson, input.Priority, "CLI");

            if (input.JsonFlag)
            {
                var result = new { JobId = job.JobId, Priority = job.Priority, Status = job.Status.ToString() };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]Job queued successfully![/]");
                AnsiConsole.MarkupLine($"Job ID: [cyan]{job.JobId}[/]");
                AnsiConsole.MarkupLine($"Priority: [yellow]{job.Priority}[/]");
                AnsiConsole.MarkupLine($"Status: [blue]{job.Status}[/]");
            }

            return true;
        }
        catch (Exception ex)
        {
            if (input.JsonFlag)
            {
                var error = new { Error = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(error));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            return false;
        }
    }
}

[Area("optimize")]
[Description("List optimization jobs in queue", Name = "queue list")]
public class OptimizeQueueListCommand : OaktonAsyncCommand<OptimizeQueueListInput>
{
    public override async Task<bool> Execute(OptimizeQueueListInput input)
    {
        try
        {
            // For this command, we need to build Orleans client separately since no CommonTradingInput
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services => 
                {
                    // Add Orleans client services - this would need proper configuration
                    services.AddOrleansClient(builder => 
                    {
                        builder.UseLocalhostClustering(); // This should be configurable
                    });
                })
                .Build();

            var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
            var queueGrain = grainFactory.GetGrain<IOptimizationQueueGrain>("global");

            if (input.SummaryFlag)
            {
                var status = await queueGrain.GetQueueStatusAsync();
                
                if (input.JsonFlag)
                {
                    Console.WriteLine(JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    var table = new Table()
                        .AddColumn("Metric")
                        .AddColumn("Value");

                    table.AddRow("Queued Jobs", status.QueuedCount.ToString());
                    table.AddRow("Running Jobs", status.RunningCount.ToString());
                    table.AddRow("Completed Jobs", status.CompletedCount.ToString());
                    table.AddRow("Failed Jobs", status.FailedCount.ToString());
                    table.AddRow("Cancelled Jobs", status.CancelledCount.ToString());
                    table.AddRow("Active Silos", status.ActiveSilos.ToString());
                    
                    if (status.AverageJobDuration.HasValue)
                        table.AddRow("Avg Duration", status.AverageJobDuration.Value.Humanize());

                    AnsiConsole.Write(table);
                }
            }
            else
            {
                OptimizationJobStatus? statusFilter = null;
                if (!string.IsNullOrEmpty(input.Status))
                {
                    if (!Enum.TryParse<OptimizationJobStatus>(input.Status, true, out var parsed))
                    {
                        AnsiConsole.MarkupLine($"[red]Invalid status: {input.Status}[/]");
                        return false;
                    }
                    statusFilter = parsed;
                }

                var jobs = await queueGrain.GetJobsAsync(statusFilter, input.Limit);

                if (input.JsonFlag)
                {
                    Console.WriteLine(JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    var table = new Table()
                        .AddColumn("Job ID")
                        .AddColumn("Status")
                        .AddColumn("Priority")
                        .AddColumn("Created")
                        .AddColumn("Duration")
                        .AddColumn("Progress")
                        .AddColumn("Submitted By");

                    foreach (var job in jobs)
                    {
                        var duration = job.Duration?.Humanize() ?? "-";
                        var progress = job.Status == OptimizationJobStatus.Running && job.Progress != null 
                            ? job.Progress.PerUn.ToString("P1") 
                            : job.Status == OptimizationJobStatus.Completed ? "100%" : "-";

                        table.AddRow(
                            job.JobId.ToString()[..8] + "...",
                            GetStatusMarkup(job.Status),
                            job.Priority.ToString(),
                            job.CreatedTime.Humanize(),
                            duration,
                            progress,
                            job.SubmittedBy ?? "Unknown"
                        );
                    }

                    AnsiConsole.Write(table);
                    AnsiConsole.MarkupLine($"Showing {jobs.Count} job(s)");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            if (input.JsonFlag)
            {
                var error = new { Error = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(error));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            return false;
        }
    }

    private static string GetStatusMarkup(OptimizationJobStatus status)
    {
        return status switch
        {
            OptimizationJobStatus.Queued => "[yellow]Queued[/]",
            OptimizationJobStatus.Running => "[blue]Running[/]",
            OptimizationJobStatus.Completed => "[green]Completed[/]",
            OptimizationJobStatus.Failed => "[red]Failed[/]",
            OptimizationJobStatus.Cancelled => "[orange1]Cancelled[/]",
            _ => status.ToString()
        };
    }
}

[Area("optimize")]
[Description("Cancel optimization job", Name = "queue cancel")]
public class OptimizeQueueCancelCommand : OaktonAsyncCommand<OptimizeQueueCancelInput>
{
    public override async Task<bool> Execute(OptimizeQueueCancelInput input)
    {
        try
        {
            if (!Guid.TryParse(input.JobId, out var jobId))
            {
                if (input.JsonFlag)
                {
                    var error = new { Error = "Invalid job ID format" };
                    Console.WriteLine(JsonSerializer.Serialize(error));
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid job ID format[/]");
                }
                return false;
            }

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services => 
                {
                    services.AddOrleansClient(builder => 
                    {
                        builder.UseLocalhostClustering();
                    });
                })
                .Build();

            var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
            var queueGrain = grainFactory.GetGrain<IOptimizationQueueGrain>("global");

            var cancelled = await queueGrain.CancelJobAsync(jobId);

            if (input.JsonFlag)
            {
                var result = new { Success = cancelled, JobId = input.JobId };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                if (cancelled)
                {
                    AnsiConsole.MarkupLine($"[green]Job {input.JobId} cancelled successfully[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Job {input.JobId} could not be cancelled (may already be completed or not found)[/]");
                }
            }

            return cancelled;
        }
        catch (Exception ex)
        {
            if (input.JsonFlag)
            {
                var error = new { Error = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(error));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            return false;
        }
    }
}

[Area("optimize")]
[Description("Get optimization job status", Name = "queue status")]
public class OptimizeQueueStatusCommand : OaktonAsyncCommand<OptimizeQueueStatusInput>
{
    public override async Task<bool> Execute(OptimizeQueueStatusInput input)
    {
        try
        {
            if (!Guid.TryParse(input.JobId, out var jobId))
            {
                if (input.JsonFlag)
                {
                    var error = new { Error = "Invalid job ID format" };
                    Console.WriteLine(JsonSerializer.Serialize(error));
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid job ID format[/]");
                }
                return false;
            }

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services => 
                {
                    services.AddOrleansClient(builder => 
                    {
                        builder.UseLocalhostClustering();
                    });
                })
                .Build();

            var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
            var queueGrain = grainFactory.GetGrain<IOptimizationQueueGrain>("global");

            var job = await queueGrain.GetJobAsync(jobId);

            if (job == null)
            {
                if (input.JsonFlag)
                {
                    var error = new { Error = "Job not found" };
                    Console.WriteLine(JsonSerializer.Serialize(error));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Job {input.JobId} not found[/]");
                }
                return false;
            }

            if (input.JsonFlag)
            {
                Console.WriteLine(JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                var table = new Table()
                    .AddColumn("Property")
                    .AddColumn("Value");

                table.AddRow("Job ID", job.JobId.ToString());
                table.AddRow("Status", GetStatusMarkup(job.Status));
                table.AddRow("Priority", job.Priority.ToString());
                table.AddRow("Created", job.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                table.AddRow("Submitted By", job.SubmittedBy ?? "Unknown");
                
                if (job.StartedTime.HasValue)
                    table.AddRow("Started", job.StartedTime.Value.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                
                if (job.CompletedTime.HasValue)
                    table.AddRow("Completed", job.CompletedTime.Value.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                
                if (job.Duration.HasValue)
                    table.AddRow("Duration", job.Duration.Value.Humanize());

                if (job.Status == OptimizationJobStatus.Running && job.Progress != null)
                {
                    table.AddRow("Progress", $"{job.Progress.PerUn:P1} ({job.Progress.Completed}/{job.Progress.Queued})");
                    
                    if (job.EstimatedCompletionTime.HasValue)
                    {
                        var remaining = job.EstimatedCompletionTime.Value - DateTimeOffset.UtcNow;
                        table.AddRow("Est. Completion", remaining > TimeSpan.Zero ? remaining.Humanize() : "Soon");
                    }
                }

                if (!string.IsNullOrEmpty(job.AssignedSiloId))
                    table.AddRow("Assigned Silo", job.AssignedSiloId);

                if (!string.IsNullOrEmpty(job.ResultPath))
                    table.AddRow("Results Path", job.ResultPath);

                if (!string.IsNullOrEmpty(job.ErrorMessage))
                    table.AddRow("Error", $"[red]{job.ErrorMessage}[/]");

                AnsiConsole.Write(table);
            }

            return true;
        }
        catch (Exception ex)
        {
            if (input.JsonFlag)
            {
                var error = new { Error = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(error));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            return false;
        }
    }

    private static string GetStatusMarkup(OptimizationJobStatus status)
    {
        return status switch
        {
            OptimizationJobStatus.Queued => "[yellow]Queued[/]",
            OptimizationJobStatus.Running => "[blue]Running[/]",
            OptimizationJobStatus.Completed => "[green]Completed[/]",
            OptimizationJobStatus.Failed => "[red]Failed[/]",
            OptimizationJobStatus.Cancelled => "[orange1]Cancelled[/]",
            _ => status.ToString()
        };
    }
}

#endregion