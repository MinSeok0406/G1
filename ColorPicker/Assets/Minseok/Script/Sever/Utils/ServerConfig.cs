using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerConfig
{
    public static string Scheme { get; private set; } = "http";
    public static string Host { get; private set; } = "api.colorpicker";
    public static int Port { get; private set; } = 51000;
    public static string BasePath { get; private set; } = "/api/"; // 반드시 슬래시로 감싸진 형태

    public static void Configure(string scheme, string host, int port, string basePath = "/api/")
    {
        Scheme = string.IsNullOrWhiteSpace(scheme) ? "http" : scheme.Trim();
        Host = host?.Trim() ?? "localhost";
        Port = port;
        BasePath = NormalizePath(basePath);
    }

    public static Uri BaseUri => new Uri($"{Scheme}://{Host}:{Port}{BasePath}");

    /// <summary>상대경로를 서버 BaseUri와 합쳐 절대 URL 생성. 이미 절대 URL이면 그대로 반환.</summary>
    public static string Combine(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            return BaseUri.ToString();

        var p = pathOrUrl.Trim();
        if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return p; // 이미 절대 URL

        p = p.TrimStart('/');
        return new Uri(BaseUri, p).ToString();
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/";
        path = path.Trim();
        if (!path.StartsWith("/")) path = "/" + path;
        if (!path.EndsWith("/")) path += "/";
        return path;
    }

    // GameServer fallback (ServerList 비었을 때 사용)
    public const string GameHostFallback = "api.colorpicker";
    public const int GamePortFallback = 50000;
}
