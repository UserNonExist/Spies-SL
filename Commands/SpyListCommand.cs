using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using SpiesSl.Enums;

namespace SpiesSl.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SpyListCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("spy.list"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        
        if (Entrypoint.SpyHandlers.Spies.Count == 0)
        {
            response = "There are no spies.";
            return true;
        }
        
        response = "Spies: ";
        
        foreach (KeyValuePair<Player, SpyType> player in Entrypoint.SpyHandlers.Spies)
        {
            response += "\n" + player.Key.Nickname + " - " + player.Value.ToString();
        }
        
        return true;
    }

    public string Command { get; } = "spylist";
    public string[] Aliases { get; } = Array.Empty<string>();
    public string Description { get; } = "List all spies.";
}