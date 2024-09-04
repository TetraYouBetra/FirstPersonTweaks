using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FirstPersonTweaks.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration cfg;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("First Person Tweaks")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 110);
        SizeCondition = ImGuiCond.Always;

        cfg = plugin.cfg;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        //if (cfg.IsConfigWindowMovable)
        //{
        //    Flags &= ~ImGuiWindowFlags.NoMove;
        //}
        //else
        //{
        //    Flags |= ImGuiWindowFlags.NoMove;
        //}
    }

    public override void Draw()
    {
        var isEnabledValue = cfg.isEnabled;
        if (ImGui.Checkbox("Enabled", ref isEnabledValue))
        {
            cfg.isEnabled = isEnabledValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            cfg.Save();
        }

        // can't ref a property, so use a local copy
        var showBodyValue = cfg.showBody;
        if (ImGui.Checkbox("Show Body", ref showBodyValue))
        {
            cfg.showBody = showBodyValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            cfg.Save();
        }

        var showMountValue = cfg.showMount;
        if (ImGui.Checkbox("Show Mount", ref showMountValue))
        {
            cfg.showMount = showMountValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            cfg.Save();
        }

        var showWeaponValue = cfg.showWeapon;
        if (ImGui.Checkbox("Show Weapon", ref showWeaponValue))
        {
            cfg.showWeapon = showWeaponValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            cfg.Save();
        }
    }
}
