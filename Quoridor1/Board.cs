using System;
using System.Collections.Generic;
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




        public int[,] horizontalWalls = new int[N, N]; // 横方向の壁を格納する配列
        public int[,] verticalWalls = new int[N, N];   // 縦方向の壁を格納する配列
        public bool[,] horizontalMountable = new bool[N, N]; // 横壁設置可能位置
        public List<(int,int)> horizontalMountableList { get{ return bool2xyList(horizontalMountable);} } // 横壁設置可能位置のリスト
        public bool[,] verticalMountable = new bool[N, N];   // 縦壁設置可能位置
        public List<(int, int)> verticalMountableList { get { return bool2xyList(verticalMountable); } } // 縦壁設置可能位置のリスト
        private static List<(int, int)> bool2xyList(bool[,] b) // bool配列から(x,y)リストを作成
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

        /// <summary>
        /// コンストラクタ。PictureBoxを受け取り、盤を初期化。
        /// </summary>
        public Board()
        {
            Reset(); // 盤面を初期化
        }

        /// <summary>
        /// 盤面をリセットし、プレイヤー位置や移動可能グラフを初期化。
        /// </summary>
        private void Reset()
        {
            horizontalWalls = new int[N, N]; // 横壁をクリア
            verticalWalls = new int[N, N];   // 縦壁をクリア
            moveGraph = new int[N * N, N * N]; // 移動グラフを初期化

            player[0] = new Player(N / 2, N - 1, PlayerType.Manual); // プレイヤー0を下端中央に配置
            player[1] = new Player(N / 2, 0, PlayerType.AI);     // プレイヤー1を上端中央に配置

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
            WallManager.RefreshWallMountable(this); // 壁の設置可能位置を更新
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
                MessageBox.Show("Black wins!");
                return 0;
            }
            else if (player[1].y == N - 1) // プレイヤー1が下端に到達
            {
                gameOver = true;
                MessageBox.Show("White wins!");
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
        /// (x,y)座標を1次元のインデックスに変換。
        /// </summary>
        public static int xy2to1(int x, int y) => x + N * y; // x + y行分で計算
    }
}
