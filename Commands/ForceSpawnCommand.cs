using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace SpiesSl.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ForceSpawnCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("spy.forcespawn"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        
        if (Entrypoint.EventHandlers.ForceSpawning)
        {
            Entrypoint.EventHandlers.ForceSpawning = false;
            response = "Spy will NOT guaranteed to spawn in the next wave.";
            return true;
        }
        
        Entrypoint.EventHandlers.ForceSpawning = true;
        response = "Spy will now GUARANTEED to spawn in the next wave.";
        return true;
    }

    public string Command { get; } = "forcespawnspy";
    public string[] Aliases { get; } = { "fss" };
    public string Description { get; } = "Force spawn a spy on the next spawnwave.";
}