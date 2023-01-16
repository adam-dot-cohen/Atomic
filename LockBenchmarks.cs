using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceReloggerLib;

namespace Atomic;
[SimpleJob(runStrategy: RunStrategy.Throughput, runtimeMoniker: RuntimeMoniker.Net70)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class LockBenchmarks
{
    private long _interlocked = 0;
    private Atomic<long> _atomic = new (0);
    private AtomicWithSpinLock<long> _atomicWithSpinLock = new (0);
    
    [Benchmark(Description="Interlocked")]
    public void Interlock() => Interlocked.Increment(ref _interlocked);
    
    [Benchmark]
    public void AtomicSpinWait() => _atomic.Op(x=> x.Value++ );

    [Benchmark]
    public void SpinLock() => _atomicWithSpinLock.Op(x => x.Value++ );
}
