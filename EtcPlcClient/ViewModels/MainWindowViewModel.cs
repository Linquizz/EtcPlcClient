using System;
using System.Threading;
using System.Threading.Tasks;
using EtcPlcClient.Helpers;
using EtcPlcClient.Models;
using EtcPlcClient.Services;

namespace EtcPlcClient.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IModbusService _modbus;
    private readonly ISettingsService _settings;
    private CancellationTokenSource? _pollCts;


    private string _ipAddress = "";
    private string _port = "";
    private string _unitId = "";

    public string IpAddress
    {
        get => _ipAddress;
        set { SetField(ref _ipAddress, value); ValidateInputs(); }
    }

    public string Port
    {
        get => _port;
        set { SetField(ref _port, value); ValidateInputs(); }
    }

    public string UnitId
    {
        get => _unitId;
        set { SetField(ref _unitId, value); ValidateInputs(); }
    }

    private bool _canConnect;
    private bool _isConnecting;
    private bool _isConnected;
    private string _statusText = "Нет соединения";
    private bool _statusOk;

    public bool CanConnect
    {
        get => _canConnect;
        private set => SetField(ref _canConnect, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            SetField(ref _isConnected, value);
            OnPropertyChanged(nameof(IsDisconnected));
        }
    }
    public bool IsDisconnected => !_isConnected;

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public bool StatusOk
    {
        get => _statusOk;
        private set => SetField(ref _statusOk, value);
    }

    private bool _do1, _do2, _do3, _do4;

    public bool DO1 { get => _do1; private set => SetField(ref _do1, value); }
    public bool DO2 { get => _do2; private set => SetField(ref _do2, value); }
    public bool DO3 { get => _do3; private set => SetField(ref _do3, value); }
    public bool DO4 { get => _do4; private set => SetField(ref _do4, value); }

    public int ActiveDoCount =>
        (_do1 ? 1 : 0) + (_do2 ? 1 : 0) + (_do3 ? 1 : 0) + (_do4 ? 1 : 0);

    private string _ai1 = "—", _ai2 = "—", _ai3 = "—", _ai4 = "—";

    public string AI1 { get => _ai1; private set => SetField(ref _ai1, value); }
    public string AI2 { get => _ai2; private set => SetField(ref _ai2, value); }
    public string AI3 { get => _ai3; private set => SetField(ref _ai3, value); }
    public string AI4 { get => _ai4; private set => SetField(ref _ai4, value); }

    private string _errorMessage = "";

    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetField(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public MainWindowViewModel(IModbusService modbus, ISettingsService settings)
    {
        _modbus = modbus;
        _settings = settings;

        var saved = _settings.Load();
        _ipAddress = saved.IpAddress;
        _port = saved.Port > 0 ? saved.Port.ToString() : "";
        _unitId = saved.UnitId > 0 ? saved.UnitId.ToString() : "";

        ValidateInputs();
    }

    public async Task ConnectAsync()
    {
        if (_isConnecting) return;

        ErrorMessage = "";
        _isConnecting = true;
        StatusText = "Подключение...";
        ValidateInputs();

        try
        {
            byte uid = byte.Parse(UnitId);
            int p = int.Parse(Port);

            await Task.Run(() => _modbus.Connect(IpAddress, p, uid));

            _settings.Save(new ConnectionSettings
            {
                IpAddress = IpAddress,
                Port = p,
                UnitId = uid
            });

            IsConnected = true;
            StatusText = $"Подключено · {IpAddress}:{p}";
            StatusOk = true;

            _pollCts = new CancellationTokenSource();
            _ = PollLoopAsync(_pollCts.Token);
        }
        catch (Exception ex)
        {
            IsConnected = false;
            StatusText = "Нет соединения";
            StatusOk = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isConnecting = false;
            ValidateInputs();
        }
    }

    public void Disconnect()
    {
        _pollCts?.Cancel();
        _pollCts = null;

        _modbus.Disconnect();
        IsConnected = false;
        StatusText = "Нет соединения";
        StatusOk = false;

        DO1 = DO2 = DO3 = DO4 = false;
        AI1 = AI2 = AI3 = AI4 = "—";
        OnPropertyChanged(nameof(ActiveDoCount));

        ValidateInputs();
    }

    public async Task ToggleDoAsync(int channel)
    {
        if (!IsConnected) return;

        bool current = channel switch
        {
            1 => DO1,
            2 => DO2,
            3 => DO3,
            4 => DO4,
            _ => false
        };

        try
        {
            await Task.Run(() => _modbus.WriteCoil(channel - 1, !current));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка записи DO{channel}: {ex.Message}";
        }
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var state = await Task.Run(() => _modbus.ReadAllInputs(), ct);
                ApplyState(state);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Потеря связи: {ex.Message}";
                Disconnect();
                break;
            }

            await Task.Delay(400, ct).ConfigureAwait(false);
        }
    }

    private void ApplyState(PlcState state)
    {
        DO1 = state.DO1;
        DO2 = state.DO2;
        DO3 = state.DO3;
        DO4 = state.DO4;

        AI1 = $"{state.AI1:F2}";
        AI2 = $"{state.AI2:F2}";
        AI3 = $"{state.AI3:F2}";
        AI4 = $"{state.AI4:F2}";

        OnPropertyChanged(nameof(ActiveDoCount));
    }

    private void ValidateInputs()
    {
        bool valid = IpPortValidator.IsValidIp(_ipAddress)
                  && IpPortValidator.IsValidPort(_port)
                  && IpPortValidator.IsValidUnitId(_unitId);

        CanConnect = valid && !IsConnected && !_isConnecting;
    }

    public void Dispose()
    {
        _pollCts?.Cancel();
        _modbus.Disconnect();
    }
}