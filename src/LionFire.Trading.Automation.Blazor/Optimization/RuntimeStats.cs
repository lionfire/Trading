using Humanizer;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class RuntimeStats : ReactiveObject, IDisposable
{

    bool isDisposed = false;

    #region Static

    public static RuntimeStats Instance => instance ??= new();
    static RuntimeStats? instance;


    //static Process? currentProcess;
    static string? instanceName = null; // = $"{Process.GetCurrentProcess().ProcessName}_{currentProcess.Id}";
    static bool InitializedPerformanceCounters = false;
    static PerformanceCounter? memoryCounter = null;
    static PerformanceCounter? cpuCounter = null;

    #endregion

    public Exception? Exception { get; private set; }

    #region Lifecycle

    public RuntimeStats()
    {
        OnStart();
    }

    void OnStart()
    {
        if (!OperatingSystem.IsBrowser())
#pragma warning disable CA1416 // Validate platform compatibility
        {
            _ = Task.Run(async () =>
            {

                try
                {
                    var t = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

                    while (!isDisposed)
                    {
                        Process currentProcess = Process.GetCurrentProcess(); // Have to do this every time, otherwise stats are cached

                        try
                        {
                            PrivateMemorySize = currentProcess.PrivateMemorySize64;
                            VirtualMemorySize64 = currentProcess.VirtualMemorySize64;
                            WorkingSet64 = currentProcess.WorkingSet64;
                            //Debug.WriteLine($"Private memory size: {PrivateMemorySize} bytes");
                            //Debug.WriteLine($"VirtualMemorySize64: {VirtualMemorySize64} bytes");
                            //Debug.WriteLine($"WorkingSet64: {WorkingSet64} bytes");
                        }
                        catch (Exception ex2)
                        {
                            Debug.WriteLine($"Error getting PrivateMemorySize size: {ex2}");
                        }

                        if (!InitializedPerformanceCounters)
                        {
                            instanceName = GetCurrentProcessInstanceName(currentProcess);

                            if (instanceName != null)
                            {
                                memoryCounter = new PerformanceCounter
                                {
                                    CategoryName = ".NET CLR Memory",
                                    //CounterName = "Large Object Heap size",
                                    CounterName = "# Bytes in all Heaps",
                                    InstanceName = instanceName
                                };

                                cpuCounter = new PerformanceCounter("Process", "% Processor Time", instanceName);
                                InitializedPerformanceCounters = true;
                            }
                        }
                        if (InitializedPerformanceCounters)
                        {
                            try
                            {
                                CpuUsage = cpuCounter!.NextValue() / 100.0 / (double)System.Environment.ProcessorCount;
                                //Debug.WriteLine($"CPU Usage: {CpuUsage}%");
                            }
                            catch (Exception ex2)
                            {
                                //Debug.WriteLine($"Error getting CPU usage: {ex2}");
                            }

                            //try
                            //{

                            //    LargeObjectHeapSize = (long)memoryCounter!.RawValue;
                            //    //LargeObjectHeapSize = (long)performanceCounter!.NextValue();

                            //    Debug.WriteLine($"Large Object Heap size: {LargeObjectHeapSize} bytes");
                            //}
                            //catch (Exception ex2)
                            //{
                            //    Debug.WriteLine($"Error getting Large Object Heap size: {ex2}");
                            //}
                        }
                        await t.WaitForNextTickAsync();
                    }

                }
                catch (Exception ex)
                {
                    Exception = ex;
                    Debug.WriteLine($"RuntimeStats Error: {ex}");
                }
                Debug.WriteLine($"RuntimeStats finished.");
            });
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }


    public void Dispose()
    {
        isDisposed = true;
    }

    #endregion

    #region Stats

    [Reactive]
    long _privateMemorySize;
    //public string PrivateMemoryBytesString => PrivateMemorySize.Bytes().Humanize();
    [Reactive]
    long _workingSet64;
    [Reactive]
    long _VirtualMemorySize64;


    [Reactive]
    long _largeObjectHeapSize;

    [Reactive]
    double _cpuUsage;

    #endregion


    public static string? GetCurrentProcessInstanceName(Process process)
    {
        try
        {
            if (!OperatingSystem.IsBrowser())
            {
#pragma warning disable CA1416 // Validate platform compatibility

                string processName = process.ProcessName;
                int processId = process.Id;

                string[] instanceNames = new PerformanceCounterCategory("Process").GetInstanceNames();

                foreach (string instanceName in instanceNames)
                {
                    try
                    {
                        using (PerformanceCounter processIdCounter = new PerformanceCounter("Process", "ID Process", instanceName, true))
                        {
                            if ((int)processIdCounter.RawValue == processId)
                            {
                                return instanceName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"GetCurrentProcessInstanceName exception for instanceName '{instanceName}': {ex}");
                    }
                }
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCurrentProcessInstanceName failure': {ex}");
        }
        return null; // Return null if no matching instance name is found
    }
}

