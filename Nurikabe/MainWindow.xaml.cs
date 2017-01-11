using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace Nurikabe
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int[] PuzzleNum = new int[8] {0, 0, 216, 1548, 1589, 1163, 1388, 1513} ;
        DispatcherTimer testallupdatetimer = new DispatcherTimer(DispatcherPriority.Send);
        public MainWindow()
        {
            InitializeComponent();
            testallupdatetimer.Interval = new TimeSpan(0, 0, 1);
            testallupdatetimer.Tick += new EventHandler(testallupdate);
        }

        private void gaoshuju()
        {
            int[] bu = new int[111];
            var files = Directory.GetFiles(@"D:\Documents\Apps\Nurikabe\data");
            foreach (var file in files)
            {
                //System.Windows.MessageBox.Show(file);
                XmlDocument xml = new XmlDocument();
                String sss = System.IO.File.ReadAllText(file);
                //System.Windows.MessageBox.Show(sss);
                xml.Load(file);
                var node = xml.SelectSingleNode("puzzle").SelectSingleNode("header").SelectSingleNode("properties");
                var nodes = node.ChildNodes;
                int diff = 0, id = 0;
                foreach (var i in nodes)
                {
                    var j = i as XmlNode;
                    if (j.Attributes["name"].Value == "difficulty")
                    {
                        diff = Convert.ToInt32(j.Attributes["value"].Value);
                        break;
                    }
                }
                id = ++bu[diff];
                String ss = @"D:\Documents\Apps\Nurikabe\data" + @"\" + diff.ToString();
                //System.Windows.MessageBox.Show(ss);
                if (!Directory.Exists(ss))
                {
                    Directory.CreateDirectory(ss);
                }
                xml.Save(ss + @"\" + id.ToString() + ".xml");
            }
        }

        private void Puzzle_Generate_Button_Click(object sender, RoutedEventArgs e)
        {
            Puzzlemap puzzlemap = new Puzzlemap();
            int x, y;
            x = Convert.ToInt32(HeightTextBox.Text);
            y = Convert.ToInt32(WidthTextBox.Text);
            puzzlemap.initialize(x, y);
            String[] s = ContentTextBox.Text.Replace("\r\n", " ").Split(' ');
            int now = 0;
            for (int i = 0; i < x; i ++ )
                for (int j = 0; j < y; j++)
                    puzzlemap.set_map(i, j, Convert.ToInt32(s[now++]));
            System.Windows.MessageBox.Show(x.ToString() + " " + y.ToString());
            ShowPuzzle spwindow = new ShowPuzzle();
            spwindow.SetPuzzlemap(puzzlemap);
            spwindow.ShowDialog();
        }

        private void Switch_Button_Click(object sender, RoutedEventArgs e)
        {
            if (InputGrid.Visibility == System.Windows.Visibility.Visible)
                InputGrid.Visibility = System.Windows.Visibility.Hidden;
            else InputGrid.Visibility = System.Windows.Visibility.Visible;
            if (XMLGrid.Visibility == System.Windows.Visibility.Visible)
                XMLGrid.Visibility = System.Windows.Visibility.Hidden;
            else XMLGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void SetXML_Button_Click(object sender, RoutedEventArgs e)
        {
            int diff = 0, num = 0;
            try
            {
                diff = Convert.ToInt32(DiffTextBox.Text);
                num = Convert.ToInt32(PuzzleNumTextBox.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("数字读取错误！");
            }
            if (diff < 2 || diff > 7)
            {
                System.Windows.MessageBox.Show("难度应为 2 到 7 之间");
                return;
            }
            if (num < 1 || num > PuzzleNum[diff])
            {
                System.Windows.MessageBox.Show("谜题号应为 1 到 " + PuzzleNum[diff] + " 之间");
                return;
            }
            String file = @"data\" + diff.ToString() + @"\" + num.ToString() + @".xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(file);
            XmlNode node = xml.SelectSingleNode("puzzle").SelectSingleNode("data");
            XmlNode nd2 = node.SelectSingleNode("dimensions");
            int x = Convert.ToInt32(nd2.Attributes["height"].Value);
            int y = Convert.ToInt32(nd2.Attributes["width"].Value);
            nd2 = node.SelectSingleNode("source");
            String[] s = nd2.InnerText.Split(' ');
            int nowpos = 0;
            Puzzlemap pzmp = new Puzzlemap();
            pzmp.initialize(x, y);
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    int t = Convert.ToInt32(s[nowpos++]);
                    pzmp.set_map(i, j, t);
                }
            nd2 = node.SelectSingleNode("solution");
            s = nd2.InnerText.Split(' ');
            nowpos = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    int t = Convert.ToInt32(s[nowpos++]);
                    pzmp.set_answer(i, j, t == 1);
                }
            ShowPuzzle sp = new ShowPuzzle();
            if (pzmp.X > 15) sp.SetPuzzlemap(pzmp, 30, 25);
            else sp.SetPuzzlemap(pzmp);
            sp.ShowDialog();
        }

        int nowdoingdiff, nowdoingnum;
        string nowresstring;

        void testallupdate(object sender, EventArgs e)
        {
            DiffTextBox.Text = nowdoingdiff.ToString();
            PuzzleNumTextBox.Text = nowdoingnum.ToString();
        }

        void testall_main()
        {
            string resultstring = "";
            int totalblock = 0, solvedblock = 0, totalmap = 0, solvedmap = 0;
            int[] kused = new int[20];
            for (int diff = 2; diff <= 7; diff++)
            {
                nowdoingdiff = diff;
                int nowtotalblock = 0, nowsolvedblock = 0, nowtotalmap = 0, nowsolvedmap = 0;
                int[] nowkused = new int[20];
                for (int num = 1; num <= PuzzleNum[diff]; num++)
                {
                    nowdoingnum = num;
                    nowtotalmap++;
                    int need = 0, solve = 0;
                    String file = @"data\" + diff.ToString() + @"\" + num.ToString() + @".xml";
                    XmlDocument xml = new XmlDocument();
                    xml.Load(file);
                    XmlNode node = xml.SelectSingleNode("puzzle").SelectSingleNode("data");
                    XmlNode nd2 = node.SelectSingleNode("dimensions");
                    int x = Convert.ToInt32(nd2.Attributes["height"].Value);
                    int y = Convert.ToInt32(nd2.Attributes["width"].Value);
                    nd2 = node.SelectSingleNode("source");
                    String[] s = nd2.InnerText.Split(' ');
                    int nowpos = 0;
                    Puzzlemap pzmp = new Puzzlemap();
                    pzmp.initialize(x, y);
                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                        {
                            int t = Convert.ToInt32(s[nowpos++]);
                            if (t == 0) need++;
                            pzmp.set_map(i, j, t);
                        }
                    nd2 = node.SelectSingleNode("solution");
                    s = nd2.InnerText.Split(' ');
                    nowpos = 0;
                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                        {
                            int t = Convert.ToInt32(s[nowpos++]);
                            pzmp.set_answer(i, j, t == 1);
                        }
                    for (; ; )
                    {
                        int res = exsys.engine(pzmp);
                        if (res == -1) break;
                        pzmp.add_step(exsys.X, exsys.Y, exsys.BLACK ? 1 : 2);
                        solve++;
                        nowkused[res]++;
                    }
                    if (need == solve) nowsolvedmap++;
                    nowtotalblock += need;
                    nowsolvedblock += solve;
                }
                resultstring += ("难度 " + diff + " 题目测试结果：\n总谜题数：" + nowtotalmap + " 完全解出谜题数：" + nowsolvedmap + " 总未知方块数：" + nowtotalblock + " 解出方块数：" + nowsolvedblock + "\n各条规则使用次数\n");
                for (int i = 1; i <= 11; i++)
                {
                    resultstring += nowkused[i].ToString() + " ";
                    kused[i] += nowkused[i];
                }
                resultstring += "\n";
                totalblock += nowtotalblock;
                totalmap += nowtotalmap;
                solvedblock += nowsolvedblock;
                solvedmap += nowsolvedmap;
            }
            resultstring += "\n";
            resultstring += ("所有题目测试结果：\n总谜题数：" + totalmap + " 完全解出谜题数：" + solvedmap + " 总未知方块数：" + totalblock + " 解出方块数：" + solvedblock + "\n各条规则使用次数\n");
            for (int i = 1; i <= 11; i++)
            {
                resultstring += kused[i].ToString() + " ";
                kused[i] += kused[i];
            }
            MessageBox.Show(resultstring);
            nowresstring = resultstring;
        }

        bool testallrunning = false;

        delegate void testallasync();

        void testallcallback(IAsyncResult ar)
        {
            testallasync func = (testallasync)ar.AsyncState;
            testallrunning = false;
            testallupdatetimer.Stop();
            func.EndInvoke(ar);
        }

        private void TestAll_Button_Click(object sender, RoutedEventArgs e)
        {
            if (testallrunning) return;
            testallrunning = true;
            testallasync func = new testallasync(testall_main);
            testallupdatetimer.Start();
            func.BeginInvoke(testallcallback, func);
        }
    }
}
