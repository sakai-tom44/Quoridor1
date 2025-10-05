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
        public static bool MakeMove(Board board, int playerNumber)
        {
            if (board.gameOver) return false; // ゲーム終了後は動かない

            if (board.player[board.currentPlayerNumber].playerType == PlayerType.Random)
                return RandomMove(board);
            if (board.player[board.currentPlayerNumber].playerType == PlayerType.AI)
                return MinimaxMove(board, 1); // ミニマックス法を使用


            return false; // 該当する操作方法がない場合
        }

        /// <summary>
        /// ランダムに移動または壁の設置を行う。
        /// </summary>
        private static bool RandomMove(Board board)
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
        /// ミニマックス法を使用して最良の移動を決定し、実行。
        /// </summary>
        /// <param name="playerNumber">AIのプレイヤー番号 (0または1)</param>
        private static bool MinimaxMove(Board board, int depth)
        {
            /*
            int bestMoveScore = int.MinValue; // 最良の移動スコアを初期化
            (int, int)? bestMove = null; // 最良の移動を初期化
            int bestWallScore = int.MinValue; // 最良の壁設置スコアを初期化
            (int, int, WallOrientation)? bestWall = null; // 最良の壁設置を初期化
            */
            /*
            Queue<(int, Board)>dummyBoard = new Queue<(int, Board)>() ; // 評価関数用のダミーボードキュー
            dummyBoard.Enqueue((0, new Board(board))); // 現在の盤面をキューに追加

            while (dummyBoard.Count > 0) // キューが空になるまで探索
            {
                var (d, b) = dummyBoard.Dequeue(); // キューから先頭を取り出し
                if (d >= depth) continue; // 深さ制限
                // 各プレイヤーの可能な移動を列挙
                foreach (var move in b.player[b.currentPlayerNumber].possibleMoves) // 各移動候補に対して
                {
                    Board newBoard = new Board(b); // 盤面をコピー
                    newBoard.player[newBoard.currentPlayerNumber].x = move.Item1; // 移動を適用
                    newBoard.player[newBoard.currentPlayerNumber].y = move.Item2;
                    newBoard.NextPlayer(); // プレイヤー交代
                    dummyBoard.Enqueue((d + 1, newBoard)); // 新しい盤面をキューに追加
                }
                foreach (var wall in b.verticalMountableList) // 各設置可能な縦壁に対して
                {
                    Board newBoard = new Board(b); // 盤面をコピー
                    WallManager.PlaceWall(newBoard, wall.Item1, wall.Item2, WallOrientation.Vertical); // 壁設置を適用
                    newBoard.NextPlayer(); // プレイヤー交代
                    dummyBoard.Enqueue((d + 1, newBoard)); // 新しい盤面をキューに追加
                }
                foreach (var wall in b.horizontalMountableList) // 各設置可能な横壁に対して
                {
                    Board newBoard = new Board(b); // 盤面をコピー
                    WallManager.PlaceWall(newBoard, wall.Item1, wall.Item2, WallOrientation.Horizontal); // 壁設置を適用
                    newBoard.NextPlayer(); // プレイヤー交代
                    dummyBoard.Enqueue((d + 1, newBoard)); // 新しい盤面をキューに追加
                }
            }
            */
            // --- 移動候補を並列評価 ---
            var moveResults = board.currentPlayer.possibleMoves // 移動候補リスト
                .AsParallel() // 並列LINQで評価
                .Select(move =>
                {
                    Board newBoard = new Board(board); // スレッドごとに盤面をコピー
                    newBoard.TryMovePlayer(move.Item1, move.Item2); // 移動適用
                    int score = newBoard.EvaluateBoardState(board.currentPlayerNumber);
                    return (Score: score, Move: move);
                })
                .ToList();

            // --- 縦壁候補を並列評価 ---
            var verticalResults = board.verticalMountableList // 設置可能な縦壁リスト
                .AsParallel()
                .Select(wall =>
                {
                    Board newBoard = new Board(board);
                    newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Vertical);
                    int score = newBoard.EvaluateBoardState(board.currentPlayerNumber);
                    return (Score: score, Wall: (wall.Item1, wall.Item2, WallOrientation.Vertical));
                })
                .ToList();

            // --- 横壁候補を並列評価 ---
            var horizontalResults = board.horizontalMountableList // 設置可能な横壁リスト
                .AsParallel()
                .Select(wall =>
                {
                    Board newBoard = new Board(board);
                    newBoard.wallManager.PlaceWall(wall.Item1, wall.Item2, WallOrientation.Horizontal);
                    int score = newBoard.EvaluateBoardState(board.currentPlayerNumber);
                    return (Score: score, Wall: (wall.Item1, wall.Item2, WallOrientation.Horizontal));
                })
                .ToList();

            // --- 各カテゴリで最良の候補を選ぶ ---
            var bestMove = moveResults.OrderByDescending(r => r.Score).FirstOrDefault();
            var bestVertical = verticalResults.OrderByDescending(r => r.Score).FirstOrDefault();
            var bestHorizontal = horizontalResults.OrderByDescending(r => r.Score).FirstOrDefault();

            // --- 壁候補の中で最良のものを決定 ---
            var bestWallCandidate = bestVertical.Score > bestHorizontal.Score ? bestVertical : bestHorizontal;

            // --- 移動と壁を比較し、最良の行動を決定 ---
            if (bestWallCandidate.Score > bestMove.Score)
            {
                // 最良の壁設置を実行
                var w = bestWallCandidate.Wall;
                board.wallManager.PlaceWall(w.Item1, w.Item2, w.Item3);
                return true; // 壁の設置が成功したことを示す
            }
            else
            {
                // 最良の移動を実行
                board.player[board.currentPlayerNumber].x = bestMove.Move.Item1;
                board.player[board.currentPlayerNumber].y = bestMove.Move.Item2;
                return true; // 移動が成功したことを示す
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
        public void RandomParam() { 
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