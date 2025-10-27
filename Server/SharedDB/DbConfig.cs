using System;
using System.Collections.Generic;
using System.Text;

namespace SharedDB
{
    public static class DbConfig
    {
        public static string GameConnection =>
            Env("ConnectionStrings__GameConnection")
            ?? Env("GameConnection")
            ?? throw new InvalidOperationException("GameConnection이 설정되지 않았습니다.");

        public static string SharedConnection =>
            Env("ConnectionStrings__SharedConnection")
            ?? Env("SharedConnection")
            ?? throw new InvalidOperationException("SharedConnection이 설정되지 않았습니다.");

        static string Env(string key) => System.Environment.GetEnvironmentVariable(key);
    }
}
