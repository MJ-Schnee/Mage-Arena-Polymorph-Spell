using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using MageConfigurationAPI.Data;

namespace PolymorphSpell;

internal static class PolymorphSpellConfig
{
    internal static ConfigEntry<float> CooldownConfig { get; private set; }

    internal static ConfigEntry<float> RangeConfig { get; private set; }

    internal static ConfigEntry<float> DefaultSpellDurationSecConfig { get; private set; }

    internal static ConfigEntry<float> CastingLevelDurationIncreaseSecConfig { get; private set; }

    internal static ConfigEntry<bool> TeamChestConfig { get; private set; }

    internal static ConfigEntry<float> PolymorphHealth { get; private set; }

    private static bool _mageConfigApiExists;

    private static BaseUnityPlugin _plugin;

    /// <summary>
    /// Loads all configuration options
    /// </summary>
    /// <param name="plugin">Plugin to attach</param>
    public static void LoadConfig(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
        _mageConfigApiExists = Chainloader.PluginInfos.ContainsKey("com.d1gq.mage.configuration.api");

        CooldownConfig = BindConfig(
            "Cooldown",
            60f,
            "Time until spell can be used again (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        RangeConfig = BindConfig(
            "Range",
            20f,
            "Maximum distance a target can be from the caster (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        DefaultSpellDurationSecConfig = BindConfig(
            "DefaultSpellDuration",
            5f,
            "Duration in seconds target will be polymorphed for (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        CastingLevelDurationIncreaseSecConfig = BindConfig(
            "CastingLevelDurationIncrease",
            1f,
            "Duration in seconds polymorph will extend per spell level for (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        TeamChestConfig = BindConfig(
            "Team Chest",
            true,
            "Whether the page can spawn in the team chest"
        );

        PolymorphHealth = BindConfig(
            "PolymorphHealth",
            20f,
            "Health of polymorph (capped to player's health pre-polymorph)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );
    }

    /// <summary>
    /// Binds the config option to a variable and adds it to the in-game config menu if available
    /// </summary>
    /// <typeparam name="T">Type of config entry</typeparam>
    /// <param name="key">Config name</param>
    /// <param name="defaultValue">Config default value</param>
    /// <param name="description">Config description</param>
    /// <param name="acceptableValues">Optional config value range/list</param>
    /// <returns></returns>
    private static ConfigEntry<T> BindConfig<T>(string key, T defaultValue, string description, AcceptableValueBase acceptableValues = null)
    {
        ConfigEntry<T> configEntry = _plugin.Config.Bind(
            PluginInfo.PLUGIN_NAME,
            key,
            defaultValue,
            new ConfigDescription(description, acceptableValues)
        );

        if (_mageConfigApiExists)
        {
            new ModConfig(_plugin, configEntry, MageConfigurationAPI.Enums.SettingsFlag.ShowInLobbyMenu);
        }

        return configEntry;
    }
}
