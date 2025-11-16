# RaidExtractor Build Instructions

## Prerequisites

- .NET 8 SDK or later
- Windows operating system (required for process memory access)
- Raid: Shadow Legends client installed

## Build

Navigate to the repository root and run:

```bash
dotnet build RaidExtractor.sln -c Release
```

This will build both projects:
- `RaidExtractor.Core` (extraction library)
- `RaidExtractor` (console application)

## Output

Build artifacts are located in:
```
RaidExtractor/bin/Release/net8.0/
```

The main executable is:
```
RaidExtractor.exe
```

## Run

### Basic Usage

```bash
RaidExtractor.exe --scan --output C:\RaidKiller\exports
```

### Commands

**Extract data:**
```bash
RaidExtractor.exe --scan --output <directory>
```

**Display version:**
```bash
RaidExtractor.exe --version
```

**Show help:**
```bash
RaidExtractor.exe --help
```

## Output Files

When extraction completes successfully, the following files are created in the output directory:

- `roster.json` - Champion data including stats, skills, masteries, and gear
- `artifacts.json` - All artifact/gear data with bonuses and metadata
- `account.json` - Account-level data (arena, shards, great hall, presets)
- `metadata.json` - Extraction metadata (timestamp, version, export path)

On failure:
- `error.json` - Error details with timestamp

## Expected Directory Structure

```
RaidExtractor/
├── RaidExtractor/
│   ├── Program.cs
│   ├── ExtractorVersion.cs
│   └── RaidExtractor.csproj
├── RaidExtractor.Core/
│   ├── Extractor.cs
│   ├── AccountDumpClient.cs
│   ├── StaticDataHandler.cs
│   └── RaidExtractor.Core.csproj
├── RaidExtractor.sln
├── BUILD.md
└── schema.md
```

## Logs

Logs are written to:
```
logs/raidextractor.log
```

The log file is created in the same directory as the executable.

## Troubleshooting

### "RAID client not detected"
- Ensure Raid: Shadow Legends is running before executing the extractor
- The game must be fully loaded (past the login screen)

### "Update required. Game version does not match expected version."
- The game client has been updated
- Static data offsets need to be regenerated (developer task)

### Build fails with "dotnet: command not found"
- Install .NET 8 SDK from https://dotnet.microsoft.com/download
- Verify installation: `dotnet --version`

### Missing DLL errors at runtime
- Ensure all dependencies were restored during build
- Run: `dotnet restore RaidExtractor.sln`
- Rebuild: `dotnet build RaidExtractor.sln -c Release`

### Access denied errors
- Run as Administrator (required for process memory access)
- Ensure antivirus is not blocking the executable

### Empty or partial JSON output
- Check `logs/raidextractor.log` for detailed error messages
- Check for `error.json` in the output directory

## Integration with RaidKiller

The exported JSON files follow a stable schema (see `schema.md`) designed for consumption by the RaidKiller hybrid application.

Expected workflow:
1. Run extractor: `RaidExtractor.exe --scan --output <path>`
2. Verify output files exist
3. Import JSON files into RaidKiller
4. Parse using schema definitions in `schema.md`
