```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                     | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| DictionaryLookup_Small     | 2.544 ns | 0.0180 ns | 0.0168 ns |  1.00 |    0.01 |         - |          NA |
| MetadataObjectLookup_Small | 2.425 ns | 0.0671 ns | 0.0627 ns |  0.95 |    0.02 |         - |          NA |
| DictionaryLookup_Large     | 2.949 ns | 0.0580 ns | 0.0542 ns |  1.16 |    0.02 |         - |          NA |
| MetadataObjectLookup_Large | 3.098 ns | 0.0225 ns | 0.0211 ns |  1.22 |    0.01 |         - |          NA |
