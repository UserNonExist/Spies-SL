﻿using System;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using PlayerRoles;

namespace CiSpy.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
public class RevealCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!Player.TryGet(sender, out Player player))
        {
            response = "Can't get player instance.";
            return false;
        }
        
        if (player.SessionVariables["IsSpy"] is bool IsSpy && !IsSpy)
        {
            response = "You are not a spy.";
            return false;
        }
        
        player.SessionVariables["IsSpy"] = false;
        player.SessionVariables["Damagable"] = true;
        
        if (player.SessionVariables["ShootedAsSpy"] is bool ShootedAsSpy && !ShootedAsSpy)
        {
            switch (player.Role.Type)
            {
                case RoleTypeId.NtfPrivate:
                    player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.None);
                    break;
                case RoleTypeId.ChaosRifleman:
                    player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                    break;
            }
        }
        else
        {
            switch (player.Role.Type)
            {
                case RoleTypeId.ChaosRifleman:
                    player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.None);
                    break;
                case RoleTypeId.NtfSpecialist:
                    player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                    break;
            }
        }
        
        player.SessionVariables["ShootedAsSpy"] = false;
        
        player.IsSpawnProtected = false;
        player.ShowHint(Entrypoint.Instance.Translation.SpyRevealedMessage, 10f);
        Entrypoint.EventHandlers.SpyList.Remove(player);
        
        Timing.CallDelayed(0.2f, () =>
        {
            player.ClearBroadcasts();
        });
        
        response = "You have revealed yourself.";
        return true;
    }

    public string Command { get; } = "reveal";
    public string[] Aliases { get; } = { };
    public string Description { get; } = "Reveal the spy.";
}