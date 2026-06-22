using EtcPlcClient.Models;

namespace EtcPlcClient.Services;

public interface ISettingsService
{
    ConnectionSettings Load();
    void Save(ConnectionSettings settings);
}