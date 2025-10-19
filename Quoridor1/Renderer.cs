using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Quoridor1
{
    /// <summary>
    /// ゲームの描画を担当するクラス。
    /// グリッド線、壁、プレイヤーをPictureBox上に描画。
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// 盤面全体を描画する。
        /// グリッド、壁、プレイヤーを順に描画してPictureBoxに表示。
        /// </summary>
        /// <param name="board">描画対象の盤面</param>
        /// <param name="pictureBox">描画先のPictureBox</param>
        public static void DrawBoard(Board board, PictureBox pictureBox)
        {
            int cellSize = pictureBox.Width / Board.N; // 1マスのサイズ（ピクセル）

            // PictureBoxサイズに基づいて新しいBitmapを作成
            Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Width);
            Pen[] wallPens = new Pen[2]; // 壁用の青い太線と赤い太線

            // Graphicsオブジェクトを取得し、ペンやブラシを用意
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen gridPen = new Pen(Color.DarkGray, 2))      // グリッド用の灰色の細線
            using (wallPens[0] = new Pen(Color.Blue, Board.lineWidth)) // 壁用の青い太線
            using (wallPens[1] = new Pen(Color.Red, Board.lineWidth))  // 壁用の赤い太線
            {
                // グリッド線の描画（縦横のラインを描く）
                for (int i = 0; i <= Board.N; i++)
                {
                    g.DrawLine(gridPen, 0, i * cellSize, pictureBox.Width, i * cellSize);   // 横線
                    g.DrawLine(gridPen, i * cellSize, 0, i * cellSize, pictureBox.Width);   // 縦線
                }

                // 壁の描画
                for (int xi = 0; xi < Board.N; xi++)
                {
                    for (int yi = 0; yi < Board.N; yi++)
                    {
                        // 横壁がある場合は青線を描く
                        if (board.horizontalWalls[xi, yi] > 0)
                            g.DrawLine(wallPens[board.horizontalWalls[xi, yi] - 1],
                                xi * cellSize, (yi + 1) * cellSize,
                                (xi + 2) * cellSize, (yi + 1) * cellSize);

                        // 縦壁がある場合は青線を描く
                        if (board.verticalWalls[xi, yi] > 0)
                            g.DrawLine(wallPens[board.verticalWalls[xi, yi] - 1],
                                (xi + 1) * cellSize, yi * cellSize,
                                (xi + 1) * cellSize, (yi + 2) * cellSize);
                    }
                }

                // プレイヤーを描画（黒：player0, 白：player1）
                DrawPlayer(board, pictureBox, g, Brushes.White, 1);
                DrawPlayer(board, pictureBox, g, Brushes.Black, 0);
            }

            // 既存の画像を破棄し、新しい描画結果をPictureBoxに設定
            if (pictureBox.Image != null) pictureBox.Image.Dispose();
            pictureBox.Image = bmp;
        }

        /// <summary>
        /// プレイヤーの位置に丸を描画する。
        /// </summary>
        /// <param name="board">描画対象の盤面</param>
        /// <param name="pictureBox">描画先のPictureBox</param>
        /// <param name="g">Graphicsオブジェクト</param>
        /// <param name="brush">プレイヤーの色を指定するブラシ</param>
        /// <param name="p">描画対象のプレイヤー</param>
        private static void DrawPlayer(Board board, PictureBox pictureBox, Graphics g, Brush brush, int playerNumber)
        {
            int cellSize = pictureBox.Width / Board.N; // 1マスのサイズ（ピクセル）
            // プレイヤーの座標をセル単位からピクセル座標に変換
            int x = board.player[playerNumber].x * cellSize + 2; // 左上のx座標（2px余白）
            int y = board.player[playerNumber].y * cellSize + 2; // 左上のy座標（2px余白）

            // プレイヤーを楕円（実質的には円）として塗りつぶし描画
            g.FillEllipse(brush, x, y, cellSize - 4, cellSize - 4);

            if (playerNumber != board.currentPlayerNumber) return; // 手番のプレイヤーでなければ次の手を描かない
            if (board.player[playerNumber].possibleMoves == null) return; // nextMoveが未設定なら終了
            foreach ((int,int) move in board.player[playerNumber].possibleMoves) // nextMoveに含まれる各移動候補位置に対して
            {
                int x2 = move.Item1 * cellSize + cellSize * 3/8;
                int y2 = move.Item2 * cellSize + cellSize * 3/8;
                g.FillEllipse(brush, x2, y2, cellSize/4, cellSize/4); // 小さな円を描画
            }
        }
    }
}
