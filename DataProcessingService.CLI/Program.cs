using System.Collections.Concurrent;
using System.Text.Json;
using DataProcessingService.Contracts;
using DataProcessingService.Entities;
using DataProcessingService.Logic;

internal class Program
{
    private static object _lockObj = new();
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private static string? _rootPath;
    private static string? _outputPath;
    private static string? _dateBasedOutput;

    private static int _dailyFileCounter = 0;
    private static int _parsedLines = 0;
    private static int _parsedErrors = 0;
    private static List<string> _invalidFilePaths = new List<string>();

    private static FileSystemWatcher? _watcher;
    private static Timer? _timer;

    private static void Main(string[] args)
    {
        _rootPath = GetPathFromConfiguration();
        _outputPath = _rootPath + "/output";

        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);

        Console.CancelKeyPress += OnExitCallback;
        InitializeWatcher();
        InitializeTimer();

        Console.WriteLine("Press Ctrl+C to exit.");
        new AutoResetEvent(false).WaitOne();
    }

    private static void InitializeTimer()
    {
        DateTime now = DateTime.Now;
        DateTime midnight = DateTime.Today.AddDays(1);
        TimeSpan timeUntilNextRun = midnight - now;

        _timer = new Timer(OnMidnightLog, null, timeUntilNextRun, TimeSpan.FromDays(1));
    }

    private static async void OnMidnightLog(object? state) => await Task.Run(async () =>
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!string.IsNullOrEmpty(_dateBasedOutput))
            {
                using var writer = new StreamWriter($"{_dateBasedOutput}/meta.log");

                var metaData = new MetaData
                {
                    InvalidFilePaths = _invalidFilePaths,
                    ParsedErrors = _parsedErrors,
                    ParsedLines = _parsedLines,
                    ParsedFiles = _dailyFileCounter
                };

                var options = new JsonSerializerOptions();
                options.Converters.Add(new DateOnlyConverter());
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.WriteIndented = true;

                string jsonString = JsonSerializer.Serialize(metaData, options);

                await writer.WriteAsync(jsonString);
            }

            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset midnight = now.Date.AddDays(1);
            TimeSpan timeUntilMidnight = midnight - now;
            _timer = new Timer(OnMidnightLog, null, timeUntilMidnight, TimeSpan.FromDays(1));
        }
        catch (ObjectDisposedException ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Critical Error occured: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            _semaphore.Release();
        }
    });

    private static void OnExitCallback(object? sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Successfully disposed!");
        _watcher.Dispose();
        _timer.Dispose();

        Environment.Exit(0);
    }

    private static void InitializeWatcher()
    {
        _watcher = new FileSystemWatcher();
        _watcher.Path = _rootPath;
        _watcher.NotifyFilter = NotifyFilters.Attributes |
            NotifyFilters.CreationTime |
            NotifyFilters.FileName |
            NotifyFilters.LastAccess |
            NotifyFilters.LastWrite |
            NotifyFilters.Size |
            NotifyFilters.Security;
        _watcher.Filter = "*.*";
        _watcher.Created += OnFileAddAsync;
        _watcher.EnableRaisingEvents = true;
    }

    private static async void OnFileAddAsync(object sender, FileSystemEventArgs e) => await Task.Run(async () =>
    {
        await _semaphore.WaitAsync();

        try
        {
            _dateBasedOutput = $"{_outputPath}/{DateTime.Now.Date:dd/MM/yyyy}";

            if (!Directory.Exists(_dateBasedOutput))
                Directory.CreateDirectory(_dateBasedOutput);

            string inputFile = e.FullPath;

            ParseOutput? parseResult = null;

                if (Path.GetExtension(inputFile) == ".csv")
                {
                    IDataReader dataReader = new CsvDataReader();
                    parseResult = await dataReader.ParsePaymentTransactionsAsync(inputFile);

                    var writer = new TxtDataWriter();
                    await writer.WriteDataAsync(parseResult, _dateBasedOutput, ++_dailyFileCounter);
                }
                else if (Path.GetExtension(inputFile) == ".txt")
                {
                    IDataReader dataReader = new TxtDataReader();
                    parseResult = await dataReader.ParsePaymentTransactionsAsync(inputFile);

                    var writer = new TxtDataWriter();
                    await writer.WriteDataAsync(parseResult, _dateBasedOutput, ++_dailyFileCounter);
                } 

            if (parseResult != null)
            {
                _parsedLines += parseResult.ParsedLines;
                _parsedErrors += parseResult.ParsedErrors;

                if (parseResult.ParsedErrors > 1)
                    _invalidFilePaths.Add(parseResult.FilePath);
            }
        }
        catch (ObjectDisposedException ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Critical Error occured: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error occured: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            _semaphore.Release();
        }
    });

    private static string GetPathFromConfiguration()
    {
        string configurationPath = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\configuration.json");
        Configuration configuration = JsonSerializer.Deserialize<Configuration>(configurationPath);

        return configuration.FolderPath;
    }
}
