namespace MycoMerger
{
    /// <summary>
    /// Enumeration of MycoMerger constants to define result of the merge when a result card is not specified.
    /// </summary>
    /// <remarks>
    /// Specified in a card's MergeData or the MycoMerger extended property formatted string with a prefix of "MycoMerger_" followed by the Enum name.
    /// When no result card is specified and a merge occurs, the merge behavior follows the player's DefaultMergeResult setting.
    /// The UserDefault constant specifies to use the player's DefaultMergeResult setting. If that is also UserDefault, then the result is equal to SourceCard.
    /// SourceCard refers to the source of the merge data. When both cards contain merge data, it's usually the left or first card that's chosen, except in the case where the merge data of both cards are valid yet specify different results then the result of the merge is chosen randomly between them.
    /// In the case of a tie of the chosen criteria, the result is equal to SourceCard. If a Special Stat Icon exists, it's considered as a value of 1 for Attack. This may lead to unintuitive results for modded special stat icons.
    /// NumBaseSigils is the number of base or default sigils the card has while NumSigils includes both base and added sigils. 
    /// </remarks>
    public enum MergeResult
    {
        SourceCard,      // Source of MergeData
        UserDefault,     // User's DefaultMergeResult setting 
        Random,          // Random
        NumSigils,       // Total Number of Sigils
        NumBaseSigils,   // Number of Base Sigils
        Attack,          // Attack, + 1:0 Special Stat Icon (Special Stat Icons are considered 1 Attack)
        Health,          // Health
        TotalStats,      // Attack + 1:0 Special Stat Icon + Health
        PowerLevel       // (Attack + 1:0 Special Stat Icon) * 2 + Health + SigilPower (Sigils have their own power level defined in game)
    }
}
