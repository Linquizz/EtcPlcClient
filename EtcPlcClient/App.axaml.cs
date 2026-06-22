using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EtcPlcClient.Services;
using EtcPlcClient.ViewModels;
using EtcPlcClient.Views;

namespace EtcPlcClient;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var modbus = new ModbusService();
            var settings = new SettingsService();
            var vm = new MainWindowViewModel(modbus, settings);

            desktop.MainWindow = new MainWindow { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}