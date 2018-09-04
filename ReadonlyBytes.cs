using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace hashes
{
    public class ReadonlyBytes : IEnumerable<byte>
    {
        private byte[] data;
        private int hash = -1; // -1 ::= uninitialized hash

        public ReadonlyBytes(params byte[] list)
        {
            if (list == null)
                throw new ArgumentNullException();
            data = new byte[list.Length];
            Array.Copy(list, data, list.Length);
        }

        public int Length { get => data.Length; }

        public byte this[int ix] { get => data[ix]; }

        public override int GetHashCode()
        {
            const long prime0 = 17;
            const long prime1 = 2147483029; // prime1 < 2147483648 == 2^31
            const long prime2 = 331; // 256 < prime2 < 512

            if (hash == -1) // lazy calculation
            {
                long t = prime0;
                foreach (var b in data)
                    t = (t * prime2 + b) % prime1;
                hash = (int)t; // 0 <= t < prime1 < 2^31
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (!GetHashCode().Equals(obj.GetHashCode())) return false;
            var that = obj as IEnumerable<byte>;
            if (that == null) return false;
            var a = GetEnumerator();
            var b = that.GetEnumerator();
            for (; ; )
            {
                bool hasA = a.MoveNext();
                bool hasB = b.MoveNext();
                if (hasA != hasB) return false;
                if (!hasA) return true;
                if (a.Current != b.Current) return false;
            }
        }

        public static ReadonlyBytes operator +(ReadonlyBytes a, ReadonlyBytes b)
        {
            var r = new ReadonlyBytes
            {
                data = new byte[a.Length + b.Length]
            };
            a.data.CopyTo(r.data, 0);
            b.data.CopyTo(r.data, a.Length);
            return r;
        }

        // шаманские пляски.. во славу сатаны, конечно.
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public virtual IEnumerator<byte> GetEnumerator() =>
            ((IEnumerable<byte>)data).GetEnumerator();

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("[");
            bool first = true;
            foreach (var b in data)
            {
                if (first) first = false;
                else s.Append(", ");
                s.Append(b);
            }
            s.Append("]");
            return s.ToString();
        }
    }
}
