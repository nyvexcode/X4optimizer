// x4 Optimizer - modo "gráfico de batata" para o Roblox.
//
// A lógica de edição do GlobalBasicSettings_13.xml é baseada no projeto 66mods
// (MIT License, Copyright (c) 2026 66mods (66 and slavr) and contributors).
// Veja LICENSE-66mods.txt.

using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace X4Optimizer;

/// <summary>Uma configuração que o modo batata altera.</summary>
public sealed record SettingChange(string Property, string Value, string Display);

public static class RobloxPotato
{
    private const string BackupExtension = ".x4backup";

    /// <summary>
    /// Arquivo de configurações gráficas do cliente oficial do Roblox.
    /// Todos os launchers (oficial, Bloxstrap, Fishstrap, Froststrap) usam este mesmo arquivo.
    /// </summary>
    public static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Roblox", "GlobalBasicSettings_13.xml");

    private static string BackupPath => SettingsPath + BackupExtension;

    /// <summary>Tudo que o modo batata escreve. A prévia e a escrita usam esta mesma lista.</summary>
    public static IReadOnlyList<SettingChange> Plan() =>
    [
        new("GraphicsQualityLevel", "1", "Qualidade gráfica: 1 de 10"),
        new("SavedQualityLevel", "1", "Qualidade salva: 1"),
        new("MaxQualityEnabled", "false", "Qualidade máxima: desligada"),
        new("VignetteEnabled", "false", "Vinheta: desligada"),
        new("VignetteEnabledCustomOption", "false", "Vinheta (custom): desligada"),
        new("PerformanceStatsVisible", "false", "Overlay de desempenho: desligado"),
        new("OnScreenProfilerEnabled", "false", "Profiler na tela: desligado"),
        new("MicroProfilerWebServerEnabled", "false", "Servidor do MicroProfiler: desligado"),
        new("ReducedMotion", "true", "Movimento reduzido: ligado"),
        new("PlayerNamesEnabled", "false", "Nomes dos jogadores: desligado"),
        new("PlayerListVisible", "false", "Lista de jogadores: desligada"),
        new("BadgeVisible", "false", "Pop-ups de badge: desligados"),
    ];

    public static bool SettingsExist() => File.Exists(SettingsPath);
    public static bool HasBackup() => File.Exists(BackupPath);

    public static int RunningRobloxProcesses() =>
        ProcessNames.Sum(name =>
        {
            var found = Process.GetProcessesByName(name);
            foreach (var p in found) p.Dispose();
            return found.Length;
        });

    private static readonly string[] ProcessNames = ["RobloxPlayerBeta", "RobloxCrashHandler"];

    /// <summary>Fecha o cliente do Roblox. O Roblox reescreve o arquivo ao fechar, então precisa estar fechado antes de aplicar.</summary>
    public static void CloseRoblox()
    {
        foreach (var name in ProcessNames)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(5000);
                    }
                    catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception)
                    {
                        // Já fechou ou está protegido.
                    }
                }
            }
        }
    }

    /// <summary>Aplica o modo batata. Retorna as configurações aplicadas e as que não existem nesta versão do cliente.</summary>
    public static (IReadOnlyList<SettingChange> Applied, IReadOnlyList<SettingChange> Missing) Apply()
    {
        var original = File.ReadAllBytes(SettingsPath);

        // Só cria o backup uma vez: ele deve guardar o estado ORIGINAL do jogador,
        // mesmo que o modo batata seja aplicado várias vezes.
        if (!File.Exists(BackupPath))
            File.WriteAllBytes(BackupPath, original);

        var document = XDocument.Parse(Encoding.UTF8.GetString(original), LoadOptions.PreserveWhitespace);
        var applied = new List<SettingChange>();
        var missing = new List<SettingChange>();

        foreach (var change in Plan())
        {
            // Só altera elementos que já existem: o tipo do elemento é decisão do cliente.
            var node = document.Descendants().FirstOrDefault(x => (string?)x.Attribute("name") == change.Property);
            if (node is null) { missing.Add(change); continue; }
            node.Value = change.Value;
            applied.Add(change);
        }

        WriteAtomic(SettingsPath, new UTF8Encoding(false).GetBytes(document.ToString(SaveOptions.DisableFormatting)));
        return (applied, missing);
    }

    /// <summary>Restaura as configurações originais guardadas no backup.</summary>
    public static void Restore()
    {
        if (!File.Exists(BackupPath))
            throw new FileNotFoundException("Nenhum backup encontrado. O modo batata ainda não foi aplicado.");
        WriteAtomic(SettingsPath, File.ReadAllBytes(BackupPath));
        File.Delete(BackupPath);
    }

    private static void WriteAtomic(string path, byte[] bytes)
    {
        var directory = Path.GetDirectoryName(path)!;
        var temporary = Path.Combine(directory, $".{Path.GetFileName(path)}.x4.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
