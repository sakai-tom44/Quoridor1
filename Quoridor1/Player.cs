namespace Quoridor1
{
    public class Player
    {
        public int x, y, k; // x,yは現在位置、kは1次元配列での位置
        public (int,int) pos { get { return (x, y); } } // 現在位置をタプルで取得
        public List<(int,int)> possibleMoves; // 次の移動候補
        public PlayerType playerType; // プレイヤーの操作方法の種類
        public int placeWallCount = 0; // 置いた壁の数

        public Player(int startX, int startY, PlayerType playerType)
        {
            x = startX;
            y = startY;
            k = Board.xy2to1(x, y);
            this.playerType = playerType;
        }

        public void Move(int newX, int newY, int newK)
        {
            x = newX;
            y = newY;
            k = newK;
        }
        /// <summary>
        /// 現在の移動候補を更新する
        /// </summary>
        /// <param name="opponent">対戦相手を指定</param>
        public void RefreshPossibleMoves(Board board, Player opponent)
        {
            possibleMoves = PossibleMoves((x, y), (opponent.x, opponent.y), board.moveGraph); // 次の移動候補をクリア
        }
        /// <summary>
        /// 指定された盤面状態における次の移動候補を計算して返す
        /// </summary>
        /// <param name="current">自分の位置</param>
        /// <param name="opponent">相手の位置</param>
        /// <param name="moveGraph">盤面の移動可能グラフ</param>
        /// <returns>次の移動候補のリスト</returns>
        public static List<(int, int)> PossibleMoves((int,int) current, (int, int) opponent, int[,] moveGraph)
        {
            int x, y;
            (x,y) = current;
            int ox, oy;
            (ox, oy) = opponent;

            List<(int, int)> possibleMoves = new List<(int, int)>(); // 次の移動候補をクリア

            List<(int, int)> moveStack = new List<(int, int)>() { (0, 1), (1, 0), (0, -1), (-1, 0) }; // 上下左右の移動ベクトル

            foreach ((int, int) stack in moveStack) // 上下左右の移動を試みる
            {
                int xi = stack.Item1 + x; // 移動先のx座標
                int yi = stack.Item2 + y; // 移動先のy座標

                if ((xi < 0) || (xi >= Board.N) || (yi < 0) || (yi >= Board.N)) continue; // 盤外ならスキップ
                if (moveGraph[Board.xy2to1(x, y), Board.xy2to1(xi, yi)] == 0) continue; // 移動できないならスキップ
                if ((xi == ox) && (yi == oy)) // 相手のいるマスに移動しようとした場合
                {
                    int xi2 = xi + stack.Item1; // 相手の向こう側のマス
                    int yi2 = yi + stack.Item2;// 相手の向こう側のマス

                    bool flag = false; // 相手の向こう側に移動できるかどうかのフラグ

                    // 一つの条件分岐にまとめてmoveGraphを参照すると配列の長さの外を参照する可能性があるため、二段階に分けて確認(エラー回避)
                    if (!((xi2 < 0) || (xi2 >= Board.N) || (yi2 < 0) || (yi2 >= Board.N))) // 相手の向こう側のマスが盤外でなくかつ↓
                    {
                        if (moveGraph[Board.xy2to1(xi, yi), Board.xy2to1(xi2, yi2)] == 1) // 相手の向こう側に移動できるなら
                        {
                            flag = true;
                        }
                    }
                    if (flag) // 相手の向こう側に移動できるなら
                    {
                        possibleMoves.Add((xi2, yi2)); // 相手の向こう側に移動候補を追加
                    }
                    else // 相手の向こう側に移動できないなら、横に回避できるか確認
                    {
                        List<(int, int)> sideStack = new List<(int, int)>();
                        if (stack.Item1 == 0) // 縦移動しようとしている場合
                        {
                            sideStack.Add((1, 0)); // 右
                            sideStack.Add((-1, 0)); // 左
                        }
                        else // 横移動しようとしている場合
                        {
                            sideStack.Add((0, 1)); // 上
                            sideStack.Add((0, -1)); // 下
                        }
                        foreach ((int, int) s in sideStack) // 横に回避できるか確認
                        {
                            int xi3 = ox + s.Item1; // 回避先のx座標
                            int yi3 = oy + s.Item2; // 回避先のy座標
                            if ((xi3 < 0) || (xi3 >= Board.N) || (yi3 < 0) || (yi3 >= Board.N)) continue; // 盤外ならスキップ
                            if (moveGraph[Board.xy2to1(ox, oy), Board.xy2to1(xi3, yi3)] == 1) // 横に回避できるなら
                            {
                                possibleMoves.Add((xi3, yi3)); // 回避先に移動候補を追加
                            }
                        }
                    }
                }
                else // 相手のいないマスに移動する場合
                {
                    possibleMoves.Add((xi, yi)); // 移動候補を追加
                }
            }

            return possibleMoves;
        }
    }
    /// <summary>
    /// プレイヤーの操作方法の種類
    /// </summary>
    public enum PlayerType { Manual, Random , AI}
}

