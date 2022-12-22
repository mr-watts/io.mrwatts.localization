using System;
using UnityEngine.Localization.Tables;

namespace MrWatts.Internal.Localization
{
    public interface ILocalizationManager
    {
        StringTable? StringTable { get; }

        /// <summary>
        /// Fires when the StringTable is loaded or updated
        /// </summary>
        event EventHandler<SelectedLocaleChangedEventArgs>? SelectedLocaleChanged;

        /// <summary>
        /// Get the localized string by keyID
        /// </summary>
        /// <param name="keyID">The keyID of the localized string to get</param>
        /// <param name="stringTable">An optional StringTable to use instead the default StringTable</param>
        /// <returns>The localized text. Returns an empty string if the StringTable is not yet loaded</returns>
        string GetLocalizedString(long keyID, StringTable? stringTable = null);
    }
}
