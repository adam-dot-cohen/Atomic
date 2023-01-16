using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Atomic;
public sealed class AtomicWithSpinLock<T> where T : unmanaged
{
    private T _value;

    public AtomicWithSpinLock(T value) => _value = value;

    public T Value => _value;

    public T Op(Func<Operation,T> del)
    {
        bool entered = false;
        try
        {
            _lock.Enter(ref entered);

            return del(new Operation(this));
        }
        finally
        {
            _lock.Exit();
        }
    }
    public void Op(Action<Operation> del)
    {
        bool entered = false;
        try
        {
            _lock.Enter(ref entered);
            del(new Operation(this));
        }
        finally
        {
            _lock.Exit();
        }
    }
    public sealed class Operation
    {
        private AtomicWithSpinLock<T> _atomic;
				
        public Operation(AtomicWithSpinLock<T> atomic) => _atomic = atomic;

        public T Value
        {
            get =>  _atomic.Value;
            set => _atomic._value = value;
        }

        public T CompareExchange(T value, T compareand)
        {
            var result = this.Value;

            if (IsEqual(compareand)) this.Value = value;

            return result;
        }

        public T Exchange(T value)
        {
            var result = this.Value;

            this.Value = value;

            return result;
        }
        public bool IsEqual(T compareand)
            => (MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref _atomic._value), Unsafe.SizeOf<T>()).SequenceEqual(
                MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref compareand), Unsafe.SizeOf<T>())));
    }

    private SpinLock _lock = new();
}
