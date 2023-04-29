using iSchool.Organization.Domain.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 简易版 比例 '1:100'
    /// </summary>
    public readonly struct SmRatio
    {
        public readonly int Left;
        public readonly int Right;

        public SmRatio(int left, int right)
        {
            this.Left = left;
            this.Right = right;
        }

        public static SmRatio Parse(string ratioStr)
        {
            var str = ratioStr.AsSpan();
            var i = str.IndexOf(':');
            if (i == -1) throw new ArgumentException("is not ratio");
            return new SmRatio(int.Parse(str[..i]), int.Parse(str[(i + 1)..]));
        }

        public override string ToString()
        {
            return $"{Left}:{Right}";
        }

        public decimal RightToLeftValue(decimal rigthValue) => rigthValue / Right * Left;
        public decimal LeftToRightValue(decimal leftValue) => leftValue / Left * Right;

        public double RightToLeftValue(double rigthValue) => rigthValue / Right * Left;
        public double LeftToRightValue(double leftValue) => leftValue / Left * Right;

        
    }
}
