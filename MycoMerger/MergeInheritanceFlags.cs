using System;

namespace MycoMerger
{
    /// <summary>
    /// Determines what will be inherited by the result card of the merge.
    /// </summary>
    /// <remarks>
    /// For Attack and Health, the result card gets a buff to its attack equal to the difference of its base stat and the combined stat of the merging cards. If the combined stat is lower than the result's base stat, the result's base stat is used.
    /// For BaseSigils and AddedSigils, the base sigils of the merging cards are added first before any added or modified sigils if both flags are set.
    /// Sigils the resulting card already has will not be inherited by it.
    /// MergeData containing no MergeInheritance automatically uses the player's DefaultMergeInheritance. If that is also UserDefault, the merge uses Vanilla.
    /// Flags can be added together using the | operator: <c>MergeInheritance Vanilla = MergeInheritance.Attack | MergeInheritance.Health | MergeInheritance.AddedSigils</c> in code.
    /// Their <c>.ToString()</c> representation separates flags using a comma (<c>,</c>) but inside merge data use a plus (<c>+</c>) instead.
    /// </remarks>
    [Flags]
    public enum MergeInheritance
    {
        None         = 0,            // Inherit nothing, just give the result card back
        UserDefault  = 1 << 0,       // User's DefaultMergeInheritance setting 
        Attack       = 1 << 1,       // Inherit combined attack minus result card's base attack
        Health       = 1 << 2,       // Inherit combined health minus result card's base health
        AddedSigils  = 1 << 3,       // Inherit the modified or added sigils from the merging cards
        BaseSigils   = 1 << 4,       // Inherit the base sigils of the merging cards, inherited before AddedSigils
        
        Vanilla = Attack | Health | AddedSigils,
        Stats = Attack | Health,
        AllSigils = AddedSigils | BaseSigils,
        All = Attack | Health | AddedSigils | BaseSigils
    }
}
