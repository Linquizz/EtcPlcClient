namespace EtcPlcClient.Models;

public class ConnectionSettings
{
    public string IpAddress { get; set; } = "";
    public int Port { get; set; } = 0;
    public byte UnitId { get; set; } = 0;
}