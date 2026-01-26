using Orleans;

namespace LionFire.Trading.Grains.User;

/// <summary>
/// Orleans grain interface for managing user UI preferences.
/// Stores column visibility settings, view preferences, and other user-specific UI state.
/// </summary>
/// <remarks>
/// The grain key is the user ID (e.g., "Anonymous" for unauthenticated users).
/// Preferences are persisted to Orleans storage.
/// </remarks>
public interface IUserPreferencesG : IGrainWithStringKey
{
    /// <summary>
    /// Gets the column visibility preferences for a specific view.
    /// </summary>
    /// <param name="viewId">Unique identifier for the view (e.g., "bots-list", "positions-list")</param>
    /// <returns>Column visibility settings, or null if no preferences have been set</returns>
    ValueTask<ColumnPreferences?> GetColumnPreferences(string viewId);

    /// <summary>
    /// Sets the column visibility preferences for a specific view.
    /// </summary>
    /// <param name="viewId">Unique identifier for the view</param>
    /// <param name="preferences">Column visibility settings to save</param>
    ValueTask SetColumnPreferences(string viewId, ColumnPreferences preferences);

    /// <summary>
    /// Gets all column preferences for all views.
    /// </summary>
    /// <returns>Dictionary of view IDs to column preferences</returns>
    ValueTask<Dictionary<string, ColumnPreferences>> GetAllColumnPreferences();

    /// <summary>
    /// Clears column preferences for a specific view, restoring defaults.
    /// </summary>
    /// <param name="viewId">Unique identifier for the view</param>
    ValueTask ClearColumnPreferences(string viewId);

    #region Bot Autostart

    /// <summary>
    /// Gets whether bots should be automatically started on silo startup.
    /// </summary>
    /// <returns>True if autostart is enabled (default), false if disabled</returns>
    ValueTask<bool> GetBotAutostartEnabled();

    /// <summary>
    /// Sets whether bots should be automatically started on silo startup.
    /// </summary>
    /// <param name="enabled">True to enable autostart, false to disable</param>
    ValueTask SetBotAutostartEnabled(bool enabled);

    #endregion

    #region Bot Optimization Preferences

    /// <summary>
    /// Gets the optimization preferences for a specific bot type.
    /// </summary>
    /// <param name="botTypeKey">Full type name of the bot (e.g., "MyNamespace.MyBot")</param>
    /// <returns>Bot optimization preferences, or null if no preferences have been set</returns>
    ValueTask<BotOptimizationPreferences?> GetBotOptimizationPreferences(string botTypeKey);

    /// <summary>
    /// Sets the optimization preferences for a specific bot type.
    /// </summary>
    /// <param name="botTypeKey">Full type name of the bot</param>
    /// <param name="preferences">Optimization preferences to save</param>
    ValueTask SetBotOptimizationPreferences(string botTypeKey, BotOptimizationPreferences preferences);

    /// <summary>
    /// Gets all bot optimization preferences for all bot types.
    /// </summary>
    /// <returns>Dictionary of bot type keys to optimization preferences</returns>
    ValueTask<Dictionary<string, BotOptimizationPreferences>> GetAllBotOptimizationPreferences();

    /// <summary>
    /// Clears optimization preferences for a specific bot type, restoring defaults.
    /// </summary>
    /// <param name="botTypeKey">Full type name of the bot</param>
    ValueTask ClearBotOptimizationPreferences(string botTypeKey);

    #endregion
}
