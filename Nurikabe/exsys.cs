using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Nurikabe
{
    public class exsys
    {
        public static int X, Y;
        public static bool BLACK;
        public static String[] knowledge = new String[] {
            "",                                                                                     //0
            "白格数量足够，其相邻的格子是墙",                                                       //1
            "一个格子与两个不属于一个连通块且均含数字的白格相邻，则其为墙",                         //2
            "当2的非对面两个格子是墙，另两格子同时相邻的格子是墙",                                  //3 "白连通块与目标大小差为1，且恰有两个可行格，他们均与一格相邻，则那一格是墙"
            "白格数量不足或不含数字，且只有一格可以延伸，那一格是白格",                             //4
            "墙连通块只有一格可以延伸，且知识4推理失败，那一格是墙",                                //5
            "2x2里三格是墙，剩下的是白格",                                                          //6
            "没有数字可以延伸到，那一格是墙",                                                       //7
            "当数字的非对面两个格子是墙，另两格子同时相邻的那个格子相邻有数字，则其为墙",           //8
            "一格与其他白格八连通后会将已有墙分隔，则其为墙",                                       //9
            "一格与墙八连通后会将已有白格与所有数字分隔，则其为白格",                               //10
            "一格相邻于多个不连通白格，其中一连通块含数字，若这些块格数和大于等于数字，则其为黑格"  //11
            //"所有可行白格方案均导致有一格是白格，那一格是白格",                                     //???
        };
        static int[,] Near4 = new int[4, 2] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
        static int[,] Near8 = new int[8, 2] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        private static bool knowledge1(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.map[i, j] != 0)
                    {
                        List<point> list = bfs4(pzmap, i, j, false);
                        if (list.Count == pzmap.map[i, j])
                        {
                            for (int k = 0; k < list.Count; k++)
                                for (int k2 = 0; k2 < 4; k2++)
                                {
                                    int XX = list[k].x + Near4[k2, 0], YY = list[k].y + Near4[k2, 1];
                                    if (XX < 0 || XX >= x) continue;
                                    if (YY < 0 || YY >= y) continue;
                                    if (pzmap.map[XX, YY] != 0) continue;
                                    if (pzmap.black[XX, YY] != 0) continue;
                                    X = XX;
                                    Y = YY;
                                    BLACK = true;
                                    return true;
                                }
                        }
                    }
                }
            return false;
        }
        private static bool knowledge2(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y, cnt = 0;
            int[,] map = new int[x, y];
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.map[i, j] != 0)
                    {
                        var list = bfs4(pzmap, i, j, false);
                        cnt++;
                        foreach (var ll in list)
                            map[ll.x, ll.y] = cnt;
                    }
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.black[i, j] == 0 && pzmap.map[i, j] == 0)
                    {
                        HashSet<int> set = new HashSet<int>();
                        for (int k = 0; k < 4; k++)
                        {
                            int XX = i + Near4[k, 0], YY = j + Near4[k, 1];
                            if (XX < 0 || XX >= x) continue;
                            if (YY < 0 || YY >= y) continue;
                            if (map[XX, YY] != 0) set.Add(map[XX, YY]);
                        }
                        if (set.Count > 1)
                        {
                            X = i;
                            Y = j;
                            BLACK = true;
                            return true;
                        }
                    }
            return false;
        }
        private static bool knowledge3(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.map[i, j] == 2)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            int num = 0;
                            for (int k2 = 0; k2 < 4; k2++)
                            {
                                int XX = i + Near4[(k + k2) % 4, 0], YY = j + Near4[(k + k2) % 4, 1];
                                if (XX < 0 || XX >= x) num += 1 << k2;
                                else if (YY < 0 || YY >= y) num += 1 << k2;
                                else if (pzmap.black[XX, YY] == 1) num += 1 << k2;
                                else if (pzmap.black[XX, YY] == 2)
                                {
                                    num = 999;
                                    break;
                                }
                            }
                            if (num == 3)
                            {
                                int XX = i + Near4[(k + 2) % 4, 0] + Near4[(k + 3) % 4, 0], YY = j + Near4[(k + 2) % 4, 1] + Near4[(k + 3) % 4, 1];
                                if (pzmap.black[XX, YY] == 1) continue;
                                X = XX;
                                Y = YY;
                                BLACK = true;
                                return true;
                            }
                        }
                    }
                }
            return false;
        }
        private static bool knowledge4(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            bool[,] done = new bool[x, y];
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if ((pzmap.black[i, j] == 2 || pzmap.map[i, j] != 0) && !done[i, j])
                    {
                        List<point> list = bfs4(pzmap, i, j, false);
                        List<point> ooo = new List<point>();
                        int num = -1;
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (pzmap.map[list[k].x, list[k].y] != 0)
                            {
                                num = pzmap.map[list[k].x, list[k].y];
                            }
                            for (int k2 = 0; k2 < 4; k2++)
                            {
                                int XX = list[k].x + Near4[k2, 0], YY = list[k].y + Near4[k2, 1];
                                if (XX < 0 || XX >= x) continue;
                                if (YY < 0 || YY >= y) continue;
                                if (pzmap.map[XX, YY] != 0) continue;
                                if (pzmap.black[XX, YY] != 0) continue;
                                ooo.Add(new point(XX, YY));
                            }
                        }
                        //MessageBox.Show("ij:" + i + " " + j + "; num:" + num + "; listsize:" + list.Count + "; ooosize:" + ooo.Count);
                        bool flag = ooo.Count > 0;
                        for (int k = 0; k + 1 < ooo.Count; k++)
                            if (ooo[k].x != ooo[k + 1].x || ooo[k].y != ooo[k + 1].y) flag = false;
                        if (flag && (num == -1 || num > list.Count))
                        {
                            X = ooo[0].x;
                            Y = ooo[0].y;
                            BLACK = false;
                            return true;
                        }
                        foreach (point ii in list)
                            done[ii.x, ii.y] = true;
                    }
                }
            return false;
        }
        private static bool knowledge5(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            bool[,] done = new bool[x, y];
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.black[i, j] == 1 && !done[i, j])
                    {
                        List<point> list = bfs4(pzmap, i, j, true);
                        List<point> ooo = new List<point>();
                        for (int k = 0; k < list.Count; k++)
                            for (int k2 = 0; k2 < 4; k2++)
                            {
                                int XX = list[k].x + Near4[k2, 0], YY = list[k].y + Near4[k2, 1];
                                if (XX < 0 || XX >= x) continue;
                                if (YY < 0 || YY >= y) continue;
                                if (pzmap.map[XX, YY] != 0) continue;
                                if (pzmap.black[XX, YY] != 0) continue;
                                ooo.Add(new point(XX, YY));
                            }
                        bool flag = ooo.Count > 0;
                        for (int k = 0; k + 1 < ooo.Count; k++)
                            if (ooo[k].x != ooo[k + 1].x || ooo[k].y != ooo[k + 1].y) flag = false;
                        if (flag)
                        {
                            X = ooo[0].x;
                            Y = ooo[0].y;
                            BLACK = true;
                            return true;
                        }
                        foreach (point ii in list)
                            done[ii.x, ii.y] = true;
                    }
                }
            return false;
        }
        private static bool knowledge6(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x - 1; i++)
                for (int j = 0; j < y - 1; j++)
                {
                    int num = 0;
                    for (int k1 = 0; k1 < 2; k1++)
                        for (int k2 = 0; k2 < 2; k2++)
                            if (pzmap.black[i + k1, j + k2] == 1) num++;
                    if (num == 3)
                    {
                        for (int k1 = 0; k1 < 2; k1++)
                            for (int k2 = 0; k2 < 2; k2++)
                                if (pzmap.black[i + k1, j + k2] == 0 && pzmap.map[i + k1, j + k2] == 0)
                                {
                                    X = i + k1;
                                    Y = j + k2;
                                    BLACK = false;
                                    return true;
                                }
                    }
                }
            return false;
        }
        private static bool knowledge7(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            int[,] map = new int[x, y];
            List<point> line = new List<point>();
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.map[i, j] != 0)
                    {
                        var list = bfs4(pzmap, i, j, false);
                        foreach (var ll in list)
                        {
                            map[ll.x, ll.y] = pzmap.map[i, j] - list.Count + 1;
                            line.Add(ll);
                        }
                    }
            for (int now = 0; now < line.Count; now++)
            {
                int xx = line[now].x, yy = line[now].y;
                int mm = map[xx, yy];
                for (int k = 0; k < 4; k++)
                {
                    int XX = xx + Near4[k, 0], YY = yy + Near4[k, 1];
                    if (XX < 0 || XX >= x) continue;
                    if (YY < 0 || YY >= y) continue;
                    if (pzmap.black[XX, YY] == 1) continue;
                    if (map[XX, YY] >= mm - 1) continue;
                    map[XX, YY] = mm - 1;
                    line.Add(new point(XX, YY));
                }
            }
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.black[i, j] == 0)
                    {
                        if (map[i, j] == 0)
                        {
                            X = i;
                            Y = j;
                            BLACK = true;
                            return true;
                        }
                    }
            return false;
        }
        private static bool knowledge8(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.map[i, j] != 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            int num = 0;
                            for (int k2 = 0; k2 < 4; k2++)
                            {
                                int XX = i + Near4[(k + k2) % 4, 0], YY = j + Near4[(k + k2) % 4, 1];
                                if (XX < 0 || XX >= x) num += 1 << k2;
                                else if (YY < 0 || YY >= y) num += 1 << k2;
                                else if (pzmap.black[XX, YY] == 1) num += 1 << k2;
                                else if (pzmap.black[XX, YY] == 2)
                                {
                                    num = 99999;
                                    break;
                                }
                            }
                            if (num == 3)
                            {
                                int XX = i + Near4[(k + 2) % 4, 0] + Near4[(k + 3) % 4, 0], YY = j + Near4[(k + 2) % 4, 1] + Near4[(k + 3) % 4, 1];
                                if (pzmap.black[XX, YY] != 0) continue;
                                bool flag = false;
                                for (int q = 0; q < 4; q++)
                                {
                                    int XXX = XX + Near4[q, 0], YYY = YY + Near4[q, 1];
                                    if (XXX < 0 || XXX >= pzmap.X) continue;
                                    if (YYY < 0 || YYY >= pzmap.Y) continue;
                                    if (pzmap.map[XXX, YYY] != 0) flag = true;
                                }
                                if (!flag) continue;
                                X = XX;
                                Y = YY;
                                BLACK = true;
                                return true;
                            }
                        }
                    }
                }
            return false;
        }
        private static bool knowledge9(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.black[i, j] == 0 && pzmap.map[i, j] == 0)
                    {
                        if (bfs4withUnknown(pzmap, i, j, true))
                        {
                            X = i;
                            Y = j;
                            BLACK = true;
                            return true;
                        }
                    }
                }
            return false;
        }
        private static bool knowledge10(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    if (pzmap.black[i, j] == 0 && pzmap.map[i, j] == 0)
                    {
                        if (bfs4withUnknown(pzmap, i, j, false))
                        {
                            X = i;
                            Y = j;
                            BLACK = false;
                            return true;
                        }
                    }
                }
            return false;
        }
        private static bool knowledge11(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y, cnt = 0;
            int[,] map = new int[x, y];
            int[] num = new int[x * y];
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.map[i, j] != 0)
                    {
                        var list = bfs4(pzmap, i, j, false);
                        cnt++;
                        foreach (var ll in list)
                            map[ll.x, ll.y] = cnt;
                        num[cnt] = list.Count - pzmap.map[i, j];
                    }
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.black[i, j] == 2 && map[i, j] == 0)
                    {
                        var list = bfs4(pzmap, i, j, false);
                        cnt++;
                        foreach (var ll in list)
                            map[ll.x, ll.y] = cnt;
                        num[cnt] = list.Count;
                    }
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    if (pzmap.black[i, j] == 0 && pzmap.map[i, j] == 0)
                    {
                        HashSet<int> set = new HashSet<int>();
                        for (int k = 0; k < 4; k++)
                        {
                            int XX = i + Near4[k, 0], YY = j + Near4[k, 1];
                            if (XX < 0 || XX >= x) continue;
                            if (YY < 0 || YY >= y) continue;
                            if (map[XX, YY] != 0) set.Add(map[XX, YY]);
                        }
                        if (set.Count > 1)
                        {
                            int flag = 0;
                            foreach (var k in set)
                                if (num[k] > 0) flag += 1;
                                else if (num[k] < 0) flag += 2;
                            if (flag == 3)
                            {
                                int tot = 1;
                                foreach (var k in set)
                                    tot += num[k];
                                if (tot > 0)
                                {
                                    X = i;
                                    Y = j;
                                    BLACK = true;
                                    return true;
                                }
                            }
                        }
                    }
            return false;
        }
        public static int engine(Puzzlemap pzmap)
        {
            int x = pzmap.X, y = pzmap.Y;
            if (knowledge1(pzmap)) return 1;
            if (knowledge2(pzmap)) return 2;
            if (knowledge3(pzmap)) return 3;
            if (knowledge4(pzmap)) return 4;
            if (knowledge5(pzmap)) return 5;
            if (knowledge6(pzmap)) return 6;
            if (knowledge7(pzmap)) return 7;
            if (knowledge8(pzmap)) return 8;
            if (knowledge9(pzmap)) return 9;
            if (knowledge10(pzmap)) return 10;
            if (knowledge11(pzmap)) return 11;
            return -1;
        }

        private static List<point> bfs4(Puzzlemap pzmap, int x, int y, bool is_black)
        {
            List<point> re = new List<point>();
            bool[,] done = new bool[pzmap.X, pzmap.Y];
            done[x, y] = true;
            re.Add(new point(x, y));
            int now = 0;
            for (; now < re.Count; now++)
            {
                int xx = re[now].x, yy = re[now].y;
                for (int k = 0; k < 4; k++)
                {
                    int XX = xx + Near4[k, 0], YY = yy + Near4[k, 1];
                    if (XX < 0 || XX >= pzmap.X) continue;
                    if (YY < 0 || YY >= pzmap.Y) continue;
                    if (done[XX, YY]) continue;
                    if (pzmap.map[XX, YY] == 0 && pzmap.black[XX, YY] != (is_black ? 1 : 2)) continue;
                    if (pzmap.map[XX, YY] != 0 && is_black) continue;
                    re.Add(new point(XX, YY));
                    done[XX, YY] = true;
                }
            }
            return re;
        }
        //return true if can decide (x, y) == is_black
        private static bool bfs4withUnknown(Puzzlemap pzmap, int x, int y, bool is_black)
        {
            List<point> re = new List<point>();
            bool[,] done = new bool[pzmap.X, pzmap.Y];

            if (is_black)
            {
                for (int i = 0; i < pzmap.X; i++)
                    for (int j = 0; j < pzmap.Y; j++)
                        if (pzmap.black[i, j] == 1)
                        {
                            re.Add(new point(i, j));
                            goto bfs4withUnknownEnd;
                        }
            bfs4withUnknownEnd: ;
                if (re.Count == 0) return false;
            }
            else
            {
                for (int i = 0; i < pzmap.X; i++)
                    for (int j = 0; j < pzmap.Y; j++)
                        if (pzmap.map[i, j] != 0)
                            re.Add(new point(i, j));
            }
            int now = 0;
            for (; now < re.Count; now++)
            {
                //MessageBox.Show(re.Count.ToString() + " " + re[now].x + " " + re[now].y);
                int xx = re[now].x, yy = re[now].y;
                for (int k = 0; k < 4; k++)
                {
                    int XX = xx + Near4[k, 0], YY = yy + Near4[k, 1];
                    if (XX < 0 || XX >= pzmap.X) continue;
                    if (YY < 0 || YY >= pzmap.Y) continue;
                    if (XX == x && YY == y) continue;
                    if (done[XX, YY]) continue;
                    if (pzmap.map[XX, YY] == 0 && pzmap.black[XX, YY] == (is_black ? 2 : 1)) continue;
                    if (pzmap.map[XX, YY] != 0 && is_black) continue;
                    re.Add(new point(XX, YY));
                    done[XX, YY] = true;
                }
            }
            for (int i = 0; i < pzmap.X; i++)
                for (int j = 0; j < pzmap.Y; j++)
                    if (pzmap.black[i, j] == (is_black ? 1 : 2))
                        if (!done[i, j]) return true;
            return false;
        }

        internal static string GetKnowledge(int k)
        {
            String re = "";
            int tot = 0;
            foreach (var i in knowledge[k])
            {
                re += i;
                if (++tot == 25)
                    re += "\n";
            }
            return re;
        }
    }
}
