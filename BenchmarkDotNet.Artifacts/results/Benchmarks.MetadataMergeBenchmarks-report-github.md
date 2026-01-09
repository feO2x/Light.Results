```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method               | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| MergeDictionaries    | 72.55 ns | 0.387 ns | 0.343 ns |  1.00 | 0.0554 |     464 B |        1.00 |
| MergeMetadataObjects | 72.87 ns | 0.983 ns | 0.920 ns |  1.00 | 0.0257 |     216 B |        0.47 |
