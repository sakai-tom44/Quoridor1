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

        public int cellSize { get { return pictureBox.Width / Board.N; } } // 1マスのサイズ（ピクセル）

        public PictureBox pictureBox; // 盤を描画するPictureBox

        public int[,] horizontalWalls = new int[N, N]; // 横方向の壁を格納する配列
        public int[,] verticalWalls = new int[N, N];   // 縦方向の壁を格納する配列
        public int[,] moveGraph; // マス間の移動可能性を示す隣接行列

        public Player player0; // プレイヤー0
        public Player player1; // プレイヤー1

        /// <summary>
        /// コンストラクタ。PictureBoxを受け取り、盤を初期化。
        /// </summary>
        public Board(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox; // 渡されたPictureBoxを保持

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

            player0 = new Player(N / 2, N - 1); // プレイヤー0を下端中央に配置
            player1 = new Player(N / 2, 0);     // プレイヤー1を上端中央に配置

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
        public void RefreshBoard()
        {
            player0.RefreshNextMove(this, player1); // プレイヤー0の次の移動候補を更新
            player1.RefreshNextMove(this, player0); // プレイヤー1の次の移動候補を更新
        }

        /// <summary>
        /// プレイヤー0を指定座標へ移動させる。
        /// </summary>
        public bool TryMovePlayer0(int xi, int yi)
        {
            if (player0.nextMove.IndexOf((xi, yi)) >= 0)
            {
                player0.Move(xi, yi, xy2to1(xi, yi)); // プレイヤー0を移動
                return true; // 移動成功
            }
            return false; // 移動失敗
        }

        /// <summary>
        /// (x,y)座標を1次元のインデックスに変換。
        /// </summary>
        public static int xy2to1(int x, int y) => x + N * y; // x + y行分で計算
    }
}
