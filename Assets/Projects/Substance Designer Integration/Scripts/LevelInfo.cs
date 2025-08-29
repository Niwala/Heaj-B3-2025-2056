using UnityEngine;

namespace Heaj.LevelSelector
{
    public struct LevelInfo
    {
        public LevelType type;
        public int depth;
        public int height;

        public Vector2 center;
        public Vector3 position => new Vector3(center.x, 0, center.y);

        public LevelInfo(LevelType type, int depth, int height)
        {
            this.type = type;
            this.depth = depth;
            this.height = height;
            this.center = new Vector2(depth, height);
        }
    }

    public enum LevelType
    {
        Start,
        Chest,
        Combat,
        Shop,
        Fire,
        Boss,
        BigBoss
    }
}