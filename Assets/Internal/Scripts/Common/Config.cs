
public class Config
{
    public const string TILE_NAME_WATER = "Water";
    public const string TILE_NAME_OBSTACLE = "Obstacle";
    public const string TILE_NAME_BUILDING = "Building";
    public const string TILE_NAME_PATH = "Path";
    public const string TILE_NAME_WIRE = "Wire";
}

public enum PartType
{
    Wire = 0,
    Transformer,
    PowerPole
}

public enum HouseType
{
    None = 0,
    BlueHouse,
    RedHouse,
}

public enum TileType
{
    Empty = 0,
    Ground,
    Path,
    Water,
    Obstacle,
    Building,
    Wire
}