using System.Collections;
using System.Collections.Generic;
using System.Windows;
using AvalonDock.Themes;

namespace SomaFrameQD.Framework.Themes
{
    internal static class BuiltInThemeResources
    {
        public static ResourceDictionary CreateAvalonDockDictionary(ITheme theme)
        {
            DictionaryTheme avalonDockTheme;
            var themeType = theme.GetType();

            if (themeType == typeof(LightTheme))
                avalonDockTheme = new Vs2013LightTheme();
            else if (themeType == typeof(DarkTheme))
                avalonDockTheme = new Vs2013DarkTheme();
            else if (themeType == typeof(BlueTheme))
                avalonDockTheme = new Vs2013BlueTheme();
            else
                return null;

            var dictionary = avalonDockTheme.ThemeResourceDictionary;
            FreezeEligibleResources(dictionary, new HashSet<ResourceDictionary>());
            return dictionary;
        }

        // Freezes eligible Freezable resources (SolidColorBrush, LinearGradientBrush, Pen, Geometry, etc.)
        // in AvalonDock 5's code-generated VS2013 dictionary tree to eliminate WPF dependency-property
        // change-listener allocations and Gen-2 GC overhead on .NET Framework 4.8.
        private static void FreezeEligibleResources(
            ResourceDictionary dictionary,
            HashSet<ResourceDictionary> visited)
        {
            if (dictionary == null || !visited.Add(dictionary))
            {
                return;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Value is Freezable freezable && !freezable.IsFrozen && freezable.CanFreeze)
                {
                    freezable.Freeze();
                }
            }

            foreach (var merged in dictionary.MergedDictionaries)
            {
                FreezeEligibleResources(merged, visited);
            }
        }
    }
}
