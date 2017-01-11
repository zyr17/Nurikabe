using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Nurikabe
{
    public class point
    {
        public int x, y;
        public point() { }
        public point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }
    }
    public class Puzzlemap
    {
        public int[,] map;
        public int[,] black;//0:white 1:black 2:point
        bool [,] answer;//0:white 1:black
        public int X, Y;
        public class Moves
        {
            public int x, y, prev, to;
            public Moves() { }
            public Moves(int xx, int yy, int pprev, int tto)
            {
                x = xx;
                y = yy;
                prev = pprev;
                to = tto;
            }

        }
        public List<Moves> dolist;
        public int nowdopos;
        public void initialize(int x, int y)
        {
            X = x;
            Y = y;
            map = new int[x, y];
            black = new int[x, y];
            answer = null;
            if (dolist == null) dolist = new List<Moves>();
            else dolist.Clear();
            nowdopos = -1;
        }
        public void init_answer()
        {
            answer = new bool[X, Y];
        }
        public void set_map(int x, int y, int num)
        {
            map[x, y] = num;
            black[x, y] = 0;
        }
        public void set_answer(int x, int y, bool is_black)
        {
            if (answer == null) init_answer();
            answer[x, y] = is_black;
        }
        public void next_step()
        {
            if (nowdopos + 1 >= dolist.Count) return;
            do_step(++nowdopos, "to");
        }
        public void prev_step()
        {
            if (nowdopos <= -1) return;
            do_step(nowdopos--, "prev");
        }
        private void do_step(int step, String which)
        {
            if (step <= -1 || step >= dolist.Count) return;
            if (which != "to" && which != "prev") throw new Exception();
            black[dolist[step].x, dolist[step].y] = (which == "to") ? dolist[step].to : dolist[step].prev;
        }
        public void add_step(int x, int y, int change = -1)
        {
            if (x < 0 || x >= X) throw new Exception();
            if (y < 0 || y >= Y) throw new Exception();
            if (change == -1) change = (black[x, y] + 1) % 3;
            if (change < 0 || change > 2) throw new Exception();
            dolist.RemoveRange(nowdopos + 1, dolist.Count - nowdopos - 1);
            dolist.Add(new Moves(x, y, black[x, y], change));
            do_step(++nowdopos, "to");
        }
        public void show_answer()
        {
            if (answer == null)
            {
                System.Windows.MessageBox.Show("No solution!");
            }
            else
            {
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                        if (map[i, j] == 0)
                            if (answer[i, j]) black[i, j] = 1;
                            else black[i, j] = 2;
                        else black[i, j] = 0;
            }
        }
        public int check()
        {
            if (answer == null)
            {
                System.Windows.MessageBox.Show("No solution!");
                return -1;
            }
            else
            {
                int wrong = 0;
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                        if (black[i, j] != 0)
                            if (black[i, j] == 1 && answer[i, j] == false) wrong++;
                            else if (black[i, j] == 2 && answer[i, j] == true) wrong++;
                return wrong;
            }
        }
        public List<point> get_wrong()
        {
            List<point> re = new List<point>();
            if (answer == null)
            {
                System.Windows.MessageBox.Show("No solution!");
                return re;
            }
            else
            {
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                        if (black[i, j] != 0)
                            if ((black[i, j] == 1 && answer[i, j] == false) || (black[i, j] == 2 && answer[i, j] == true))
                                re.Add(new point(i, j));
                return re;
            }
        }

        internal bool has_answer()
        {
            return answer != null;
        }
    }
}
