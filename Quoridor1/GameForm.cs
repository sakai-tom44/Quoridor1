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
        private Board mainBoard; // ゲームボードのインスタンス
        private bool manualWait = false; // 手動操作待機フラグ

        public int cellSize { get { return pictureBox1.Width / Board.N; } } // 1マスのサイズ（ピクセル）

        public EvaluateParam[] evaluateParams = new EvaluateParam[2] { new EvaluateParam(), new EvaluateParam() }; // 評価関数のパラメータ

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
            Task.Run(() => // 非同期タスクで実行
            {
                while (true) // 無限ループ
                {
                    System.Threading.Thread.Sleep(1); // 待機してCPU負荷を軽減

                    if (mainBoard.gameOver)
                    {
                        if (Board.autoReset) // 自動リセットが有効なら
                        {
                            System.Threading.Thread.Sleep(500); // 0.5秒待機
                            reset(); // ゲームをリセット
                        }
                        continue; // ゲームが終了している場合は何もしない
                    }
                    if (mainBoard.player[mainBoard.currentPlayerNumber].playerType == PlayerType.Manual) // 現在のプレイヤーが手動操作の場合
                    {
                        manualWait = true; // 手動操作待機フラグを立てる
                        
                        while (manualWait) // 手動操作待機中
                        {
                            System.Threading.Thread.Sleep(10); // 待機してCPU負荷を軽減
                        }
                    }
                    else
                    {
                        AI.ComputeNextAction(mainBoard ,mainBoard.currentPlayerNumber); // AIの手を実行
                    }

                    mainBoard.NextPlayer(); // 手番を次のプレイヤーに変更
                    Renderer.DrawBoard(mainBoard, pictureBox1); // 初期盤を描画
                    mainBoard.CheckGameOver(); // ゲーム終了をチェック
                }
            });
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
            mainBoard = new Board(evaluateParams); // 新しい盤を生成してPictureBoxに関連付け

            Renderer.DrawBoard(mainBoard, pictureBox1); // 初期盤を描画
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
            if (mainBoard.gameOver) return; // ゲームが終了している場合は無視
            if (!manualWait) return; // 手動操作待機中でない場合は無視
            if (mainBoard.player[mainBoard.currentPlayerNumber].playerType != PlayerType.Manual) return; // 現在手番のプレイヤーが手動操作でない場合は無視

            bool acted = false; // 行動が成功したかどうかのフラグ

            // 縦壁設置の判定（セル境界付近のx座標かどうか）
            if (x % cellSize < Board.lineWidth || x % cellSize >= (cellSize - Board.lineWidth))
            {
                int xi = (x - 10) / cellSize; // マスのx座標を計算
                int yi = y / cellSize;       // マスのy座標を計算
                if (mainBoard.verticalMountable[xi, yi])// 縦壁設置が合法か確認
                {
                    acted = true; // 壁設置が成功した場合
                    mainBoard.wallManager.PlaceWall(xi, yi, WallOrientation.Vertical); // 縦壁設置
                }
            }
            // 横壁設置の判定（セル境界付近のy座標かどうか）
            else if (y % cellSize < Board.lineWidth || y % cellSize >= (cellSize - Board.lineWidth))
            {
                int xi = x / cellSize;       // マスのx座標を計算
                int yi = (y - 10) / cellSize; // マスのy座標を計算
                if (mainBoard.horizontalMountable[xi, yi]) // 横壁設置が合法か確認
                {
                    acted = true; // 壁設置が成功した場合
                    mainBoard.wallManager.PlaceWall(xi, yi, WallOrientation.Horizontal); // 横壁設置
                }
            }
            // 壁でなければプレイヤーの移動を試みる
            else
            {
                int xi = x / cellSize; // マスのx座標を計算
                int yi = y / cellSize; // マスのy座標を計算
                acted = mainBoard.TryMovePlayer(xi, yi); // プレイヤーを移動
            }

            // 何か行動が成功した場合は手番を終わる
            if (acted)
            {
                manualWait = false; // 手動操作待機フラグを下ろす
            }
        }
    }
}
