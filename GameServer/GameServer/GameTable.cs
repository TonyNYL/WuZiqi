//-------------------GameTable.cs-----------------//
using System;
using System.Timers;
using System.Windows.Forms;
namespace GameServer
{
    class GameTable
    {
        private const int None = -1;       //无棋子
        private const int Black = 0;       //黑色棋子
        private const int White = 1;       //白色棋子
        public Player[] gamePlayer;        //保存同一桌的玩家信息
        private int[,] grid = new int[15, 15];       //15*15的方格
        private int NextdotColor = 0;            //应该产生黑棋子还是白棋子
        private ListBox listbox;
        Service service;

        public GameTable(ListBox listbox)
        {
            gamePlayer = new Player[2];
            gamePlayer[0] = new Player();
            gamePlayer[1] = new Player();
            this.listbox = listbox;
            service = new Service(listbox);
            ResetGrid();
        }

        /// <summary>重置棋盘</summary>
        public void ResetGrid()
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = None;
                }
            }
        }

        /// <summary>发送产生的棋子信息</summary>
        /// <param name="i">指定棋盘的第几行</param>
        /// <param name="j">指定棋盘的第几列</param>
        /// <param name="dotColor">棋子颜色</param>
        public void SetDot(int i, int j, int dotColor)
        {
            //向两个用户发送产生的棋子信息，并判断是否有相邻棋子
            //发送格式：SetDot,行,列,颜色
            grid[i, j] = dotColor;
            service.SendToBoth(this, string.Format("SetDot,{0},{1},{2}", i, j, dotColor));
            if (judgeWin() != -1)
            {
                ShowWin(judgeWin());
            }
        }

        private int judgeWin()
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    if (grid[i, j] != -1)
                    {
                        //右
                        for (int x = 1; x < 5 && x + i <= grid.GetUpperBound(0); x++)
                        {
                            if (grid[i + x, j] != grid[i, j])
                            {
                                break;
                            }
                            else
                            {
                                if (x == 4)
                                {
                                    return grid[i, j];
                                }

                            }
                        }
                        //下
                        for (int y = 1; y < 5 && y + j <= grid.GetUpperBound(1); y++)
                        {
                            if (grid[i, j + y] != grid[i, j])
                            {
                                break;
                            }
                            else
                            {
                                if (y == 4)
                                {
                                    return grid[i, j];
                                }

                            }
                        }
                        //右下
                        for (int y = 1; y < 5 && y + i <= grid.GetUpperBound(0) && y + j <= grid.GetUpperBound(1); y++)
                        {

                            if (grid[i + y, j + y] != grid[i, j])
                            {
                                break;
                            }
                            else
                            {
                                if (y == 4)
                                {
                                    return grid[i, j];
                                }

                            }
                        }
                        //左下
                        for (int y = 1; y < 5 && i - y > 0 && y + j <= grid.GetUpperBound(1); y++)
                        {

                            if (grid[i - y, j + y] != grid[i, j])
                            {
                                break;
                            }
                            else
                            {
                                if (y == 4)
                                {
                                    return grid[i, j];
                                }

                            }
                        }


                    }
                }
            }
            return None;
        }
        /// <summary>出现相邻点的颜色为dotColor</summary>
        /// <param name="dotColor">相邻点的颜色</param>
        private void ShowWin(int dotColor)
        {
            gamePlayer[0].started = false;
            gamePlayer[1].started = false;
            this.ResetGrid();
            //发送格式：Win,相邻点的颜色,黑方成绩,白方成绩
            service.SendToBoth(this, string.Format("Win,{0}",dotColor));
        }
        /// <summary>消去棋子的信息</summary>
        /// <param name="i">指定棋盘的第几行</param>
        /// <param name="j">指定棋盘的第几列</param>
        /// <param name="color">指定棋子颜色</param>
        public void UnsetDot(int i, int j, int color)
        {
            //向两个用户发送消去棋子的信息
            //格式：UnsetDot,行,列,黑方成绩,白方成绩
            grid[i, j] = None;
            string str = string.Format("UnsetDot,{0},{1}",
                i, j);
            service.SendToBoth(this, str);
        }

    }
}
