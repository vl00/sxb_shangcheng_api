using iSchool.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Conventions
{
    public class ConvQueryStringValueProvider : QueryStringValueProvider
    {
        private readonly HashSet<string> _keys;
        private readonly IQueryCollection _values;
        private readonly RouteValueDictionary _routeValues;

        public ConvQueryStringValueProvider(IQueryCollection values, CultureInfo culture) : base(null, values, culture)
        {
        }

        public ConvQueryStringValueProvider(IEnumerable<string> keys, IQueryCollection values, RouteValueDictionary routeValues) : base(BindingSource.Query, values, CultureInfo.InvariantCulture)
        {
            _keys = new HashSet<string>(keys);
            _values = values;
            _routeValues = routeValues;

        }

        public override bool ContainsPrefix(string prefix)
        {
            return base.ContainsPrefix(prefix);
        }

        public override ValueProviderResult GetValue(string key)
        {
            var result = base.GetValue(key);
            if (_keys != null && !_keys.Contains(key))
                return result;

            string value = null;
            if (result != ValueProviderResult.None)
                value = result.FirstValue;
            else if (_routeValues.Keys.Contains(key))
                value = _routeValues[key].ToString();

            //#if !DEBUG
            //            //防止爬虫调用接口
            //            if (UrlShortIdUtil.IsNumeric(value))
            //            {
            //                throw new Infrastructure.CustomResponseException("id 数据类型不正确！");
            //            }
            //#endif


            //if (value != null && !UrlShortIdUtil.IsNumeric(value))
            if (value != null)
            {
                return new ValueProviderResult(UrlShortIdUtil.Base322Long(value).ToString(), result.Culture);
            }
            return result;
        }
    }
}
