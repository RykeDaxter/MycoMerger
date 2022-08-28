using BepInEx;
using DiskCardGame;
using InscryptionAPI.Card;
using System;
using System.Collections.Generic;

namespace MycoMerger
{
    public static class MergerManager
    {
        /// <summary>
        /// Dictionary of SourceCards and their corresponding list of TargetCards. 
        /// </summary>
        /// <remarks>
        /// SourceCards are cards who are a source of Merge Data. They have a MycoMerger extended property that contains a custom-formatted string specifying a dictionary of TargetCards and what the result of the SourceCard and TargetCard should be.
        /// </remarks>
        internal static Dictionary<string, List<string>> SourceMergeDict;

        /// <summary>
        /// Dictionary of TargetCards and their corresponding list of SourceCards. 
        /// </summary>
        /// <remarks>
        /// TargetCards are cards who are targeted by a source of MergeData. TargetCards may have no MycoMerger extended property but they are specified as a potential combination for a card with that information.
        /// </remarks>
        internal static Dictionary<string, List<string>> TargetMergeDict;

        private static int _MaxModificationSigils = MycoMergerConfig.userMaxAddedSigils;
        /// <summary>
        /// Maximum number of modified sigils a card will have as a result of a merge. This does not include their base/default sigils. Might be allowed to be overriden by mods.
        /// </summary>
        public static int MaxModificationSigils
        {
            get { return MycoMergerConfig.userMaxAddedSigilsOverride ? _MaxModificationSigils : MycoMergerConfig.userMaxAddedSigils; }
            set { _MaxModificationSigils = value; }
        }

        private static MergeResult _DefaultMergeResult = MycoMergerConfig.userDefaultMergeResult;
        /// <summary>
        /// Defines the result of a merge when the result is not specified. Might be allowed to be overriden by mods.
        /// </summary>
        public static MergeResult DefaultMergeResult
        {
            get { return MycoMergerConfig.userDefaultMergeResultOverride ? _DefaultMergeResult : MycoMergerConfig.userDefaultMergeResult; }
            set { _DefaultMergeResult = value; }
        }

        private static MergeInheritance _DefaultMergeInheritance = MycoMergerConfig.userDefaultMergeInheritance;
        /// <summary>
        /// Defines the default inheritance of a merge when the inheritance is not specified. Might be allowed to be overriden by mods.
        /// </summary>
        public static MergeInheritance DefaultMergeInheritance
        {
            get { return MycoMergerConfig.userDefaultMergeInheritanceOverride ? _DefaultMergeInheritance : MycoMergerConfig.userDefaultMergeInheritance; }
            set { _DefaultMergeInheritance = value; }
        }



