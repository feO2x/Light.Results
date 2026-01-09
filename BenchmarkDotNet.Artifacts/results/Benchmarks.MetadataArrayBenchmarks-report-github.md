```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method               | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CreateObjectArray    | 19.4078 ns | 0.0755 ns | 0.0706 ns | 1.000 | 0.0220 |     184 B |        1.00 |
| CreateMetadataArray  | 20.5135 ns | 0.0715 ns | 0.0668 ns | 1.057 | 0.0373 |     312 B |        1.70 |
| ObjectArrayAccess    |  0.0000 ns | 0.0000 ns | 0.0000 ns | 0.000 |      - |         - |        0.00 |
| MetadataArrayAccess  |  0.0000 ns | 0.0000 ns | 0.0000 ns | 0.000 |      - |         - |        0.00 |
| ObjectArrayIterate   |  1.2055 ns | 0.0103 ns | 0.0097 ns | 0.062 |      - |         - |        0.00 |
| MetadataArrayIterate |  4.1453 ns | 0.1095 ns | 0.1605 ns | 0.214 |      - |         - |        0.00 |
