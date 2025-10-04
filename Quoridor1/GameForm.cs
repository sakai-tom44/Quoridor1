using System; // 基本クラスライブラリを使用
using System.Drawing; // 描画関連（未使用だがGUI開発用）
using System.Windows.Forms; // Windowsフォームアプリケーション用

namespace Quoridor1
{
    /// <summary>
    /// Quoridorのゲーム用フォーム。
    /// ボードやレンダラーを管理し、入力イベントを処理。
    /// </summary>
    public partial class GameForm : Form
    {
        private Board board; // ゲームボードのインスタンス
        private Renderer renderer; // 描画処理を担当するレンダラー
        private WallManager wallManager; // 壁の設置を管理するマネージャー

        /// <summary>
        /// コンストラクタ。フォームを初期化。
        /// </summary>
        public GameForm()
        {
            InitializeComponent(); // フォームデザイナで生成されたUIを初期化
        }

        /// <summary>
        /// フォームのロード時に呼ばれるイベントハンドラ。
        /// ゲームをリセットして開始。
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            reset(); // ゲームの状態をリセット
        }

        /// <summary>
        /// ゲームをリセットし、新しいBoardとRendererを生成して描画。
        /// </summary>
        private void reset()
        {
            board = new Board(pictureBox1); // 新しい盤を生成してPictureBoxに関連付け

            renderer = new Renderer(board); // ボードに基づくレンダラーを作成
            renderer.DrawBoard(); // 初期盤を描画

            wallManager = new WallManager(board); // 壁マネージャーを初期化
        }

        /// <summary>
        /// PictureBox上でマウスボタンを離した時のイベントハンドラ。
        /// クリック位置に応じて壁の設置や移動を試みる。
        /// </summary>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseShitei(e.X, e.Y); // マウス座標を指定して処理を実行
        }

        /// <summary>
        /// マウスで指定された座標を処理し、
        /// 壁の設置またはプレイヤーの移動を試みる。
        /// </summary>
        private void mouseShitei(int x, int y)
        {
            bool acted = false; // 行動が成功したかどうかのフラグ

            // 縦壁設置の判定（セル境界付近のx座標かどうか）
            if (x % board.cellSize < Board.lineWidth || x % board.cellSize >= (board.cellSize - Board.lineWidth))
            {
                int xi = (x - 10) / board.cellSize; // マスのx座標を計算
                int yi = y / board.cellSize;       // マスのy座標を計算
                if (board.verticalMountable[xi,yi])// 縦壁設置が合法か確認
                {
                    acted = true; // 壁設置が成功した場合
                    wallManager.SetWall(xi, yi, WallOrientation.Vertical); // 縦壁設置
                }
            }
            // 横壁設置の判定（セル境界付近のy座標かどうか）
            else if (y % board.cellSize < Board.lineWidth || y % board.cellSize >= (board.cellSize - Board.lineWidth))
            {
                int xi = x / board.cellSize;       // マスのx座標を計算
                int yi = (y - 10) / board.cellSize; // マスのy座標を計算
                if (board.horizontalMountable[xi, yi]) // 横壁設置が合法か確認
                {
                    acted = true; // 壁設置が成功した場合
                    wallManager.SetWall(xi, yi, WallOrientation.Horizontal); // 横壁設置
                }
            }
            // 壁でなければプレイヤーの移動を試みる
            else
            {
                int xi = x / board.cellSize; // マスのx座標を計算
                int yi = y / board.cellSize; // マスのy座標を計算
                acted = board.TryMovePlayer0(xi, yi); // プレイヤー0を移動
            }

            // 何か行動が成功した場合は盤面を再描画
            if (acted)
            {
                board.RefreshBoard(); // 盤面情報を更新
                renderer.DrawBoard(); // 盤面を再描画
            }
        }
    }
}
