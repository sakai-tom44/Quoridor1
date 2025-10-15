using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Quoridor1
{
    /// <summary>
    /// Quoridorのゲーム盤を表現するクラス。
    /// プレイヤーの位置、壁、移動可能グラフを管理。
    /// </summary>
    public class Board
    {
        public const int N = 9; // 盤面のサイズ（9x9）
        public const int lineWidth = 8; // 壁の太さ（ピクセル）
        public const int wallCount = 10; // 各プレイヤーの壁の数
        public const bool autoReset = true; // ゲーム終了後に自動でリセットするか



        public int[,] horizontalWalls = new int[N, N]; // 横方向の壁を格納する配列
        public int[,] verticalWalls = new int[N, N];   // 縦方向の壁を格納する配列
        public bool[,] horizontalMountable = new bool[N, N]; // 横壁設置可能位置
        public List<(int,int)> horizontalMountableList { get{ return bool2xyList(horizontalMountable);} } // 横壁設置可能位置のリスト
        public bool[,] verticalMountable = new bool[N, N];   // 縦壁設置可能位置
        public List<(int, int)> verticalMountableList { get { return bool2xyList(verticalMountable); } } // 縦壁設置可能位置のリスト
        private List<(int, int)> bool2xyList(bool[,] b) // bool配列から(x,y)リストを作成
        {
            List<(int, int)> list = new List<(int, int)>();
            for (int x = 0; x < N - 1; x++)
                for (int y = 0; y < N - 1; y++)
                    if (b[x, y]) list.Add((x, y));
            return list;
        }

        public int[,] moveGraph; // マス間の移動可能性を示す隣接行列

        public Player[] player = new Player[2]; // プレイヤー配列 player[0]: 黒, player[1]: 白

        public int currentPlayerNumber = 0; // 現在のプレイヤー（0または1）
        public Player currentPlayer { get { return player[currentPlayerNumber]; } } // 現在のプレイヤー
        public Player opponentPlayer { get { return player[1 - currentPlayerNumber]; } } // 相手のプレイヤー
        public bool gameOver = false; // ゲーム終了フラグ
        public WallManager wallManager; // 壁の管理クラス

        public EvaluateParam[] e; // 評価関数のパラメータ
        /// <summary>
        /// コンストラクタ。盤を初期化。
        /// </summary>
        public Board(EvaluateParam[] e)
        {
            wallManager = new WallManager(this); // 壁マネージャを初期化
            this.e = e;
            Reset(); // 盤面を初期化
        }

        /// <summary>
        /// コピーコンストラクタ。指定された盤の状態を複製。
        /// </summary>
        /// <param name="copyBoard">複製元の盤</param>
        public Board(Board copyBoard)
        {
            this.horizontalWalls = (int[,])copyBoard.horizontalWalls.Clone();
            this.verticalWalls = (int[,])copyBoard.verticalWalls.Clone();
            this.moveGraph = (int[,])copyBoard.moveGraph.Clone();
            this.player[0] = copyBoard.player[0].Clone();
            this.player[1] = copyBoard.player[1].Clone();
            this.currentPlayerNumber = copyBoard.currentPlayerNumber;
            this.gameOver = copyBoard.gameOver;
            this.wallManager = new WallManager(this);
            this.e = copyBoard.e;
            RefreshBoard();
        }

        /// <summary>
        /// 盤面をリセットし、プレイヤー位置や移動可能グラフを初期化。
        /// </summary>
        private void Reset()
        {
            horizontalWalls = new int[N, N]; // 横壁をクリア
            verticalWalls = new int[N, N];   // 縦壁をクリア
            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    horizontalMountable[x,y] = true; // 各要素をtrueに初期化
            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    verticalMountable[x,y] = true; // 各要素をtrueに初期化

            moveGraph = new int[N * N, N * N]; // 移動グラフを初期化

            player[0] = new Player(N / 2, N - 1, PlayerType.AI); // プレイヤー0を下端中央に配置
            player[1] = new Player(N / 2, 0, PlayerType.Random);     // プレイヤー1を上端中央に配置

            // 各マス間の隣接関係を構築
            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    int k1 = xy2to1(x, y); // (x,y)座標を1次元インデックスに変換
                    if (y != (N - 1)) // 上下に隣接がある場合
                    {
                        int k2 = xy2to1(x, y + 1); // 隣接マスのインデックス
                        moveGraph[k1, k2] = moveGraph[k2, k1] = 1; // 移動可能に設定
                    }
                    if (x != (N - 1)) // 左右に隣接がある場合
                    {
                        int k2 = xy2to1(x + 1, y); // 隣接マスのインデックス
                        moveGraph[k1, k2] = moveGraph[k2, k1] = 1; // 移動可能に設定
                    }
                }
            }

            RefreshBoard(); // 盤面情報を更新
        }
        /// <summary>
        /// 盤面の状態を更新
        /// </summary>
        /// <remarks>
        /// 壁配置の有効な位置を更新し、現在のゲーム状態に基づいて両プレイヤーの次の可能な手を再計算
        /// </remarks>
        public void RefreshBoard()
        {
            //Console.WriteLine(player[0].placeWallCount +" "+ player[1].placeWallCount);
            //DebugBoard.PrintMountable(this); // デバッグ用に設置可能位置を表示
            wallManager.RefreshWallMountable(); // 壁の設置可能位置を更新
            player[0].RefreshPossibleMoves(this, player[1]); // プレイヤー0の次の移動候補を更新
            player[1].RefreshPossibleMoves(this, player[0]); // プレイヤー1の次の移動候補を更新
        }

        /// <summary>
        /// ゲーム終了を確認し、終了していればメッセージを表示。
        /// </summary>
        /// <returns>勝者のプレイヤー番号（0または1）、ゲームが続行中なら-1</returns>
        public int CheckGameOver()
        {
            if (player[0].y == 0) // プレイヤー0が上端に到達
            {
                gameOver = true;
                if (!autoReset) MessageBox.Show("Black wins!");
                Console.WriteLine($"win a:{e[0].a} b:{e[0].b} c:{e[0].c}");
                e[1].RandomParam(); // 敗者の評価関数パラメータをランダムに変更
                return 0;
            }
            else if (player[1].y == N - 1) // プレイヤー1が下端に到達
            {
                gameOver = true;
                if (!autoReset) MessageBox.Show("White wins!");
                Console.WriteLine($"win a:{e[1].a} b:{e[1].b} c:{e[1].c}");
                e[0].RandomParam(); // 敗者の評価関数パラメータをランダムに変更
                return 1;
            }
            return -1; // ゲームは終了していない
        }

        /// <summary>
        /// プレイヤーを指定座標へ移動させる。
        /// </summary>
        public bool TryMovePlayer(int xi, int yi)
        {
            if (player[currentPlayerNumber].possibleMoves.IndexOf((xi, yi)) >= 0) // 移動可能な位置か確認
            {
                player[currentPlayerNumber].Move(xi, yi, xy2to1(xi, yi)); // プレイヤーを移動
                return true; // 移動成功
            }
            return false; // 移動失敗
        }

        /// <summary>
        /// 手番を交代する。
        /// </summary>
        public void NextPlayer()
        {
            currentPlayerNumber = 1 - currentPlayerNumber; // 手番を交代

            RefreshBoard(); // 盤面情報を更新
        }

        /// <summary>
        /// 盤面の評価関数。
        /// </summary>
        /// AIの戦略に応じて調整可能。
        public int EvaluateBoardState(int playerNumber)
        {
            int p = playerNumber;

            // 盤面の評価関数を実装
            // 例えば、各プレイヤーのゴールまでの最短距離を計算し、その差を評価値とする
            int score = 0;
            int currentDistance = ShortestPathToGoal(player[playerNumber].pos, moveGraph, (playerNumber) * (N - 1));// 自分のゴールまでの距離を計算
            int opponentDistance = ShortestPathToGoal(player[1 - playerNumber].pos, moveGraph, (1 - playerNumber) * (N - 1));// 相手のゴールまでの距離を計算

            //Console.WriteLine(opponentDistance+" - "+currentDistance);
            if (currentDistance == 0) { 
                //Console.WriteLine($"Move to ({player[playerNumber].pos.Item1}, {player[playerNumber].pos.Item2}) => Win");
                return int.MaxValue; 
            } // ゴールできる場合、最大スコアを返す

            // 評価値を計算（距離が短いほど高評価）
            score += (opponentDistance * e[p].a - currentDistance * e[p].b); // 自分が有利なら正のスコア
            //Console.WriteLine($"Move to ({player[playerNumber].pos.Item1}, {player[playerNumber].pos.Item2}) => Score: {opponentDistance * e[p].a} - {currentDistance * e[p].b} = {score}");
            // 壁の数も考慮（壁を置いた数が少ないほど有利）
            score += (player[1 - playerNumber].placeWallCount - player[playerNumber].placeWallCount) * e[p].c;
            //Console.WriteLine($"EvaluateBoardState: Player {playerNumber}, Score: {score}, CurrentDistance: {currentDistance}, OpponentDistance: {opponentDistance}, WallsUsed: {board.player[playerNumber].placeWallCount}");

            return score;
        }

        /// <summary>
        /// 指定された座標からのゴールまでの最短距離を計算。
        /// </summary>
        /// <param name="xy">開始座標(x, y)</param>
        /// <param name="moveGraph">移動グラフ</param>
        /// <param name="goalY">ゴールのy座標</param>
        /// <returns>最短距離(マス)</returns>
        private int ShortestPathToGoal((int, int) xy, int[,] moveGraph, int goalY)
        {
            return BreadthFirstSearch(xy, moveGraph, goalY); // 幅優先探索で最短距離を計算
        }

        /// <summary>
        /// 幅優先探索 (BFS) を使用して最短経路を計算。
        /// </summary>
        /// <param name="xy">開始座標(x, y)</param>
        /// <param name="moveGraph">移動グラフ</param>
        /// <param name="goalY">ゴールのy座標</param>
        /// <returns>最短距離(マス)</returns>
        private int BreadthFirstSearch((int, int) xy, int[,] moveGraph, int goalY)
        {
            if (xy.Item2 == goalY) return 0; // 既にゴールにいる場合、距離は0
            // 幅優先探索（BFS）を使用して最短経路 (暫定案)
            // 全てのボトルネック。ここを高速化できればAIの効率化に繋がる。
            // 例えば、A*(A-star)アルゴリズムの導入や、事前計算によるキャッシュの利用など。

            Queue<(int, int, int)> queue = new Queue<(int, int, int)>(); // (x座標, y座標, 距離)のキュー
            int[,] visited = new int[N, N]; // 訪問済みマスを訪問した距離で管理（0: 未訪問, 1以上: 訪問済みで距離）
            visited[xy.Item1, xy.Item2] = 1; // スタート地点を訪問済みに設定
            queue.Enqueue((xy.Item1, xy.Item2, 1)); // (x座標, y座標, 距離)

            while (queue.Count > 0) // キューが空になるまで探索
            {
                var (x, y, dist) = queue.Dequeue(); // キューから先頭を取り出し
                (int, int)[] adjacent = { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) }; // 上下左右の隣接セル
                foreach (var (nx, ny) in adjacent) // 隣接セルを順に調査
                {
                    if (nx < 0 || nx >= N || ny < 0 || ny >= N) continue; // 盤外ならスキップ
                    if (visited[nx, ny] > 0) continue; // 既に訪問済みならスキップ
                    if (moveGraph[(x + N * y), (nx + N * ny)] == 0) continue; // 移動できないならスキップ

                    visited[nx, ny] = dist + 1; // 訪問済みに設定
                    if (ny == goalY)
                    {
                        //DebugBoard.DebugPrintVisited(visited); // デバッグ用に訪問済みマスを表示
                        return dist; // ゴールに到達した場合、距離を返す
                    }
                    queue.Enqueue((nx, ny, dist + 1)); // キューに追加
                }

            }
            return int.MaxValue; // ゴールに到達できない場合
        }

        /// <summary>
        /// (x,y)座標を1次元のインデックスに変換。
        /// </summary>
        public int xy2to1(int x, int y) => x + N * y; // x + y行分で計算
    }
}
