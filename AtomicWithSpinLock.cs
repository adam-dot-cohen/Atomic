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
    private Operation _operation;

    private T _value;

    public AtomicWithSpinLock(T value)
    {
        _value = value;
        _operation = new Operation(this);
    }

    public void Op(Action<Operation> del)
    {
        bool entered = false;
        try
        {
            _lock.Enter(ref entered);
            del(_operation);
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

    private SpinLock _lock = new(false);
}
