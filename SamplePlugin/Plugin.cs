using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FirstPersonTweaks.Windows;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System.Diagnostics.Metrics;
using System.Drawing;
using System;
using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Interface.Windowing;
namespace FirstPersonTweaks;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface? PluginInterface { get; private set; } = null;
    [PluginService] public static IFramework? Framework { get; private set; } = null;
    [PluginService] public static IClientState? ClientState { get; private set; } = null;
    [PluginService] public static ITitleScreenMenu? TitleScreenMenu { get; private set; } = null;
    [PluginService] public static ICondition? Condition { get; private set; } = null;
    [PluginService] public static ISigScanner? SigScanner { get; private set; } = null;
    [PluginService] public static IChatGui? ChatGui { get; private set; } = null;
    [PluginService] public static IGameGui? GameGui { get; private set; } = null;
    [PluginService] public static ICommandManager? CommandManager { get; private set; } = null;
    [PluginService] public static IObjectTable? ObjectTable { get; private set; } = null;
    [PluginService] public static IPartyList? PartyList { get; private set; } = null;
    [PluginService] public static IPluginLog? Log { get; private set; } = null;
    [PluginService] public static ITargetManager? TargetManager { get; private set; } = null;
    [PluginService] public static IGameInteropProvider Interop { get; private set; } = null;

    private DisableCulling disableCulling;
    private HideHead hideHead;

    private const string CommandName = "/fptweaks";

    public Configuration cfg { get; init; }

    public readonly WindowSystem WindowSystem = new("FirstPersonTweaks");
    private ConfigWindow ConfigWindow { get; init; }


    public Plugin()
    {
        try
        {
            cfg = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            // you might normally want to embed resources and load them from the manifest stream        

            ConfigWindow = new ConfigWindow(this);

            WindowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Tweaks for the first person camera mode. /fptweaks"
            });

            Interop.InitializeFromAttributes(this);
            disableCulling = new DisableCulling(Interop);
            hideHead = new HideHead(Interop);

            PluginInterface.UiBuilder.Draw += DrawUI;

            // This adds a button to the plugin installer entry of this plugin which allows
            // to toggle the display status of the configuration ui
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

            // Adds another button that is doing the same but for the main ui of the plugin
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;


            Framework!.Update += Update;
            Framework!.Update += InitializeCheck;
            ClientState!.Login += OnLogin;
            ClientState!.Logout += OnLogout;

        }
        catch (Exception e) { Log!.Info($"Failed loading plugin\n{e}"); }
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        disableCulling.Dispose();
        hideHead.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }
       

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => ConfigWindow.Toggle();

    private unsafe bool Initialize()
    {
        return true;
    }

    private void InitializeCheck(IFramework framework)
    {
       
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    private void Update(IFramework framework)
    {
        bool isCutscene = Condition![ConditionFlag.OccupiedInCutSceneEvent] || Condition![ConditionFlag.WatchingCutscene] || Condition![ConditionFlag.WatchingCutscene78];
        hideHead.InCutscene(isCutscene);
        hideHead.Update();
    }

    private void OnLogin()
    {
        
    }
    private void OnLogout()
    {
        
    }
}
