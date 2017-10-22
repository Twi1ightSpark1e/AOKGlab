using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public enum SquareContent
    {
        Player = -1, Empty, LightBarrier, HeavyBarrier, Wall
    }

    public partial class Square : UserControl
    {
        private SquareContent squareContent;
        public SquareContent SquareContent
        {
            get => squareContent;
            set
            {
                squareContent = value;
                switch (value)
                {
                    case SquareContent.Wall:
                        ForeColor = Color.Black;
                        break;
                    case SquareContent.Player:
                        ForeColor = Color.Blue;
                        break;
                    case SquareContent.LightBarrier:
                        ForeColor = Color.DarkGoldenrod;
                        break;
                    case SquareContent.HeavyBarrier:
                        ForeColor = Color.Gray;
                        break;
                    case SquareContent.Empty:
                        ForeColor = Color.White;
                        break;
                }
            }
        }
        public Square()
        {
            InitializeComponent();
            SquareContent = SquareContent.Empty;
        }

        private void Square_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(new Point(1, 1), new Size(Width - 1, Height - 1));
            e.Graphics.DrawRectangle(Pens.Black, rectangle);
            e.Graphics.FillRectangle(new SolidBrush(ForeColor), rectangle);
        }
    }
}
