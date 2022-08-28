namespace MycoMerger
{
    internal static class MycoMergerConfig
    {
        internal static int userMaxAddedSigils;
        internal static bool userMaxAddedSigilsOverride;
        internal static MergeResult userDefaultMergeResult;
        internal static bool userDefaultMergeResultOverride;
        internal static MergeInheritance userDefaultMergeInheritance;
        internal static bool userDefaultMergeInheritanceOverride;

        internal static void BindConfigs(Plugin thisPlugin)
        {
            userMaxAddedSigils = thisPlugin.Config.Bind("General",
                "MaxAddedSigils",
                4,
                "The maximum number of added abilities the resulting card of a Mycologist merge can have.\nMay be overridden by mods if MaxAddedSigilsOverride is true."
            ).Value;
            userDefaultMergeResult = thisPlugin.Config.Bind("General",
                "DefaultMergeResult",
                MergeResult.TotalStats,
                "Defines the default result of a merge that occurs when the result is not specified.\nMay be overridden by mods if DefaultMergeResultOverride is true. No practical effect in current version."
            ).Value;
            userDefaultMergeInheritance = thisPlugin.Config.Bind("General",
                "DefaultMergeInheritance",
                MergeInheritance.Vanilla,
                "Defines the default inheritance the result of a merge receives from the two cards used for it.\nMay be overridden by mods if DefaultMergeInheritanceOverride is true.\nYou can set a combination of Attack, Health, AddedSigils, and BaseSigils using commas (,). (Example: Attack, BaseSigils)"
            ).Value;
            userMaxAddedSigilsOverride = thisPlugin.Config.Bind("Overrides",
                "MaxAddedSigilsOverride",
                true,
                "Allow mods to override your personal MaxAddedSigils setting when true.\nIf false, always use your MaxAddedSigils setting."
            ).Value;
            userDefaultMergeResultOverride = thisPlugin.Config.Bind("Overrides",
                "DefaultMergeResultOverride",
                true,
                "Allow mods to override your personal DefaultMergeResult setting when true.\nIf false, always use your DefaultMergeResult setting."
            ).Value;
            userDefaultMergeInheritanceOverride = thisPlugin.Config.Bind("Overrides",
                "DefaultMergeInheritanceOverride",
                true,
                "Allow mods to override your personal DefaultMergeInheritance setting when true.\nIf false, always use your DefaultMergeResult setting."
            ).Value;
        }
    }
}
