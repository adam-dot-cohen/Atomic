// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

IConfig BenchConfig = DefaultConfig.Instance.AddJob(Job.Default.AsDefault()
    // .WithLaunchCount(1).WithInvocationCount(1).WithWarmupCount(1).WithUnrollFactor(1)
    .WithRuntime(CoreRuntime.Core70)
    .WithJit(Jit.RyuJit)
    .WithArguments(new[] { new MsBuildArgument("/p:Optimize=true") }));

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, BenchConfig);

Console.ReadLine();
