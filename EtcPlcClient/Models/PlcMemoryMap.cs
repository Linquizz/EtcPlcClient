namespace EtcPlcClient.Models;

public static class PlcMemoryMap
{
    public const string DefaultIPAddress = "127.0.0.1";
    public const int DefaultPort = 1502;
    public const byte DefaultUnitID = 1;

    public const int CoilStartAddress = 0;
    public const int CoilCount = 4;

    public const int HoldingStartAddress = 0;
    public const int HoldingRegisterCount = 32;
}