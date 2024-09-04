using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FirstPersonTweaks.Structures;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using sCameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

namespace FirstPersonTweaks;

public unsafe class HideHead : IDisposable
{
    private readonly Hook<ChangeEquipmentDelegate> changeEquipmentHook;
    private delegate void ChangeEquipmentDelegate(UInt64 address, CharEquipSlots index, CharEquipSlotData* item);
    private readonly Hook<RenderSkeletonListDelegate> renderSkeletonListHook;
    private delegate void RenderSkeletonListDelegate(UInt64 RenderSkeletonLinkedList, float frameTiming);
    private readonly Hook<CameraUpdateRotationDelegate> cameraUpdateRotationHook;
    private delegate void CameraUpdateRotationDelegate(GameCamera* gameCamera);

    private sCameraManager* scCameraManager = sCameraManager.Instance();

    CharEquipData currentEquipmentSet = new CharEquipData();
    bool haveSavedEquipmentSet = false;

    int timer = 100;
    CharEquipSlotData hiddenEquipHead = new CharEquipSlotData(6154, 99, 0, 0);
    CharEquipSlotData hiddenEquipEars = new CharEquipSlotData(0, 0, 0, 0);
    CharEquipSlotData hiddenEquipNeck = new CharEquipSlotData(0, 0, 0, 0);
    private ChangedType<CameraModes> gameMode = new ChangedType<CameraModes>(CameraModes.None);
    private ChangedTypeBool inCutscene = new ChangedTypeBool();

