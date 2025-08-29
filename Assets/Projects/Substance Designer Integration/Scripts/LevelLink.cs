namespace Heaj.LevelSelector
{
    public struct LevelLink
    {
        public int from;
        public int to;

        public LevelLink(int from, int to)
        {
            this.from = from;
            this.to = to;
        }
    }
}