using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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

            [Option("output", Required = false, Default = "./export",
              HelpText = "Destination output directory path. Defaults to ./export")]
            public string? Output { get; set; }

            [Option("version", Required = false, Default = false,
              HelpText = "Display version information.")]
            public bool Version { get; set; }
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
            if (options.Version)
            {
                Console.WriteLine($"RaidExtractor v{ExtractorVersion.Current}");
                return;
            }

            if (!options.Scan)
            {
                Console.WriteLine("RaidExtractor - .NET 8 Console Edition");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  RaidExtractor --scan --output <path>");
                Console.WriteLine("  RaidExtractor --version");
                Console.WriteLine("  RaidExtractor --help");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --scan          Execute extraction scan");
                Console.WriteLine("  --output <path> Output directory path (default: ./export)");
                Console.WriteLine("  --version       Display version information");
                Console.WriteLine("  --help          Display this help screen");
                return;
            }

            string outputDirectory = options.Output ?? "./export";

            Log($"Starting RaidExtractor v{ExtractorVersion.Current} scan...");

            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                    Log($"Created output directory: {outputDirectory}");
                }

                Log("Initializing extractor...");
                Extractor raidExtractor = new Extractor();

                Log("Extracting data from Raid: Shadow Legends...");
                AccountDump? dump = raidExtractor.GetDump();

                if (dump == null)
                {
                    Log("ERROR: RAID client not detected.");
                    Console.WriteLine("ERROR: RAID client not detected. Please ensure Raid: Shadow Legends is running.");
                    WriteErrorFile(outputDirectory, "RAID client not detected");
                    return;
                }

                Log("Extraction completed successfully.");
                Log("Starting JSON export...");

                ExportJsonFiles(dump, outputDirectory);

                LogExtractionSummary(dump, true);
                Console.WriteLine($"SUCCESS: Extraction completed. Files exported to {outputDirectory}");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message.Contains("Raid needs to be running")
                    ? "RAID client not detected"
                    : ex.Message;

                Log($"ERROR: Fatal error during extraction - {errorMessage}");
                Log($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"ERROR: Extraction failed - {errorMessage}");
                WriteErrorFile(outputDirectory, errorMessage);
                LogExtractionSummary(null, false);
            }
        }

        private static void LogExtractionSummary(AccountDump? dump, bool success)
        {
            Log("[INFO] Extraction Summary:");
            if (dump != null)
            {
                Log($"[INFO] Champions: {dump.Heroes?.Count ?? 0}");
                Log($"[INFO] Artifacts: {dump.Artifacts?.Count ?? 0}");
            }
            else
            {
                Log("[INFO] Champions: 0");
                Log("[INFO] Artifacts: 0");
            }
            Log($"[INFO] Status: {(success ? "Success" : "Failure")}");
        }

        private static void ExportJsonFiles(AccountDump dump, string outputDirectory)
        {
            if (!ValidateData(dump, outputDirectory))
            {
                return;
            }

            Log("Exporting roster.json");
            var roster = ConvertToRoster(dump);
            WriteJsonFile(Path.Combine(outputDirectory, "roster.json"), roster);

            Log("Exporting artifacts.json");
            var artifacts = ConvertToArtifacts(dump);
            WriteJsonFile(Path.Combine(outputDirectory, "artifacts.json"), artifacts);

            Log("Exporting account.json");
            var account = ConvertToAccount(dump);
            WriteJsonFile(Path.Combine(outputDirectory, "account.json"), account);

            Log("Exporting metadata.json");
            var metadata = CreateMetadata(outputDirectory);
            WriteJsonFile(Path.Combine(outputDirectory, "metadata.json"), metadata);

            Log("Export complete.");
        }

        private static bool ValidateData(AccountDump dump, string outputDirectory)
        {
            var errors = new List<string>();

            if (dump.Heroes == null)
            {
                errors.Add("Roster data is null");
            }

            if (dump.Artifacts == null)
            {
                errors.Add("Artifacts data is null");
            }

            if (errors.Count > 0)
            {
                string errorMessage = "Validation failed: " + string.Join(", ", errors);
                Log($"ERROR: {errorMessage}");
                WriteErrorFile(outputDirectory, errorMessage);
                return false;
            }

            return true;
        }

        private static void WriteJsonFile(string filePath, object data)
        {
            string json = JsonConvert.SerializeObject(data, SerializerSettings);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        private static void WriteErrorFile(string outputDirectory, string errorMessage)
        {
            try
            {
                var error = new
                {
                    error = errorMessage,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                WriteJsonFile(Path.Combine(outputDirectory, "error.json"), error);
            }
            catch
            {
                // Silently fail
            }
        }

        private static object ConvertToRoster(AccountDump dump)
        {
            var champions = (dump.Heroes ?? new List<Hero>()).Select(hero => new
            {
                championId = hero.Id,
                name = hero.Name,
                rarity = hero.Rarity,
                role = hero.Role,
                fraction = hero.Fraction,
                element = hero.Element,
                grade = hero.Grade,
                level = hero.Level,
                experience = hero.Experience,
                fullExperience = hero.FullExperience,
                awakenLevel = hero.AwakenLevel,
                locked = hero.Locked,
                inStorage = hero.InStorage,
                marker = hero.Marker,
                stats = new
                {
                    health = hero.Health,
                    attack = hero.Attack,
                    defense = hero.Defense,
                    speed = hero.Speed,
                    accuracy = hero.Accuracy,
                    resistance = hero.Resistance,
                    criticalChance = hero.CriticalChance,
                    criticalDamage = hero.CriticalDamage,
                    criticalHeal = hero.CriticalHeal
                },
                skills = hero.Skills?.Select(skill => new
                {
                    id = skill.Id,
                    typeId = skill.TypeId,
                    level = skill.Level
                }).ToList() ?? new List<object>(),
                masteries = hero.Masteries ?? new List<int>(),
                artifacts = hero.Artifacts ?? new List<int>()
            }).ToList();

            return new { champions };
        }

        private static object ConvertToArtifacts(AccountDump dump)
        {
            var artifactsList = (dump.Artifacts ?? new List<Artifact>()).Select(artifact => new
            {
                artifactId = artifact.Id,
                set = artifact.SetKind,
                kind = artifact.Kind,
                rank = artifact.Rank,
                rarity = artifact.Rarity,
                level = artifact.Level,
                isActivated = artifact.IsActivated,
                isSeen = artifact.IsSeen,
                requiredFraction = artifact.RequiredFraction,
                sellPrice = artifact.SellPrice,
                price = artifact.Price,
                failedUpgrades = artifact.FailedUpgrades,
                primaryBonus = artifact.PrimaryBonus != null ? new
                {
                    kind = artifact.PrimaryBonus.Kind,
                    isAbsolute = artifact.PrimaryBonus.IsAbsolute,
                    value = artifact.PrimaryBonus.Value
                } : null,
                secondaryBonuses = artifact.SecondaryBonuses?.Select(bonus => new
                {
                    kind = bonus.Kind,
                    isAbsolute = bonus.IsAbsolute,
                    value = bonus.Value,
                    enhancement = bonus.Enhancement,
                    level = bonus.Level
                }).ToList() ?? new List<object>()
            }).ToList();

            return new { artifacts = artifactsList };
        }

        private static object ConvertToAccount(AccountDump dump)
        {
            return new
            {
                arenaLeague = dump.ArenaLeague,
                shards = dump.Shards ?? new Dictionary<string, ShardInfo>(),
                greatHall = dump.GreatHall ?? new Dictionary<RaidExtractor.Core.Native.Element, Dictionary<RaidExtractor.Core.Native.StatKindId, int>>(),
                stagePresets = dump.StagePresets ?? new Dictionary<int, int[]>()
            };
        }

        private static object CreateMetadata(string outputPath)
        {
            return new
            {
                extractionTimestamp = DateTime.UtcNow.ToString("o"),
                extractorVersion = ExtractorVersion.Current,
                exportPath = Path.GetFullPath(outputPath)
            };
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
