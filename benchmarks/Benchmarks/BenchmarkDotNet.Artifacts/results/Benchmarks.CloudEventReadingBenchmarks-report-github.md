```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3


```

| Method                              |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|-------------------------------------|----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| ReadResult_SmallPayload             |  1.357 μs | 0.0132 μs | 0.0117 μs |  1.00 |    0.01 | 0.0286 |      - |     504 B |        1.00 |
| ReadResult_MediumPayload            |  8.146 μs | 0.1039 μs | 0.0972 μs |  6.00 |    0.09 | 0.3204 |      - |    5416 B |       10.75 |
| ReadResult_LargePayload             | 74.760 μs | 1.4771 μs | 1.9207 μs | 55.08 |    1.46 | 3.0518 | 0.8545 |   52000 B |      103.17 |
| ReadResult_Failure                  |  1.384 μs | 0.0143 μs | 0.0120 μs |  1.02 |    0.01 | 0.0572 |      - |     960 B |        1.90 |
| ReadResultWithEnvelope_SmallPayload |  1.382 μs | 0.0108 μs | 0.0096 μs |  1.02 |    0.01 | 0.0286 |      - |     504 B |        1.00 |
