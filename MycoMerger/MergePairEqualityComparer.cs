using DiskCardGame;
using System.Collections.Generic;

namespace MycoMerger
{
    class MergePairEqualityComparer : IEqualityComparer<(CardInfo, CardInfo)>
    {
        public bool Equals((CardInfo, CardInfo) mergePair1, (CardInfo, CardInfo) mergePair2)
        {
            if (string.Equals(mergePair1.Item1.name, mergePair2.Item1.name)
                && string.Equals(mergePair1.Item2.name, mergePair2.Item2.name))
                return true;
            else if (string.Equals(mergePair1.Item1.name, mergePair2.Item2.name)
                && string.Equals(mergePair1.Item2.name, mergePair2.Item1.name))
                return true;
            else
                return false;
        }

        public int GetHashCode((CardInfo, CardInfo) mergePair)
        {
            int hCode = string.Compare(mergePair.Item1.name, mergePair.Item2.name) <= 0 ? string.Concat(mergePair.Item1.name, mergePair.Item2.name).GetHashCode() : string.Concat(mergePair.Item2.name, mergePair.Item1.name).GetHashCode();
            return hCode.GetHashCode();
        }
    }
}
