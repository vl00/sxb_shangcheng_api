//using Microsoft.Data.SqlClient;
//using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace iSchool
{
    /// <summary>
    /// 经纬度
    /// </summary>
    public sealed partial class LngLatLocation
    {
        public LngLatLocation(double lng, double lat, int srid = 4326)
        {
            Lng = lng;
            Lat = lat;
            SRID = srid;
        }

        public double Lng { get; set; }
        public double Lat { get; set; }
        public int SRID { get; set; }

        public static void Init_With_Dapper()
        {            
            Dapper.SqlMapper.AddTypeHandler(typeof(LngLatLocation), new LngLatLocationTypeHandler1());

            ///
            /// 2020.07.29
            /// 经调试发现, sqlclient读取Udt类型的数据表字段时会强制?使用 Microsoft.SqlServer.Types.dll !!?, 如没找到此dll, 会报错.
            /// 但Microsoft.SqlServer.Types.dll没netcore版本,直接引用会使项目出现黄色警告线... 所以这里用动态加载.
            ///

            if (Type.GetType("Microsoft.SqlServer.Types.SqlGeography,Microsoft.SqlServer.Types") == null)
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(typeof(LngLatLocation).Assembly.ManifestModule.FullyQualifiedName), "Microsoft.SqlServer.Types.dll"));
        }
    }

    public class LngLatLocationTypeHandler1 : Dapper.SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            /// dapper2.x 貌似使用 System.Data.SqlClient.dll
            if (parameter is SqlParameter sqlParameter)
            {
                sqlParameter.SqlDbType = SqlDbType.Udt;
                sqlParameter.UdtTypeName = "GEOGRAPHY";
                parameter.Value = value == null ? (object)DBNull.Value :
                    value == DBNull.Value ? value :
                    value is LngLatLocation location ? (new SqlServerBytesWriter { IsGeography = true }).Write(new Point(location.Lng, location.Lat) { SRID = location.SRID }) :
                    throw new NotSupportedException();
            }
        }

        public object Parse(Type destinationType, object value)
        {
            if (value == null || value is DBNull) return null;
            if (destinationType == typeof(LngLatLocation))
            {
                // select语句查询`(geography类型字段).Serialize()`
                if (value is byte[] bys)
                {
                    var p = (Point)(new SqlServerBytesReader { IsGeography = true }).Read(bys);
                    return new LngLatLocation(p.X, p.Y, p.SRID);
                }

                // select语句直接查询geography类型字段
                dynamic sqlGeography = value;
                return sqlGeography == null ? null : new LngLatLocation(sqlGeography.Long.Value, sqlGeography.Lat.Value, sqlGeography.STSrid.Value);
            }
            throw new NotSupportedException();
        }
    }

    public sealed partial class LngLatLocation
    {
        #region DistanceByLiejia
        /// <summary>
        /// 2个经纬度的距离 - 烈嘉算法
        /// </summary>
        /// <param name="other"></param>
        /// <returns>单位：米</returns>
        public double DistanceByLiejia(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            if (this.SRID != 4326)
            {
                throw new InvalidOperationException("srid is not 4326");
            }

            double rad(double d)
            {
                return d * Math.PI / 180.0;
            }

            double radLat1 = rad(this.Lat);
            double radLat2 = rad(other.Lat);
            double a = radLat1 - radLat2;
            double b = rad(this.Lng) - rad(other.Lng);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * SridList.GetEllipsoidParameters(this.SRID).semi_major;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        #endregion DistanceByLiejia

        #region DotSpatial算法
        /// <summary>
        /// 2个经纬度的距离 - DotSpatial算法
        /// <br/>modify from https://github.com/ststeiger/DotSpatial
        /// <br/>较接近sqlserver的算法
        /// </summary>
        /// <returns>单位：米</returns>
        public static double DistanceBySpatial(double lng1, double lat1, double lng2, double lat2)
        {
            
        }

        public double DistanceBySpatial(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            if (this.SRID != 4326)
            {
                throw new InvalidOperationException("srid is not 4326");
            }
            return DistanceBySpatial(this.Lng, this.Lat, other.Lng, other.Lat);
        }
        #endregion DotSpatial算法
    }

    public sealed partial class LngLatLocation
    {
        /// <summary>
        /// 2个经纬度的距离 - sqlserver内部算法
        /// </summary>
        /// <param name="other"></param>
        /// <returns>单位：米</returns>
        [Obsolete("没找到支持linux的SqlServerSpatialXXX.dll")]
        public double DistanceBySql(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            return GeodeticPointDistance(new Point(this.Lat, this.Lng), new Point(other.Lat, other.Lng), SridList.GetEllipsoidParameters(this.SRID));
        }

        

        

     
        

        
    }
}
