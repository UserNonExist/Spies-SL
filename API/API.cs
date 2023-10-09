using Exiled.API.Features;
using SpiesSl.Enums;

namespace SpiesSl.API;

public class API
{
    public static bool IsSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a spy.");
        else
            Log.Debug($"[SPY] checking if {player.Nickname} is a spy.");

        return Entrypoint.SpyHandlers.IsSpy(player);
    }
    
    public static bool IsNtfSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a NTF spy.");
        else
            Log.Debug($"[SPY] checking if {player.Nickname} is a NTF spy.");

        return Entrypoint.SpyHandlers.GetSpyType(player) == SpyType.NtfSpy;
    }
    
    public static bool IsChaosSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a Chaos spy.");
        else
            Log.Debug($"[SPY] checking if {player.Nickname} is a Chaos spy.");

        return Entrypoint.SpyHandlers.GetSpyType(player) == SpyType.ChaosSpy;
    }
    
    public static void SpawnNtfSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is spawning {player.Nickname} as a NTF spy.");
        else
            Log.Debug($"[SPY] spawning {player.Nickname} as a NTF spy.");
        
        Entrypoint.SpyHandlers.SpawnNtfSpy(player);
    }
    
    public static void SpawnChaosSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is spawning {player.Nickname} as a Chaos spy.");
        else
            Log.Debug($"[SPY] spawning {player.Nickname} as a Chaos spy.");
        
        Entrypoint.SpyHandlers.SpawnChaosSpy(player);
    }
}