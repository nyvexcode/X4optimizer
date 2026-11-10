using X4Optimizer;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== x4 Optimizer · Roblox Batata ===\n");

if (!RobloxPotato.SettingsExist())
{
    Console.WriteLine("Não encontrei o arquivo de configurações do Roblox:");
    Console.WriteLine(RobloxPotato.SettingsPath);
    Console.WriteLine("\nAbra o Roblox uma vez, feche, e rode o x4 Optimizer de novo.");
    return 1;
}

// Uso: x4Optimizer [apply|restore]. Sem argumento, abre o menu.
var choice = args.Length > 0 ? args[0].ToLowerInvariant() : null;
if (choice is null)
{
    Console.WriteLine("1) Aplicar gráfico de batata");
    Console.WriteLine("2) Restaurar configurações originais");
    Console.WriteLine("3) Sair");
    Console.Write("\nEscolha: ");
    choice = Console.ReadLine()?.Trim() switch { "1" => "apply", "2" => "restore", _ => "exit" };
}

try
{
    switch (choice)
    {
        case "apply": return Apply();
        case "restore": return Restore();
        default: return 0;
    }
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Xml.XmlException or FileNotFoundException)
{
    Console.WriteLine($"\nErro: {e.Message}");
    return 1;
}

static bool EnsureRobloxClosed()
{
    var running = RobloxPotato.RunningRobloxProcesses();
    if (running == 0) return true;

    Console.WriteLine("O Roblox está aberto. Ele reescreve as configurações ao fechar, então precisa ser fechado antes.");
    Console.Write("Fechar o Roblox agora? (s/n): ");
    if (!string.Equals(Console.ReadLine()?.Trim(), "s", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Cancelado. Feche o Roblox e tente de novo.");
        return false;
    }
    RobloxPotato.CloseRoblox();
    return true;
}

static int Apply()
{
    Console.WriteLine("\nO modo batata vai aplicar:");
    foreach (var change in RobloxPotato.Plan()) Console.WriteLine($"  • {change.Display}");
    Console.Write("\nContinuar? (s/n): ");
    if (!string.Equals(Console.ReadLine()?.Trim(), "s", StringComparison.OrdinalIgnoreCase)) return 0;

    if (!EnsureRobloxClosed()) return 1;

    var (applied, missing) = RobloxPotato.Apply();
    Console.WriteLine($"\nPronto! {applied.Count} configurações aplicadas.");
    if (missing.Count > 0)
        Console.WriteLine($"{missing.Count} não existem nesta versão do Roblox e foram ignoradas.");
    Console.WriteLine("Backup do original guardado. Use a opção 2 para voltar ao normal.");
    return 0;
}

static int Restore()
{
    if (!RobloxPotato.HasBackup())
    {
        Console.WriteLine("\nNão há backup: o modo batata ainda não foi aplicado.");
        return 1;
    }
    if (!EnsureRobloxClosed()) return 1;

    RobloxPotato.Restore();
    Console.WriteLine("\nConfigurações originais restauradas.");
    return 0;
}
