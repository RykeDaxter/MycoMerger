using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Reflection;

namespace MycoMerger
{
    [BepInDependency("cyantist.inscryption.api")]
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "rykedaxter.inscryption.mycomerger";
        public const string PluginName = "MycoMerger";
        public const string PluginVersion = "1.0.1";

        private static Harmony harmony;
        internal static ManualLogSource Log;

        private void Awake()
        {
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
            Log = base.Logger;

            MycoMergerConfig.BindConfigs(this);
        }

        private void Start()
        {
            CardManager.ModifyCardList += MergerManager.SyncMergerOnModifyCardList;
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }
}
