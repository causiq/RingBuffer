``` ini

BenchmarkDotNet=v0.10.14, OS=macOS 10.14 (18A391) [Darwin 18.0.0]
Intel Core i5-7500 CPU 3.40GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=2.1.402
  [Host]     : .NET Core 2.1.4 (CoreCLR 4.6.26814.03, CoreFX 4.6.26814.02), 64bit RyuJIT DEBUG
  DefaultJob : .NET Core 2.1.4 (CoreCLR 4.6.26814.03, CoreFX 4.6.26814.02), 64bit RyuJIT


```
|        Method |     Mean |    Error |   StdDev |
|-------------- |---------:|---------:|---------:|
| tryPutChannel | 386.3 ms | 8.404 ms | 9.678 ms |
|    tryPutMVar | 149.3 ms | 2.969 ms | 3.177 ms |
