```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  Job-WWFMMQ : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a

IterationCount=3  WarmupCount=1

```

| Method                      |       Mean |     Error |    StdDev | Allocated |
|-----------------------------|-----------:|----------:|----------:|----------:|
| CreateSuccess               |  2.7333 ns | 0.3058 ns | 0.0168 ns |         - |
| CreateFailure               | 14.7732 ns | 0.6131 ns | 0.0336 ns |         - |
| TryGetValue_Success         |  0.0000 ns | 0.0000 ns | 0.0000 ns |         - |
| TryGetValue_Failure         |  0.0000 ns | 0.0000 ns | 0.0000 ns |         - |
| EnumerateErrors_Single      |  0.3473 ns | 0.3088 ns | 0.0169 ns |         - |
| EnumerateErrors_Multiple    |  2.8093 ns | 0.9789 ns | 0.0537 ns |         - |
| AccessErrorByIndex_Single   |  5.3586 ns | 0.1106 ns | 0.0061 ns |         - |
| AccessErrorByIndex_Multiple |  5.2847 ns | 0.7806 ns | 0.0428 ns |         - |
| ErrorsCount                 |  0.0000 ns | 0.0000 ns | 0.0000 ns |         - |
| FirstError                  |  2.5291 ns | 1.2200 ns | 0.0669 ns |         - |
| Map_Success                 | 14.7054 ns | 0.8256 ns | 0.0453 ns |         - |
| Map_Failure                 | 16.5351 ns | 1.1623 ns | 0.0637 ns |         - |
