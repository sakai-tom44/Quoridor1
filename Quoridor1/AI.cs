using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;

namespace Quoridor1
{
    public class AI
    {
        private Board board; // ゲームボードのインスタンス

        Random rand = new Random(); // ランダム数生成器
        public AI(Board board)
        {
            this.board = board; // Boardインスタンスを保持
        }

        /// <summary>
        /// 次の一手を計算し、AIプレイヤーが移動。
        /// </summary>
        public bool MakeMove(int playerNumber)
        {
            if (board.player[board.currentPlayer].playerType == PlayerType.Random)
                return RandomMove(playerNumber);


            return false; // 該当する操作方法がない場合
        }

        private bool RandomMove(int playerNumber)
        {
            // 現在のプレイヤーと相手プレイヤーを取得
            Player currentPlayer = board.player[board.currentPlayer];
            Player opponentPlayer = board.player[1 - board.currentPlayer];
            int maxH = board.horizontalMountable.Cast<bool>().Count(x => x); // 設置可能な横壁の数
            int maxV = board.verticalMountable.Cast<bool>().Count(x => x);   // 設置可能な縦壁の数
            int maxWall = maxH + maxV; // 設置可能な壁の数
            // 移動可能なマスを取得
            List<(int, int)> possibleMoves = currentPlayer.possibleMoves;
            if ((possibleMoves.Count == 0 || rand.Next(2) == 0) && maxWall > 0) // 移動できるマスがない場合、または50%の確率、かつ設置可能な壁がある場合
            {
                // 壁をランダムに設置
                int r = rand.Next(maxWall); // ランダムに設置位置を選択
                if (maxV > r)
                {
                    for (int x = 0; x < Board.N - 1; x++)
                    {
                        for (int y = 0; y < Board.N - 1; y++)
                        {
                            if (board.verticalMountable[x, y])
                            {
                                if (r == 0)
                                {
                                    board.wallManager.PlaceWall(x, y, WallOrientation.Vertical);
                                    // 手番を交代
                                    board.NextPlayer();
                                    return true; // 壁の設置が成功したことを示す
                                }
                                r--;
                            }
                        }
                    }
                }
                else
                {
                    r -= maxV;
                    for (int x = 0; x < Board.N - 1; x++)
                    {
                        for (int y = 0; y < Board.N - 1; y++)
                        {
                            if (board.horizontalMountable[x, y])
                            {
                                if (r == 0)
                                {
                                    board.wallManager.PlaceWall(x, y, WallOrientation.Horizontal);
                                    // 手番を交代
                                    board.NextPlayer();
                                    return true; // 壁の設置が成功したことを示す
                                }
                                r--;
                            }
                        }
                    }
                }
            }
            else
            {
                // ランダムに移動先を選択
                var move = possibleMoves[rand.Next(possibleMoves.Count)];
                currentPlayer.x = move.Item1;
                currentPlayer.y = move.Item2;
                // 手番を交代
                board.NextPlayer();
                return true; // 移動が成功したことを示す
            }

            return false; // 移動できるマスがない場合
        }
    }
}
