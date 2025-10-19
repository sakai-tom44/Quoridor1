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
        /// 先読みの最大深さ
        /// </summary>
        private const int MAX_DEPTH = 3;

        /// <summary>
        /// 次の一手を計算し、AIプレイヤーが移動。
        /// </summary>
        public static bool ComputeNextAction(Board board, int playerNumber)
        {
            if (board.gameOver) return false; // ゲーム終了後は動かない

            if (board.player[board.currentPlayerNumber].playerType == PlayerType.Random)
                return RandomAction(board);
            if (board.player[board.currentPlayerNumber].playerType == PlayerType.Evaluate)
                return EvaluateBestAction(board); // 評価関数を使用
            if (board.player[board.currentPlayerNumber].playerType == PlayerType.Minmax)
                return MinimaxBestAction(board); // ミニマックス法を使用


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
            int maxH = currentPlayer.horizontalMountable.Cast<bool>().Count(x => x); // 設置可能な横壁の数
            int maxV = currentPlayer.verticalMountable.Cast<bool>().Count(x => x);   // 設置可能な縦壁の数
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
                            if (currentPlayer.verticalMountable[x, y])
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
                            if (currentPlayer.horizontalMountable[x, y])
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
            foreach (var wall in board.currentPlayer.verticalMountableList) // 各設置可能な縦壁に対して
            {
                Board newBoard = new Board(board); // 盤面をコピー
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Vertical); // 壁設置を適用
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 最良のスコアと壁設置を更新
                if (score > bestWallScore) { bestWallScore = score; bestWall = (wall.Item1, wall.Item2, WallOrientation.Vertical); }
            }
            foreach (var wall in board.currentPlayer.horizontalMountableList) //各設置可能な横壁に対して
            {
                Board newBoard = new Board(board); // 盤面をコピー
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Horizontal); // 壁設置を適用
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 最良のスコアと壁設置を更新
                if (score > bestWallScore) { bestWallScore = score; bestWall = (wall.Item1, wall.Item2, WallOrientation.Horizontal); }
            }
            if (bestWallScore > bestMoveScore && bestWall.HasValue)
            { // 最良の壁設置を実行
                board.wallManager.PlaceWall(bestWall.Value.Item1, bestWall.Value.Item2, bestWall.Value.Item3);
                return true; // 壁の設置が成功したことを示す
            }
            else if (bestMove.HasValue)
            { // 最良の移動を実行
                board.player[board.currentPlayerNumber].x = bestMove.Value.Item1; board.player[board.currentPlayerNumber].y = bestMove.Value.Item2;
                return true; // 移動が成功したことを示す
            }
            return false; // 移動できるマスがない場合
        }

        /// <summary>
        /// 1手先を探索し、Minimaxで最良の行動を実際に実行する
        /// </summary>
        public static bool MinimaxBestAction(Board board)
        {
            int bestScore = int.MinValue;
            (int, int)? bestMove = null;
            (int, int, WallOrientation)? bestWall = null;

            int currentPlayer = board.currentPlayerNumber;

            // -------------------------------
            // 移動候補を探索
            // -------------------------------
            foreach (var move in board.currentPlayer.possibleMoves)
            {
                Board newBoard = new Board(board);
                newBoard.TryMovePlayer(move.Item1, move.Item2);

                int score = Minimax(newBoard, 1, false, currentPlayer);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                    bestWall = null;
                }
            }

            // -------------------------------
            // 壁候補（縦・横）を探索
            // -------------------------------
            foreach (var wall in board.currentPlayer.verticalMountableList)
            {
                Board newBoard = new Board(board);
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Vertical);

                int score = Minimax(newBoard, 1, false, currentPlayer);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = null;
                    bestWall = (wall.Item1, wall.Item2, WallOrientation.Vertical);
                }
            }
            foreach (var wall in board.currentPlayer.horizontalMountableList)
            {
                Board newBoard = new Board(board);
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Horizontal);

                int score = Minimax(newBoard, 1, false, currentPlayer);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = null;
                    bestWall = (wall.Item1, wall.Item2, WallOrientation.Horizontal);
                }
            }

            // -------------------------------
            // 最良の行動を実際に実行
            // -------------------------------
            if (bestWall.HasValue)
            {
                board.wallManager.PlaceWall(bestWall.Value.Item1, bestWall.Value.Item2, bestWall.Value.Item3);
                return true;
            }
            else if (bestMove.HasValue)
            {
                board.TryMovePlayer(bestMove.Value.Item1, bestMove.Value.Item2);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Minimax再帰関数
        /// </summary>
        /// <param name="board">現在の盤面</param>
        /// <param name="depth">現在の深さ</param>
        /// <param name="isMaximizing">最大化ノードか最小化ノードか</param>
        /// <param name="aiPlayer">AIプレイヤー番号（0または1）</param>
        /// <returns>評価スコア</returns>
        private static int Minimax(Board board, int depth, bool isMaximizing, int aiPlayer)
        {
            // ------------------------------------------
            // 勝利チェック（CheckGameOverを使用）
            // ------------------------------------------
            int winner = board.CheckGameOver(); // -1: 継続中, 0: プレイヤー0勝利, 1: プレイヤー1勝利

            if (winner == aiPlayer)
                return int.MaxValue - depth; // AIが勝っていたら高評価（早い勝利ほど高スコア）

            if (winner == 1 - aiPlayer)
                return -int.MaxValue + depth; // 相手が勝っていたら大幅マイナス

            // -------------------------------
            // 再帰の終了条件（最大深さに到達）
            // -------------------------------
            if (depth >= MAX_DEPTH)
            {
                return board.EvaluateBoardState(aiPlayer);
            }

            // -------------------------------
            // 現在の手番を切り替え
            // -------------------------------
            board.NextPlayer();

            // -------------------------------
            // 探索用の変数
            // -------------------------------
            int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

            // -------------------------------
            // 移動候補を全探索
            // -------------------------------
            foreach (var move in board.currentPlayer.possibleMoves)
            {
                Board newBoard = new Board(board);
                newBoard.TryMovePlayer(move.Item1, move.Item2);

                int score = Minimax(newBoard, depth + 1, !isMaximizing, aiPlayer);

                if (isMaximizing)
                    bestScore = Math.Max(bestScore, score);
                else
                    bestScore = Math.Min(bestScore, score);
            }

            // -------------------------------
            // 壁設置候補（縦）
            // -------------------------------
            foreach (var wall in board.currentPlayer.verticalMountableList)
            {
                Board newBoard = new Board(board);
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Vertical);

                int score = Minimax(newBoard, depth + 1, !isMaximizing, aiPlayer);

                if (isMaximizing)
                    bestScore = Math.Max(bestScore, score);
                else
                    bestScore = Math.Min(bestScore, score);
            }

            // -------------------------------
            // 壁設置候補（横）
            // -------------------------------
            foreach (var wall in board.currentPlayer.horizontalMountableList)
            {
                Board newBoard = new Board(board);
                newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Horizontal);

                int score = Minimax(newBoard, depth + 1, !isMaximizing, aiPlayer);

                if (isMaximizing)
                    bestScore = Math.Max(bestScore, score);
                else
                    bestScore = Math.Min(bestScore, score);
            }

            return bestScore;
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