﻿using System.Drawing;
using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;

namespace CSSharpUtils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="CCSPlayerController"/> to perform common player actions.
/// </summary>
public static class PlayerControllerExtensions
{
    public enum FadeFlags
    {
        FADE_IN,
        FADE_OUT,
        FADE_STAYOUT
    }

    /// <summary>
    /// Freezes the player, preventing them from moving.
    /// </summary>
    /// <param name="playerController">The player controller to freeze.</param>
    public static void Freeze(this CCSPlayerController? playerController)
    {
        if (!playerController.IsPlayer())
            return;

        playerController!.PlayerPawn.Value!.Freeze();
    }
    
    /// <summary>
    /// Unfreezes the player, allowing them to move.
    /// </summary>
    /// <param name="playerController">The player controller to unfreeze.</param>
    public static void Unfreeze(this CCSPlayerController? playerController)
    {
        if (!playerController.IsPlayer())
            return;

        playerController!.PlayerPawn.Value!.Unfreeze();
    }

    /// <summary>
    /// Renames the player with a specified name and clan tag (optional).
    /// </summary>
    /// <param name="playerController">The player controller to set the name for.</param>
    /// <param name="name">The new name for the player.</param>
    public static void SetName(this CCSPlayerController? playerController, string name)
    {
        if (!playerController.IsPlayer())
            return;
        
        if (name == playerController!.PlayerName)
            return;
        
        playerController.PlayerName = name;
        Utilities.SetStateChanged(playerController, "CBasePlayerController", "m_iszPlayerName");
    }
    
    /// <summary>
    /// Renames the player with a specified name and clan tag (optional).
    /// </summary>
    /// <param name="playerController">The player controller to set the clantag for.</param>
    /// <param name="clantag">The new clan tag for the player.</param>
    /// <remarks>
    /// </remarks>
    public static void SetClantag(this CCSPlayerController? playerController, string clantag = "")
    {
        if (!playerController.IsPlayer())
            return;
        
        if (clantag == playerController!.Clan)
            return;
        
        playerController.Clan = clantag;
        Utilities.SetStateChanged(playerController, "CCSPlayerController", "m_szClan");
        
        var fakeEvent = new EventNextlevelChanged(false);
        fakeEvent.FireEventToClient(playerController);
    }
    
    /// <summary>
    /// Moves the player to a specified team.
    /// </summary>
    /// <param name="playerController">The player controller to move.</param>
    /// <param name="team">The team to move the player to.</param>
    public static void MoveToTeam(this CCSPlayerController? playerController, CsTeam team)
    {
        if (!playerController.IsPlayer() || playerController!.TeamNum == (byte)team)
            return;

        // Queue for next frame to avoid threading issues
        Server.NextFrame(() => { playerController.ChangeTeam(team); });
    }

    /// <summary>
    /// Gets the eye position of the player.
    /// </summary>
    /// <param name="playerController">The player controller to get the eye position for.</param>
    /// <returns>The eye position as a <see cref="Vector"/>.</returns>
    public static Vector GetEyePosition(this CCSPlayerController? playerController)
    {
        if (!playerController.IsPlayer())
            return Vector.Zero;

        var absOrigin = playerController?.PlayerPawn.Value?.AbsOrigin ?? Vector.Zero;
        var camera = playerController?.PlayerPawn.Value?.CameraServices;

        return new Vector(absOrigin.X, absOrigin.Y, absOrigin.Z + camera?.OldPlayerViewOffsetZ);
    }

    /// <summary>
    /// Sets the armor value for the player, optionally including a helmet and heavy armor.
    /// </summary>
    /// <param name="playerController">The player controller to set armor for.</param>
    /// <param name="armor">The armor value to set.</param>
    /// <param name="helmet">Whether to include a helmet.</param>
    /// <param name="heavy">Whether to include heavy armor.</param>
    public static void SetArmor(this CCSPlayerController? playerController, int armor, bool helmet = false, bool heavy = false)
    {
        if (!playerController.IsPlayer() || !playerController!.PawnIsAlive)
            return;

        playerController.PlayerPawn.Value!.ArmorValue = armor;
        Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CCSPlayerPawnBase", "m_ArmorValue");

        if (!helmet && !heavy)
            return;

        var services = new CCSPlayer_ItemServices(playerController.PlayerPawn.Value.ItemServices!.Handle);
        services.HasHelmet = helmet;
        services.HasHeavyArmor = heavy;
        Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBasePlayerPawn", "m_pItemServices");
    }
    
