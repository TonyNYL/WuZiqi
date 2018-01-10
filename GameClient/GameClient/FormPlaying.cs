using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
namespace GameClient
{
    public partial class FormPlaying : Form
    {
        private int tableIndex;
        private int side;
        private DotColor[,] grid = new DotColor[15, 15]; //保存颜色，用于消点时进行判断
        private Bitmap blackBitmap;
        private Bitmap whiteBitmap;
        private int hold = 0;              //黑色先持子
        //命令是否来自服务器
        private bool isReceiveCommand = false;
        private Service service;
        delegate void LabelDelegate(Label label, string str);
        delegate void ButtonDelegate(Button button, bool flag);
        delegate void RadioButtonDelegate(RadioButton radioButton, bool flag);
        delegate void SetDotDelegate(int i, int j, int dotColor);
        LabelDelegate labelDelegate;
        ButtonDelegate buttonDelegate;
        RadioButtonDelegate radioButtonDelegate;
        public FormPlaying(int TableIndex, int Side, StreamWriter sw)
        {
            InitializeComponent();
            this.tableIndex = TableIndex;
            this.side = Side;
            labelDelegate = new LabelDelegate(SetLabel);
            buttonDelegate = new ButtonDelegate(SetButton);
            radioButtonDelegate = new RadioButtonDelegate(SetRadioButton);
            blackBitmap = new Bitmap(Properties.Resources.black);
            whiteBitmap = new Bitmap(Properties.Resources.white);
            service = new Service(listBox1, sw);
        }
        /// <summary>载入窗体时发生的事件</summary>
        private void FormPlaying_Load(object sender, EventArgs e)
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = DotColor.None;
                }
            }
            labelSide0.Text = "";
            labelSide1.Text = "";
        }
        /// <summary>设置标签显示信息</summary>
        /// <param name="Label">要设置的Label</param>
        /// <param name="string">要设置的信息</param>
        public void SetLabel(Label label, string str)
        {
            if (label.InvokeRequired)
            {
                this.Invoke(labelDelegate, label, str);
            }
            else
            {
                label.Text = str;
            }
        }
        /// <summary>设置button是否可用</summary>
        /// <param name="Button">要设置的Button</param>
        /// <param name="flag">是否可用</param>
        private void SetButton(Button button, bool flag)
        {
            if (button.InvokeRequired)
            {
                this.Invoke(buttonDelegate, button, flag);
            }
            else
            {
                button.Enabled = flag;
            }
        }
        /// <summary>设置radioButton选择状态</summary>
        /// <param name="radioButton">要设置的RadioButton</param>
        /// <param name="flag">是否选中</param>
        private void SetRadioButton(RadioButton radioButton, bool flag)
        {
            if (radioButton.InvokeRequired)
            {
                this.Invoke(radioButtonDelegate, radioButton, flag);
            }
            else
            {
                radioButton.Checked = flag;
            }
        }
        /// <summary>设置棋子状态</summary>
        /// <param name="i">第几行</param>
        /// <param name="j">第几列</param>
        /// <param name="dotColor">棋子颜色</param>
        public void SetDot(int i, int j, DotColor dotColor)
        {
            service.AddItemToListBox(string.Format("{0},{1},{2}", i, j, dotColor));
            grid[i, j] = dotColor;
            pictureBox1.Invalidate();
        }
        /// <summary>重新开始新游戏</summary>
        /// <param name="str">警告信息</param>
        public void Restart(string str)
        {
            MessageBox.Show(str, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ResetGrid();
            SetButton(buttonStart, true);
        }
        /// <summary>重置棋盘</summary>
        public void ResetGrid()
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = DotColor.None;
                }
            }
            pictureBox1.Invalidate();
        }
        /// <summary>取消棋子</summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        public void UnsetDot(int x, int y)
        {
            grid[x / 20 - 1, y / 20 - 1] = DotColor.None;
            pictureBox1.Invalidate();
        }
        /// <summary>绘制图像</summary>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    if (grid[i, j] != DotColor.None)
                    {
                        if (grid[i, j] == DotColor.Black)
                        {
                            g.DrawImage(blackBitmap, (i + 1) * 20, (j + 1) * 20);
                        }
                        else
                        {
                            g.DrawImage(whiteBitmap, (i + 1) * 20, (j + 1) * 20);
                        }
                    }
                }
            }
        }

        /// <summary>向服务器发送消息</summary>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            //字符串格式：Talk,桌号,对话内容
            service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
        }
        /// <summary>对话内容改变时触发的事件</summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                //字符串格式：Talk,桌号,对话内容
                service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
            }
        }
        /// <summary>点击帮助按钮时触发的事件</summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            string str =
                "哈哈哈，五子棋不会玩么？还点帮助。\n";
            MessageBox.Show(str, "帮助信息");
        }
        /// <summary>点击开始按钮时触发的事件</summary>
        private void buttonStart_Click(object sender, EventArgs e)
        {
            service.SendToServer(string.Format("Start,{0},{1}", tableIndex, side));
            this.buttonStart.Enabled = false;
        }
        /// <summary>点击退出按钮时触发的事件</summary>
        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>关闭窗体时触发的事件</summary>
        private void FormPlaying_FormClosing(object sender, FormClosingEventArgs e)
        {
            //格式：GetUp,桌号,座位号
            service.SendToServer(string.Format("GetUp,{0},{1}", tableIndex, side));
        }
        /// <summary>FormRoom中的线程调用此方法关闭此窗体</summary>
        public void StopFormPlaying()
        {
            Application.Exit();
        }
        /// <summary>在pictureBox1中按下鼠标触发的事件</summary>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (hold == side)
            {
                int x = e.X / 20;
                int y = e.Y / 20;
                if (!(x < 1 || x > 15 || y < 1 || y > 15))
                {
                    if (grid[x - 1, y - 1] == DotColor.None)
                    {
                        int color = (int)grid[x - 1, y - 1];
                        //发送格式：SetDot,桌号,座位号,行,列,颜色
                        service.SendToServer(string.Format(
                           "setDot,{0},{1},{2},{3}", tableIndex, side, x - 1, y - 1));
                    }
                }
            }
            else
            {
                MessageBox.Show("现在不是你持子。");
            }
            
        }
        //改变当前持子方
        public void changeHold()
        {
            hold = (hold + 1) % 2;
        }
        public void sendRestart()
        {
            service.SendToServer(string.Format("restar,{0}",tableIndex));
        }
        /// <summary>
        /// 设置玩家信息，格式：座位号，labelSide显示的信息，listbox显示的信息
        /// </summary>
        /// <param name="sideString">指定玩家</param>
        /// <param name="labelSideString">labelSide显示的信息</param>
        /// <param name="listBoxString">listbox显示的信息</param>
        public void SetTableSideText(string sideString, string labelSideString, string listBoxString)
        {
            string s = "白方";
            if (sideString == "0")
            {
                s = "黑方：";
            }
            //判断自己是黑方还是白方
            if (sideString == side.ToString())
            {
                SetLabel(labelSide1, s + labelSideString);
            }
            else
            {
                SetLabel(labelSide0, s + labelSideString);
            }
            service.AddItemToListBox(listBoxString);
        }
        /// <summary>显示谈话信息</summary>
        /// <param name="talkMan">谈话者</param>
        /// <param name="str">要显示的信息</param>
        public void ShowTalk(string talkMan, string str)
        {
            service.AddItemToListBox(string.Format("{0}说：{1}", talkMan, str));
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="str">要显示的消息</param>
        public void ShowMessage(string str)
        {
            service.AddItemToListBox(str);
        }

        private void buttonTie_Click(object sender, EventArgs e)
        {
            if (side == 0)
            {
                service.SendToServer(string.Format(
                                           "Tie,{0},{1}", tableIndex, side));
            }
            if (side == 1)
            {
                service.SendToServer(string.Format("Tie,{0},{1}", tableIndex, side));
            }
        }


    }
}