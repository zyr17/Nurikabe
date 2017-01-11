using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Nurikabe
{
    /// <summary>
    /// ShowPuzzle.xaml 的交互逻辑
    /// </summary>
    public partial class ShowPuzzle : Window
    {
        TextBlock CheckedWhite
        {
            get
            {
                TextBlock re = new TextBlock(new Run("          •          "));
                re.Margin = new Thickness(1, 0, 1, 1);
                re.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                re.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                re.FontSize = PuzzleFontSize;
                return re;
            }
        }
        TextBlock CheckedBlack
        {
            get
            {
                TextBlock re = new TextBlock();
                re.Background = Brushes.Black;
                return re;
            }
        }
        TextBlock Unchecked {
            get
            {
                TextBlock re = new TextBlock();
                re.Background = Brushes.White;
                re.Margin = new Thickness(1);
                return re;
            }
        }
        TextBlock Number
        {
            get
            {
                TextBlock re = new TextBlock(new Run("0"));
                re.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                re.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                re.FontSize = PuzzleFontSize;
                re.Margin = new Thickness(1);
                return re;
            }
        }
        Border PuzzleBlockBorder
        {
            get
            {
                Border re = new Border();
                re.BorderBrush = Brushes.Black;
                re.BorderThickness = new Thickness(1);
                return re;
            }
        }
        Puzzlemap pzmap;
        public int PuzzleBlockSize, PuzzleFontSize;
        public const int WidthSizeAdd = 20, HeightSizeAdd = 243;
        TextBlock[,] PuzzleBlock;
        public ShowPuzzle()
        {
            InitializeComponent();
            pzmap = new Puzzlemap();
            UpdatePrevNext();
        }
        public void SetPuzzlemap(Puzzlemap inpzmp, int blocksize = 50, int fontsize = 40)
        {
            pzmap = inpzmp;
            PuzzleFontSize = fontsize;
            PuzzleMapGrid.ColumnDefinitions.Clear();
            PuzzleMapGrid.RowDefinitions.Clear();
            PuzzleBlockSize = blocksize;
            for (int i = pzmap.X; i-- > 0; )
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(PuzzleBlockSize);
                PuzzleMapGrid.RowDefinitions.Add(rd);
            }
            for (int i = pzmap.Y; i-- > 0; )
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(PuzzleBlockSize);
                PuzzleMapGrid.ColumnDefinitions.Add(cd);
            }
            PuzzleBlock = new TextBlock[pzmap.X, pzmap.Y];
            for (int i = 0; i < pzmap.X; i++)
                for (int j = 0; j < pzmap.Y; j++)
                {
                    PuzzleBlock[i, j] = Unchecked;
                    if (pzmap.map[i, j] != 0)
                    {
                        PuzzleBlock[i, j] = Number;
                        PuzzleBlock[i, j].Text = pzmap.map[i, j].ToString();
                    }
                    Border bb = PuzzleBlockBorder;
                    PuzzleMapGrid.Children.Add(bb);
                    Grid.SetRow(bb, i);
                    Grid.SetColumn(bb, j);
                    PuzzleMapGrid.Children.Add(PuzzleBlock[i, j]);
                    Grid.SetRow(PuzzleBlock[i, j], i);
                    Grid.SetColumn(PuzzleBlock[i, j], j);
                }
            this.Height = PuzzleBlockSize * pzmap.X + HeightSizeAdd;
            this.Width = PuzzleBlockSize * pzmap.Y + WidthSizeAdd;
            if (this.Width < 600) this.Width = 600;
            UpdatePrevNext();
        }

        private void PuzzleBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int ypos = (int)(e.GetPosition(PuzzleMapGrid).X / PuzzleBlockSize), xpos = (int)(e.GetPosition(PuzzleMapGrid).Y / PuzzleBlockSize);
            if (xpos >= pzmap.X || ypos > pzmap.Y) return;
            if (pzmap.map[xpos, ypos] != 0) return;
            pzmap.add_step(xpos, ypos);
            UpdatePuzzleBlock(xpos, ypos);
        }
        private void UpdatePuzzleBlock(int xpos, int ypos)
        {
            if (pzmap.map[xpos, ypos] != 0)
                return;
            PuzzleMapGrid.Children.Remove(PuzzleBlock[xpos, ypos]);
            if (pzmap.black[xpos, ypos] == 0)
            {
                PuzzleBlock[xpos, ypos] = Unchecked;
            }
            else if (pzmap.black[xpos, ypos] == 1)
            {
                PuzzleBlock[xpos, ypos] = CheckedBlack;
            }
            else if (pzmap.black[xpos, ypos] == 2)
            {
                PuzzleBlock[xpos, ypos] = CheckedWhite;
            }
            PuzzleMapGrid.Children.Add(PuzzleBlock[xpos, ypos]);
            Grid.SetRow(PuzzleBlock[xpos, ypos], xpos);
            Grid.SetColumn(PuzzleBlock[xpos, ypos], ypos);
            UpdatePrevNext();
        }

        private void UpdatePrevNext()
        {
            PrevButton.IsEnabled = NextButton.IsEnabled = false;
            if (pzmap == null) return;
            if (pzmap.dolist == null) return;
            if (pzmap.nowdopos != -1) PrevButton.IsEnabled = true;
            if (pzmap.nowdopos + 1 != pzmap.dolist.Count) NextButton.IsEnabled = true;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            pzmap.prev_step();
            UpdatePuzzleBlock(pzmap.dolist[pzmap.nowdopos + 1].x, pzmap.dolist[pzmap.nowdopos + 1].y);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            pzmap.next_step();
            UpdatePuzzleBlock(pzmap.dolist[pzmap.nowdopos].x, pzmap.dolist[pzmap.nowdopos].y);
        }

        private void SolutionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!pzmap.has_answer())
            {
                MessageBox.Show("没有答案！");
                return;
            }
            MessageBoxButton btn = MessageBoxButton.OKCancel;
            if (System.Windows.MessageBox.Show("你确定要直接给出答案吗？", "警告", btn) == MessageBoxResult.OK)
            {
                pzmap.show_answer();
                for (int i = 0; i < pzmap.X; i++)
                    for (int j = 0; j < pzmap.Y; j++)
                        UpdatePuzzleBlock(i, j);
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (!pzmap.has_answer())
            {
                MessageBox.Show("没有答案！");
                return;
            }
            int wrong = pzmap.check();
            if (wrong == 0)
            {
                MessageBox.Show("到目前为止没有任何错误！");
            }
            else
            {
                if (MessageBox.Show("你现在有 " + wrong.ToString() + " 个错误，是否标出？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Not avaliable now!");
                }
            }
        }

        private void Again_Button_Click(object sender, RoutedEventArgs e)
        {
            pzmap.black = new int[pzmap.X, pzmap.Y];
            pzmap.dolist.Clear();
            pzmap.nowdopos = -1;
            UpdatePrevNext();
            for (int i = 0; i < pzmap.X; i++)
                for (int j = 0; j < pzmap.Y; j++)
                    UpdatePuzzleBlock(i, j);
        }

        private int Do_One_Step()
        {
            int knlednum = exsys.engine(pzmap);
            if (knlednum == -1)
            {
                MessageBox.Show("无法推理出东西了！");
                return -1;
            }
            return knlednum;
        }

        private void set_exans(int k)
        {
            KnowledgeNumTextBlock.Text = "运用第 " + k + " 条知识";
            ExpertSystemAnswerTextBlock.Text = "坐标：(" + exsys.X + ", " + exsys.Y + "), 颜色：" + (exsys.BLACK ? "黑" : "白");
            KnowledgeContentTextBlock.Text = exsys.GetKnowledge(k);
        }

        private void One_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            ExpertSystemAnswerTextBlock.Text = KnowledgeContentTextBlock.Text = KnowledgeNumTextBlock.Text = "";
            int re = Do_One_Step();
            if (re == -1) return;
            pzmap.add_step(exsys.X, exsys.Y, exsys.BLACK ? 1 : 2);
            UpdatePrevNext();
            UpdatePuzzleBlock(exsys.X, exsys.Y);
            set_exans(re);
        }

        private void Full_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                int re = Do_One_Step();
                if (re == -1) return;
                pzmap.add_step(exsys.X, exsys.Y, exsys.BLACK ? 1 : 2);
                UpdatePrevNext();
                UpdatePuzzleBlock(exsys.X, exsys.Y);
            }
        }
    }
}
