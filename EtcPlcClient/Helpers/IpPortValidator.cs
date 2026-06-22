using System.Net;
using System.Text.RegularExpressions;

namespace EtcPlcClient.Helpers;

public static class IpPortValidator
{
    public static bool IsValidIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;
        return IPAddress.TryParse(ip, out _);
    }

    public static bool IsValidPort(string? port)
    {
        return int.TryParse(port, out int p) && p is >= 1 and <= 65535;
    }

    public static bool IsValidUnitId(string? uid)
    {
        return byte.TryParse(uid, out _);
    }
}