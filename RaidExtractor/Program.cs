using System;
using System.IO;
using System.IO.Compression;
using CommandLine;
using RaidExtractor.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RaidExtractor
{
    class Program
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "raidextractor.log");

        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                    OverrideSpecifiedNames = false
                },
            },
        };

        public class Options
        {
            [Option("scan", Required = false, Default = false,
              HelpText = "Run the extraction scan on the running Raid: Shadow Legends process.")]
            public bool Scan { get; set; }

            [Option("output", Required = false, Default = "artifacts.json",
              HelpText = "Destination output file path. Defaults to artifacts.json")]
            public string? Output { get; set; }
        }

        static void Main(string[] args)
        {
            InitializeLogging();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(errors => HandleParseErrors(errors));
        }

        private static void InitializeLogging()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create log directory: {ex.Message}");
            }
        }

        private static void Log(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            Console.WriteLine(logMessage);

            try
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Silently fail if we can't write to log file
            }
        }

        private static void RunWithOptions(Options options)
        {
            if (!options.Scan)
            {
                Console.WriteLine("RaidExtractor - .NET 8 Console Edition");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  RaidExtractor --scan --output <path>");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --scan          Execute extraction scan");
                Console.WriteLine("  --output <path> Output file path (default: artifacts.json)");
                return;
            }

            Log("Starting RaidExtractor scan...");

            try
            {
                Log("Initializing extractor...");
                Extractor raidExtractor = new Extractor();

                Log("Extracting data from Raid: Shadow Legends...");
                AccountDump? dump = raidExtractor.GetDump();

                if (dump == null)
                {
                    Log("ERROR: Failed to extract data. Raid may not be running.");
                    Console.WriteLine("ERROR: Could not extract data. Please ensure Raid: Shadow Legends is running.");
                    return;
                }

                Log("Extraction completed successfully.");

                string outputPath = options.Output ?? "artifacts.json";

                Log($"Serializing data to {outputPath}...");
                string json = JsonConvert.SerializeObject(dump, SerializerSettings);

                File.WriteAllText(outputPath, json);

                Log($"Output file created: {outputPath}");
                Log($"Extraction complete. Extracted {dump.Heroes?.Count ?? 0} heroes and {dump.Artifacts?.Count ?? 0} artifacts.");

                Console.WriteLine($"SUCCESS: Extraction completed. Output written to {outputPath}");
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"ERROR: Extraction failed - {ex.Message}");
            }
        }

        private static void HandleParseErrors(System.Collections.Generic.IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                if (error is not HelpRequestedError && error is not VersionRequestedError)
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
    }
}
