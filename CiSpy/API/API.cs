using System.Collections.Generic;
using Exiled.API.Features;

namespace CiSpy.API;

public class API
{
    public static bool IsSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a spy.");
        else
            Log.Debug($"A plugin is checking if {player.Nickname} is a spy.");
        
        
        if (Entrypoint.EventHandlers.SpyList.Contains(player))
            return true;
        return false;
    }
    
    public static bool IsNtfSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a NTF spy.");
        else
            Log.Debug($"A plugin is checking if {player.Nickname} is a NTF spy.");
        
        if (Entrypoint.EventHandlers.NtfSpyList.Contains(player))
            return true;
        return false;
    }
    
    public static bool IsChaosSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is checking if {player.Nickname} is a Chaos spy.");
        else
            Log.Debug($"A plugin is checking if {player.Nickname} is a Chaos spy.");
        
        if (Entrypoint.EventHandlers.ChaosSpyList.Contains(player))
            return true;
        return false;
    }

    public static void SpawnNtfSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is spawning {player.Nickname} as a NTF spy.");
        else
            Log.Debug($"A plugin is spawning {player.Nickname} as a NTF spy.");
        
        Entrypoint.EventHandlers.SpawnNtfSpy(player);
    }
    
    public static void SpawnChaosSpy(Player player, string pluginName = null)
    {
        if (pluginName != null)
            Log.Debug($"{pluginName} is spawning {player.Nickname} as a Chaos spy.");
        else
            Log.Debug($"A plugin is spawning {player.Nickname} as a Chaos spy.");
        
        Entrypoint.EventHandlers.SpawnChaosSpy(player);
    }
}