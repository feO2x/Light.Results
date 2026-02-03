```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  Job-MEHJPP : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a

IterationCount=5  WarmupCount=1

```

| Method                 |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|------------------------|-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| Errors1                |   208.4 ns |  2.55 ns |  0.39 ns |  1.00 |    0.00 | 0.1080 |      - |     904 B |        1.00 |
| Errors3_UniqueTargets  |   462.0 ns | 14.81 ns |  3.85 ns |  2.22 |    0.02 | 0.1287 |      - |    1080 B |        1.19 |
| Errors3_SharedTarget   |   398.8 ns | 13.51 ns |  2.09 ns |  1.91 |    0.01 | 0.1078 |      - |     904 B |        1.00 |
| Errors5_UniqueTargets  |   782.0 ns | 52.25 ns | 13.57 ns |  3.75 |    0.06 | 0.2575 | 0.0010 |    2160 B |        2.39 |
| Errors5_SharedTargets  |   637.9 ns |  9.55 ns |  2.48 ns |  3.06 |    0.01 | 0.1669 |      - |    1400 B |        1.55 |
| Errors10_UniqueTargets | 1,536.2 ns | 68.71 ns | 10.63 ns |  7.37 |    0.05 | 0.5207 | 0.0038 |    4368 B |        4.83 |
| Errors10_SharedTargets | 1,190.1 ns | 14.23 ns |  2.20 ns |  5.71 |    0.01 | 0.3414 | 0.0019 |    2864 B |        3.17 |