        /// <summary>
        /// Checks if card has ExtendedProperty with key of this mod's Plugin name which is <c>"MycoMerger"</c> and that it is not empty.
        /// </summary>
        /// <param name="sourceCard"></param>
        /// <returns>True if card has an Extended Property of "MycoMerger" and that property is not empty. False otherwise.</returns>
        public static bool HasMergeData(CardInfo sourceCard)
        {
            return !sourceCard.GetExtendedProperty(Plugin.PluginName).IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Checks if card has ExtendedProperty with key of this mod's Plugin name which is <c>"MycoMerger"</c> and that it is not empty.
        /// </summary>
        /// <param name="sourceCard"></param>
        /// <returns></returns>
        public static bool HasMergeData(string sourceCardName)
        {
            return !CardLoader.GetCardByName(sourceCardName).GetExtendedProperty(Plugin.PluginName).IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Retrieves merge data (a custom-formatted string representing the MycoMergerObject dictionary) stored in card and converts into a dictionary to return.
        /// </summary>
        /// <remarks>
        /// Returns a new dictionary if card has no valid merge data. 
        /// </remarks>
        /// <param name="sourceCard"></param>
        /// <returns>Dictionary containing MycoMerger-related data of card.</returns>
        public static Dictionary<string, string> GetMergeData(CardInfo sourceCard)
        {
            if (!HasMergeData(sourceCard))
            {
                return new Dictionary<string, string>();
            }
            return MycoMergerSerializer.MMDataStringToDict(sourceCard.GetExtendedProperty(Plugin.PluginName));
        }

        /// <summary>
        /// Retrieves merge data (a custom-formatted string representing the MycoMergerObject dictionary) stored in card and converts into a dictionary to return.
        /// </summary>
        /// <remarks>
        /// Returns a new dictionary if card has no valid merge data. 
        /// </remarks>
        /// <param name="sourceCard"></param>
        /// <returns>Dictionary containing MycoMerger-related data of card.</returns>
        public static Dictionary<string, string> GetMergeData(string sourceCardName)
        {
            CardInfo sourceCard = CardLoader.GetCardByName(sourceCardName);

            if (!HasMergeData(sourceCard))
            {
                return new Dictionary<string, string>();
            }
            return MycoMergerSerializer.MMDataStringToDict(sourceCard.GetExtendedProperty(Plugin.PluginName));
        }

        /// <summary>
        /// Sets MycoMerger-related data as a formatted string into the passed card as an extended property with key of <c>"MycoMerger"</c>.
        /// </summary>
        /// <remarks>
        /// If the given mergeData is null, sets the mergeData to a new dictionary.
        /// </remarks>
        /// <param name="sourceCard"></param>
        /// <param name="mergeData"></param>
        public static void SetMergeData(CardInfo sourceCard, Dictionary<string, string> mergeData)
        {
            if (mergeData == null)
            {
                Plugin.Log.LogWarning($"Card [{sourceCard.name}]: MergeData was attempted to be set to null.");
                mergeData = new();
            }
            sourceCard.SetExtendedProperty(Plugin.PluginName, MycoMergerSerializer.DictToMMDataString(mergeData));
        }

        /// <summary>
        /// Sets MycoMerger-related data as a formatted string into the card of that name as an extended property with key of <c>"MycoMerger"</c>.
        /// </summary>
        /// <remarks>
        /// If the given mergeData is null, sets the mergeData to a new dictionary.
        /// </remarks>
        /// <param name="sourceCard"></param>
        /// <param name="mergeData"></param>
        public static void SetMergeData(string sourceCardName, Dictionary<string, string> mergeData)
        {
            if (mergeData == null)
            {
                Plugin.Log.LogWarning($"Card [{sourceCardName}]: MergeData was attempted to be set to null.");
                mergeData = new();
            }
            CardLoader.GetCardByName(sourceCardName).SetExtendedProperty(Plugin.PluginName, MycoMergerSerializer.DictToMMDataString(mergeData));
        }

        /// <summary>
        /// Add a single entry to a <paramref name="sourceCard"/> for a merge with <paramref name="targetCardName"/> into <paramref name="result"/> which can be a card name or merge result.
        /// </summary>
        /// <remarks>
        /// The <paramref name="result"/> parameter can be a <c>MergeResult</c> prepended with <c>MycoMerger_</c>. <c>MergeInheritance</c> information can be added without touching the default <paramref name="mergeInheritance"/> by adding <c>":NameOfMergeInheritance"</c> at the end. For example: <c>"MycoMerger_Attack:AddedSigils" or <c>"Stoat:Health+BaseSigils"</c>.
        /// </remarks>
        /// <param name="sourceCard">Card to add MycoMerger data to.</param>
        /// <param name="targetCardName">Target card for the merge with this card.</param>
        /// <param name="result">Result of merge, can be a card name or MergeResult. May have MergeInheritance information added at the end after a ':' character with separate flags delimited with '+'. <c>MergeResult</c> must be prefixed with <c>MycoMerger_</c></param>
        /// <param name="mergeInheritance">Optional specifier for behavior of merge.</param>
        public static void AddMergeData(CardInfo sourceCard, string targetCardName, string result, MergeInheritance mergeInheritance = MergeInheritance.UserDefault)
        {
            Dictionary<string, string> newMergeData;
            newMergeData = HasMergeData(sourceCard) ? GetMergeData(sourceCard) : new();
            if (newMergeData.ContainsKey(targetCardName))
            {
                Plugin.Log.LogWarning($"Card [{sourceCard.name}]: Already targets card [{targetCardName}] with merge result [{newMergeData[targetCardName]}]. Setting new merge result [{result}].");
                newMergeData[targetCardName] = (mergeInheritance == MergeInheritance.UserDefault) ? result : String.Concat(result, ":", mergeInheritance.ToString().Replace(',', '+'));
            }
            else
            {
                newMergeData.Add(targetCardName, (mergeInheritance == MergeInheritance.UserDefault) ? result : String.Concat(result, ":", mergeInheritance.ToString().Replace(',', '+')));
            }
            SetMergeData(sourceCard, newMergeData);
        }

        /// <summary>
        /// Add a single entry to a card of <paramref name="sourceCardName"/> for a merge with <paramref name="targetCardName"/> into <paramref name="result"/> which can be a card name or merge result.
        /// </summary>
        /// <remarks>
        /// The <paramref name="result"/> parameter can be a <c>MergeResult</c> prepended with <c>MycoMerger_</c>. <c>MergeInheritance</c> information can be added without touching the default <paramref name="mergeInheritance"/> by adding <c>":NameOfMergeInheritance"</c> at the end. For example: <c>"MycoMerger_Attack:AddedSigils" or <c>"Stoat:Health+BaseSigils"</c>.
        /// </remarks>
        /// <param name="sourceCard">Card to add MycoMerger data to.</param>
        /// <param name="targetCardName">Target card for the merge with this card.</param>
        /// <param name="result">Result of merge, can be a card name or MergeResult. May have MergeInheritance information added at the end after a ':' character with separate flags delimited with '+'. <c>MergeResult</c> must be prefixed with <c>MycoMerger_</c></param>
        /// <param name="mergeInheritance">Optional specifier for behavior of merge. Separate flags delimited with <c>'+'</c></param>
        public static void AddMergeData(string sourceCardName, string targetCardName, string result, string mergeInheritance = "UserDefault")
        {
            CardInfo sourceCard = CardLoader.GetCardByName(sourceCardName);
            Dictionary<string, string> newMergeData;
            newMergeData = HasMergeData(sourceCard) ? GetMergeData(sourceCard) : new();

            if (newMergeData.ContainsKey(targetCardName))
            {
                Plugin.Log.LogWarning($"Card [{sourceCard.name}]: Already targets card [{targetCardName}] with merge result [{newMergeData[targetCardName]}]. Setting new merge result [{result}].");
                newMergeData[targetCardName] = (GetMergeInheritance(mergeInheritance).Item2 == MergeInheritance.UserDefault) ? result : String.Concat(result, ":", mergeInheritance);

            }
            else
            {
                newMergeData.Add(targetCardName, ((GetMergeInheritance(mergeInheritance).Item2 == MergeInheritance.UserDefault) ? result : String.Concat(result, ":", mergeInheritance)));
            }
            SetMergeData(sourceCard, newMergeData);
        }

        /// <summary>
        /// Checks if the string is a <c>MergeResult</c> and returns a corresponding <c>MergeResult</c> Enum. If it is not a <c>MergeResult</c>, returns <c>DefaultMergeResult</c>.
        /// </summary>
        /// <remarks>Ignores any MergeInheritance information.</remarks>
        /// <param name="mergeResult"></param>
        /// <returns><c>True<c> and the corresponding <c>MergeResult</c> if the string corresponds to a <c>MergeResult</c>. <c>False</c> and <c>DefaultMergeResult</c> otherwise.</returns>
        public static (bool, MergeResult) GetMergeResultEnum(string mergeResult)
        {
            if (mergeResult.Contains(":")) { mergeResult = mergeResult.Split(new[] { ':' }, 2)[0]; }

            if (mergeResult.Length > Plugin.PluginName.Length + 1
                && String.Equals(String.Concat(Plugin.PluginName, "_"), mergeResult.Substring(0, Plugin.PluginName.Length + 1))
                && Enum.TryParse(mergeResult.Substring(Plugin.PluginName.Length + 1), out MergeResult mergeEnum))
            {
                return (true, mergeEnum);
            }
            else
            {
                return (false, DefaultMergeResult);
            }
        }

        /// <summary>
        /// Resolves the merge result of two cards from the given criteria.
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <param name="mergeEnum"></param>
        /// <returns><c>CardInfo</c> of resolved merge result.</returns>
        public static CardInfo ResolveMergeResultEnum(CardInfo card1, CardInfo card2, MergeResult mergeEnum) => mergeEnum switch
        {
            MergeResult.SourceCard => card1,
            MergeResult.UserDefault => DefaultMergeResult == MergeResult.UserDefault ? card1 : ResolveMergeResultEnum(card1, card2, DefaultMergeResult),
            MergeResult.Random => new System.Random().Next() > (Int32.MaxValue / 2) ? card1 : card2,
            MergeResult.NumSigils => card1.Abilities.Count >= card2.Abilities.Count ? card1 : card2,
            MergeResult.NumBaseSigils => card1.DefaultAbilities.Count >= card2.DefaultAbilities.Count ? card1 : card2,
            MergeResult.Attack => (card1.Attack + (card1.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) >= card2.Attack + (card2.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0)) ? card1 : card2,
            MergeResult.Health => card1.Health >= card2.Health ? card1 : card2,
            MergeResult.TotalStats => (card1.Attack + (card1.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) + card1.Health) >= (card2.Attack + (card2.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) + card2.Health) ? card1 : card2,
            MergeResult.PowerLevel => card1.PowerLevel >= card2.PowerLevel ? card1 : card2,
            _ => card1,
        };

        /// <summary>
        /// Retrieves a cloned instance of the result of merging the source card and target card. If there is no merge data, it returns a result using DefaultMergeResult.
        /// </summary>
        /// <param name="sourceCard"></param>
        /// <param name="targetCard"></param>
        /// <returns>CardInfo instance clone of the resulting card from the merge.</returns>
        public static CardInfo GetMergeResultInstance(CardInfo sourceCard, CardInfo targetCard)
        {
            if (HasMergeData(sourceCard) && GetMergeData(sourceCard).ContainsKey(targetCard.name))
            {
                (bool, MergeResult) mergeEnumTuple;
                if ((mergeEnumTuple = GetMergeResultEnum(GetMergeData(sourceCard)[targetCard.name])).Item1)
                {
                    return CardLoader.Clone(ResolveMergeResultEnum(sourceCard, targetCard, mergeEnumTuple.Item2));
                }
                else    // Return merge result card in merge data
                {
                    string resultCard = GetMergeData(sourceCard)[targetCard.name];
                    if (resultCard.Contains(":"))
                    {
                        return CardLoader.Clone(CardLoader.GetCardByName(resultCard.Split(new[] { ':' }, 2)[0]));
                    }
                    else
                    {
                        return CardLoader.Clone(CardLoader.GetCardByName(resultCard));
                    }

                }
            }
            else
            {
                return CardLoader.Clone(ResolveMergeResultEnum(sourceCard, targetCard, DefaultMergeResult));
            }
        }

        /// <summary>
        /// Checks if the string has <c>MergeInheritance</c> information and returns a corresponding <c>MergeInheritance</c>. If it does not contain a <c>MergeInheritance</c> name, returns <c>DefaultMergeInheritance</c>.
        /// </summary>
        /// <param name="mergeResult">A string containing the full data for a merge result.</param>
        /// <returns><c>True</c> and the corresponding <c>MergeInheritance</c> if the merge data string has a <c>MergeInheritance</c> name. <c>False</c> and <c>DefaultMergeInheritance</c> otherwise.</returns>
        public static (bool, MergeInheritance) GetMergeInheritance(string mergeResult)
        {
            if (mergeResult.Contains(":")) { mergeResult = mergeResult.Split(new[] { ':' }, 2)[1]; }
            if (mergeResult.Contains("+")) { mergeResult = mergeResult.Replace('+', ','); }

            if (Enum.TryParse(mergeResult, out MergeInheritance mergeInheritance))
            {
                return (true, mergeInheritance);
            }
            else
            {
                return (false, DefaultMergeInheritance);
            }
        }

        /// <summary>
        /// Syncs MycoMerger data from every card into MergerManager's internal dictionaries.
        /// </summary>
        /// <remarks>
        /// The dictionaries keep information of which cards have MycoMerger data and which cards are targeted by Mycomerger data. They have no information on the merge results.
        /// </remarks>
        public static void SyncMergeDictionaries()
        {
            Dictionary<string, List<string>> sourceCardDict = new();
            Dictionary<string, List<string>> targetCardDict = new();

            foreach (CardInfo card in CardManager.AllCardsCopy)
            {
                if (HasMergeData(card))
                {
                    if (!sourceCardDict.ContainsKey(card.name))
                    {
                        sourceCardDict.Add(card.name, new List<string>());
                    }

                    foreach (KeyValuePair<string, string> kv in GetMergeData(card))
                    {
                        sourceCardDict[card.name].Add(kv.Key);

                        if (!targetCardDict.ContainsKey(kv.Key))
                        {
                            targetCardDict.Add(kv.Key, new List<string>());
                        }
                        targetCardDict[kv.Key].Add(card.name);
                    }
                }
            }
            SourceMergeDict = sourceCardDict;
            TargetMergeDict = targetCardDict;
            //Plugin.Log.LogInfo("Merger Dictionaries synchronized.");
        }

        /// <summary>
        /// Validates all cards with MycoMerger data and removes invalid data. Returns true for no invalid data, false if invalid data found and removed.
        /// </summary>
        /// <returns>True if no invalid data found. False if invalid data found and removed so card data was changed.</returns>
        public static bool ValidateMycoMergerData()
        {
            bool validationFlag = true;
            List<string> allCardNamesList = new();
            foreach (CardInfo card in CardManager.AllCardsCopy)
            {
                allCardNamesList.Add(card.name);
            }

            foreach (KeyValuePair<string, List<string>> sourceMergeEntry in SourceMergeDict)
            {
                Dictionary<string, string> mergeDataDict = GetMergeData(sourceMergeEntry.Key);
                List<string> keysToRemove = new();
                bool validDataFlag = true;

                foreach (KeyValuePair<string, string> mergeData in mergeDataDict)
                {
                    if (allCardNamesList.Contains(mergeData.Key)
                        && (allCardNamesList.Contains(mergeData.Value.Split(':')[0]) || GetMergeResultEnum(mergeData.Value).Item1)
                        && (!mergeData.Value.Contains(":") || GetMergeInheritance(mergeData.Value).Item1))
                    {
                        // Pass
                    }
                    else
                    {
                        keysToRemove.Add(mergeData.Key);
                        Plugin.Log.LogWarning($"Invalid data found in card [{sourceMergeEntry.Key}]. Removing entry [ {mergeData.Key}:{mergeData.Value} ].");
                        if (validDataFlag) { validDataFlag = false; }
                    }

                }
                if (!validDataFlag)
                {
                    foreach (string key in keysToRemove)
                    {
                        mergeDataDict.Remove(key);
                    }
                    SetMergeData(sourceMergeEntry.Key, mergeDataDict);
                    if (validationFlag) { validationFlag = false; }
                }
            }

            return validationFlag;
        }

        /// <summary>
        /// Checks every card's MergeData string if the first card name starts with <c>!RESULT!</c> and assigns that MergeData to the first card name excluding <c>!RESULT!</c> to be the source card, the second card name as the target card, and the checked card as the merge result.
        /// </summary>
        /// <remarks>
        /// <c>!RESULT!</c> signifies the card the MergeData is in is the result of two other cards. This MergeData is then assigned to the first card. This is meant so JSONCardLoader new card JSON files can directly include the MergeData in the card's file instead of creating a file for every different card used in a merge.
        /// </remarks>
        public static void AssignFromResultMergeData()
        {
            foreach (CardInfo card in CardManager.AllCardsCopy)
            {
                if (HasMergeData(card))
                {
                    bool newDataFlag = false;
                    List<string> keysToRemove = new();
                    Dictionary<string, string> mergeData = GetMergeData(card);
                    foreach (KeyValuePair<string, string> kv in mergeData)
                    {
                        if (kv.Key.StartsWith("!RESULT!"))
                        {
                            if (!newDataFlag) { newDataFlag = true; }
                            string sourceCardName = kv.Key.Substring(8);
                            Dictionary<string, string> sourceNewMergeData = GetMergeData(sourceCardName);
                            if (kv.Value.Contains(":"))     // Check for MergeInheritance
                            {
                                string[] tempStringArray = kv.Value.Split(':');
                                string newMergeValue = String.Concat(card.name, ":", tempStringArray[1]);
                                if (sourceNewMergeData.ContainsKey(tempStringArray[0]))
                                {
                                    Plugin.Log.LogWarning($"Assigning From Result [{card.name}] - Card [{sourceCardName}]: Already targets card [{tempStringArray[0]}] with merge result [{sourceNewMergeData[tempStringArray[0]]}]. Setting new merge result [{newMergeValue}].");
                                    sourceNewMergeData[tempStringArray[0]] = newMergeValue;
                                }
                                else
                                {
                                    sourceNewMergeData.Add(tempStringArray[0], newMergeValue);
                                }
                            }
                            else
                            {
                                if (sourceNewMergeData.ContainsKey(kv.Value))
                                {
                                    Plugin.Log.LogWarning($"Assigning From Result [{card.name}] - Card [{sourceCardName}]: Already targets card [{kv.Value}] with merge result [{sourceNewMergeData[kv.Value]}]. Setting new merge result [{card.name}].");
                                    sourceNewMergeData[kv.Value] = card.name;
                                }
                                else
                                {
                                    sourceNewMergeData.Add(kv.Value, card.name);
                                }

                            }
                            keysToRemove.Add(kv.Key);
                            SetMergeData(sourceCardName, sourceNewMergeData);
                        }
                    }
                    if (newDataFlag)
                    {
                        foreach (string key in keysToRemove)
                        {
                            mergeData.Remove(key);
                        }
                        SetMergeData(card.name, mergeData);
                    }
                }
            }
        }

        /// <summary>
        /// When the card list is modified, syncs MergerManager internal dictionaries and checks if all MycoMerger data in cards is valid, replacing invalid data with Stoat.
        /// </summary>
        /// <remarks>
        /// Uses the InscryptionAPI CardManager event.
        /// </remarks>
        /// <param name="cards"></param>
        /// <returns>Card list without modification if no errors, wi</returns>
        public static List<CardInfo> SyncMergerOnModifyCardList(List<CardInfo> cards)
        {
            AssignFromResultMergeData();
            SyncMergeDictionaries();
            if (!ValidateMycoMergerData())
            {
                SyncMergeDictionaries();
            }
            return cards;
        }

    }
}