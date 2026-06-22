using EtcPlcClient.Models;

namespace EtcPlcClient.Services;

public interface IModbusService
{
    bool IsConnected { get; }

    void Connect(string ip, int port, byte unitId);
    void Disconnect();

    PlcState ReadAllInputs();
    void WriteCoil(int address, bool value);
}