using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace Quoridor1
{
    public class AI
    {
        private Board board; // ゲームボードのインスタンス
        public AI(Board board)
        {
            this.board = board; // Boardインスタンスを保持
        }

        /// <summary>
        /// 次の一手を計算し、AIプレイヤーが移動。
        /// </summary>
        public bool MakeMove(int playerNumber)
        {
            // AIの手番でなければ何もしない
            if (board.player[board.currentPlayer].playerType != PlayerType.AI) return false;
            // 現在のプレイヤーと相手プレイヤーを取得
            Player currentPlayer = board.player[board.currentPlayer];
            Player opponentPlayer = board.player[1 - board.currentPlayer];
            // 移動可能なマスを取得
            List<(int, int)> possibleMoves = currentPlayer.possibleMoves;
            if (possibleMoves.Count > 0)
            {
                // ランダムに移動先を選択
                Random rand = new Random();
                var move = possibleMoves[rand.Next(possibleMoves.Count)];
                currentPlayer.x = move.Item1;
                currentPlayer.y = move.Item2;
                // 手番を交代
                board.NextPlayer();
                return true; // 移動が成功したことを示す
            }
            return false; // 移動できるマスがない場合
        }
    }
}
