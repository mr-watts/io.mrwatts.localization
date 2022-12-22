using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MrWatts.Internal.Localization
{
    public static class LocalizedStringExtensions
    {
        public static void AddOrUpdate<T>(this LocalizedString localizedString, string key, T value)
        {
            if (localizedString.ContainsKey(key))
            {
                ((Variable<T>)localizedString[key]).Value = value;
            }
            else
            {
                localizedString.Add(key, new Variable<T>() { Value = value });
            }

            localizedString.RefreshString();
        }
    }
}