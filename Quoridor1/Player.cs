namespace Quoridor1
{
    public class Player
    {
        public int x, y, k;
        public List<(int,int)> nextMove;

        public Player(int startX, int startY)
        {
            x = startX;
            y = startY;
            k = Board.xy2to1(x, y);
        }

        public void Move(int newX, int newY, int newK)
        {
            x = newX;
            y = newY;
            k = newK;
        }
        /// <summary>
        /// 次の移動候補を更新する
        /// </summary>
        /// <param name="opponent">対戦相手を指定</param>
        public void RefreshNextMove(Board board, Player opponent)
        {
            nextMove = new List<(int,int)>(); // 次の移動候補をクリア

            List<(int,int)> moveStack = new List<(int,int)>() {(0, 1), (1, 0), (0, -1), (-1, 0)}; // 上下左右の移動ベクトル

            foreach ((int,int) stack in moveStack) // 上下左右の移動を試みる
            {
                int xi = stack.Item1 + x; // 移動先のx座標
                int yi = stack.Item2 + y; // 移動先のy座標

                if ((xi < 0) || (xi >= Board.N) || (yi < 0) || (yi >= Board.N)) continue; // 盤外ならスキップ
                if (board.moveGraph[Board.xy2to1(x,y),Board.xy2to1(xi,yi)] == 0) continue; // 移動できないならスキップ
                if ((xi == opponent.x) && (yi == opponent.y)) // 相手のいるマスに移動しようとした場合
                {
                    int xi2 = xi + stack.Item1; // 相手の向こう側のマス
                    int yi2 = yi + stack.Item2;// 相手の向こう側のマス
                    if ((xi2 < 0) || (xi2 >= Board.N) || (yi2 < 0) || (yi2 >= Board.N)) continue; // 盤外ならスキップ
                    if (board.moveGraph[Board.xy2to1(xi, yi), Board.xy2to1(xi2, yi2)] == 1) // 相手の向こう側に移動できるなら
                    {
                        nextMove.Add((xi2, yi2)); // 相手の向こう側に移動候補を追加
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
                            int xi3 = opponent.x + s.Item1; // 回避先のx座標
                            int yi3 = opponent.y + s.Item2; // 回避先のy座標
                            if ((xi3 < 0) || (xi3 >= Board.N) || (yi3 < 0) || (yi3 >= Board.N)) continue; // 盤外ならスキップ
                            if (board.moveGraph[Board.xy2to1(opponent.x, opponent.y), Board.xy2to1(xi3, yi3)] == 1) // 横に回避できるなら
                            {
                                nextMove.Add((xi3, yi3)); // 回避先に移動候補を追加
                            }
                        }
                    }
                }
                else // 相手のいないマスに移動する場合
                {
                    nextMove.Add((xi, yi)); // 移動候補を追加
                }
            }
        }
    }
}

