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
        Bomb = -2, Player, Empty, LightBarrier, HeavyBarrier, Wall
    }
    public partial class Square : UserControl
    {
        public static readonly int EnumNegativesCount = Enum.GetValues(typeof(SquareContent)).Cast<int>().Where((x) => x < 0).Count();

        private SquareContent squareContent;
        public SquareContent SquareContent
        {
            get => squareContent;
            set
            {
                squareContent = value;
                switch (value)
                {
                    case SquareContent.Bomb:
                        ForeColor = Color.Black;
                        break;
                    case SquareContent.Wall:
                        ForeColor = Color.DimGray;
                        break;
                    case SquareContent.Player:
                        ForeColor = Color.Blue;
                        break;
                    case SquareContent.LightBarrier:
                        ForeColor = Color.Yellow;
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
