using System;

namespace CandyRogueBase
{
    [System.Serializable]
    public enum eDir //移動方向.
    {
        Left,   //左.
        Up,     //上.
        Right,  //右.
        Down    //下.
    }
    public enum eLen //移動量(グリッド単位).
    {
        Zero = 0,   //0
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
        public static (float, float) ToUnitPos2D(eDir d)
        {
            float dirXf;
            float dirYf;
            switch (d)
            {
                case eDir.Left: //左
                    dirXf = -1f;
                    dirYf = 0f;
                    break;
                case eDir.Up:   //上
                    dirXf = 0f;
                    dirYf = 1f;
                    break;
                case eDir.Right://右
                    dirXf = 1f;
                    dirYf = 0f;
                    break;
                default:        //下
                    dirXf = 0f;
                    dirYf = -1f;
                    break;
            }
            return (dirXf, dirYf);
        }
        public static Pos2D ToPos2D(eDir d, eLen l)
        {
            var (dirXf, dirYf) = ToUnitPos2D(d);
            int dirXi = (int)Math.Round(dirXf * (int)l);
            int dirYi = (int)Math.Round(dirYf * (int)l);
            return new Pos2D(dirXi, dirYi);
        }
        public Pos2D ToPos2D()
        {
            return ToPos2D(dir, len);
        }
    }

    [System.Serializable]
    public struct Pos2D //グリッド座標.
    {
        public int x;
        public int y;
        public Pos2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public static Pos2D operator +(Pos2D p1, Pos2D p2)
        {
            return new Pos2D(p1.x + p2.x, p1.y + p2.y);
        }
        public static Pos2D operator *(int n, Pos2D p)
        {
            return new Pos2D(n * p.x, n * p.y);
        }
    }

    public enum eAct
    {
        NoMove
    }
    public struct Behavior
    {
        public bool isMove;
        public Vec2D move;
        public eAct act;
        public Behavior(bool isMove, Vec2D? move = null, eAct act = eAct.NoMove) //null許容型: intや構造体などの値型はnullを入れられないが，?をつけることでnullを入れられるようになる.
        {
            this.isMove = isMove;
            this.move = move ?? new Vec2D(eDir.Up, eLen.One); //null非許容型にnull許容型を代入するときは，??をつけて後ろにnullだった時に代入するものを書く.
            this.act = act;
        }
    }

    public enum eMapGimmick //ダンジョンマップのギミック.
    {
        Null = -1,  //未定義・領域外.
        Wall = 0,   //壁.
        Floor = 1,  //床.
        Stair = 2   //階段.
    }
}