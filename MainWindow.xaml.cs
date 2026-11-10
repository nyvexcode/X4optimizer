using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace X4Optimizer;

public partial class MainWindow : Window
{
    private static readonly Brush Ok = Hex("#3DDC97");
    private static readonly Brush Warn = Hex("#FFB547");
    private static readonly Brush Bad = Hex("#FF6B6B");
    private static readonly Brush Idle = Hex("#5B6272");

    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private bool busy;

    public MainWindow()
    {
        InitializeComponent();
        ChangesList.ItemsSource = RobloxPotato.Plan().Select(c => c.Display).ToList();
        timer.Tick += (_, _) => Refresh();
        timer.Start();
        Refresh();
    }

    private static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private static void SetRow(Ellipse dot, TextBlock text, Brush color, string value)
    {
        dot.Fill = color;
        text.Text = value;
    }

    private void Refresh()
    {
        var hasConfig = RobloxPotato.SettingsExist();
        var running = RobloxPotato.RunningRobloxProcesses() > 0;
        var hasBackup = RobloxPotato.HasBackup();

        SetRow(DotConfig, TxtConfig, hasConfig ? Ok : Bad,
            hasConfig ? "Configurações do Roblox encontradas"
                      : "Configurações não encontradas — abra o Roblox uma vez e feche");
        SetRow(DotRoblox, TxtRoblox, running ? Warn : Ok,
            running ? "Roblox está aberto" : "Roblox está fechado");
        SetRow(DotBackup, TxtBackup, hasBackup ? Ok : Idle,
            hasBackup ? "Backup do original guardado" : "Nenhum backup ainda");

        ApplyButton.IsEnabled = hasConfig && !busy;
        RestoreButton.IsEnabled = hasConfig && hasBackup && !busy;
    }

    private void ShowMessage(string text, Brush color)
    {
        Message.Text = text;
        Message.Foreground = color;
    }

    /// <summary>Garante que o Roblox esteja fechado. O cliente reescreve o arquivo ao fechar e desfaria a mudança.</summary>
    private async Task<bool> EnsureRobloxClosedAsync()
    {
        if (RobloxPotato.RunningRobloxProcesses() == 0) return true;

        if (AutoClose.IsChecked != true)
        {
            ShowMessage("Feche o Roblox primeiro (ou marque a opção de fechar automaticamente).", Warn);
            return false;
        }
        await Task.Run(RobloxPotato.CloseRoblox);
        return true;
    }

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        busy = true;
        Refresh();
        try
        {
            if (!await EnsureRobloxClosedAsync()) return;

            var (applied, missing) = await Task.Run(RobloxPotato.Apply);
            var text = $"Pronto! {applied.Count} configurações aplicadas.";
            if (missing.Count > 0)
                text += $" {missing.Count} não existem nesta versão do Roblox e foram ignoradas.";
            ShowMessage(text, Ok);
        }
        catch (Exception error)
        {
            ShowMessage($"Erro: {error.Message}", Bad);
        }
        finally
        {
            busy = false;
            Refresh();
        }
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        busy = true;
        Refresh();
        try
        {
            if (!await EnsureRobloxClosedAsync()) return;

            await Task.Run(RobloxPotato.Restore);
            ShowMessage("Configurações originais restauradas.", Ok);
        }
        catch (Exception error)
        {
            ShowMessage($"Erro: {error.Message}", Bad);
        }
        finally
        {
            busy = false;
            Refresh();
        }
    }
}
