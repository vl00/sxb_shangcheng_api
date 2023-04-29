using iSchool.Organization.Domain.Security;
using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ViewModels
{
    public class NameCodeDto
    {
        /// <summary>用于显示的名称</summary>
        public string Name { get; set; } = default!;
        /// <summary>用于数据的code|id等</summary>
        public string Code { get; set; } = default!;
    }

    public class NameCodeDto<T>
    {
        /// <summary>用于显示的名称</summary>
        public string Name { get; set; } = default!;
        /// <summary>用于数据的code|id等</summary>
        public T Code { get; set; } = default!;
    }
}
#nullable disable