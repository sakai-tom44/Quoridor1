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
        private Task task; // 非同期タスク用

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

            // 【重要】
            // めんどくさくて非同期でUpdateを回すことにしたが将来的には絶対に修正する場所。
            // あらゆる問題の元凶になる可能性がある。
            // 次の候補としてasync/awaitで書き直すことを検討中。
            task = Task.Run(() => // 非同期タスクで実行
            {
                while (true) // 無限ループ
                {
                    System.Threading.Thread.Sleep(1); // 待機してCPU負荷を軽減
                    Update(); // ゲームの進行を更新
                }
            });
        }

        /// <summary>
        /// ゲームの進行を更新するメソッド。
        /// </summary>
        /// (Unity の Update に相当させるつもり。)
        private void Update()
        {
            if (board.player[board.currentPlayer].playerType != PlayerType.Manual) // 現在のプレイヤーが手動操作でない場合
            {
                board.ai.MakeMove(board.currentPlayer); // AIの手を実行
            }
        }

        /// <summary>
        /// リセットボタンがクリックされたときのイベントハンドラ。
        /// ゲームをリセット。
        /// </summary>
        private void Reset_Button_Click(object sender, EventArgs e)
        {
            reset(); // リセットボタンがクリックされたときにゲームをリセット
        }

        /// <summary>
        /// ゲームをリセットし、新しいBoardとRendererを生成して描画。
        /// </summary>
        private void reset()
        {
            board = new Board(pictureBox1); // 新しい盤を生成してPictureBoxに関連付け

            board.renderer.DrawBoard(); // 初期盤を描画
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
            if (board.player[board.currentPlayer].playerType != PlayerType.Manual) return; // 現在手番のプレイヤーが手動操作でない場合は無視

            bool acted = false; // 行動が成功したかどうかのフラグ

            // 縦壁設置の判定（セル境界付近のx座標かどうか）
            if (x % board.cellSize < Board.lineWidth || x % board.cellSize >= (board.cellSize - Board.lineWidth))
            {
                int xi = (x - 10) / board.cellSize; // マスのx座標を計算
                int yi = y / board.cellSize;       // マスのy座標を計算
                if (board.verticalMountable[xi, yi])// 縦壁設置が合法か確認
                {
                    acted = true; // 壁設置が成功した場合
                    board.wallManager.PlaceWall(xi, yi, WallOrientation.Vertical); // 縦壁設置
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
                    board.wallManager.PlaceWall(xi, yi, WallOrientation.Horizontal); // 横壁設置
                }
            }
            // 壁でなければプレイヤーの移動を試みる
            else
            {
                int xi = x / board.cellSize; // マスのx座標を計算
                int yi = y / board.cellSize; // マスのy座標を計算
                acted = board.TryMovePlayer(xi, yi); // プレイヤーを移動
            }

            // 何か行動が成功した場合は盤面を再描画
            if (acted)
            {
                board.NextPlayer(); // 手番を次のプレイヤーに変更
            }
        }
    }
}
