using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace iSchool.Infrastructure
{
    public static class ReflectHelper
    {
        //private static ConcurrentDictionary<string, IEnumerable<PropertyInfo>> keyValuePairs = new ConcurrentDictionary<string, IEnumerable<PropertyInfo>>();
        //private static int maxCount = 1000;

        //public static IEnumerable<PropertyInfo> GetProperties<T>(string[] exclundPropertyNames = null)
        //{
        //    var key = nameof(T);
        //    if (keyValuePairs.ContainsKey(key))
        //    {
        //        return keyValuePairs[key];
        //    }


        //    var type = typeof(T);
        //    var properties = type.GetProperties();
        //    List<PropertyInfo> filter = properties.ToList();
        //    if (exclundPropertyNames != null)
        //    {
        //        filter = new List<PropertyInfo>();
        //        foreach (var property in properties)
        //        {
        //            if (!exclundPropertyNames.Contains(property.Name))
        //            {
        //                filter.Add(property);
        //            }
        //        }
        //    }

        //    if (keyValuePairs.Count >= maxCount)
        //    {
        //        //remove
        //    }
        //    keyValuePairs.AddOrUpdate(key, filter, (key, value) => filter);
        //    return filter;
        //}


        /// <summary>
        /// mapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination MapperProperty<TSource, TDestination>(TSource source)
            where TSource : class
            where TDestination : class
        {
            if (source == null)
                return default;

            //create instance
            TDestination destination = Activator.CreateInstance<TDestination>();

            //type
            Type sourceType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            //properties
            var sourceProperties = sourceType.GetProperties();
            var destinationProperties = destinationType.GetProperties();

            //mapper
            foreach (var destinationProp in destinationProperties)
            {
                if (!destinationProp.CanWrite)
                    continue;

                var sourceProp = sourceProperties.FirstOrDefault(s => s.Name == destinationProp.Name && s.PropertyType == destinationProp.PropertyType);
                if (sourceProp != null && sourceProp.CanRead)
                {
                    destinationProp.SetValue(destination, sourceProp.GetValue(source));
                }
            }
            return destination;
        }

        public static IEnumerable<TDestination> MapperProperty<TSource, TDestination>(IEnumerable<TSource> sources)
            where TSource : class
            where TDestination : class
        {
            foreach (var item in sources)
            {
                yield return MapperProperty<TSource, TDestination>(item);
            }
        }
    }
}
