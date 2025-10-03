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
        /// 縦壁を設置する。合法手ならtrue、そうでなければfalse。
        /// </summary>
        public bool SetVerticalWall(int x, int y)
        {
            if ((x == Board.N - 1) || (y == Board.N - 1)) return false; // 盤の端には置けない
            if (board.verticalWalls[x, y] != 0 || board.verticalWalls[x, y + 1] != 0) return false; // 既に壁があるなら不可

            // 壁を仮置き：隣接関係を遮断
            board.moveGraph[Board.xy2to1(x, y), Board.xy2to1(x + 1, y)] = 0;
            board.moveGraph[Board.xy2to1(x + 1, y), Board.xy2to1(x, y)] = 0;
            board.moveGraph[Board.xy2to1(x, y + 1), Board.xy2to1(x + 1, y + 1)] = 0;
            board.moveGraph[Board.xy2to1(x + 1, y + 1), Board.xy2to1(x, y + 1)] = 0;

            // 壁を置いても道が繋がっているか確認
            if (CheckConnected(Board.xy2to1(x, y), Board.xy2to1(x + 1, y), Board.xy2to1(x, y + 1), Board.xy2to1(x + 1, y + 1)))
            {
                board.verticalWalls[x, y] = board.verticalWalls[x, y + 1] = 2; // 壁を確定設置
                return true;
            }
            // 不正なら元に戻す
            board.moveGraph[Board.xy2to1(x, y), Board.xy2to1(x + 1, y)] = 1;
            board.moveGraph[Board.xy2to1(x + 1, y), Board.xy2to1(x, y)] = 1;
            board.moveGraph[Board.xy2to1(x, y + 1), Board.xy2to1(x + 1, y + 1)] = 1;
            board.moveGraph[Board.xy2to1(x + 1, y + 1), Board.xy2to1(x, y + 1)] = 1;
            return false;
        }

        /// <summary>
        /// 横壁を設置する。合法手ならtrue、そうでなければfalse。
        /// </summary>
        public bool SetHorizontalWall(int x, int y)
        {
            if ((x == Board.N - 1) || (y == Board.N - 1)) return false; // 盤の端には置けない
            if (board.horizontalWalls[x, y] != 0 || board.horizontalWalls[x + 1, y] != 0) return false; // 既に壁があるなら不可

            // 壁を仮置き：隣接関係を遮断
            board.moveGraph[Board.xy2to1(x, y), Board.xy2to1(x, y + 1)] = 0;
            board.moveGraph[Board.xy2to1(x, y + 1), Board.xy2to1(x, y)] = 0;
            board.moveGraph[Board.xy2to1(x + 1, y), Board.xy2to1(x + 1, y + 1)] = 0;
            board.moveGraph[Board.xy2to1(x + 1, y + 1), Board.xy2to1(x + 1, y)] = 0;

            // 壁を置いても道が繋がっているか確認
            if (CheckConnected(Board.xy2to1(x, y), Board.xy2to1(x, y + 1), Board.xy2to1(x + 1, y), Board.xy2to1(x + 1, y + 1)))
            {
                board.horizontalWalls[x, y] = board.horizontalWalls[x + 1, y] = 2; // 壁を確定設置
                return true;
            }
            // 不正なら元に戻す
            board.moveGraph[Board.xy2to1(x, y), Board.xy2to1(x, y + 1)] = 1;
            board.moveGraph[Board.xy2to1(x, y + 1), Board.xy2to1(x, y)] = 1;
            board.moveGraph[Board.xy2to1(x + 1, y), Board.xy2to1(x + 1, y + 1)] = 1;
            board.moveGraph[Board.xy2to1(x + 1, y + 1), Board.xy2to1(x + 1, y)] = 1;
            return false;
        }

        /// <summary>
        /// 壁を置いた後も道が繋がっているかチェックする。
        /// </summary>
        private bool CheckConnected(int k1, int k2, int k3, int k4)
        {
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
                    if (board.moveGraph[k, i] > 0)
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
