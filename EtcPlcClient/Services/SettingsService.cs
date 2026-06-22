using System;
using System.IO;
using System.Text.Json;
using EtcPlcClient.Models;

namespace EtcPlcClient.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "connection_settings.json");

    public ConnectionSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new ConnectionSettings();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<ConnectionSettings>(json)
                   ?? new ConnectionSettings();
        }
        catch
        {
            return new ConnectionSettings();
        }
    }

    public void Save(ConnectionSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {

        }
    }
}