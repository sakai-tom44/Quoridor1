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
            if (board.gameOver) return false; // ゲーム終了後は動かない

            if (board.player[board.currentPlayer].playerType == PlayerType.Random)
                return RandomMove(playerNumber);
            if (board.player[board.currentPlayer].playerType == PlayerType.AI)
                return MinimaxMove(playerNumber); // ミニマックス法を使用


            return false; // 該当する操作方法がない場合
        }

        /// <summary>
        /// ランダムに移動または壁の設置を行う。
        /// </summary>
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
        private bool MinimaxMove(int playerNumber)
        {
            // 現在のプレイヤーと相手プレイヤーを取得
            (int,int) currentPlayer = board.player[board.currentPlayer].pos; // 自分
            (int,int) opponentPlayer = board.player[1 - board.currentPlayer].pos; // 相手
            var moveGraph = board.moveGraph; // 移動可能グラフ
            int bestMoveScore = int.MinValue; // 最良の移動スコアを初期化
            (int, int)? bestMove = null; // 最良の移動を初期化
            int bestWallScore = int.MinValue; // 最良の壁設置スコアを初期化
            (int, int, WallOrientation)? bestWall = null; // 最良の壁設置を初期化

            foreach (var move in board.player[board.currentPlayer].possibleMoves) // 各移動候補に対して
            {
                // 評価関数を使用して盤面を評価
                int score = EvaluateBoardState(move, opponentPlayer, moveGraph, playerNumber);
                // 最良のスコアと移動を更新
                if (score > bestMoveScore)
                {
                    bestMoveScore = score;
                    bestMove = move;
                }
            }
            
            foreach (var wall in board.verticalMountableList) // 各設置可能な縦壁に対して
            {
                (int,int) xy1, xy2, xy3, xy4;
                (xy1, xy2, xy3, xy4) = WallManager.Wall2xy4(wall.Item1, wall.Item2, WallOrientation.Vertical); // 壁で遮断される4つのマスの座標を取得
                int[,] dummyGraph = WallManager.Disconnect(xy1, xy2, xy3, xy4, moveGraph); // 壁を置いた場合の移動グラフを生成
                int score = EvaluateBoardState(currentPlayer, opponentPlayer, dummyGraph, playerNumber);
                // 最良のスコアと壁設置を更新
                if (score > bestWallScore)
                {
                    bestWallScore = score;
                    bestWall = (wall.Item1, wall.Item2, WallOrientation.Vertical);
                }
            }

            foreach (var wall in board.horizontalMountableList) // 各設置可能な横壁に対して
            {
                (int, int) xy1, xy2, xy3, xy4;
                (xy1, xy2, xy3, xy4) = WallManager.Wall2xy4(wall.Item1, wall.Item2, WallOrientation.Horizontal); // 壁で遮断される4つのマスの座標を取得
                int[,] dummyGraph = WallManager.Disconnect(xy1, xy2, xy3, xy4, moveGraph); // 壁を置いた場合の移動グラフを生成
                int score = EvaluateBoardState(currentPlayer, opponentPlayer, dummyGraph, playerNumber);
                // 最良のスコアと壁設置を更新
                if (score > bestWallScore)
                {
                    bestWallScore = score;
                    bestWall = (wall.Item1, wall.Item2, WallOrientation.Horizontal);
                }
            }

            if (bestWallScore > bestMoveScore && bestWall.HasValue)
            {
                // 最良の壁設置を実行
                board.wallManager.PlaceWall(bestWall.Value.Item1, bestWall.Value.Item2, bestWall.Value.Item3);
                return true; // 壁の設置が成功したことを示す
            }
            else if (bestMove.HasValue)
            {
                // 最良の移動を実行
                board.player[board.currentPlayer].x = bestMove.Value.Item1;
                board.player[board.currentPlayer].y = bestMove.Value.Item2;
                return true; // 移動が成功したことを示す
            }
            return false; // 移動できるマスがない場合
        }

        /// <summary>
        /// 盤面の評価関数。
        /// </summary>
        /// AIの戦略に応じて調整可能。
        private int EvaluateBoardState((int,int) current, (int, int) opponent, int[,] moveGraph, int playerNumber)
        {
            // 盤面の評価関数を実装
            // 例えば、各プレイヤーのゴールまでの最短距離を計算し、その差を評価値とする
            int score = 0;
            int currentDistance = ShortestPathToGoal(current, moveGraph, (playerNumber) * (Board.N - 1));// 自分のゴールまでの距離を計算
            int opponentDistance = ShortestPathToGoal(opponent, moveGraph, (1 - playerNumber) * (Board.N - 1));// 相手のゴールまでの距離を計算
            // 評価値を計算（距離が短いほど高評価）
            score += (opponentDistance - currentDistance/2) * 10; // 自分が有利なら正のスコア
            // 壁の数も考慮（壁が多いほど有利）
            score += (board.player[playerNumber].placeWallCount - board.player[1 - playerNumber].placeWallCount) * 10;
            return score;
        }

        /// <summary>
        /// 指定された座標からのゴールまでの最短距離を計算。
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="moveGraph"></param>
        /// <param name="goalY"></param>
        /// <returns>最短距離(マス)</returns>
        private int ShortestPathToGoal((int,int) xy, int[,] moveGraph, int goalY)
        {
            // 幅優先探索（BFS）を使用して最短経路 (暫定案)
            Queue<(int, int, int)> queue = new Queue<(int, int, int)>();
            int[,] visited = new int[Board.N, Board.N];
            visited[xy.Item1, xy.Item2] = 1; // スタート地点を訪問済みに設定
            queue.Enqueue((xy.Item1, xy.Item2, 1)); // (x座標, y座標, 距離)

            while (queue.Count > 0)
            {
                var (x, y, dist) = queue.Dequeue();
                if (y == goalY) return dist; // ゴールに到達した場合、距離を返す
                (int, int)[] adjacent = { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) }; // 上下左右の隣接セル
                foreach (var (nx, ny) in adjacent)
                {
                    if (nx < 0 || nx >= Board.N || ny < 0 || ny >= Board.N) continue; // 盤外ならスキップ
                    if (visited[nx, ny] > 0) continue; // 既に訪問済みならスキップ
                    if (moveGraph[Board.xy2to1(x, y), Board.xy2to1(nx, ny)] == 0) continue; // 移動できないならスキップ

                    visited[nx, ny] = dist + 1;
                    queue.Enqueue((nx, ny, dist + 1));
                }
            }
            return int.MaxValue; // ゴールに到達できない場合
        }
    }
}
