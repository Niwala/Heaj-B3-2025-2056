using UnityEngine;

namespace Heaj.LevelSelector
{
    public class RoomChances : ScriptableObject
    {
        public Texture2D texture;
        public Vector2Int spriteCount;
        public Room[] rooms = new Room[0];

        public Vector4 GetRoomUvs(int roomID)
        {
            return GetUvs(rooms[roomID].spriteID);
        }

        public Vector4 GetUvs(int spriteID)
        {
            float x = (spriteID % spriteCount.x) / (float)spriteCount.x;
            float y = (spriteID / spriteCount.y) / (float)spriteCount.y;
            return new Vector4(x, y, x + 1.0f / spriteCount.x, y + 1.0f / spriteCount.y);
        }
    }

    [System.Serializable]
    public struct Room
    {
        public string name;
        public int spriteID;
        public RoomType type;
        public int chances;
        public Vector2Int requireCount;
    }

    public enum RoomType
    {
        Default,
        Required,
        Start,
        End
    }
}