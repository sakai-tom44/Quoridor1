using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace Quoridor1
{
    public class WallManager
    {
        private Board board;// ゲームの盤面データを保持するBoard

        public WallManager(Board board)
        {
            this.board = board;                 // Boardインスタンスを保持
        }

        /// <summary>
        /// 壁を設置する。
        /// </summary>
        public void SetWall(int x, int y, WallOrientation wallOrientation)
        {
            Console.WriteLine("SetWall: x={0}, y={1}, orientation={2}", x, y, wallOrientation);

            (int,int) xy1, xy2, xy3, xy4;
            (xy1, xy2, xy3, xy4) = Wall2xy4(x, y, wallOrientation); // 壁で遮断される4つのマスの座標を取得

            Disconnect(xy1, xy2, xy3, xy4); // 壁を置いて道を切断
            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
                board.verticalWalls[x, y] = board.verticalWalls[x, y + 1] = 2; // 壁を確定設置
            else // 横壁の場合
                board.horizontalWalls[x, y] = board.horizontalWalls[x + 1, y] = 2; // 壁を確定設置

        }

        /// <summary>
        /// 壁を設置する事ができるか確認。合法手ならtrue、そうでなければfalse。
        /// </summary>
        public bool CheckWall(int x, int y, WallOrientation wallOrientation)
        {
            Console.WriteLine("CheckWall: x={0}, y={1}, orientation={2}", x, y, wallOrientation);

            (int, int) xy1, xy2, xy3, xy4;
            (xy1, xy2, xy3, xy4) = Wall2xy4(x, y, wallOrientation); // 壁で遮断される4つのマスの座標を取得

            if ((x == Board.N - 1) || (y == Board.N - 1)) return false; // 盤の端には置けない

            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
            {
                if (board.verticalWalls[x, y] != 0 || board.verticalWalls[x, y + 1] != 0) return false; // 既に壁があるなら不可
            }
            else // 横壁の場合
            {
                if (board.horizontalWalls[x, y] != 0 || board.horizontalWalls[x + 1, y] != 0) return false; // 既に壁があるなら不可
            }

            // 壁を置いても道が繋がっているか確認
            return (CheckConnected(xy1, xy2, xy3, xy4)); // 道が繋がっているなら
            
        }

        /// <summary>
        /// x,y座標と壁の向きから、壁で遮断される4つのマスの座標を返す。
        /// </summary>
        private ((int,int), (int, int), (int, int), (int, int)) Wall2xy4(int x, int y, WallOrientation wallOrientation)
        {
            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
                return ((x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)); // 縦壁で遮断される4つのマスの座標
            else // 横壁の場合
                return ((x, y), (x, y + 1), (x + 1, y), (x + 1, y + 1)); // 横壁で遮断される4つのマスの座標
        }

        /// <summary>
        /// 壁を置き道を切断
        /// </summary>
        private void Disconnect((int, int) xy1, (int, int) xy2, (int, int) xy3, (int, int) xy4)
        {
            int k1 = Board.xy2to1(xy1.Item1, xy1.Item2); //　壁の座標から1次元インデックスに変換
            int k2 = Board.xy2to1(xy2.Item1, xy2.Item2);
            int k3 = Board.xy2to1(xy3.Item1, xy3.Item2);
            int k4 = Board.xy2to1(xy4.Item1, xy4.Item2);

            board.moveGraph[k1, k2] = 0; // k1とk2、k3とk4の接続を遮断
            board.moveGraph[k2, k1] = 0;
            board.moveGraph[k3, k4] = 0;
            board.moveGraph[k4, k3] = 0;
        }

        /// <summary>
        /// 壁を置いた後も道が繋がっているかチェックする。
        /// </summary>
        private bool CheckConnected((int,int) xy1, (int, int) xy2, (int, int) xy3, (int, int) xy4)
        {
            int k1 = Board.xy2to1(xy1.Item1, xy1.Item2); //　壁の座標から1次元インデックスに変換
            int k2 = Board.xy2to1(xy2.Item1, xy2.Item2);
            int k3 = Board.xy2to1(xy3.Item1, xy3.Item2);
            int k4 = Board.xy2to1(xy4.Item1, xy4.Item2);

            int[,] dummyGraph = new int[board.moveGraph.GetLength(0), board.moveGraph.GetLength(1)]; // moveGraphのコピーを作成
            Array.Copy(board.moveGraph, dummyGraph, board.moveGraph.Length); // コピーを作成

            dummyGraph[k1, k2] = 0; // k1とk2、k3とk4の接続を遮断
            dummyGraph[k2, k1] = 0;
            dummyGraph[k3, k4] = 0;
            dummyGraph[k4, k3] = 0;

            List<int> openlist = new List<int>() { k1 }; // これから調べる場所
            int[] closed = new int[Board.N * Board.N]; // 0はまだ未訪問．1は処理済．
            int c = 3; // つながっていない数(k2とk3とk4のこと）
            while (openlist.Count > 0)
            {
                int k = openlist[0]; // 先頭要素を取り出して
                openlist.RemoveAt(0);
                //Console.WriteLine("{0} を展開する", k);
                for (int i = 0; i < (Board.N * Board.N); i++) // 先頭要素からつながっているところを取り出す
                {
                    if (dummyGraph[k, i] > 0)
                    {
                        //Console.WriteLine("　つながっている　{0} -> {1}", k, i);
                        if (closed[i] == 0) // これはまだ調べていないらしい
                        {
                            openlist.Add(i); // 調べるべきリストに追加する
                        }
                        if (i == k2) // つながっていたのがk2なら
                        {
                            //Console.WriteLine("{0}->{1}は接続されていた", k1, i);
                            c--; if (c == 0) { return true; } // k2,k3につながっていた
                            k2 = -3; // k2はつながっていたので目標から削除
                        }
                        if (i == k3) // つながっていたのがk3なら
                        {
                            //Console.WriteLine("{0}->{1}は接続されていた", k1, i);
                            c--; if (c == 0) { return true; } // k2,k3につながっていた
                            k3 = -3; // k3はつながっていたので目標から削除
                        }
                        if (i == k4) // つながっていたのがk4なら
                        {
                            //Console.WriteLine("{0}->{1}は接続されていた", k1, i);
                            c--; if (c == 0) { return true; } // k2,k3につながっていた
                            k4 = -3; // k3はつながっていたので目標から削除
                        }
                    }
                }
                closed[k] = 1; // kは調べたので印をつけておく
            }
            //Console.WriteLine("接続が断たれた");
            return false;
        }
    }
}
