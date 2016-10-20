using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstarDemo
{
    public partial class Form1 : Form
    {
        MeshSearch mainMesh;
        int unitSize = 4;// 网格单元尺寸
        // 定义绘图用颜色
        Color block_color = Color.Black;
        Color normal_color = Color.White;
        Color start_color = Color.Yellow;
        Color dest_color = Color.Red;
        Color path_color = Color.Green;
        public Form1()
        {
            InitializeComponent();
            initializeWidget();
            initializeGlobalParam();
        }
        void initializeWidget()
        {
            button_gen.Click += new EventHandler(btn_generateShortestPath_Click);
            button_genFile.Click += new EventHandler(btn_genBitmapFile_Click);
            toolStripMenuItem_file_openMeshMap.Click += new EventHandler(mst_openMeshMap_Click);
            panel_bg.AutoScroll = true;
        }
        void initializeGlobalParam()
        {

        }
        void btn_generateShortestPath_Click(object obj, EventArgs ea)
        {
            if (mainMesh == null)
            {
                MessageBox.Show("请先打开网格地图!");
                return;
            }
            try
            {
                int startX = int.Parse(textBox_startX.Text);
                int startY = int.Parse(textBox_startY.Text);
                int destX = int.Parse(textBox_destX.Text);
                int destY = int.Parse(textBox_destY.Text);
                mainMesh.search(mainMesh.MeshMap[startY, startX], mainMesh.MeshMap[destY, destX]);
                // 显示路径点
                if (!mainMesh.FLAG_SUCESS)
                {
                    MessageBox.Show("寻路失败！注意起始点和目标点不能在障碍物（黑色部分）上，起点必须可以到达终点！");
                    return;
                }
                Bitmap bitmap = (Bitmap)pictureBox_show.Image;
                Graphics gp = Graphics.FromImage(bitmap);
                // 筛选路径点， 去掉歧路
                LinkedList<MeshSearch.MeshNode> Path_Node = new LinkedList<MeshSearch.MeshNode>();
                Path_Node.AddFirst(mainMesh.MeshMap[destY, destX]);
                while (mainMesh.Path_Node.Count > 0)
                {
                    if (mainMesh.Path_Node.First().X == Path_Node.First().fatherX &&
                        mainMesh.Path_Node.First().Y == Path_Node.First().fatherY)
                    {
                        Path_Node.AddFirst(mainMesh.Path_Node.First());
                    }
                    mainMesh.Path_Node.RemoveFirst();
                }
                foreach(MeshSearch.MeshNode mn in Path_Node)
                {
                    gp.FillRectangle(new SolidBrush(path_color), new Rectangle(mn.X * unitSize, mn.Y * unitSize, unitSize, unitSize));
                }
                // 标注起点和终点
                gp.FillRectangle(new SolidBrush(start_color), new Rectangle(startX * unitSize, startY * unitSize, unitSize, unitSize));
                gp.DrawString("Start", new Font("Arial", 22), new SolidBrush(Color.Yellow), new Point(startX * unitSize, startY * unitSize));
                gp.FillRectangle(new SolidBrush(dest_color), new Rectangle(destX * unitSize, destY * unitSize, unitSize, unitSize));
                gp.DrawString("Dest", new Font("Arial", 22), new SolidBrush(Color.Red), new Point(destX * unitSize, destY * unitSize));
                pictureBox_show.Image = bitmap;
                if (mainMesh.FLAG_SUCESS)
                {
                    MessageBox.Show("寻路成功!");
                }
                else
                {
                    MessageBox.Show("寻路失败!");
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        void btn_genBitmapFile_Click(object obj, EventArgs ea)
        {
            if (saveFileDialog_mp.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = (Bitmap)pictureBox_show.Image;
                bitmap.Save(saveFileDialog_mp.FileName);
            }
        }
        void mst_openMeshMap_Click(object obj,EventArgs ea)
        {
            if (openFileDialog_mp.ShowDialog() == DialogResult.OK)
            {
                mainMesh = new MeshSearch(openFileDialog_mp.FileName);
                // 根据网格绘图
                Bitmap bitmap = new Bitmap(mainMesh.MeshMap.GetLength(1) * unitSize, mainMesh.MeshMap.GetLength(0) * unitSize);
                Graphics gp = Graphics.FromImage(bitmap);
                foreach(MeshSearch.MeshNode mn in mainMesh.MeshMap)
                {
                    if (mn.isBlock)
                    {
                        gp.FillRectangle(new SolidBrush(block_color), new Rectangle(mn.X * unitSize, mn.Y * unitSize, unitSize, unitSize));
                    }
                    else
                    {
                        gp.FillRectangle(new SolidBrush(normal_color), new Rectangle(mn.X * unitSize, mn.Y * unitSize, unitSize, unitSize));
                    }
                }
                // 显示图片
                pictureBox_show.Image = bitmap;
            }
        }
    }
}
