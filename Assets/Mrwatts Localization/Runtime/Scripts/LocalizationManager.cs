using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MrWatts.Internal.Localization
{
    public sealed class LocalizationManager : ILocalizationManager
    {
        private StringTable? _stringTable;
        public StringTable? StringTable
        {
            get => _stringTable;
            private set
            {
                _stringTable = value;
                SelectedLocaleChanged?.Invoke(this, new SelectedLocaleChangedEventArgs(_stringTable));
            }
        }

        /// <inheritdoc />
        public event EventHandler<SelectedLocaleChangedEventArgs>? SelectedLocaleChanged;

        private TableReference tableReference;

        public LocalizationManager(Guid tableGuid)
        {
            tableReference = tableGuid;

            Constructor();
        }

        public LocalizationManager(string tableName)
        {
            tableReference = tableName;

            Constructor();
        }

        private void Constructor()
        {
            LocalizationSettings.SelectedLocaleChanged += UpdateStringTable;

            UpdateStringTable();
        }

        private void UpdateStringTable(Locale locale = null)
        {
            LocalizationSettings.StringDatabase.GetTableAsync(tableReference, locale).Completed += (x) => StringTable = x.Result;
        }

        /// <inheritdoc />
        public string GetLocalizedString(long keyID, StringTable? stringTable = null)
        {
            if (stringTable == null)
            {
                if (StringTable == null)
                {
                    return string.Empty;
                }

                return StringTable.GetEntry(keyID).GetLocalizedString();
            }
            else
            {
                return stringTable.GetEntry(keyID).GetLocalizedString();
            }
        }
    }
}