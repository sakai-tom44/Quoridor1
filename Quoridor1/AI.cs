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
        private const int MAX_DEPTH = 5;
        public const int SIMULATION_PER_ACTION = 100; // 各候補手に対するシミュレーション回数

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
            if (board.player[board.currentPlayerNumber].playerType == PlayerType.MonteCarlo)
                return MonteCarloBestAction(board); // モンテカルロ法を使用


            return false; // 該当する操作方法がない場合
        }

        /// <summary>
        /// ランダムに移動または壁の設置を行う。
        /// </summary>
        private static bool RandomAction(Board board)
        {
            // 移動可能なマスを取得
            var actions = GetAllActions(board);
            if (actions.Count > 0)
            {
                int idx = rand.Next(actions.Count); // ランダムに1手選択
                var action = actions[idx];
                ApplyAction(board, action);
                return true; // 行動が成功したことを示す
            }

            return false; // 移動できるマスがない場合
        }

        /// <summary>
        /// 評価関数を使用して一手先の最良の行動を決定し、実行。
        /// </summary>
        /// <param name="playerNumber">AIのプレイヤー番号 (0または1)</param>
        private static bool EvaluateBestAction(Board board)
        {
            int bestScore = int.MinValue; // 最良のスコアを初期化
            Action bestAction = default; // 最良の行動を初期化

            // 候補手一覧を取得（移動・縦壁・横壁を含む）
            var actions = GetAllActions(board);

            foreach ( var action in actions )
            {
                Board newBoard = new Board(board); // 盤面をコピー
                ApplyAction(newBoard, action); // 行動を適用
                int score = newBoard.EvaluateBoardState(board.currentPlayerNumber); // 盤面を評価
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }

            if (bestScore > int.MinValue)
            {
                ApplyAction(board, bestAction); // 最良の行動を実行
                return true; // 行動が成功したことを示す
            }

            return false; // 移動できるマスがない場合
        }

        /// <summary>
        /// モンテカルロ法を使用して最良の行動を決定し、実行。
        /// </summary>
        /// <param name="board">現在の盤面状態</param>
        /// <returns>行動が成功したかどうか</returns>
        public static bool MonteCarloBestAction(Board board) 
        {
            // 現在の手番プレイヤー番号を保存（勝利判定はこのプレイヤー基準で行う）
            int player = board.currentPlayerNumber;

            // 候補手一覧を取得（移動・縦壁・横壁を含む）
            var actions = GetAllActions(board);

            // 候補が無ければ失敗
            if (actions.Count == 0) return false;

            // 結果格納用
            double bestWinRate = double.NegativeInfinity;
            Action bestAction = default;

            // ここはシンプルな実装：各手ごとに直列でシミュレーションを行う。
            // 必要なら Parallel.For 等で並列化（ランダム種の競合に注意）。
            foreach (var act in actions)
            {
                int wins = 0;

                // 並列ループでプレイアウトを実行
                Parallel.For(0, SIMULATION_PER_ACTION, () =>
                {
                    // 各スレッドで個別の勝利数カウンタを持つ
                    return 0;
                },
                (i, loopState, localWins) =>
                {
                    // 盤面をコピーしてプレイアウトを行う（Board(Board) コピーコンストラクタを想定）
                    Board sim = new Board(board);

                    // 実際の手を適用（コピーに）
                    ApplyAction(sim, act);

                    // 次プレイヤーへ移行（ApplyActionは手を適用しただけなのでターン切替が必要なら行う）
                    sim.NextPlayer();

                    // ランダムプレイアウトの実行（乱数はスレッドローカルで安全）
                    int winner = PlayOutRandom(sim);

                    // プレイアウトの勝者が起点プレイヤーなら勝利と数える
                    if (winner == player) localWins++;

                    // スレッドローカル変数として返す
                    return localWins;
                },
                localWins => Interlocked.Add(ref wins, localWins) // 各スレッドの結果を合計
                );

                double winRate = (double)wins / SIMULATION_PER_ACTION;
                Console.WriteLine($"Action: {(act.IsMove ? $"Move to ({act.X},{act.Y})" : $"Place {(act.Orientation == WallOrientation.Vertical ? "Vertical" : "Horizontal")} Wall at ({act.X},{act.Y})")}, Win Rate: {winRate:P2}");

                // 最良手を更新
                if (winRate > bestWinRate)
                {
                    bestWinRate = winRate;
                    bestAction = act;
                }
            }

            // 最良手が見つかれば実盤に対してそれを実行してtrueを返す
            if (bestWinRate > double.NegativeInfinity)
            {
                ApplyAction(board, bestAction);
                return true;
            }

            return false;
        }

        /// <summary>
        /// ランダムプレイアウトを行い、ゲームの勝者を返す。
        /// <para>戻り値: -1=継続（理論上起きない）、0=player0勝利, 1=player1勝利</para>
        /// </summary>
        /// <param name="simBoard">プレイアウト用の盤（メソッド内で破壊的に操作される）</param>
        /// <returns>勝者のプレイヤー番号</returns>
        private static int PlayOutRandom(Board simBoard)
        {
            // ローカルな乱数をスレッドごとに生成して競合を防ぐ
            var rnd = ThreadLocalRandom.Current;

            // ゲームが終わるまでランダムに手を打ち続ける
            while (true)
            {
                // まず勝敗チェック
                int winner = simBoard.CheckGameOver(); // -1: 継続, 0: p0勝利, 1: p1勝利
                if (winner != -1) return winner;

                // 現在手番のすべての合法手を列挙
                var actions = GetAllActions(simBoard);

                // もし合法手が1つも無ければターンをスキップ（通常はあり得ないが保険）
                if (actions.Count == 0)
                {
                    simBoard.NextPlayer();
                    continue;
                }

                // ランダムに1手選択して適用
                int idx = rnd.Next(actions.Count);
                ApplyAction(simBoard, actions[idx]);

                // ターンを進める
                simBoard.NextPlayer();
            }
        }

        /// <summary>
        /// 1手先を探索し、Minimaxで最良の行動を実際に実行する
        /// </summary>
        public static bool MinimaxBestAction(Board board)
        {
            int bestScore = int.MinValue;
            Action bestAction = default;

            int currentPlayer = board.currentPlayerNumber;

            /// -------------------------------
            /// 全ての行動候補を取得して探索
            /// -------------------------------

            foreach (var action in GetAllActions(board))
            {
                Board newBoard = new Board(board);
                ApplyAction(newBoard, action);
                int score = Minimax(newBoard, 1, false, currentPlayer, int.MinValue, int.MaxValue);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }

            // -------------------------------
            // 最良の行動を実際に実行
            // -------------------------------
            if (bestScore > int.MinValue)
            {
                ApplyAction(board, bestAction);
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
        /// <param name="alpha">アルファ値（最大化ノード用）</param>
        /// <param name="beta">ベータ値（最小化ノード用）</param>
        /// <returns>評価スコア</returns>
        private static int Minimax(Board board, int depth, bool isMaximizing, int aiPlayer, int alpha, int beta)
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
                //Console.WriteLine("Evaluate at depth " + depth);
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

            /// -------------------------------
            /// 全ての行動候補を取得して探索
            /// -------------------------------
            foreach (var action in GetAllActions(board))
            {
                Board newBoard = new Board(board);
                ApplyAction(newBoard, action);
                int score = Minimax(newBoard, depth + 1, !isMaximizing, aiPlayer, alpha, beta);
                if (isMaximizing)
                {
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);
                }
                if (beta <= alpha)
                {
                    //Console.WriteLine($"Alpha-Beta Pruning at depth {depth}   /   bata:{beta} <= alpha{alpha}");
                    break; // アルファベータカット
                }
            }

            return bestScore;
        }

        /// <summary>
        /// 現在の手番プレイヤーの全ての行動候補（移動と壁設置）を列挙して返す。
        /// </summary>
        private static List<Action> GetAllActions(Board board)
        {
            var list = new List<Action>();

            // 移動候補を追加（possibleMoves は (int x,int y) のタプル列を仮定）
            foreach (var mv in board.currentPlayer.possibleMoves)
            {
                list.Add(Action.MoveTo(mv.Item1, mv.Item2));
            }

            // 縦壁設置候補を追加（verticalMountableList は (int x,int y) のタプル列を仮定）
            foreach (var w in board.currentPlayer.verticalMountableList)
            {
                list.Add(Action.PlaceWall(w.Item1, w.Item2, WallOrientation.Vertical));
            }

            // 横壁設置候補を追加（horizontalMountableList は (int x,int y) のタプル列を仮定）
            foreach (var w in board.currentPlayer.horizontalMountableList)
            {
                list.Add(Action.PlaceWall(w.Item1, w.Item2, WallOrientation.Horizontal));
            }

            return list;
        }

        /// <summary>
        /// 指定したアクションを盤面に適用する（移動/壁設置を実際に行う）。
        /// </summary>
        private static void ApplyAction(Board board, Action action)
        {
            if (action.IsMove)
            {
                // 移動を適用（TryMove系の戻り値は無視する：プレイアウトでは不正な手はGetAllActionsで除外済）
                board.TryMovePlayer(action.X, action.Y);
            }
            else
            {
                // 壁を適用（wallManagerのPlaceWallは合法判定を行うはず）
                board.wallManager.PlaceWall(action.X, action.Y, action.Orientation);
            }
        }

        /// <summary>
        /// 単純なアクション表現（移動 または 壁設置）。
        /// </summary>
        private readonly struct Action
        {
            public readonly bool IsMove; // true=移動, false=壁設置
            public readonly int X;       // 移動先または壁のx
            public readonly int Y;       // 移動先または壁のy
            public readonly WallOrientation Orientation; // 壁向き（移動なら無視）

            private Action(bool isMove, int x, int y, WallOrientation orientation)
            {
                IsMove = isMove;
                X = x;
                Y = y;
                Orientation = orientation;
            }

            public static Action MoveTo(int x, int y) => new Action(true, x, y, WallOrientation.Vertical);
            public static Action PlaceWall(int x, int y, WallOrientation ori) => new Action(false, x, y, ori);
        }

        /// <summary>
        /// スレッドローカルな Random を提供するユーティリティ。
        /// Parallel/Threadから呼ばれても安全に乱数を得られる。
        /// </summary>
        private static class ThreadLocalRandom
        {
            // seed 用にスレッドセーフなインスタンスを作る
            private static int seed = Environment.TickCount;

            private static readonly ThreadLocal<Random> threadLocal =
                new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

            public static Random Current => threadLocal.Value;
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