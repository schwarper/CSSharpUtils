using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace CSSharpUtils.Extensions;

public static class PlayerPawnExtensions
{
    private static void SetMoveType(this CCSPlayerPawn pawn, MoveType_t moveType)
    {
        pawn.MoveType = moveType;
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        Schema.GetRef<MoveType_t>(pawn.Handle, "CBaseEntity", "m_nActualMoveType") = moveType;
    }
    
    public static void Freeze(this CCSPlayerPawn pawn) => pawn.SetMoveType(MoveType_t.MOVETYPE_OBSOLETE);
    public static void Unfreeze(this CCSPlayerPawn pawn) => pawn.SetMoveType(MoveType_t.MOVETYPE_WALK);

    public static void Bury(this CBasePlayerPawn pawn)
    {
        Vector? absOrigin = pawn.AbsOrigin;

        Vector vector = new(absOrigin!.X, absOrigin.Y, absOrigin.Z - 10.0f);
        pawn.Teleport(vector, pawn.AbsRotation, pawn.AbsVelocity);
    }

    public static void UnBury(this CBasePlayerPawn pawn)
    {
        Vector? absOrigin = pawn.AbsOrigin;

        Vector vector = new(absOrigin!.X, absOrigin.Y, absOrigin.Z + 10.0f);
        pawn.Teleport(vector, pawn.AbsRotation, pawn.AbsVelocity);
    }

    public static void TeleportToPlayer(this CCSPlayerPawn playerPawn, CCSPlayerPawn targetPawn)
    {
        Vector? position = targetPawn.AbsOrigin;
        QAngle? angle = targetPawn.AbsRotation;

        if (position == null || angle == null)
        {
            return;
        }

        Vector velocity = targetPawn.AbsVelocity;

        playerPawn.Teleport(position, angle, velocity);
    }

    public static void Glow(this CBasePlayerPawn playerPawn, Color color)
    {
        playerPawn.RenderMode = RenderMode_t.kRenderTransColor;
        playerPawn.Render = color;

        Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
    }
}