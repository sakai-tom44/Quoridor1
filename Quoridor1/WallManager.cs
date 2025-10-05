using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace Quoridor1
{
    public class WallManager
    {
        private Board board; // 壁マネージャが管理する盤面

        public WallManager(Board board)
        {
            this.board = board;
        }
        /// <summary>
        /// 壁を設置する。
        /// </summary>
        public void PlaceWall(int x, int y, WallOrientation wallOrientation)
        {
            //Console.WriteLine("PlaceWall: x={0}, y={1}, orientation={2}", x, y, wallOrientation);
            if (!CheckWall(x, y, wallOrientation, board.moveGraph, board.player[board.currentPlayerNumber].placeWallCount, board.verticalWalls, board.horizontalWalls))
            {
                //Console.WriteLine("壁の設置に失敗");
                return; // 壁の設置に失敗
            }

            (int,int) xy1, xy2, xy3, xy4;
            (xy1, xy2, xy3, xy4) = Wall2xy4(x, y, wallOrientation); // 壁で遮断される4つのマスの座標を取得

            board.moveGraph = Disconnect(xy1, xy2, xy3, xy4, board.moveGraph); // 壁を置いて道を切断
            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
                board.verticalWalls[x, y] = board.verticalWalls[x, y + 1] = 2; // 壁を確定設置
            else // 横壁の場合
                board.horizontalWalls[x, y] = board.horizontalWalls[x + 1, y] = 2; // 壁を確定設置

            board.player[board.currentPlayerNumber].placeWallCount++; // 現在のプレイヤーの設置した壁の数を増やす
        }

        /// <summary>
        /// 壁を設置する事ができるか確認。合法手ならtrue、そうでなければfalse。
        /// </summary>
        public bool CheckWall(int x, int y, WallOrientation wallOrientation, int[,] moveGraph, int placeWallCount, int[,] verticalWalls, int[,] horizontalWalls)
        {
            //Console.WriteLine("CheckWall: x={0}, y={1}, orientation={2}", x, y, wallOrientation);

            if (placeWallCount >= Board.wallCount) return false; // 既に壁を置き切っているなら不可

            (int, int) xy1, xy2, xy3, xy4;
            (xy1, xy2, xy3, xy4) = Wall2xy4(x, y, wallOrientation); // 壁で遮断される4つのマスの座標を取得

            if ((x == Board.N - 1) || (y == Board.N - 1)) return false; // 盤の端には置けない

            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
            {
                if (verticalWalls[x, y] != 0 || verticalWalls[x, y + 1] != 0) return false; // 既に壁があるなら不可
            }
            else // 横壁の場合
            {
                if (horizontalWalls[x, y] != 0 || horizontalWalls[x + 1, y] != 0) return false; // 既に壁があるなら不可
            }

            // 壁を置いても道が繋がっているか確認
            return (CheckConnected(xy1, xy2, xy3, xy4, moveGraph)); // 道が繋がっているなら
            
        }

        /// <summary>
        /// x,y座標と壁の向きから、壁で遮断される4つのマスの座標を返す。
        /// </summary>
        public ((int,int), (int, int), (int, int), (int, int)) Wall2xy4(int x, int y, WallOrientation wallOrientation)
        {
            if (wallOrientation == WallOrientation.Vertical) // 縦壁の場合
                return ((x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)); // 縦壁で遮断される4つのマスの座標
            else // 横壁の場合
                return ((x, y), (x, y + 1), (x + 1, y), (x + 1, y + 1)); // 横壁で遮断される4つのマスの座標
        }

        /// <summary>
        /// 壁を置いた場合の移動グラフを返す。
        /// </summary>
        /// <param name="moveGraph">元の移動グラフ</param>
        /// <returns>壁を置いた後の移動グラフ</returns>
        public int[,] Disconnect((int, int) xy1, (int, int) xy2, (int, int) xy3, (int, int) xy4, int[,] moveGraph)
        {
            int[,] copyGraph = new int[moveGraph.GetLength(0), moveGraph.GetLength(1)]; // moveGraphのコピーを作成
            Array.Copy(moveGraph, copyGraph, moveGraph.Length); // コピーを作成

            int k1 = board.xy2to1(xy1.Item1, xy1.Item2); //　壁の座標から1次元インデックスに変換
            int k2 = board.xy2to1(xy2.Item1, xy2.Item2);
            int k3 = board.xy2to1(xy3.Item1, xy3.Item2);
            int k4 = board.xy2to1(xy4.Item1, xy4.Item2);

            copyGraph[k1, k2] = 0; // k1とk2、k3とk4の接続を遮断
            copyGraph[k2, k1] = 0;
            copyGraph[k3, k4] = 0;
            copyGraph[k4, k3] = 0;

            return copyGraph;
        }

        /// <summary>
        /// 壁を置いた後も道が繋がっているかチェックする。
        /// </summary>
        private bool CheckConnected((int,int) xy1, (int, int) xy2, (int, int) xy3, (int, int) xy4, int[,] moveGraph)
        {
            int k1 = board.xy2to1(xy1.Item1, xy1.Item2); //　壁の座標から1次元インデックスに変換
            int k2 = board.xy2to1(xy2.Item1, xy2.Item2);
            int k3 = board.xy2to1(xy3.Item1, xy3.Item2);
            int k4 = board.xy2to1(xy4.Item1, xy4.Item2);

            int[,] dummyGraph = new int[moveGraph.GetLength(0), moveGraph.GetLength(1)]; // moveGraphのコピーを作成
            Array.Copy(moveGraph, dummyGraph, moveGraph.Length); // コピーを作成

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

        /// <summary>
        /// 現在の壁を置ける場所を再計算して更新。
        /// </summary>
        public void RefreshWallMountable()
        {
            (board.verticalMountable, board.horizontalMountable) = WallMountable(board.moveGraph, board.player[board.currentPlayerNumber].placeWallCount, board.verticalWalls, board.horizontalWalls); // 壁を置けるか確認して更新            
        }

        /// <summary>
        /// 壁を置ける場所を再計算して返す。
        /// </summary>
        /// <param name="moveGraph">移動グラフ</param>
        /// <param name="placeWallCount">プレイヤーが置いた壁の数</param>
        /// <param name="verticalWalls">縦壁の配置</param>
        /// <param name="horizontalWalls">横壁の配置</param>
        /// <returns>縦壁と横壁の設置可能位置のbool配列</returns>
        public (bool[,], bool[,]) WallMountable(int[,] moveGraph, int placeWallCount, int[,] verticalWalls, int[,] horizontalWalls)
        {
            bool[,] horizontal = new bool[Board.N, Board.N];
            bool[,] vertical = new bool[Board.N, Board.N];
            for (int x = 0; x < Board.N - 1; x++) // 端には置けないので-1まで
                for (int y = 0; y < Board.N - 1; y++) // 端には置けないので-1まで
                {
                    horizontal[x, y] = CheckWall(x, y, WallOrientation.Horizontal, moveGraph, placeWallCount, verticalWalls, horizontalWalls) ? true : false; // 横壁を置けるか確認
                    vertical[x, y] = CheckWall(x, y, WallOrientation.Vertical, moveGraph, placeWallCount, verticalWalls, horizontalWalls) ? true : false; // 縦壁を置けるか確認
                }
            return (vertical, horizontal);
        }
    }
    /// <summary>
    /// 壁の方向を表す列挙型
    /// </summary>
    public enum WallOrientation { Horizontal, Vertical}
}
