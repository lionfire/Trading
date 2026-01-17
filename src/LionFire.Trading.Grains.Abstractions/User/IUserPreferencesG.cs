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
}
