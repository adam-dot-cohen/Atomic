using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Atomic;
public sealed class Atomic<T> where T : unmanaged
{
    private AtomicSpinWait _wait = new();
	
    private T _value;

    public Atomic(T value) => _value = value;

    public T Value => _value;

    public T Op(Func<Operation, T> del)
    {		
        _wait.Acquire();
        var result = del(new Operation(this));
        _wait.Release();
        return result;
    }

    public void Op(Action<Operation> del)
    {
        _wait.Acquire();
        del(new Operation(this));
        _wait.Release();
    }

    public sealed class Operation
    {
        private Atomic<T> _atomic;

        public Operation(Atomic<T> atomic) => _atomic = atomic;

        public T Value
        {
            get => _atomic.Value;
            set => _atomic._value = value;
        }

        public bool CompareExchange(T value, T compareand)
        {
            if (!IsEqual(compareand))
            {
                this.Value = value;
                return true;
            }
            return false;
        }

        public T Exchange(T value)
        {
            var result = this.Value;

            this.Value = value;

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEqual(T compare)
            => (MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref _atomic._value), Unsafe.SizeOf<T>()).SequenceEqual(
                MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref compare), Unsafe.SizeOf<T>())));
    }	
	
    [StructLayout(LayoutKind.Auto)]
    private struct AtomicSpinWait
    {
        private int _value;
        public AtomicSpinWait() => _value = 0;
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Acquire()
        {
            for (var sw = new SpinWait(); Interlocked.CompareExchange(ref _value, 1, 0) == 1; sw.SpinOnce());
        }
		
        public void Release() => _value = 0;
    }
}
