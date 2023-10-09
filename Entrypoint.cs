using System;
using Exiled.API.Features;
using Exiled.CustomItems;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Exiled.Events.Commands.Reload;
using MEC;
using SpiesSl.Handlers;
using SpiesSl.Items;

namespace SpiesSl;

public class Entrypoint : Plugin<Configs, Translations>
{
    public override string Author { get; } = "User_NotExist";
    public override string Name { get; } = "SpiesSl";
    public override Version Version { get; } = new Version(2, 1, 0);
    public override Version RequiredExiledVersion { get; } = new Version(8, 2, 1);
    
    public static Entrypoint Instance { get; private set; }
    public static EventHandlers EventHandlers;
    public static SpyHandlers SpyHandlers;

    public override void OnEnabled()
    {
        Instance = this;
        SpyHandlers = new SpyHandlers();
        EventHandlers = new EventHandlers();

        Timing.CallDelayed(5f, () =>
        {
            Config.Lj61E.Register();
        });
        
        Exiled.Events.Handlers.Server.RespawningTeam += EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnChangingRole;
        Exiled.Events.Handlers.Player.Verified += EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Left += EventHandlers.OnLeft;
        Exiled.Events.Handlers.Player.Dying += EventHandlers.OnDying;
        Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnHurting;
        Exiled.Events.Handlers.Player.Shooting += EventHandlers.OnShooting;
        Exiled.Events.Handlers.Player.ThrownProjectile += EventHandlers.OnThrownProjectile;
        Exiled.Events.Handlers.Player.Handcuffing += EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRestartingRound;
        
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Server.RespawningTeam -= EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnChangingRole;
        Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Left -= EventHandlers.OnLeft;
        Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnDying;
        Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnHurting;
        Exiled.Events.Handlers.Player.Shooting -= EventHandlers.OnShooting;
        Exiled.Events.Handlers.Player.ThrownProjectile -= EventHandlers.OnThrownProjectile;
        Exiled.Events.Handlers.Player.Handcuffing -= EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRestartingRound;

        Config.Lj61E.Unregister();
        
        EventHandlers = null;
        SpyHandlers = null;
        Instance = null;
        
        base.OnDisabled();
    }
}
