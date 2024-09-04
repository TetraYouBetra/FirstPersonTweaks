using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
namespace FirstPersonTweaks;

public unsafe class DisableCulling : IDisposable
{
    private readonly Hook<ShouldDrawDelegate> shouldDrawHook;
    private delegate byte ShouldDrawDelegate(CameraBase* thisPtr, GameObject* gameObject, Vector3* sceneCameraPos, Vector3* lookAtVector);

    public DisableCulling(IGameInteropProvider gameInteropProvider)
    {
        shouldDrawHook = gameInteropProvider.HookFromSignature<ShouldDrawDelegate>("E8 ?? ?? ?? ?? 84 C0 75 18 48 8D 0D ?? ?? ?? ?? B3 01", ShouldDrawDetour);
        shouldDrawHook.Enable();
    }

    public void Dispose()
    {
        shouldDrawHook.Dispose();
    }

    private byte ShouldDrawDetour(CameraBase* thisPtr, GameObject* gameObject, Vector3* sceneCameraPos, Vector3* lookAtVector)
    {
        var objectKind = gameObject->GetObjectKind();
        if (objectKind == ObjectKind.Pc ||
            objectKind == ObjectKind.BattleNpc ||
            objectKind == ObjectKind.EventNpc ||
            objectKind == ObjectKind.Mount ||
            objectKind == ObjectKind.Companion ||
            objectKind == ObjectKind.Retainer)
        {
            return 1;
        }
        else
        {
            return shouldDrawHook.Original(thisPtr, gameObject, sceneCameraPos, lookAtVector);
        }
    }
}
