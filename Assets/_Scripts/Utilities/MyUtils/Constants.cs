public static class Constants
{
    public static class MapInfo
    {
        public const int GridSize = 1;
        public const int PixelPerUnit = 32;
        public const float MainRoomThreshold = 1.1f;
        public const float TriangleThreshold = 23f;
        public const float WallIsoOffset = 0.46f;
    }

    public static class InjectID
    {
        public const string Player = "Player";
    }

    public enum DungeonRoomType
    {
        NA,  // Not Assigned
        Entry,
        Boss,
        Shop,
        MiniBoss
    }

    public static class TagName
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Wall = "Wall";
    }
}
