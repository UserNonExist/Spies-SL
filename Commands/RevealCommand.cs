using System;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using SpiesSl.Enums;

namespace SpiesSl.Commands;

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
        
        if (!Entrypoint.SpyHandlers.Spies.ContainsKey(player))
        {
            response = "You are not a spy.";
            return false;
        }

        float lastHealth = player.Health;

        switch (Entrypoint.SpyHandlers.Spies[player])
        {
            case SpyType.ChaosSpy:
                player.Role.Set(RoleTypeId.ChaosConscript, RoleSpawnFlags.None);
                player.Health = lastHealth;
                break;
            case SpyType.NtfSpy:
                player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                player.Health = lastHealth;
                break;
        }
        
        Timing.CallDelayed(0.1f, () =>
        {
            player.IsSpawnProtected = false;
        });
        
        Entrypoint.SpyHandlers.Spies.Remove(player);
        player.ShowHint(Entrypoint.Instance.Translation.SpyRevealedMessage, 7f);
        
        response = "You have revealed your spy role.";
        return true;
    }

    public string Command { get; } = "reveal";
    public string[] Aliases { get; } = { };
    public string Description { get; } = "Reveal your spy role.";
}