namespace EtcPlcClient.Models;

public class PlcState
{
    public bool DO1 { get; set; }
    public bool DO2 { get; set; }
    public bool DO3 { get; set; }
    public bool DO4 { get; set; }

    public double AI1 { get; set; }
    public double AI2 { get; set; }
    public double AI3 { get; set; }
    public double AI4 { get; set; }
}