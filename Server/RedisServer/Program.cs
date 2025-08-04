using System;
using StackExchange.Redis;

namespace RedisServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string redisConnectionString = "localhost:3246"; // Redis 서버 주소

            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase db = redis.GetDatabase();

            string value = db.StringGet("Minseok");
            if (value == null)
            {
                Console.WriteLine("키 'Minseok'가 존재하지 않습니다.");
            }
            else
            {
                Console.WriteLine($"키 'Minseok'의 값: {value}");
            }
        }
    }
}
