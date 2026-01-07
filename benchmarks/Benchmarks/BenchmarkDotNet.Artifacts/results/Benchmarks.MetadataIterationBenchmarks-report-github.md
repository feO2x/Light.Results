```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```

| Method                      |      Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|-----------------------------|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| DictionaryIterate_Small     | 1.4263 ns | 0.0170 ns | 0.0159 ns |  1.00 |    0.02 |         - |          NA |
| MetadataObjectIterate_Small | 0.9088 ns | 0.0108 ns | 0.0096 ns |  0.64 |    0.01 |         - |          NA |
| DictionaryIterate_Large     | 5.2705 ns | 0.0243 ns | 0.0215 ns |  3.70 |    0.04 |         - |          NA |
| MetadataObjectIterate_Large | 7.3371 ns | 0.1175 ns | 0.1099 ns |  5.14 |    0.09 |         - |          NA |
