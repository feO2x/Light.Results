```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                     | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CreateDictionary_Small     |  36.46 ns | 0.297 ns | 0.248 ns |  1.00 |    0.01 | 0.0315 |      - |     264 B |        1.00 |
| CreateMetadataObject_Small |  36.73 ns | 0.593 ns | 0.583 ns |  1.01 |    0.02 | 0.0325 |      - |     272 B |        1.03 |
| CreateDictionary_Large     | 159.45 ns | 2.238 ns | 2.094 ns |  4.37 |    0.06 | 0.1357 | 0.0002 |    1136 B |        4.30 |
| CreateMetadataObject_Large | 173.51 ns | 0.723 ns | 0.676 ns |  4.76 |    0.04 | 0.0861 |      - |     720 B |        2.73 |
