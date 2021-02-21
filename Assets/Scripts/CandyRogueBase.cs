public enum eDir //移動方向.
{
    Left,   //左.
    Up,     //上.
    Right,  //右.
    Down    //下.
}
public enum eLen //移動量(グリッド単位).
{
    One = 1,    //1
    Two = 2,    //2
    Three = 3   //3
}
public struct Vec2D //eDir，eLenを合わせたベクトル.
{
    public eDir dir;
    public eLen len;
    public Vec2D(eDir d, eLen l)
    {
        dir = d;
        len = l;
    }
}
public struct Pos2D //グリッド座標.
{
    public int x;
    public int y;
    public Pos2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
public enum eMapGimmick //ダンジョンマップのギミック.
{  
    Null = -1,  //未定義・領域外.
    Wall = 0,   //壁.
    Floor = 1,  //床.
    Stair = 2   //階段.
}
