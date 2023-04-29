using System;
using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool
{
    /// <summary>
    /// 简易版 数学区间 (a,b) [1,8) [4,5] (6,] [,9) (,)
    /// </summary>
    public readonly struct MathInterval
    {
        public MathInterval(double a, double b) : this(true, a, b, true) { }
        public MathInterval(bool i, double a, double b) : this(i, a, b, i) { }

        public MathInterval(bool ia, double a, double b, bool ib)
        {
            Ia = ia;
            Ib = ib;
            A = a;
            B = b;
            if (a > b) throw new InvalidOperationException("a must be less than b");
        }

        public readonly double A, B;
        public readonly bool Ia, Ib;

        public readonly bool Contains(in double v)
        {
            return Ia switch
            {
                true => v >= A,
                false => v > A,
            } && Ib switch
            {
                true => v <= B,
                false => v < B,
            };
        }

        public override string ToString()
        {
            return (Ia ? "[" : "(") + A.ToString() + ',' + B.ToString() + (Ib ? "]" : ")");
        }

        public static MathInterval Parse(string str)
        {
            return TryParse(str, out var v, out var ex) ? v : throw (ex ?? new InvalidCastException());
        }

        public static bool TryParse(string str, out MathInterval v)
        {
            return TryParse(str, out v, out _);
        }

        public static bool TryParse(string str, out MathInterval v, out Exception ex)
        {
            ex = null;
            v = default;
            //var g = Regex.Match(str, @"^(?<ia>[\(\[])(?<a>([\+\-]{0,1}\d+){0,1}),(?<b>[\+\-]{0,1}\d+)(?<ib>[\)\]])$").Groups;
            try
            {
                var cs = str == null ? default : str.AsSpan();
                if (cs.IsEmpty) return false;

                var ia = cs[0] switch
                {
                    '[' => true,
                    '(' => false,
                    _ => throw new InvalidCastException(nameof(Ia))
                };

                var j = cs.IndexOf(',');
                if (j < 1) return false;
                var c = cs[1..j].Trim();
                var a = c.IsEmpty ? double.NegativeInfinity
                    : double.TryParse(c, out var _a) ? _a : throw new InvalidCastException(nameof(A));

                if (j + 1 >= cs.Length) return false;
                c = cs[(j + 1)..^1].Trim();
                var b = c.IsEmpty ? double.PositiveInfinity
                    : double.TryParse(c, out var _b) ? _b : throw new InvalidCastException(nameof(B));

                var ib = cs[^1] switch
                {
                    ']' => true,
                    ')' => false,
                    _ => throw new InvalidCastException(nameof(Ib))
                };

                v = new MathInterval(ia, a, b, ib);
                return true;
            }
            catch (Exception ex0)
            {
                ex = ex0;
                return false;
            }
        }
    }
}
