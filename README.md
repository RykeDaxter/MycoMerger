MycoMerger
===============

Extends Mycologists functionality to allow non-duplicate merges and unique merge results. Supports use by other mods including JSONCardLoader.

## Features

* Merge two different cards, not just duplicates, for a specified result! 
* Change the maximum number of sigils that can be added to a card through merging.
* Change what is inherited by the result from the cards used in merging.
* Cards and mods can override a user's default settings for merges if the user doesn't disallow overriding in their configuration. (It is allowed by default.) Specific settings of a merge when available are always used over the default.

## How to use

Simply install if you're not making new cards or mods. If you are, see below.

<details>
<summary>With your mod
</summary>

* If you include MycoMerger in your project, you can use the `MergerManager` class to do:  
`MergerManager.AddMergeData("SourceCardName", "TargetCardName", "Result")`
* You can also specify `MergeInheritance` for a merge using:  
`MergerManager.AddMergeData("SourceCardName", "TargetCardName", "Result", "MergeInheritance")`
* If you don't include MycoMerger but want to add the option for any users who do install it, you can set an extended property for cards using the InscryptionAPI CardInfo extension with the following format:   
`SourceCardInfo.SetExtendedProperty("MycoMerger", new Dictionary<string, string> { "TargetCard1": "Result1", "TargetCard2": "Result2:MergeInheritance" … } )`
</details>

<details>
<summary>With JSON Loader (jldr2)
</summary>

