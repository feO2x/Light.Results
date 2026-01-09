```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| BuildDictionary           |  62.67 ns |  0.126 ns |  0.111 ns |  1.00 |    0.00 | 0.0640 |      - |     536 B |        1.00 |
| BuildMetadataObject       |  39.74 ns |  0.601 ns |  0.562 ns |  0.63 |    0.01 | 0.0220 |      - |     184 B |        0.34 |
| BuildDictionary_Large     | 531.11 ns | 10.615 ns | 10.426 ns |  8.47 |    0.16 | 0.3824 | 0.0029 |    3200 B |        5.97 |
| BuildMetadataObject_Large | 574.30 ns |  7.336 ns |  6.503 ns |  9.16 |    0.10 | 0.1593 |      - |    1336 B |        2.49 |
