```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                      | Mean       | Error     | StdDev    | Median     | Allocated |
|---------------------------- |-----------:|----------:|----------:|-----------:|----------:|
| CreateSuccess               |  1.8672 ns | 0.0063 ns | 0.0053 ns |  1.8668 ns |         - |
| CreateFailure               |  7.1729 ns | 0.0218 ns | 0.0203 ns |  7.1668 ns |         - |
| TryGetValue_Success         |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| TryGetValue_Failure         |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| EnumerateErrors_Single      |  0.3374 ns | 0.0104 ns | 0.0097 ns |  0.3340 ns |         - |
| EnumerateErrors_Multiple    |  2.7293 ns | 0.0209 ns | 0.0195 ns |  2.7343 ns |         - |
| AccessErrorByIndex_Single   |  3.2230 ns | 0.0153 ns | 0.0136 ns |  3.2186 ns |         - |
| AccessErrorByIndex_Multiple |  3.2393 ns | 0.0108 ns | 0.0101 ns |  3.2403 ns |         - |
| ErrorsCount                 |  0.0012 ns | 0.0032 ns | 0.0027 ns |  0.0000 ns |         - |
| FirstError                  |  0.9553 ns | 0.0072 ns | 0.0067 ns |  0.9574 ns |         - |
| Map_Success                 | 11.2525 ns | 0.0483 ns | 0.0452 ns | 11.2569 ns |         - |
| Map_Failure                 | 13.3766 ns | 0.0740 ns | 0.0692 ns | 13.3557 ns |         - |
