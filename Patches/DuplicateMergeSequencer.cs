using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MycoMerger.Patches
{
    [HarmonyPatch]
    class DuplicateMergeSequencerPatch
    {
        /// <summary>
        /// Small correction for localized display name from the left card to the result card in post-merge Mycologists dialogue.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="pair"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DuplicateMergeSequencer), "CombinePair")]
        private static IEnumerator MycoMergerCombinePair(IEnumerator original, SelectableCardPair pair, DuplicateMergeSequencer __instance)
        {
            Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.15f);
            LeshyAnimationController.Instance.RightArm.PlayAnimation("doctor_hand_intro");
            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("DuplicateMergeLookAway", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, new Action<DialogueEvent.Line>(__instance.OnDoctorDialogueLine));
            __instance.SetSideHeadTalking(false);
            Singleton<ViewManager>.Instance.SwitchToView(View.TableStraightDown, false, false);
            AudioController.Instance.PlaySound3D("mycologist_carnage", MixerGroup.TableObjectsSFX, __instance.largeMushroom.transform.position, 1f, 0f, null, null, null, null, false);
            yield return new WaitForSeconds(1f);
            Singleton<CameraEffects>.Instance.Shake(0.1f, 0.25f);
            __instance.bloodParticles1.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            Singleton<CameraEffects>.Instance.Shake(0.05f, 0.4f);
            __instance.paperParticles.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            __instance.bloodParticles2.gameObject.SetActive(true);
            Singleton<CameraEffects>.Instance.Shake(0.1f, 0.4f);
            yield return new WaitForSeconds(0.5f);
            if (!pair.LeftCard.Info.Mods.Exists((CardModificationInfo x) => x.fromDuplicateMerge))
            {
                if (!pair.RightCard.Info.Mods.Exists((CardModificationInfo x) => x.fromDuplicateMerge))
                {
                    goto IL_26F;
                }
            }
            AchievementManager.Unlock(Achievement.PART1_SPECIAL2);
        IL_26F:
            CardInfo info = __instance.MergeCards(pair.LeftCard.Info, pair.RightCard.Info);
            __instance.mergedCard = __instance.SpawnMergedCard(info, pair.transform.position, pair.transform.eulerAngles);
            __instance.selectionSlot.DestroyCard();
            __instance.selectionSlot.gameObject.SetActive(false);
            Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeSlots, false, false);
            yield return new WaitForSeconds(0.15f);
            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("DuplicateMergeResult", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[]
            {
                info.DisplayedNameLocalized
            }, null);
            Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.1f);
            __instance.mergedCard.ExitBoard(0.4f, new Vector3(1f, 0f, -2f));
            yield return new WaitForSeconds(0.5f);
            LeshyAnimationController.Instance.RightArm.SetTrigger("doctor_hide");
            CustomCoroutine.WaitThenExecute(1f, delegate
            {
                LeshyAnimationController.Instance.RightArm.SetHidden(true);
            }, false);
            __instance.bloodParticles1.gameObject.SetActive(false);
            __instance.bloodParticles2.gameObject.SetActive(false);
            __instance.paperParticles.gameObject.SetActive(false);
            yield break;
        }

        /// <summary>
        /// Adds valid merges according to MycoMerger data to the result of the original method's GetValidDuplicateCards(). Duplicate merges are received from the original method so this adds non-duplicate meges.
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DuplicateMergeSequencer), "GetValidDuplicateCards")]
        private static void GetValidMycoMergerCards(ref List<CardInfo> __result, DuplicateMergeSequencer __instance)
        {
            List<CardInfo> mergeCardList = new();
            Dictionary<string, List<CardInfo>> mergeDict = new();
            using (List<CardInfo>.Enumerator enumerator = RunState.DeckList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CardInfo card = enumerator.Current;
                    if (MergerManager.SourceMergeDict.ContainsKey(card.name))
                    {
                        foreach (string possibleMergeTarget in MergerManager.SourceMergeDict[card.name])
                        {
                            // Do not include duplicate merges, assuming received from original method
                            if (!String.Equals(card.name, possibleMergeTarget) && RunState.DeckList.Any((CardInfo x) => x.name.Equals(possibleMergeTarget)))
                            {
                                if (!mergeDict.ContainsKey(card.name))
                                {
                                    mergeDict.Add(card.name, new List<CardInfo>());
                                }
                                mergeDict[card.name].Add(card);
                                break;
                            }
                            /*if(String.Equals(card.name, kv.Key) && RunState.DeckList.Count((CardInfo x) => x.name.Equals(card.name)) > 1 && MergerManager.PreventingDefaultMerges)
                            {
                                Code here for adding MycoMerger duplicate merges if the original method is not run or its default DuplicateMerge behavior is prevented
                                make a prefix patch to check MergerManager.PreventingDefaultMerges and skip original method if true?
                                bool __runOriginal -> will be true for postfix if original method was run
                                __result = modifiedList? or would __result.AddRange(modifiedList) still work
                                __result if original method was not run is default for that type, usually null
                            }*/
                        }
                    }
                    if (MergerManager.TargetMergeDict.ContainsKey(card.name))
                    {
                        foreach (string possibleMergeSource in MergerManager.TargetMergeDict[card.name])
                        {
                            if (!String.Equals(card.name, possibleMergeSource) && RunState.DeckList.Any((CardInfo x) => x.name.Equals(possibleMergeSource)))
                            {
                                if (!mergeDict.ContainsKey(possibleMergeSource))
                                {
                                    mergeDict.Add(possibleMergeSource, new List<CardInfo>());
                                }
                                mergeDict[possibleMergeSource].Add(card);
                            }
                        }
                    }
                }
            }

            HashSet<Ability> hostAbilityHash = new();
            HashSet<Ability> targetAbilityHash = new();
            foreach (KeyValuePair<string, List<CardInfo>> keyValuePair in mergeDict)
            {
                List<CardInfo> targetCardList = keyValuePair.Value;
                string sourceCardName = keyValuePair.Key;
                targetCardList.Sort((CardInfo lhs, CardInfo rhs) => rhs.NumAbilities - lhs.NumAbilities);
                CardInfo sourceCardInfo = targetCardList.First((CardInfo x) => x.name.Equals(sourceCardName));
                targetCardList.RemoveAll((CardInfo x) => x.name.Equals(sourceCardName));
                hostAbilityHash.Clear();
                foreach (Ability hostAbility in sourceCardInfo.Abilities)
                {
                    hostAbilityHash.Add(hostAbility);
                }

                List<string> targetCardNameList = new();
                foreach (CardInfo targetCard in targetCardList)
                {
                    if (!targetCardNameList.Contains(targetCard.name))
                    {
                        targetCardNameList.Add(targetCard.name);
                    }
                }

                foreach (string targetCardName in targetCardNameList)
                {
                    CardInfo targetCardInfo = null;
                    int num = -1;
                    foreach (CardInfo targetCard in targetCardList)
                    {
                        if (string.Equals(targetCard.name, targetCardName))
                        {
                            targetAbilityHash.Clear();
                            foreach (Ability targetAbility in targetCard.Abilities)
                            {
                                targetAbilityHash.Add(targetAbility);
                            }
                            targetAbilityHash.UnionWith(hostAbilityHash);
                            if (num < targetAbilityHash.Count)
                            {
                                num = targetAbilityHash.Count;
                                targetCardInfo = targetCard;
                            }
                        }
                    }
                    mergeCardList.Add(sourceCardInfo);
                    mergeCardList.Add(targetCardInfo);
                }
            }

            if (__result != null)
            {
                mergeCardList.InsertRange(0, __result);
            }

            // Remove merge pairs that already exist
            List<(CardInfo, CardInfo)> mergePairsList = new();
            for (int listIndex = 0; listIndex < mergeCardList.Count; listIndex += 2)
            {
                mergePairsList.Add((mergeCardList[listIndex], mergeCardList[listIndex + 1]));
            }
            mergeCardList.Clear();
            mergePairsList = mergePairsList.Distinct(new MergePairEqualityComparer()).ToList();
            foreach ((CardInfo, CardInfo) mergePair in mergePairsList)
            {
                mergeCardList.Add(mergePair.Item1);
                mergeCardList.Add(mergePair.Item2);
            }

            __result = mergeCardList;
        }

        /// <summary>
        /// Displays valid merges when slot selected. Original method's ordering by name has been removed to preserve non-duplicate merges.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DuplicateMergeSequencer), "OnSlotSelected")]
        private static bool MycoMergerOnSlotSelected(MainInputInteractable slot, DuplicateMergeSequencer __instance)
        {
            __instance.selectionSlot.SetEnabled(false);
            __instance.selectionSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
            __instance.confirmStone.Exit();
            List<CardInfo> list = __instance.GetValidDuplicateCards();
            /* Remove ordering by name in original method
             * list = (from c in list
                    orderby c.name
                    select c).ToList<CardInfo>();*/
            (slot as SelectCardFromDeckSlot).SelectFromCards(list, new Action(__instance.OnSelectionEnded), false);
            return false;
        }

        /// <summary>
        /// Merges two cards, carrying over the sum of their attack, health, and abilities.
        /// </summary>
        /// <remarks>
        /// In the case of a merge result conflict when two cards have a different merge result for their combinations in their respective merge data, the result is random.
        /// In case of modified ability totals more than MergerManager.MaxModificationSigils for the resulting merge card, adding base/default abilities for cards contributing to the merge card before modification-added abilities, and currently adding abilities from card with more abilities first.?
        /// Special Stat Icons are worth 1 Attack.
        /// </remarks>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DuplicateMergeSequencer), "MergeCards")]
        private static bool MycoMergeCards(CardInfo card1, CardInfo card2, ref CardInfo __result)
        {
            CardInfo resultCard;
            MergeInheritance mergeInheritanceFlags;

            // Check if either are a source of MergeData
            if (MergerManager.SourceMergeDict.ContainsKey(card1.name) || MergerManager.SourceMergeDict.ContainsKey(card2.name))
            {
                // Check if both are a source of MergeData
                if (MergerManager.SourceMergeDict.ContainsKey(card1.name) && MergerManager.SourceMergeDict.ContainsKey(card2.name))
                {
                    // Both have MergerData, but does at least one of them target the other in their MergerData?
                    if (MergerManager.SourceMergeDict[card1.name].Contains(card2.name) || MergerManager.SourceMergeDict[card2.name].Contains(card1.name))
                    {
                        // Check if both cards target each other in their MergeData
                        if (MergerManager.SourceMergeDict[card1.name].Contains(card2.name) && MergerManager.SourceMergeDict[card2.name].Contains(card1.name))
                        {
                            // Both are valid sources, check if the mergeResult of their MergerData is not the same, and if it isn't pick randomly between them
                            if (!string.Equals(MergerManager.GetMergeData(card1)[card2.name], MergerManager.GetMergeData(card2)[card1.name]))
                            {
                                //Plugin.Log.LogWarning($"Different merge results in MycoMerger data of [{card1.name}] and [{card2.name}] for their combination. [{card1.name}] stored merge result is [{MergerManager.GetMergeData(card1)[card2.name]}]. [{card2.name}] stored merge result is [{MergerManager.GetMergeData(card2)[card1.name]}].");
                                if (new System.Random().Next() > (Int32.MaxValue / 2))
                                {
                                    resultCard = MergerManager.GetMergeResultInstance(card1, card2);
                                    mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card1)[card2.name]).Item2;
                                }
                                else
                                {
                                    resultCard = MergerManager.GetMergeResultInstance(card2, card1);
                                    mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card2)[card1.name]).Item2;
                                }
                                //Plugin.Log.LogInfo($"Merge result chosen randomly. Merge results in [{resultCard.name}]");
                            }
                            // Their entries targeting each other have the same mergeResult so just use the first card's
                            else
                            {
                                resultCard = MergerManager.GetMergeResultInstance(card1, card2);
                                mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card1)[card2.name]).Item2;
                            }
                        }
                        // Only one card targets the other in their MergerData, find out which. Check if card1 targets card2
                        else if (MergerManager.SourceMergeDict[card1.name].Contains(card2.name) && !MergerManager.SourceMergeDict[card2.name].Contains(card1.name))
                        {
                            resultCard = MergerManager.GetMergeResultInstance(card1, card2);
                            mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card1)[card2.name]).Item2;
                        }
                        // Because card1 does not target card2, card2 must target card1 
                        else // !MergerManager.SourceMergeDict[card1.name].Contains(card2.name) && MergerManager.SourceMergeDict[card2.name].Contains(card1.name)
                        {
                            resultCard = MergerManager.GetMergeResultInstance(card2, card1);
                            mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card2)[card1.name]).Item2;
                        }
                    }
                    // Both have MergerData but do not contain information targeting the other, perform default behavior
                    else
                    {
                        resultCard = CardLoader.Clone(MergerManager.ResolveMergeResultEnum(card1, card2, MergerManager.DefaultMergeResult));
                        mergeInheritanceFlags = MergerManager.DefaultMergeInheritance;
                    }
                }
                // Only one of them is a source of MergerData, find out which. Check if card1 has MergerData
                else if (MergerManager.SourceMergeDict.ContainsKey(card1.name) && !MergerManager.SourceMergeDict.ContainsKey(card2.name))
                {
                    // Now check if card1's MergerData targets card2
                    if (MergerManager.SourceMergeDict[card1.name].Contains(card2.name))
                    {
                        resultCard = MergerManager.GetMergeResultInstance(card1, card2);
                        mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card1)[card2.name]).Item2;
                    }
                    // It doesn't so perform default behavior
                    else
                    {
                        resultCard = CardLoader.Clone(MergerManager.ResolveMergeResultEnum(card1, card2, MergerManager.DefaultMergeResult));
                        mergeInheritanceFlags = MergerManager.DefaultMergeInheritance;
                    }
                }
                // Card1 is not the source so Card2 must have MergerData
                else // MergerManager.SourceMergeDict.ContainsKey(card2.name) && !MergerManager.SourceMergeDict.ContainsKey(card1.name)
                {
                    // Now check if card2's MergerData targets card1
                    if (MergerManager.SourceMergeDict[card2.name].Contains(card1.name))
                    {
                        resultCard = MergerManager.GetMergeResultInstance(card2, card1);
                        mergeInheritanceFlags = MergerManager.GetMergeInheritance(MergerManager.GetMergeData(card2)[card1.name]).Item2;
                    }
                    // It doesn't so perform default behavior
                    else
                    {
                        resultCard = CardLoader.Clone(MergerManager.ResolveMergeResultEnum(card1, card2, MergerManager.DefaultMergeResult));
                        mergeInheritanceFlags = MergerManager.DefaultMergeInheritance;
                    }
                }
            }
            // Neither have MergerData, perform default behavior
            else
            {
                resultCard = CardLoader.Clone(MergerManager.ResolveMergeResultEnum(card1, card2, MergerManager.DefaultMergeResult));
                mergeInheritanceFlags = MergerManager.DefaultMergeInheritance;
            }

            int resultCardAttackBuff;
            int resultCardHealthBuff;

            if (mergeInheritanceFlags.HasFlag(MergeInheritance.Attack))
            {
                resultCardAttackBuff = resultCard.Attack + (resultCard.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) >= card1.Attack + (card1.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) + card2.Attack + (card2.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) ? 0 : card1.Attack + (card1.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) + card2.Attack + (card2.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0) - resultCard.Attack - (resultCard.SpecialStatIcon != SpecialStatIcon.None ? 1 : 0);
            }
            else
            {
                resultCardAttackBuff = 0;
            }

            if (mergeInheritanceFlags.HasFlag(MergeInheritance.Health))
            {
                resultCardHealthBuff = resultCard.Health >= card1.Health + card2.Health ? 0 : card1.Health + card2.Health - resultCard.Health;
            }
            else
            {
                resultCardHealthBuff = 0;
            }

            CardModificationInfo mergeMod = DuplicateMergeSequencer.GetDuplicateMod(resultCardAttackBuff, resultCardHealthBuff);

            int resultCardModAbilitiesNum = 0;

            if (mergeInheritanceFlags.HasFlag(MergeInheritance.BaseSigils))
            {
                foreach (Ability ability in card1.DefaultAbilities)
                {
                    if (!resultCard.HasAbility(ability))
                    {
                        mergeMod.fromCardMerge = true;
                        mergeMod.abilities.Add(ability);
                        resultCardModAbilitiesNum++;
                    }
                }
                foreach (Ability ability in card2.DefaultAbilities)
                {
                    if (!resultCard.HasAbility(ability))
                    {
                        mergeMod.fromCardMerge = true;
                        mergeMod.abilities.Add(ability);
                        resultCardModAbilitiesNum++;
                    }
                }
            }

            if (mergeInheritanceFlags.HasFlag(MergeInheritance.AddedSigils))
            {
                foreach (CardModificationInfo cardModificationInfo1 in card1.Mods)
                {
                    if (cardModificationInfo1.fromCardMerge)
                    {
                        mergeMod.fromCardMerge = true;
                    }
                    foreach (Ability ability in cardModificationInfo1.abilities)
                    {
                        if (!resultCard.HasAbility(ability) && !mergeMod.HasAbility(ability) && resultCardModAbilitiesNum < MergerManager.MaxModificationSigils)
                        {
                            mergeMod.abilities.Add(ability);
                            resultCardModAbilitiesNum++;
                        }
                    }
                }
                foreach (CardModificationInfo cardModificationInfo2 in card2.Mods)
                {
                    if (cardModificationInfo2.fromCardMerge)
                    {
                        mergeMod.fromCardMerge = true;
                    }
                    foreach (Ability ability in cardModificationInfo2.abilities)
                    {
                        if (!resultCard.HasAbility(ability) && !mergeMod.HasAbility(ability) && resultCardModAbilitiesNum < MergerManager.MaxModificationSigils)
                        {
                            mergeMod.abilities.Add(ability);
                            resultCardModAbilitiesNum++;
                        }
                    }
                }
            }

            RunState.Run.playerDeck.ModifyCard(resultCard, mergeMod);
            RunState.Run.playerDeck.RemoveCard(card1);
            RunState.Run.playerDeck.RemoveCard(card2);
            RunState.Run.playerDeck.AddCard(resultCard);

            __result = resultCard;
            return false;
        }
    }
}