using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Quoridor1
{
    public class AI
    {
        private static Random rand = new Random(); // ランダム数生成器

        /// <summary>
        /// 次の一手を計算し、AIプレイヤーが移動。
        /// </summary>
        public static bool ComputeNextAction(Board board, int playerNumber)
        {
            if (board.gameOver) return false; // ゲーム終了後は動かない

            if (board.player[board.currentPlayerNumber].playerType == PlayerType.Random)
                return RandomAction(board);
            if (board.player[board.currentPlayerNumber].playerType == PlayerType.AI)
                return EvaluateBestAction(board); // ミニマックス法を使用


            return false; // 該当する操作方法がない場合
        }

        /// <summary>
        /// ランダムに移動または壁の設置を行う。
        /// </summary>
        private static bool RandomAction(Board board)
        {
            // 現在のプレイヤーと相手プレイヤーを取得
            Player currentPlayer = board.player[board.currentPlayerNumber];
            Player opponentPlayer = board.player[1 - board.currentPlayerNumber];
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
                return true; // 移動が成功したことを示す
            }

            return false; // 移動できるマスがない場合
        }

        /// <summary>
        /// 評価関数を使用して一手先の最良の行動を決定し、実行。
        /// </summary>
        /// <param name="playerNumber">AIのプレイヤー番号 (0または1)</param>
        private static bool EvaluateBestAction(Board board)
        {

            int bestMoveScore = int.MinValue; // 最良の移動スコアを初期化
            (int, int)? bestMove = null; // 最良の移動を初期化
            int bestWallScore = int.MinValue; // 最良の壁設置スコアを初期化
            (int, int, WallOrientation)? bestWall = null; // 最良の壁設置を初期化

            foreach (var move in board.currentPlayer.possibleMoves) // 各移動候補に対して
            {
                Board newBoard = new Board(board); // 盤面をコピー ]
                newBoard.TryMovePlayer(move.Item1, move.Item2); // 移動を適用 // 評価関数を使用して盤面を評価
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 最良のスコアと移動を更新
                if (score > bestMoveScore) { bestMoveScore = score; bestMove = move; }
            }
            foreach (var wall in board.verticalMountableList) // 各設置可能な縦壁に対して
            {
                Board newBoard = new Board(board); // 盤面をコピー
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Vertical); // 壁設置を適用
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 最良のスコアと壁設置を更新
                if (score > bestWallScore) { bestWallScore = score; bestWall = (wall.Item1, wall.Item2, WallOrientation.Vertical); }
            }
            foreach (var wall in board.horizontalMountableList) //各設置可能な横壁に対して
            {
                Board newBoard = new Board(board); // 盤面をコピー
                newBoard.wallManager.PlaceWall( wall.Item1, wall.Item2, WallOrientation.Horizontal); // 壁設置を適用
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 最良のスコアと壁設置を更新
                if (score > bestWallScore) { bestWallScore = score; bestWall = (wall.Item1, wall.Item2, WallOrientation.Horizontal); }
            }
            if (bestWallScore > bestMoveScore && bestWall.HasValue)
            { // 最良の壁設置を実行
                board.wallManager.PlaceWall(bestWall.Value.Item1, bestWall.Value.Item2, bestWall.Value.Item3); return true; // 壁の設置が成功したことを示す
            }
            else if (bestMove.HasValue)
            { // 最良の移動を実行
                board.player[board.currentPlayerNumber].x = bestMove.Value.Item1; board.player[board.currentPlayerNumber].y = bestMove.Value.Item2; return true; // 移動が成功したことを示す
            }
            return false; // 移動できるマスがない場合
        }
    }

    public class EvaluateParam
    {
        public int a = 10; //65
        public int b = 1; //31
        public int c = 5; //26

        public Random rand = new Random();
        public void RandomParam()
        {
            a = rand.Next(1, 100);
            b = rand.Next(1, 100);
            c = rand.Next(1, 100);
        }

        public EvaluateParam Clone()
        {
            EvaluateParam p = new EvaluateParam();
            p.a = this.a;
            p.b = this.b;
            p.c = this.c;
            return p;
        }
    }
}