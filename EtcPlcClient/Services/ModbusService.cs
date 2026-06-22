using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using EtcPlcClient.Models;
using FluentModbus;

namespace EtcPlcClient.Services;

public sealed class ModbusService : IModbusService, IDisposable
{
    private ModbusTcpClient? _client;
    private byte _unitId;
    private readonly object _lock = new();

    public bool IsConnected { get; private set; }

    public void Connect(string ip, int port, byte unitId)
    {
        lock (_lock)
        {
            DisconnectCore();

            _unitId = unitId;

            if (!IPAddress.TryParse(ip, out var ipAddress))
                throw new ArgumentException(
                    $"Некорректный IP-адрес: «{ip}».\nВведите адрес в формате 000.000.000.000, например 127.0.0.1");

            try
            {
                _client = new ModbusTcpClient();
                _client.Connect(new IPEndPoint(ipAddress, port));
            }
            catch (SocketException sex)
            {
                _client?.Dispose();
                _client = null;
                throw new InvalidOperationException(
                    $"Не удалось установить TCP-соединение с {ip}:{port}.\n" +
                    $"Убедитесь что сервер запущен и порт доступен.\n\nДетали: {sex.Message}");
            }
            catch (Exception ex)
            {
                _client?.Dispose();
                _client = null;
                throw new InvalidOperationException(
                    $"Не удалось установить TCP-соединение с {ip}:{port}.\n" +
                    $"Убедитесь что сервер запущен и порт доступен.\n\nДетали: {ex.Message}");
            }
            try
            {
                _client.ReadCoils(_unitId,
                    PlcMemoryMap.CoilStartAddress,
                    PlcMemoryMap.CoilCount);
            }
            catch (Exception ex)
            {
                _client?.Dispose();
                _client = null;
                throw new InvalidOperationException(
                    $"TCP-соединение с {ip}:{port} установлено, но ПЛК не ответил.\n" +
                    $"Проверьте Unit ID (введено: {unitId}) и настройки эмулятора.\n\n" +
                    $"Детали: {ex.Message}");
            }

            IsConnected = true;
        }
    }

    public void Disconnect()
    {
        lock (_lock)
        {
            DisconnectCore();
        }
    }

    private void DisconnectCore()
    {
        _client?.Dispose();
        _client = null;
        IsConnected = false;
    }

    public PlcState ReadAllInputs()
    {
        lock (_lock)
        {
            var state = new PlcState();
            ReadCoils(state);
            ReadHoldingRegisters(state);
            return state;
        }
    }

    public void WriteCoil(int address, bool value)
    {
        lock (_lock)
        {
            if (_client is null)
                throw new InvalidOperationException("Нет подключения к ПЛК.");
            _client.WriteSingleCoil(_unitId, address, value);
        }
    }

    private void ReadCoils(PlcState state)
    {
        if (_client is null) return;

        var coilData = _client.ReadCoils(
            _unitId,
            PlcMemoryMap.CoilStartAddress,
            PlcMemoryMap.CoilCount);

        if (coilData.Length == 0) return;

        byte coils = coilData[0];
        state.DO1 = (coils & 0b0001) != 0;
        state.DO2 = (coils & 0b0010) != 0;
        state.DO3 = (coils & 0b0100) != 0;
        state.DO4 = (coils & 0b1000) != 0;
    }

    private void ReadHoldingRegisters(PlcState state)
    {
        if (_client is null) return;

        var regData = _client.ReadHoldingRegisters<byte>(
            _unitId,
            PlcMemoryMap.HoldingStartAddress,
            PlcMemoryMap.HoldingRegisterCount);

        if (regData.Length < 32) return;

        var regs = MemoryMarshal.Cast<byte, short>(regData);

        if (regs.Length >= 4) state.AI1 = regs.GetBigEndian<double>(0);
        if (regs.Length >= 8) state.AI2 = regs.GetBigEndian<double>(4);
        if (regs.Length >= 12) state.AI3 = regs.GetBigEndian<double>(8);
        if (regs.Length >= 16) state.AI4 = regs.GetBigEndian<double>(12);
    }

    public void Dispose() => Disconnect();
}