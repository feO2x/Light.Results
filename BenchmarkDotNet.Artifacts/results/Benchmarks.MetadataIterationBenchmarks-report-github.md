```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| DictionaryIterate_Small     | 1.4888 ns | 0.0180 ns | 0.0150 ns |  1.00 |    0.01 |         - |          NA |
| MetadataObjectIterate_Small | 0.9112 ns | 0.0141 ns | 0.0110 ns |  0.61 |    0.01 |         - |          NA |
| DictionaryIterate_Large     | 5.4446 ns | 0.0555 ns | 0.0492 ns |  3.66 |    0.05 |         - |          NA |
| MetadataObjectIterate_Large | 6.7050 ns | 0.0662 ns | 0.0619 ns |  4.50 |    0.06 |         - |          NA |
