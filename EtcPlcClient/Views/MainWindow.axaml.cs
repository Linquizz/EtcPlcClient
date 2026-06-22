using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using EtcPlcClient.ViewModels;

namespace EtcPlcClient.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel Vm => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
                vm.PropertyChanged += OnVmPropertyChanged;
        };
    }

    private void OnVmPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(UpdateUi);
    }

    private void UpdateUi()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        bool ok = vm.StatusOk;

        var statusBadge = this.FindControl<Border>("StatusBadge");
        var statusDot = this.FindControl<Ellipse>("StatusDot");
        var statusText = this.FindControl<TextBlock>("StatusText");

        if (statusBadge != null)
        {
            statusBadge.Classes.Clear();
            statusBadge.Classes.Add(ok ? "badge-connected" : "badge-disconnected");
        }
        if (statusDot != null)
            statusDot.Fill = ok ? Brush.Parse("#00e676") : Brush.Parse("#ff3d3d");
        if (statusText != null)
            statusText.Foreground = ok ? Brush.Parse("#00e676") : Brush.Parse("#ff5252");

        UpdateDoRow(
            this.FindControl<Border>("Do1Row"),
            this.FindControl<Ellipse>("Do1Dot"),
            this.FindControl<Border>("Do1Track"),
            this.FindControl<Ellipse>("Do1Thumb"),
            vm.DO1);

        UpdateDoRow(
            this.FindControl<Border>("Do2Row"),
            this.FindControl<Ellipse>("Do2Dot"),
            this.FindControl<Border>("Do2Track"),
            this.FindControl<Ellipse>("Do2Thumb"),
            vm.DO2);

        UpdateDoRow(
            this.FindControl<Border>("Do3Row"),
            this.FindControl<Ellipse>("Do3Dot"),
            this.FindControl<Border>("Do3Track"),
            this.FindControl<Ellipse>("Do3Thumb"),
            vm.DO3);

        UpdateDoRow(
            this.FindControl<Border>("Do4Row"),
            this.FindControl<Ellipse>("Do4Dot"),
            this.FindControl<Border>("Do4Track"),
            this.FindControl<Ellipse>("Do4Thumb"),
            vm.DO4);

        var doActiveCount = this.FindControl<TextBlock>("DoActiveCount");
        if (doActiveCount != null)
            doActiveCount.Text = vm.ActiveDoCount.ToString();
    }

    private static void UpdateDoRow(
        Border? row,
        Ellipse? dot,
        Border? track,
        Ellipse? thumb,
        bool isOn)
    {
        if (row != null)
        {
            row.Classes.Clear();
            row.Classes.Add(isOn ? "ch-row-active" : "ch-row");
        }
        if (dot != null)
        {
            dot.Classes.Clear();
            dot.Classes.Add(isOn ? "dot-on" : "dot-off");
        }
        if (track != null)
        {
            track.Classes.Clear();
            track.Classes.Add(isOn ? "track-on" : "track-off");
        }
        if (thumb != null)
        {
            thumb.Classes.Clear();
            thumb.Classes.Add(isOn ? "thumb-on" : "thumb-off");
        }
    }

    private async void ConnectButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.ConnectAsync();
        }
        catch (Exception ex)
        {
            Vm.ErrorMessage = $"Непредвиденная ошибка: {ex.Message}";
        }
    }

    private void DisconnectButton_Click(object? sender, RoutedEventArgs e)
    {
        Vm.Disconnect();
    }

    private async void Do1Button_Click(object? sender, RoutedEventArgs e)
        => await Vm.ToggleDoAsync(1);

    private async void Do2Button_Click(object? sender, RoutedEventArgs e)
        => await Vm.ToggleDoAsync(2);

    private async void Do3Button_Click(object? sender, RoutedEventArgs e)
        => await Vm.ToggleDoAsync(3);

    private async void Do4Button_Click(object? sender, RoutedEventArgs e)
        => await Vm.ToggleDoAsync(4);

    private void DismissError_Click(object? sender, RoutedEventArgs e)
    {
        Vm.ErrorMessage = "";
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.Dispose();
        base.OnClosed(e);
    }
}