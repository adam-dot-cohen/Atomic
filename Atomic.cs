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
	
    private Operation _operation;

    private T _value;

    public Atomic(T value)
    {
        _value = value;
        _operation = new Operation(this);
    }

    public T Value => _value;

    public void Op(Action<Operation> del)
    {
        _wait.Acquire();
        del(_operation);
        _wait.Release();
    }

    public sealed class Operation
    {
        private Atomic<T> _atomic;

        public Operation(Atomic<T> atomic) => _atomic = atomic;

        public T Value
        {
            get => _atomic._value;
            set => _atomic._value = value;
        }
        public void Update(T value)
            => _atomic._value = value;

        public bool CompareExchange(T value, T compareand)
        {
            if (!_atomic.Equals(compareand))
            {
                _atomic._value = value;
                return true;
            }
            return false;
        }

        public T Exchange(T value)
        {
            var result = this.Value;

            _atomic._value = value;

            return result;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private struct AtomicSpinWait
    {
        private int _value;
        public AtomicSpinWait() => _value = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Acquire()
        {
            for (var sw = new SpinWait(); Interlocked.CompareExchange(ref _value, 1, 0) == 1; sw.SpinOnce()) ;
        }

        public void Release() => _value = 0;
    }
}
