using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using SpiesSl.Enums;

namespace SpiesSl.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SpawnSpyCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("spy.spawn"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        
        if (arguments.Count != 2)
        {
            response = "Usage: spawnspy <player_id> <team>";
            return false;
        }
        
        if (!int.TryParse(arguments.At(0), out int playerId))
        {
            response = "Invalid player id.";
            return false;
        }
        
        Player player = Player.Get(playerId);
        
        if (player == null)
        {
            response = "Player not found.";
            return false;
        }
        
        if (typeof(SpyType).GetEnumValues().Length <= int.Parse(arguments.At(1)))
        {
            response = "Invalid team id.";
            return false;
        }
        
        SpyType team = (SpyType) int.Parse(arguments.At(1));
        
        switch (team)
        {
            case SpyType.ChaosSpy:
                Entrypoint.SpyHandlers.SpawnChaosSpy(player);
                break;
            case SpyType.NtfSpy:
                Entrypoint.SpyHandlers.SpawnNtfSpy(player);
                break;
        }
        
        response = "Spawned " + player.Nickname + " as a spy.";
        return true;
    }

    public string Command { get; } = "spawnspy";
    public string[] Aliases { get; } = { "ss" };
    public string Description { get; } = "Spawn a spy. Usage: spawnspy <player_id> <team> (0 = Chaos, 1 = NTF)";
}