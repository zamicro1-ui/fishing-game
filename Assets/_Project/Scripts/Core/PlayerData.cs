namespace HolyMackerel.Core
{
    /// <summary>
    /// Plain serializable container for persistent player progress. Mutated by
    /// gameplay systems and round-tripped through JSON by <see cref="GameManager"/>.
    /// Field names are part of the on-disk save format — renaming them will
    /// invalidate existing saves.
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public int coins = 0;
        public int boatLevel = 1;
        public int depthLevel = 1;
        public int baitLevel = 1;
    }
}
