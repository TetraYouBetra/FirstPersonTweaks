using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace FirstPersonTweaks;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool isEnabled { get; set; } = false;
    public bool showBody { get; set; } = true;
    public bool showMount { get; set; } = true;
    public bool showWeapon { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
