using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using HtmlAgilityPack;
using System.Windows.Media.Animation;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Mystat_PO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public string Hw_path { get; set; }
        static IWebDriver Driver { get; set; }
        string pages { get; set; }
        HtmlAgilityPack.HtmlNode HW { get; set; }
        System.Collections.Generic.IEnumerable<HtmlAgilityPack.HtmlNode> HW_Nodes { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Driver = new ChromeDriver { Url = "https://mystat.itstep.org/ru/login" };
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(1);
            Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMinutes(1);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(1);
            //Start();
        }
        void Login(string log, string pass)
        {
            var input = Driver.FindElement(By.XPath("//*[@id=\"login_field\"]"));
            input.SendKeys(log);
            input = Driver.FindElement(By.XPath("//*[@id=\"pass_field\"]"));
            input.SendKeys(pass);
            var actions = new Actions(Driver);
            actions.MoveToElement(Driver.FindElement(By.XPath("//*[@id=\"login_city\"]")));
            actions.Click().Perform();
            Driver.FindElement(By.XPath("//*[@id=\"login_city\"]/option[11]")).Click();
            actions.MoveToElement(Driver.FindElement(By.XPath("//*[@id=\"login\"]/span")));
            actions.Click().Perform();
            if (Driver.Url != "https://mystat.itstep.org/ru/login")
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromMinutes(1));
                var Name = wait.Until((wd) =>
                  {
                      return wd.FindElement(By.XPath("//*[@id=\"page-content-wrapper\"]/div/div/span[2]/span[4]/span/a"));
                  });
                this.tb_Lord.Text = Name.Text;
                var Rate = wait.Until((wd) =>
                  {
                      return wd.FindElement(By.XPath("//*[@id=\"page-content-wrapper\"]/my-app/div/div/div/div/div[1]/div/div[1]/div[1]/div/div[2]/div[1]/p[1]"));
                  });
                this.tb_AverageRate.Text = Rate.Text;
            }
        }
        void Get_News()
        {
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromMinutes(1));
            var menu = wait.Until((wd) =>
            {
                return wd.FindElement(By.XPath("//*[@id=\"step_1\"]"));
            });
            menu.Click();
            var news = wait.Until((wd) =>
              {
                  return wd.FindElement(By.XPath("//*[@id=\"sidebar-wrapper\"]/ul/li[6]/a"));
              });
            news.Click();
            var Wait = new WebDriverWait(Driver, TimeSpan.FromMinutes(1));
            var News = Wait.Until((wd) =>
            {
                return news.FindElement(By.XPath("//*[@id=\"toScroll\"]/div/div/div[2]/div/md-list"));
            });
            File.WriteAllText("page.xml", Driver.PageSource, Encoding.Default);
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(System.IO.Path.GetFullPath("page.xml"));
            var Node = doc.DocumentNode.SelectNodes("//*[@id=\"toScroll\"]/div/div/div[2]/div");
            Node = Node[0].ChildNodes[1].ChildNodes;
            int count = 1;

            foreach (var li in Node)
            {
                if (li.Name == "md-list-item")
                {
                    pages += "Piece of news " + count.ToString() + "\n" + li.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;
                    pages += "\n" + li.ChildNodes[0].ChildNodes[1].ChildNodes[1].InnerText + "\n";
                    Driver.FindElement(By.XPath(li.XPath)).Click();
                    File.WriteAllText("page.xml", Driver.PageSource, Encoding.Default);
                    doc = web.Load(System.IO.Path.GetFullPath("page.xml"));
                    foreach (var event_text in doc.DocumentNode.SelectNodes("//*[@id=\"toScroll\"]/div/div/div[1]/my-news-detail/div/div/md-card-content/p"))
                    {
                        pages += event_text.InnerText + "\n";
                    }
                    count++;
                }
            }
            File.WriteAllText("News.txt", pages, Encoding.Default);
            this.tb_Text.Text = News.Text;
            this.tb_Text.Text = this.pages;
        }
        public void News_TextAnim()
        {
            this.tb_Text.Visibility = Visibility.Visible;
            DoubleAnimation heigh_text = new DoubleAnimation(0, 527, TimeSpan.FromSeconds(2));
            DoubleAnimation width_text = new DoubleAnimation(0, 499, TimeSpan.FromSeconds(2));
            this.tb_Text.BeginAnimation(TextBlock.WidthProperty, width_text);
            this.tb_Text.BeginAnimation(TextBlock.HeightProperty, heigh_text);
        }
        private void btn_News_Click(object sender, RoutedEventArgs e)
        {
            Get_News();
            News_TextAnim();
        }

        private void btn_SendDz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Driver.Url == "https://mystat.itstep.org/ru/homeworks")
                {
                    Driver.FindElement(By.XPath(this.HW.ChildNodes[5].ChildNodes[4].XPath)).Click();
                }
                else
                {
                    this.btn_Tasks_Click(null, null);
                    this.btn_SendDz_Click(null, null);
                }
            }
            catch (Exception)
            {
                try
                {
                    Driver.FindElement(By.XPath(this.HW.ChildNodes[5].ChildNodes[6].ChildNodes[2].XPath)).Click();
                }
                catch(Exception)
                {
                    Driver.FindElement(By.XPath(this.HW.ChildNodes[5].ChildNodes[2].XPath)).Click();
                }
            }
        }

        private void Homework_Date_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Month_Slide != null && Day_Slide != null)
            {
                if (this.tb_Date != null)
                {
                    this.tb_Date.Text = this.Day_Slide.Value.ToString() + "." + this.Month_Slide.Value.ToString() + "." + DateTime.Now.Year;
                    this.Check_HW();
                }
            }
        }

        private void btn_Tasks_Click(object sender, RoutedEventArgs e)
        {
            Driver.FindElement(By.XPath("//*[@id=\"step_1\"]")).Click();
            Driver.FindElement(By.XPath("//*[@id=\"sidebar-wrapper\"]/ul/li[3]/a")).Click();
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromMinutes(1));
            var hw = wait.Until((wd) =>
              {
                  return wd.FindElement(By.XPath("//*[@id=\"page-content-wrapper\"]/my-app/div[2]/div[1]/ul/li[1]/a"));
              });
            hw.Click();
            wait = new WebDriverWait(Driver, TimeSpan.FromMinutes(1));
            var tmp = wait.Until((wd) =>
              {
                  return wd.FindElement(By.XPath("//*[@id=\"page-content-wrapper\"]/my-app/div[2]/div[2]/div/div/div[1]/div[3]/div/p"));
              });
            HtmlWeb web = new HtmlWeb();
            File.WriteAllText("test.xml", Driver.PageSource, Encoding.Default);
            HtmlAgilityPack.HtmlDocument doc = web.Load(System.IO.Path.GetFullPath("test.xml"));
            var Node = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/my-app[1]/div[1]/div[3]/my-app[1]/div[2]/div[2]/div[1]/div[1]").ChildNodes.Where(n => n.Name == "div");
            this.HW_Nodes = Node;
            this.Check_HW();
        }
        public void Check_HW()
        {
            bool check = false;
            string search_day = "";
            string search_month = "";
            if (this.Day_Slide.Value < 10)
            {
                search_day += "0" + this.Day_Slide.Value.ToString();
            }
            else
            {
                search_day = this.Day_Slide.Value.ToString();
            }
            if (this.Month_Slide.Value < 10)
            {
                search_month += "0" + this.Month_Slide.Value.ToString();
            }
            else
            {
                search_month = this.Month_Slide.Value.ToString();
            }
            if (this.HW_Nodes != null)
            {
                string pattern = "[0-99]";
                foreach (var node in this.HW_Nodes)
                {
                    string when = node.ChildNodes[1].ChildNodes[5].InnerText;
                    string day = "31";
                    string month = "12";
                    string year = "2018";
                    int index = when.IndexOf('.') - 2;
                    day = when.Substring(when.IndexOf('.') - 2, 2);
                    month = when.Substring(when.IndexOf('.') + 1, 2);
                    year = when.Substring(when.LastIndexOf('.')+1, 4);
                    if (search_day == day && search_month == month&&DateTime.Now.Year.ToString()==year)
                    {
                        check = true;
                        this.tb_Teacher.Text = node.ChildNodes[1].ChildNodes[3].InnerText.Replace("  ", "");
                        this.tb_Date.Text = node.ChildNodes[1].ChildNodes[5].InnerText.Replace("  ", "");
                        this.tb_Theme.Text = node.ChildNodes[3].ChildNodes[1].InnerText/*.Replace(" ", "")*/;
                        this.tb_Theme.Text+= "\n"+node.ChildNodes[3].ChildNodes[4].InnerText/*.Replace(" ", "")*/;
                        var match = Regex.Match(node.ChildNodes[3].ChildNodes[10].InnerText.ToCharArray().Reverse().ToString(), pattern);
                        if (match != null)
                        {
                            this.tb_Rate.Text = match.Value;
                        }
                        try
                        {
                            this.tb_Until.Text = node.ChildNodes[5].ChildNodes[6].ChildNodes[6].InnerText;
                        }
                        catch(Exception)
                        {
                            try
                            {
                                this.tb_Until.Text = node.ChildNodes[5].ChildNodes[2].ChildNodes[3].InnerText;
                            }
                            catch (Exception)
                            {
                                this.tb_Until.Text = "Overtimed";
                            }
                        }
                        this.HW = node;
                        return;
                    }
                    if(!check)
                    {
                        this.Clear_AllTb();
                    }
                }
            }
        }
        public void Clear_AllTb()
        {
            this.tb_Rate.Text = "";
            this.tb_Teacher.Text = "";
            this.tb_Theme.Text = "";
            this.tb_Until.Text = "";
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            this.Login(this.tb_log.Text, this.tb_pass.Password);
            if (Driver.Url != "https://mystat.itstep.org/ru/login")
            {
                this.btn_Start.IsEnabled = this.tb_log.IsEnabled = this.tb_pass.IsEnabled = false;
                this.btn_Start.Visibility = this.tb_log.Visibility = this.tb_pass.Visibility = Visibility.Hidden;
                this.Enable_Controls();
            }
        }
        private void Enable_Controls()
        {
            this.Day_Slide.IsEnabled = this.Month_Slide.IsEnabled = this.btn_Tasks.IsEnabled = this.btn_SendDz.IsEnabled = this.btn_News.IsEnabled = true;
        }
    }
}
