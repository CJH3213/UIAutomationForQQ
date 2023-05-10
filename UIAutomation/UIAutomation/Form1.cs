using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIAutomation;
using System.Windows.Automation;
using System.Runtime.InteropServices;

namespace UIAutomationTest
{
    public partial class Form1 : Form
    {
        List<WindowHandle> whTXguisWithName = new List<WindowHandle>();

        public Form1()
        {
            InitializeComponent();

            UpdateTXguis(); //软件打开后先更新一遍窗口列表
        }

        //更新窗口列表
        private void UpdateTXguis()
        {
            try
            {
                //获取根窗口句柄
                WindowHandle whRoot = WindowHandle.Root;

                //获取所有TXGui同类窗口
                WindowHandle[] whTXGuis = whRoot.FindChildrenByClassName("TXGuiFoundation");

                //获取含有窗口名称的句柄
                List<WindowHandle> whRealTXguis = new List<WindowHandle>();
                List<string> nameTXguis = new List<string>();
                foreach (WindowHandle wh in whTXGuis)
                {
                    if (wh.IsVisible() &&       //排除不可见窗口
                        wh.WinName.Length > 0 &&    //排除无名窗口
                        !wh.WinName.Contains("QQ")) //排除QQ主窗口
                    {
                        whRealTXguis.Add(wh);
                        nameTXguis.Add(wh.WinName);
                    }
                }

                //装载至ListBox，如果两次获取到的窗口集合不同，更新列表
                if(whTXguisWithName.Count != whRealTXguis.Count)
                {
                    whTXguisWithName.Clear();
                    whTXguisWithName.AddRange(whRealTXguis);

                    listBox1.Enabled = false;   //将列表失能再更新，防止中途点击项目
                    listBox1.Items.Clear();
                    listBox1.Items.AddRange(nameTXguis.ToArray());
                    listBox1.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                //把报错消息写入TextBox
                textBox1.Text = ex.Message;
            }
        }

        //列表选择项发生变动时更新文本框为对应消息
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //获取该对话框的UIAutomation对象
                IntPtr hwnd = whTXguisWithName[listBox1.SelectedIndex].Hwnd;
                AutomationElement aeChatBox = AutomationElement.FromHandle(hwnd);

                AutomationElement aeMessage = aeChatBox.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "消息"));   //寻找消息支点

                //获取消息下的子对象
                AutomationElementCollection aecMes = aeMessage.FindAll(TreeScope.Children,
                    Condition.TrueCondition);

                //获取消息文字内容
                StringBuilder sb = new StringBuilder();
                foreach (AutomationElement aeMes in aecMes)
                {
                   sb.AppendLine( aeMes.Current.Name);
                }

                //把消息内容写入TextBox
                textBox1.Text = sb.ToString();

            }catch (Exception ex) 
            {
                //把报错消息写入TextBox
                textBox1.Text = ex.Message;
            }

        }

        //每一秒检查一次QQ聊天窗口变动
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateTXguis();
        }
    }
}
