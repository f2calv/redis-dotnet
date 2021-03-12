using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using CasCap;
using System.Diagnostics;
if (Debugger.IsAttached)
    _ = BenchmarkRunner.Run<MyBenchmarks>(new DebugInProcessConfig());
else
    _ = BenchmarkRunner.Run<MyBenchmarks>();