    byte HideHeadValue = 0;
    Dictionary<UInt64, stCommonSkelBoneList> commonBones = new Dictionary<UInt64, stCommonSkelBoneList>();
    private float Deg2Rad = MathF.PI / 180.0f;
    public HideHead(IGameInteropProvider gameInteropProvider)
    {
        changeEquipmentHook = gameInteropProvider.HookFromSignature<ChangeEquipmentDelegate>("E8 ?? ?? ?? ?? B1 ?? 41 FF C6", ChangeEquimentDetour);
        changeEquipmentHook.Enable();

        renderSkeletonListHook = gameInteropProvider.HookFromSignature<RenderSkeletonListDelegate>("E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8B 6C 24 ?? 48 8B 5C 24", RenderSkeletonListDetour);
        renderSkeletonListHook.Enable();

        cameraUpdateRotationHook = gameInteropProvider.HookFromSignature<CameraUpdateRotationDelegate>("E8 ?? ?? ?? ?? F3 0F 10 83 ?? ?? ?? ?? 41 0F 2E C1", CameraUpdateRotationDetour);
        cameraUpdateRotationHook.Enable();

        if (scCameraManager == null)
            scCameraManager = sCameraManager.Instance();

        RawGameCamera* renderCam = (RawGameCamera*)scCameraManager->CurrentCamera;
        if (renderCam != null)
            renderCam->BufferData->NearClip = 0.05f;


        if (Plugin.ClientState!.LocalPlayer != null)
        {
            GameObject* bonedObject = (GameObject*)Plugin.ClientState!.LocalPlayer.Address;
            Character* bonedCharacter = (Character*)Plugin.ClientState!.LocalPlayer.Address;

            if (bonedCharacter != null)
            {
                if (haveSavedEquipmentSet == false)
                {
                    currentEquipmentSet.Save(bonedCharacter);
                    haveSavedEquipmentSet = true;
                }

                UInt64 equipOffset = (UInt64)(UInt64*)&bonedCharacter->DrawData;
                //----
                // override the head neck and earing
                //----
                if (bonedCharacter->DrawData.EquipmentModelIds[(int)CharEquipSlots.Head].Variant != 99)
                {
                    HideHeadValue = bonedCharacter->DrawData.Flags1;
                    bonedCharacter->DrawData.Flags1 = 0;

                    fixed (CharEquipSlotData* ptr = &hiddenEquipHead)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Head, ptr);
                    fixed (CharEquipSlotData* ptr = &hiddenEquipNeck)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Neck, ptr);
                    fixed (CharEquipSlotData* ptr = &hiddenEquipEars)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Ears, ptr);

                    RefreshObject((GameObject*)Plugin.ClientState!.LocalPlayer.Address);
                }
            }
        }
    }

    public void RefreshObject(GameObject* obj2refresh)
    {
        obj2refresh->RenderFlags = 2;
        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Interval = 500;
        timer.Elapsed += (sender, e) => { RefreshObjectTick(timer, obj2refresh); };
        timer.Enabled = true;
    }

    public void RefreshObjectTick(System.Timers.Timer timer, GameObject* obj2refresh)
    {
        obj2refresh->RenderFlags = 0;
        timer.Enabled = false;
    }

    public void Dispose()
    {
        changeEquipmentHook.Dispose();
        renderSkeletonListHook.Dispose();
        cameraUpdateRotationHook.Dispose();
    }

    public void Update()
    {
        if (gameMode.Current == CameraModes.ThirdPerson && gameMode.Changed == true)
            FirstToThirdPersonView();
        else if (gameMode.Current == CameraModes.FirstPerson && gameMode.Changed == true)
            ThirdToFirstPersonView();

        //----
        // Changes to 3rd person when a cutscene is triggered
        // and back when it ends
        //----
        if (inCutscene.Changed)
            if (inCutscene.Current)
                FirstToThirdPersonView();
            else
                if (gameMode.Current == CameraModes.FirstPerson)
                ThirdToFirstPersonView();
    }

    public void InCutscene(bool isCutscene)
    {
        inCutscene.Current = isCutscene;
    }

    private void FirstToThirdPersonView()
    {
        RawGameCamera* renderCam = (RawGameCamera*)scCameraManager->CurrentCamera;
        if (renderCam != null)
            renderCam->BufferData->NearClip = 0.05f; // need to add default/initial tracking

        if (Plugin.ClientState!.LocalPlayer != null)
        {
            GameObject* bonedObject = (GameObject*)Plugin.ClientState!.LocalPlayer.Address;
            Character* bonedCharacter = (Character*)Plugin.ClientState!.LocalPlayer.Address;

            if (bonedCharacter != null)
            {
                UInt64 equipOffset = (UInt64)(UInt64*)&bonedCharacter->DrawData;
                fixed (CharEquipSlotData* ptr = &currentEquipmentSet.Head)
                    changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Head, ptr);
                fixed (CharEquipSlotData* ptr = &currentEquipmentSet.Ears)
                    changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Ears, ptr);
                fixed (CharEquipSlotData* ptr = &currentEquipmentSet.Neck)
                    changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Neck, ptr);

                RefreshObject((GameObject*)Plugin.ClientState!.LocalPlayer.Address);
            }

            haveSavedEquipmentSet = false;
        }
    }

    private void ThirdToFirstPersonView()
    {
        RawGameCamera* renderCam = (RawGameCamera*)scCameraManager->CurrentCamera;
        if (renderCam != null)
            renderCam->BufferData->NearClip = 0.05f;

        if (Plugin.ClientState!.LocalPlayer != null)
        {
            GameObject* bonedObject = (GameObject*)Plugin.ClientState!.LocalPlayer.Address;
            Character* bonedCharacter = (Character*)Plugin.ClientState!.LocalPlayer.Address;

            if (bonedCharacter != null)
            {
                if (haveSavedEquipmentSet == false)
                {
                    currentEquipmentSet.Save(bonedCharacter);
                    haveSavedEquipmentSet = true;
                }

                UInt64 equipOffset = (UInt64)(UInt64*)&bonedCharacter->DrawData;
                //----
                // override the head neck and earing
                //----
                if (bonedCharacter->DrawData.EquipmentModelIds[(int)CharEquipSlots.Head].Variant != 99)
                {
                    HideHeadValue = bonedCharacter->DrawData.Flags1;
                    bonedCharacter->DrawData.Flags1 = 0;

                    fixed (CharEquipSlotData* ptr = &hiddenEquipHead)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Head, ptr);
                    fixed (CharEquipSlotData* ptr = &hiddenEquipNeck)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Neck, ptr);
                    fixed (CharEquipSlotData* ptr = &hiddenEquipEars)
                        changeEquipmentHook!.Original(equipOffset, CharEquipSlots.Ears, ptr);

                    RefreshObject((GameObject*)Plugin.ClientState!.LocalPlayer.Address);
                }
            }
        }
    }

    private void CameraUpdateRotationDetour(GameCamera* gameCamera)
    {
        gameMode.Current = gameCamera->Camera.Mode;
        cameraUpdateRotationHook!.Original(gameCamera);
    }

    private void ChangeEquimentDetour(UInt64 address, CharEquipSlots index, CharEquipSlotData* item)
    {
        IPlayerCharacter player = Plugin.ClientState!.LocalPlayer!;
        if (player != null)
        {
            Character* bonedCharacter = (Character*)player.Address;
            if (bonedCharacter != null)
            {
                UInt64 equipOffset = (UInt64)(UInt64*)&bonedCharacter->DrawData;
                if (equipOffset == address)
                {
                    haveSavedEquipmentSet = true;
                    currentEquipmentSet.Data[(int)index] = item->Data;
                }
            }
        }

        Plugin.Log!.Info($"ChangeEquipmentDetour\n{item->Id}\n{index}");
        changeEquipmentHook!.Original(address, index, item);
    }

    private Character* GetCharacter(byte charFrom = 3)
    {
        Plugin.Log!.Info("Getting Character");
        IPlayerCharacter player = Plugin.ClientState!.LocalPlayer!;
        if (player == null)
            return null;

        if (player != null && (charFrom & 2) == 2)
            return (Character*)player!.Address;
        else
            return null;
    }

    private unsafe void RenderSkeletonListDetour(UInt64 RenderSkeletonLinkedList, float frameTiming)
    {
        renderSkeletonListHook!.Original(RenderSkeletonLinkedList, frameTiming);
        UpdateBoneScales();
    }

    private void UpdateBoneScales()
    {
        if (inCutscene.Current || gameMode.Current == CameraModes.ThirdPerson)
            return;

        Character* bonedCharacter = GetCharacter();
        if (bonedCharacter == null){
            Plugin.Log!.Info("Failed to get character");
            return;
        }

        Structures.Model* model = (Structures.Model*)bonedCharacter->GameObject.DrawObject;
        if (model == null){
            Plugin.Log!.Info("Failed to get model");
            return;
        }

        Skeleton* skeleton = model->skeleton;
        if (skeleton == null){
            Plugin.Log!.Info("Failed to get skeleton");
            return;
        }

        SkeletonResourceHandle* srh = skeleton->SkeletonResourceHandles[0];
        if (srh == null){
            Plugin.Log!.Info("Failed to get skeleton resource handle");
            return;
        }
        if (srh != null)
        {
            hkaSkeleton* hkaSkele = srh->HavokSkeleton;
            if (hkaSkele != null){
                if (!commonBones.ContainsKey((UInt64)hkaSkele))
                {
                    commonBones.Add((UInt64)hkaSkele, new stCommonSkelBoneList(skeleton));
                    Plugin.Log!.Info($"commonBoneCount {commonBones.Count} {commonBones[(UInt64)hkaSkele].armLength}");
                }
            }
        }

        hkaSkeleton* hkaSkel = srh->HavokSkeleton;
        if (hkaSkel == null){
            Plugin.Log!.Info("Failed to get havok skeleton");
            return;
        }

        if (!commonBones.ContainsKey((UInt64)hkaSkel)){
            Plugin.Log!.Info("Failed to find hkaSkel");
            return;
        }

        stCommonSkelBoneList csb = commonBones[(UInt64)hkaSkel];

        Transform transformS = skeleton->Transform;
        transformS.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), bonedCharacter->GameObject.Rotation);
        skeleton->Transform = transformS;

        hkQsTransformf transform;


        for (ushort p = 0; p < skeleton->PartialSkeletonCount; p++)
        {
            hkaPose* objPose = skeleton->PartialSkeletons[p].GetHavokPose(0);
            if (objPose == null)
                continue;

            if (p == 0)
            {
                if (csb.e_neck >= 0)
                {
                    //----
                    // Shrink the head and all child bones
                    //----
                    foreach (short id in csb.layout[csb.e_neck].Value)
                    {
                        transform = objPose->LocalPose[id];
                        //transform.Translation = (transform.Translation.Convert() * -1).Convert();
                        transform.Scale = new Vector3(0.000001f, 0.000001f, 0.000001f).Convert();
                        objPose->LocalPose[id] = transform;
                    }

                    //----
                    // Rotate the neck to hide the head
                    //----
                    transform = objPose->LocalPose[csb.e_neck];
                    transform.Rotation = Quaternion.CreateFromYawPitchRoll(0 * Deg2Rad, 0 * Deg2Rad, 180 * Deg2Rad).Convert();
                    transform.Scale = new Vector3(0.0001f, 0.0001f, 0.0001f).Convert();
                    objPose->LocalPose[csb.e_neck] = transform;
                }
            }
            else
            {

            }
        }
    }
}
class ChangedTypeBool
{
    private bool old = false;
    public bool Current
    {
        get => old;
        set
        {
            Changed = !(value == old);
            old = value;
        }
    }
    public bool Changed { get; private set; }
    public ChangedTypeBool(bool newVal = false)
    {
        old = newVal;
        Current = newVal;
        Changed = false;
    }
    public ChangedTypeBool Set(bool newVal)
    {
        Current = newVal;
        return this;
    }
}

class ChangedType<T>
{
    private T old = default(T);
    public T Current
    {
        get => old;
        set
        {
            Changed = false;
            if (!EqualityComparer<T>.Default.Equals(value, old))
            {
                old = value;
                Changed = true;
            }
        }
    }
    public bool Changed { get; private set; }
    public ChangedType(T newVal = default(T))
    {
        old = newVal;
        Current = newVal;
        Changed = false;
    }
    public ChangedType<T> Set(T newVal)
    {
        Current = newVal;
        return this;
    }
}
