using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.Events.Handlers;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using Player = Exiled.API.Features.Player;

namespace CiSpy.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SpyListCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("staff.spy"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        
        if (Entrypoint.EventHandlers.SpyList.Count == 0)
        {
            response = "There are no spies.";
            return false;
        }
        
        string msg = "Spies: ";

        List<Player> spyList = Entrypoint.EventHandlers.SpyList;

        foreach (var player in spyList)
        {
            if (player.SessionVariables["ShootedAsSpy"] is bool shooted && shooted)
            {
                switch (player.Role.Type)
                {
                    case RoleTypeId.ChaosConscript:
                        msg += $"\n - {player.Nickname} (Chaos)";
                        break;
                    case RoleTypeId.NtfSpecialist:
                        msg += $"\n - {player.Nickname} (NTF)";
                        break;
                }
            }
            else
            {
                switch (player.Role.Type)
                {
                    case RoleTypeId.ChaosRifleman:
                        msg += $"\n - {player.Nickname} (NTF)";
                        break;
                    case RoleTypeId.NtfPrivate:
                        msg += $"\n - {player.Nickname} (Chaos)";
                        break;
                }
            }
        }
        
        response = msg;
        return true;
    }

    public string Command { get; } = "spylist";
    public string[] Aliases { get; } = { };
    public string Description { get; } = "Get a list of players who are spies.";
}