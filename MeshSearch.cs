using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstarDemo
{
    // 利用A*算法实现静态网格搜索
    class MeshSearch
    {
        public Queue<MeshNode> OPEN_LIST;// open表
        public LinkedList<MeshNode> CLOSED_LIST;// closed表
        public LinkedList<MeshNode> Path_Node;// 路径点记录
        public MeshNode[,] MeshMap;// 静态网格地图
        public bool FLAG_SUCESS;// 是否搜索成功
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mapPath">静态网格地图的文件路径</param>
        public MeshSearch(string mapPath)
        {
            if (!File.Exists(mapPath)) return;
            try {
                StreamReader mapReader = new StreamReader(mapPath, Encoding.Default);
                string line;
                line = mapReader.ReadLine();// 读取第一行文件信息，比如宽和高
                string[] fileInfo = CodeAnalysis.getValueInCSV(line);
                if (fileInfo.Length < 2) return;
                uint width = uint.Parse(fileInfo[0]);
                uint height = uint.Parse(fileInfo[1]);
                MeshMap = new MeshNode[height, width];// 实例化网格地图
                // 读取地图节点信息
                string[] pointInfo;
                int rowIndex = 0;// 行索引
                while((line = mapReader.ReadLine()) != null)
                {
                    pointInfo = CodeAnalysis.getValueInCSV(line);
                    for(int columnIndex = 0;columnIndex < width;columnIndex++)
                    {
                        MeshMap[rowIndex, columnIndex] = new MeshNode(pointInfo[columnIndex] == "1");
                        MeshMap[rowIndex, columnIndex].X = columnIndex;
                        MeshMap[rowIndex, columnIndex].Y = rowIndex;
                    }
                    ++rowIndex;
                }
                mapReader.Close();
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="destination">终点</param>
        public void search(MeshNode start, MeshNode destination)
        {
            FLAG_SUCESS = false;
            if (start.isBlock || destination.isBlock) return;// 起点和终点不能是障碍物
            OPEN_LIST = new Queue<MeshNode>();
            CLOSED_LIST = new LinkedList<MeshNode>();
            Path_Node = new LinkedList<MeshNode>();
            start.Gx = 0;
            OPEN_LIST.Enqueue(start);
            Path_Node.AddFirst(start);
            reCursionSearch(destination);
        }
        /// <summary>
        /// 递归搜索
        /// </summary>
        /// <param name="destination">目标节点</param>
        public void reCursionSearch(MeshNode destination)
        {
            if (FLAG_SUCESS) return;
            if (OPEN_LIST.Count < 1)
            {
                // 搜索失败，退出
                FLAG_SUCESS = false;
                return;
            }
            // 取出open表的第一个节点
            MeshNode nNode = OPEN_LIST.Dequeue();
            Path_Node.AddFirst(nNode);
            CLOSED_LIST.AddLast(nNode);
            if(nNode.X == destination.X && nNode.Y == destination.Y)
            {
                // 搜索成功
                FLAG_SUCESS = true;
                return;
            }
            if (!expandNode(nNode))
            {
                //Path_Node.RemoveLast();
                reCursionSearch(destination);
            }
            // 每次扩展都要对open表重新排序，依据是估价值
            sortOpenList(destination);
            reCursionSearch(destination);
        }
        /// <summary>
        /// 扩展节点
        /// </summary>
        /// <param name="node"></param>
        public bool expandNode(MeshNode node)
        {
            bool FLAG_EXPANDED = false;
            // 按顺时针扩展， 第一个扩展的是左邻居
            // 左邻居
            if(node.X > 0)
            {
                if(!MeshMap[node.Y,node.X - 1].isBlock && !CLOSED_LIST.Contains(MeshMap[node.Y, node.X - 1]))
                {
                    MeshMap[node.Y, node.X - 1].Gx = node.Gx + 1;// 寻找代价增加
                    MeshMap[node.Y, node.X - 1].fatherX = node.X;
                    MeshMap[node.Y, node.X - 1].fatherY = node.Y;
                    bool FLAG_inOpenList = false;
                    foreach(MeshNode mn in OPEN_LIST)
                    {
                        if(mn.X == node.X - 1 && mn.Y == node.Y)
                        {
                            if(mn.Gx > node.Gx + 1)
                            {
                                mn.Gx = node.Gx + 1;
                                mn.fatherX = node.X;
                                mn.fatherY = node.Y;
                            }
                            FLAG_inOpenList = true;
                            break;
                        }
                    }
                    if(!FLAG_inOpenList)OPEN_LIST.Enqueue(MeshMap[node.Y, node.X - 1]);// 添加到open表中
                    FLAG_EXPANDED = true;
                }
            }
            // 上邻居
            if(node.Y > 0)
            {
                if (!MeshMap[node.Y - 1, node.X].isBlock && !CLOSED_LIST.Contains(MeshMap[node.Y - 1, node.X]))
                {
                    MeshMap[node.Y - 1, node.X].Gx = node.Gx + 1;// 寻找代价增加
                    MeshMap[node.Y - 1, node.X].fatherX = node.X;
                    MeshMap[node.Y - 1, node.X].fatherY = node.Y;
                    bool FLAG_inOpenList = false;
                    foreach (MeshNode mn in OPEN_LIST)
                    {
                        if (mn.X == node.X && mn.Y == node.Y - 1)
                        {
                            if (mn.Gx > node.Gx + 1)
                            {
                                mn.Gx = node.Gx + 1;
                                mn.fatherX = node.X;
                                mn.fatherY = node.Y;
                            }
                            FLAG_inOpenList = true;
                            break;
                        }
                    }
                    if(!FLAG_inOpenList)OPEN_LIST.Enqueue(MeshMap[node.Y - 1, node.X]);// 添加到open表中
                    FLAG_EXPANDED = true;
                }
            }
            // 右邻居
            if(node.X < MeshMap.GetLength(1) - 1)
            {
                if (!MeshMap[node.Y, node.X + 1].isBlock && !CLOSED_LIST.Contains(MeshMap[node.Y, node.X + 1]))
                {
                    MeshMap[node.Y, node.X + 1].Gx = node.Gx + 1;// 寻找代价增加
                    MeshMap[node.Y, node.X + 1].fatherX = node.X;
                    MeshMap[node.Y, node.X + 1].fatherY = node.Y;
                    bool FLAG_inOpenList = false;
                    foreach (MeshNode mn in OPEN_LIST)
                    {
                        if (mn.X == node.X + 1 && mn.Y == node.Y)
                        {
                            if (mn.Gx > node.Gx + 1)
                            {
                                mn.Gx = node.Gx + 1;
                                mn.fatherX = node.X;
                                mn.fatherY = node.Y;
                            }
                            FLAG_inOpenList = true;
                            break;
                        }
                    }
                    if(!FLAG_inOpenList)OPEN_LIST.Enqueue(MeshMap[node.Y, node.X + 1]);// 添加到open表中
                    FLAG_EXPANDED = true;
                }
            }
            // 下邻居
            if(node.Y < MeshMap.GetLength(0) - 1)
            {
                if (!MeshMap[node.Y + 1, node.X].isBlock && !CLOSED_LIST.Contains(MeshMap[node.Y + 1, node.X]))
                {
                    MeshMap[node.Y + 1, node.X].Gx = node.Gx + 1;// 寻找代价增加
                    MeshMap[node.Y + 1, node.X].fatherX = node.X;
                    MeshMap[node.Y + 1, node.X].fatherY = node.Y;
                    bool FLAG_inOpenList = false;
                    foreach (MeshNode mn in OPEN_LIST)
                    {
                        if (mn.X == node.X && mn.Y == node.Y + 1)
                        {
                            if (mn.Gx > node.Gx + 1)
                            {
                                mn.Gx = node.Gx + 1;
                                mn.fatherX = node.X;
                                mn.fatherY = node.Y;
                            }
                            FLAG_inOpenList = true;
                            break;
                        }
                    }
                    if(!FLAG_inOpenList)OPEN_LIST.Enqueue(MeshMap[node.Y + 1, node.X]);// 添加到open表中
                    FLAG_EXPANDED = true;
                }
            }
            return FLAG_EXPANDED;
        }
        /// <summary>
        /// 对open表依据估价值进行排序，使用快速排序法
        /// </summary>
        public void sortOpenList(MeshNode destination)
        {
            if (OPEN_LIST.Count < 1) return;
            // 为了便于排序，现将链表转换为数组，同时进行一次不完整的排序
            Queue<MeshNode> testList = OPEN_LIST;
            MeshNode[] arrayNode = new MeshNode[OPEN_LIST.Count];
            for(int index = 0; index < arrayNode.Length; index++)
            {
                if (index > 0)
                {
                    if (OPEN_LIST.ElementAt(index).F(destination) < arrayNode[index - 1].F(destination))
                    {
                        arrayNode[index] = arrayNode[index - 1];
                        arrayNode[index - 1] = OPEN_LIST.ElementAt(index);
                    }
                    else
                    {
                        arrayNode[index] = OPEN_LIST.ElementAt(index);
                    }
                }
                else
                {
                    arrayNode[index] = OPEN_LIST.ElementAt(index);
                }
            }
            // 定义起始索引和尾索引
            uint head_index = 0;
            uint tail_index = (uint)(arrayNode.Length - 1);
            uint key = arrayNode[0].F(destination);
            while(head_index < tail_index)
            {
                if (arrayNode[tail_index].F(destination) < key)
                {
                    MeshNode tempNode = arrayNode[head_index];
                    arrayNode[head_index] = arrayNode[tail_index];
                    arrayNode[tail_index] = tempNode;
                }
                --tail_index;
                if (arrayNode[head_index].F(destination) > key)
                {
                    MeshNode tempNode = arrayNode[head_index];
                    arrayNode[head_index] = arrayNode[tail_index];
                    arrayNode[tail_index] = tempNode;
                }
                ++head_index;
            }
            // 将数组装回链表
            OPEN_LIST = new Queue<MeshNode>();
            for(int index = 0; index < arrayNode.Length; index++)
            {
                OPEN_LIST.Enqueue(arrayNode[index]);
            }
        }
        // 静态网格节点类
        public class MeshNode
        {
            public int X;
            public int Y;
            public bool isBlock;// 该点是否是障碍物
            public uint Gx;// 从起点到该节点的代价
            public int fatherX; // 父节点坐标
            public int fatherY;
            public MeshNode(bool isBlock)
            {
                this.isBlock = isBlock;
            }
            /// <summary>
            /// 启发函数，表示从该点到终点可能的最短距离，保证有 H(x) 不大于 D(x)
            /// </summary>
            /// <param name="destination">目标节点</param>
            /// <returns></returns>
            public uint H(MeshNode destination)
            {
                uint absX = (uint)Math.Abs(X - destination.X);
                uint absY = (uint)Math.Abs(Y - destination.Y);
                // 距离估算采用曼哈顿距离
                return (absX + absY);
            }
            /// <summary>
            /// 估价函数
            /// </summary>
            /// <param name="destination">目标节点</param>
            /// <returns></returns>
            public uint F(MeshNode destination)
            {
                return (Gx + H(destination));
            }
        }
    }
}