* In your card's `.jldr2` file, add an entry to its `extensionProperties` with the following format: `"extensionProperties": { "MycoMerger": "[MERGEDATA]" }`.
* If there are other entries in your card's extensionProperties, it will look like this: `"extensionProperties": { "Property1": "Value1", "Property2": "Value2", "MycoMerger": "[MERGEDATA]" }`.
* `[MERGEDATA]` is composed of the following: `TargetCard:Result` with each entry separated by commas. Spaces between don't matter. Example: `Packrat:Cat, FieldMouse : FieldMouse`.
* `MergeInheritance` can be specified by adding another colon `:` and a plus sign `+` if there is more than one keyword. See **References** further below. Example:  
`Packrat:Cat:UserDefault, FieldMouse : FieldMouse : Attack + Health`.
* If the card you're making should be the result of a merge, you can use this special format: `!RESULT!:SourceCard:TargetCard`.
* `!RESULT!` means this card is the result of the merge of two other cards. This MergeData is then assigned to the SourceCard. This is mostly meant for making a new card using jldr2 the result of a merge without needing to make files for or edit other cards. This means that if you're making a new card and use this format, it's equivalent to adding this merge data in SourceCard: `TargetCard:NewCard`.
* Example in FieldMouse.jldr2:   
`"extensionProperties": { "MycoMerger": "FieldMouse:SporeMouse" }`
* Example `!RESULT!` in SporeMouse.jldr2:  
`"extensionProperties": { "MycoMerger": "!RESULT!:FieldMouse:FieldMouse" }`
* Example `MergeInheritance` in Stoat.jldr2:  
`"extensionProperties": { "MycoMerger": "Burrow:Rabbit:None" }`
* Example of other valid formatting with RatKing.jldr2:
> "extensionProperties": {  
> "OtherProperty1": "OtherValue1",  
> "MycoMerger": "!RESULT!:FieldMouse:FieldMouse,  
> !RESULT!:PackRat:PackRat:UserDefault,  
> !RESULT! : FieldMouse : PackRat : Attack+Health+AddedSigils, !RESULT!: RatKing :FieldMouse,  
> PackRat:RatKing:Vanilla, Hodag:MycoMerger_TotalStats:All  
> RatKing:RatEmperor",  
> "OtherProperty2: "OtherValue2" }
</details>

<details>
<summary>Notes
</summary>

* `SourceCard` just means the card where the merge data is stored.
* `TargetCard` is the card targeted for the merge from the `SourceCard`.
* `Result` is either another card or a `MergeResult` that determines which between the two cards used in the merge should be the result. It can also have a `MergeInheritance` specified. 
* A `MergeResult` is a special `Result` that chooses between the two cards used in the merge to be the result based on certain criteria. `UserDefault` can be used, which is `TotalStats` by default. See **References** further below.
* `MergeInheritance` specifies what is inherited by the `Result`. Vanilla inheritance is Attack+Health+AddedSigils. When not specified, uses `UserDefault` which is Vanilla by default. See **References** further below. 
* You only need to add the data for the merge you want to one of the cards involved in the merge.
* Merge data in a SourceCard may be overridden by new merge data, specifically when a TargetCard is specified for a new result. This may happen if several mods or cards target the same card combination.
* If there is valid merge data in multiple cards that specify the same two cards for a merge but the results are different, the result is randomly chosen between those results.
* Adding merge data to a card should not conflict with anything else, but the MycoMerger mod may conflict with any mod that makes changes to the Mycologists event.
* Doubled sigils are not implemented in merge inheritance because there's no point in getting more than one copy of a sigil most of the time. For it to be useful, specific sigils need to be included or excluded and I think it's better to just make a new card that have duplicated base sigils.
* Settings can be overriden by mods for more general use instead of only for specific combinations if the user does not disallow it. An example for use is to make custom challenges like one where only health and attack is inherited with merges but not the sigils.
* `!RESULT!` formatting is applicable for all use but is mostly meant for jldr2 files (see the 'With JSON Loader (jldr2)' section).
* Resulting cards from a merge of two different cards only inherit the added sigils of the cards that went into them by default but this can be changed by changing the user's setting of `DefaultMergeInheritance` to `All` or specifying `BaseSigils` as a MergeInheritance flag in merge data.
</details>

## Configuration

You can adjust the mod's configuration using your mod manager's Config editor or by manually editing the `rykedaxter.inscryption.mycologistsmerger.cfg` file in `/Bepinex/config`.

## Installation

#### Automatic

You can use the [Thunderstore](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager), [r2modman](https://inscryption.thunderstore.io/package/ebkr/r2modman/), or another compatible mod manager to automatically install this mod from the [Thunderstore page](https://inscryption.thunderstore.io/package/RykeDaxter/KCTalkingCards/).

#### Manual

You can manually install this mod by downloading the dll from the [Thunderstore page](https://inscryption.thunderstore.io/package/RykeDaxter/MycoMerger/) or the latest [Github release](https://github.com/RykeDaxter/MycoMerger) then placing `MycoMerger.dll` into `/BepInEx/plugins` assuming you have already installed the required dependencies ([BepInEx-BepInExPack_Inscryption](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) and [API_dev-API](https://inscryption.thunderstore.io/package/API_dev/API/)). For instructions on how to install these dependencies, please refer to their respective pages.

## References

<details>
<summary>Configs:
</summary>

|Section|Key|Default Value|Description|
|:-|:-|:-:|:-|
|General|MaxAddedSigils|4|The maximum number of added abilities the resulting card of a Mycologist merge can have. May be overridden by mods if MaxAddedSigilsOverride is true.|
|General|DefaultMergeResult|TotalStats|Defines the default result of a merge that occurs when the result is not specified. May be overridden by mods if DefaultMergeResultOverride is true. No practical effect in current version.|
|General|DefaultMergeInheritance|Vanilla|Defines the default inheritance the result of a merge receives from the two cards used for it.May be overridden by mods if DefaultMergeInheritanceOverride is true.You can set a combination of Attack, Health, AddedSigils, and BaseSigils using commas (,). (Example: Attack, BaseSigils)|
|Overrides|MaxAddedSigilsOverride|True|Allow mods to override your personal MaxAddedSigils setting when true. If false, always use your MaxAddedSigils setting.|
|Overrides|DefaultMergeResultOverride|True|Allow mods to override your personal DefaultMergeResult setting when true. If false, always use your DefaultMergeResult setting.|
|Overrides|DefaultMergeInheritanceOverride|True|Allow mods to override your personal DefaultMergeInheritance setting when true. If false, always use your DefaultMergeResult setting.|


</details>

<details>
<summary>MergeResult:
</summary>

You can specify these keywords instead of a card name for the merge result in order to dynamically choose which between the two cards involved in the merge will be the result. When no merge data is associated with a merge, UserDefault will typically be used to define the result. In the case of a tie, the result is equivalent to SourceCard.

Additionally, when both cards contain valid merge data targeting the other yet specify different results then the result of the merge is chosen randomly between them.

#### MergeResult

|Name|Result of Merge|
|:-|:-|
|MycoMerger_SourceCard|The card containing the merge data used or the left/first card, which is usually the source card, when neither card has valid merge data.|
|MycoMerger_UserDefault|Uses the player's DefaultMergeResult setting to define the result of the merge. That setting is one of the other constants in this table and if DefaultMergeResult is also UserDefault, then the result is equal to SourceCard.|
|MycoMerger_Random|Randomly choose between the two cards used in the merge to be the result.|
|MycoMerger_NumSigils|The card with the higher total number of sigils is the result.|
|MycoMerger_NumBaseSigils|The card with the higher total of base or default sigils, not including modified sigils, is the result. |
|MycoMerger_Attack|The card with the higher attack is the result. Special Stat Icons are always counted as 1 Attack.|
|MycoMerger_Health|The card with the higher health is the result.|
|MycoMerger_TotalStats|The card with the higher health and attack added together, including the Special Stat Icon, is the result.|
|MycoMerger_PowerLevel|(Attack + Special Stat Icon Value)*2 + Health + [Total Sigil Power Level](https://inscryption.fandom.com/wiki/Sigils#Sigil_Power_Level)|
</details>

<details>
<summary>MergeInheritance:
</summary>

Determines what will be inherited by the result card of the merge.

You can specify these keywords in the merge data to define inheritance behavior (see **How to Use** for more information). UserDefault will be used to define the inheritance when there is no specific inheritance defined. 

In code, flags can be added together using the | operator:   `MergeInheritance Vanilla = MergeInheritance.Attack | MergeInheritance.Health | MergeInheritance.AddedSigils`

Their `.ToString()` representation separates flags using a comma `,` which is used in the Configuration but inside merge data as a string, which is also for `.jldr2`, use a plus (`+`) instead:
`Attack+Health+AddedSigils`

#### MergeInheritance
|Name|Inherited|
|:-|:-|
|None|Inherit nothing, just give the result card.|
|UserDefault|Uses the player's DefaultMergeInheritance setting. That setting is one of the other constants in this table and if DefaultMergeInheritance is also UserDefault, then it uses Vanilla.|
|Attack|Inherit combined attack minus result card's base attack. If combined attack is lower, use result card's base attack.|
|Health|Inherit combined health minus result card's base health. If combined health is lower, use result card's base health.|
|AddedSigils|Inherit the modified or added sigils from the merging cards.|
|BaseSigils|Inherit the base sigils of the merging cards, inherited first before AddedSigils which is relevant based on the user's maximum number of sigils.|
|Vanilla|Equivalent to Attack + Health + AddedSigils.|
|Stats|Equivalent to Attack + Health.|
|AllSigils|Equivalent to AddedSigils + BaseSigils.|
|All|Equivalent to Attack + Health + AddedSigils + BaseSigils.|
</details>

</details>

## Future

I'm not actively working on this anymore except for maintenance and bug fixes. 

This is mostly complete except for one feature that would allow for any two cards to be merged with a selection sequence similar to the the mysterious stones sacrifice event. However, that requires Unity asset editing and problem solving which I am unable to do. If you're able to solve this and are interested in contributing, please contact me. 

<details>
<summary>Further information on the AnyMerge feature.
</summary>

It would modify the Mycologists Event (`DuplicateMerge`) to function similarly to the Ritual Stones Event (`CardMerge`) where there are two selectable slots and selecting one allows you to choose any card from your deck, thus enabling you to merge any two cards.

It involves modifying `GameTable/SpecialNodeHandler/DuplicateMerger/LargeMushroom/Anim/RitualStone` to have two of `SelectionSlot` or adding something similar from the `CardMerge` event that has `GameTable/SpecialNodeHandler/CardMerger/StoneCircle/Back Rock/HostSlot` and `GameTable/SpecialNodeHandler/CardMerger/StoneCircle/RitualStone/SacrificeSlot` to the `DuplicateMerger` event.

The logic for the feature is already implemented (`DefaultMergeResult`).
</details>

If you find an issue or have suggestions, please feel free to contact me on the [Inscryption Modding Discord](https://discord.gg/ZQPvfKEpwM) or create an Issue on the [Github repository](https://github.com/RykeDaxter/MycoMerger).

## Credits

This mod created by RykeDaxter.

## Changelog

<details>
<summary>Changelog
</summary>

### 1.0.0 
- **Release**

</details>