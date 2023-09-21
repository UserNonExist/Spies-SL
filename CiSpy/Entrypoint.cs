﻿using System;
using Exiled.API.Features;

namespace CiSpy;

public class Entrypoint : Plugin<Config, Translation>
{
    public override string Author { get; } = "User_NotExist";
    public override string Name { get; } = "CiSpy";
    public override Version Version { get; } = new Version(1, 0, 0);
    public override Version RequiredExiledVersion { get; } = new Version(8, 2, 1);
    
    public static Entrypoint Instance { get; private set; }
    public static EventHandlers EventHandlers;

    public override void OnEnabled()
    {
        Instance = this;
        EventHandlers = new EventHandlers();

        Exiled.Events.Handlers.Player.Verified += EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Dying += EventHandlers.OnDying;
        Exiled.Events.Handlers.Server.RespawningTeam += EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnHurting;
        Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;
        Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStarted;
        Exiled.Events.Handlers.Player.Handcuffing += EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Player.Shooting += EventHandlers.OnShooting;
    }

    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnDying;
        Exiled.Events.Handlers.Server.RespawningTeam -= EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnHurting;
        Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;
        Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStarted;
        Exiled.Events.Handlers.Player.Handcuffing -= EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Player.Shooting -= EventHandlers.OnShooting;
        
        Instance = null;
        EventHandlers = null;
        
    }
}