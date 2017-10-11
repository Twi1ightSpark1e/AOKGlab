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
        Empty, Barrier, Player, Wall
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
                        ForeColor = Color.Red;
                        break;
                    case SquareContent.Player:
                        ForeColor = Color.Blue;
                        break;
                    case SquareContent.Barrier:
                        ForeColor = Color.Black;
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
