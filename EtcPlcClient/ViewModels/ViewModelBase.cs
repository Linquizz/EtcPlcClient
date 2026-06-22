using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EtcPlcClient.ViewModels;

// Базовый класс для всех ViewModel — реализует INotifyPropertyChanged
// Это нужно, чтобы Avalonia автоматически обновляла UI при изменении свойств
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}