    /// <summary>
    /// Sets the health value for the player.
    /// </summary>
    /// <param name="playerController">The player controller to set health for.</param>
    /// <param name="health">The health value to set.</param>
    /// <param name="allowOverflow">Whether to allow the health to exceed the maximum health value.</param>
    public static void SetHealth(this CCSPlayerController? playerController, int health, bool allowOverflow = true)
    {
        if (!playerController.IsPlayer() || !playerController!.PawnIsAlive)
            return;

        playerController.PlayerPawn.Value!.Health = health;
        
        if (allowOverflow && health > playerController.PlayerPawn.Value.MaxHealth)
            playerController.PlayerPawn.Value.MaxHealth = health;
        
        Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
    }
    
    /// <summary>
    /// Sets the money for the player.
    /// </summary>
    /// <param name="playerController">The player controller to set money for.</param>
    /// <param name="money">The money value to set.</param>
    public static void SetMoney(this CCSPlayerController? playerController, int money)
    {
        if (!playerController.IsPlayer())
            return;
        
        var moneyServices = playerController!.InGameMoneyServices;
        if (moneyServices == null)
            return;
        
        moneyServices.Account = money;
        Utilities.SetStateChanged(playerController, "CCSPlayerController", "m_pInGameMoneyServices");
    }

    /// <summary>
    /// Checks if the specified controller has a permission.
    /// </summary>
    /// <param name="playerController">The player controller to check.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns><c>true</c> if the player has the permission; otherwise, <c>false</c>.</returns>
    public static bool HasPermission(this CCSPlayerController? playerController, string permission)
    {
        return playerController.IsPlayer() && AdminManager.PlayerHasPermissions(playerController, permission);
    }

    // code author is B3none
    public static void SetClientKills(this CCSPlayerController player, int kills)
    {
        if (player.ActionTrackingServices == null)
        {
            return;
        }

        player.ActionTrackingServices.NumRoundKills = kills;
        Utilities.SetStateChanged(player, "CCSPlayerController_ActionTrackingServices", "m_iNumRoundKills");
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pActionTrackingServices");
    }

    public static void ColorScreen(this CCSPlayerController player, Color color, float hold = 0.1f, float fade = 0.2f, FadeFlags flags = FadeFlags.FADE_IN, bool withPurge = true)
    {
        var fadeMsg = UserMessage.FromId(106);

        fadeMsg.SetInt("duration", Convert.ToInt32(fade * 512));
        fadeMsg.SetInt("hold_time", Convert.ToInt32(hold * 512));

        var flag = flags switch
        {
            FadeFlags.FADE_IN => 0x0001,
            FadeFlags.FADE_OUT => 0x0002,
            FadeFlags.FADE_STAYOUT => 0x0008,
            _ => 0x0001
        };

        if (withPurge)
            flag |= 0x0010;

        fadeMsg.SetInt("flags", flag);
        fadeMsg.SetInt("color", color.R | color.G << 8 | color.B << 16 | color.A << 24);
        fadeMsg.Send(player);
    }

    [Obsolete("Kick is deprecated as it is no longer needed. Use player.Disconnect. This method will be removed in a future update.", false)]
    /// <summary>
    /// Kicks the player from the server with a specified reason.
    /// </summary>
    /// <param name="playerController">The player controller to kick.</param>
    /// <param name="reason">The reason for kicking the player.</param>
    public static void Kick(this CCSPlayerController? playerController, string reason)
    {
        if (!playerController.IsPlayer())
            return;
        var kickCommand = string.Create(CultureInfo.InvariantCulture,
            $"kickid {playerController!.UserId!.Value} \"{reason}\"");
        // Queue for next frame to avoid threading issues
        Server.NextFrame(() => { Server.ExecuteCommand(kickCommand); });
    }

    // Author is Mesharsky
    public static void SetPlayerModelSize(this CCSPlayerController client, float value)
    {
        var playerPawnValue = client.PlayerPawn.Value;

        if (playerPawnValue == null)
            return;

        playerPawnValue.CBodyComponent!.SceneNode!.Scale = value;
        Utilities.SetStateChanged(playerPawnValue, "CBaseEntity", "m_CBodyComponent");
    }

    /// <summary>
    /// Checks if the specified controller represents a valid player.
    /// </summary>
    /// <param name="player">The player controller to check.</param>
    /// <returns><c>true</c> if the player is valid; otherwise, <c>false</c>.</returns>
    public static bool IsPlayer(this CCSPlayerController? player)
    {
        return
            player?.IsValid == true &&
            !player.IsBot &&
            SteamIDExtensions.IsSteamID(player.SteamID) &&
            player.Connected == PlayerConnectedState.PlayerConnected;
    }
}