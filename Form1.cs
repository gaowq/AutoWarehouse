using AutoWarehouse.basic;
using AutoWarehouse.cal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoWarehouse
{
    public partial class Form1 : Form
    {
        //public PictureBox PictureBox;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //pictureBox1.Load();
        }

        private const int unit = 20;

        public List<int> carList = new List<int>();

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void DrawCircular(Graphics g, Brush brush, int x, int y)
        {
            g.FillEllipse(brush, x * unit + 2, y * unit + 2, unit - 3, unit - 3);
        }

        private void DrawRect(Graphics g, Brush brush, int x, int y)
        {
            g.FillRectangle(brush, x * unit + 2, y * unit + 2, unit - 3, unit - 3);
        }

        private void DrawSmallRect(Graphics g, Brush brush, int x, int y)
        {
            g.FillRectangle(brush, x * unit + 4, y * unit + 4, unit - 7, unit - 7);
        }


        private InitCal initCalculate = new InitCal();
        public int xNum = 20;
        public int yNum = 20;
        private void start_Click(object sender, EventArgs e)
        {
            RePaint();
            //测试位置为8,9,3
            initCalculate.init(xNum, yNum, 304, 3);

            myTimer.Tick += new EventHandler(FreshCross_Click);
            myTimer.Enabled = true;
            myTimer.Interval = 1000;
        }

        public void RePaint()
        {
            this.pictureBox1.Refresh();
            Graphics g = this.pictureBox1.CreateGraphics();
            Pen pen = new Pen(Color.Gray, 1);

            for (float i = 0; i <= xNum; i++)
            {
                g.DrawLine(pen, i * unit, 0, i * unit, yNum * unit);
            }

            for (float j = 0; j <= yNum; j++)
            {
                g.DrawLine(pen, 0, j * unit, xNum * unit, j * unit);
            }

            //绘制原点
            DrawCircular(g, Brushes.Red, 1, 1);

            foreach (var car in carList)
            {
                DrawRect(g, Brushes.DarkRed, car % 100, car / 100);
                DrawSmallRect(g, Brushes.DarkRed, car % 100, car / 100 + 1);
            }
        }

        private void FreshCross_Click(object sender, EventArgs e)
        {
            carList = new List<int>();

            foreach (var car in BaseData.carList)
            {
                carList.Add(car.coordinate);
            }

            RePaint();
        }

        System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();//实例化　
    }
}
