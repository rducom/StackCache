namespace StackCache.Core.Distributed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Serializers;
    using StackExchange.Redis;

    public static class RedisTypeHelper
    {
        private static readonly Type _byteArray = typeof(byte[]);
        private static readonly Type _string = typeof(string);
        private static readonly Type _int = typeof(int);
        private static readonly Type _double = typeof(double);
        private static readonly Type _bool = typeof(bool);
        private static readonly Type _long = typeof(long);

        public static RedisValue ToRedisValue<T>(this T value, ISerializer serializer)
        {
            Type type = typeof(T);
            if (type == _string)
                return value as string;
            if (type == _byteArray)
                return value as byte[];
            if (type == _int)
                return (int)(object)value;
            if (type == _double)
                return (double)(object)value;
            if (type == _bool)
                return (bool)(object)value;
            if (type == _long)
                return (long)(object)value;
            return serializer.Serialize(value);
        }

        public static T FromRedisValue<T>(this RedisValue value, ISerializer serializer)
        {
            Type type = typeof(T);
            if (type == _string)
                return (T)((object)(string)value);
            if (type == _byteArray)
                return (T)((object)(byte[])value);
            if (type == _int)
                return (T)((object)(int)value);
            if (type == _double)
                return (T)((object)(double)value);
            if (type == _bool)
                return (T)((object)(bool)value);
            if (type == _long)
                return (T)((object)(long)value);
            return serializer.Deserialize<T>(value);
        }

        public static IEnumerable<RedisValue> ToRedisValues<T>(this IEnumerable<T> values, ISerializer serializer)
        {
            Type type = typeof(T);
            if (type == _string)
                return values.OfType<string>().Cast<RedisValue>();
            if (type == _byteArray)
                return values.OfType<byte[]>().Cast<RedisValue>();
            if (type == _int)
                return values.OfType<int>().Cast<RedisValue>();
            if (type == _double)
                return values.OfType<double>().Cast<RedisValue>();
            if (type == _bool)
                return values.OfType<bool>().Cast<RedisValue>();
            if (type == _long)
                return values.OfType<long>().Cast<RedisValue>();
            return values.Select(serializer.Serialize).Cast<RedisValue>();
        }
          
        public static IEnumerable<T> FromRedisValue<T>(this RedisValue[] values, ISerializer serializer)
        {
            Type type = typeof(T);
            if (type == _string)
                return values.Select(v => (T)((object)(string)v));
            if (type == _byteArray)
                return values.Select(v => (T)((object)(byte[])v));
            if (type == _int)
                return values.Select(v => (T)((object)(int)v));
            if (type == _double)
                return values.Select(v => (T)((object)(double)v));
            if (type == _bool)
                return values.Select(v => (T)((object)(bool)v));
            if (type == _long)
                return values.Select(v => (T)((object)(long)v));

            return values.Select(i => serializer.Deserialize<T>(i));
        }
    }
}