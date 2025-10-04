using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quoridor1
{
    public class DebugBoard
    {
        public static void PrintMoveGraph(int[,] moveGraph)
        {
            Console.WriteLine("Move Graph");
            for (int i = 0; i < Board.N * Board.N; i++)
            {
                for (int j = 0; j < Board.N * Board.N; j++)
                {
                    Console.Write(moveGraph[i, j] + " ");
                }
                Console.WriteLine();
            }
        }


        public static void PrintMountable(Board board)
        {
            Console.WriteLine("   Horizontal   |    Vertical");
            for (int y = 0; y < Board.N - 1; y++)
            {
                string str1 = "";
                string str2 = "";
                for (int x = 0; x < Board.N - 1; x++)
                {
                    str1 += board.horizontalMountable[x, y] ? "□" : "■";
                    str2 += board.verticalMountable[x, y] ? "□" : "■";
                }
                Console.WriteLine(str1 + "|" + str2);
            }
        }
    }
}
