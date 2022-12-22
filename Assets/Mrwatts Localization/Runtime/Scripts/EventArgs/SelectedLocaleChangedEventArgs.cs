using UnityEngine.Localization.Tables;

namespace MrWatts.Internal.Localization
{
    public sealed class SelectedLocaleChangedEventArgs
    {
        public StringTable StringTable { get; }

        public SelectedLocaleChangedEventArgs(StringTable stringTable)
        {
            StringTable = stringTable;
        }
    }
}