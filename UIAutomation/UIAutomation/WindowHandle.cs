using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UIAutomationTest
{
    internal class WindowHandle
    {
        private IntPtr _hwnd = IntPtr.Zero;
        private string _className = null;
        private string _winName = null;

        //桌面句柄，根句柄
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();

        //通过窗口类名获取句柄
        [DllImport("user32.dll", SetLastError = true)]  //SetLastError：保留win32上一次错误信息
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //通过句柄获取该窗口下的子窗口
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        //通过句柄获取窗口标题
        [DllImport("user32", SetLastError = true)]
        private static extern int GetWindowText(
            IntPtr hWnd,//窗口句柄
            StringBuilder lpString,//标题
            int nMaxCount //最大值
            );

        //通过句柄获取窗口类名
        [DllImport("user32.dll")]
        private static extern int GetClassName(
            IntPtr hWnd,//句柄
            StringBuilder lpString, //类名
            int nMaxCount //最大值
            );

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //控制窗体，向目标窗体发送系统消息
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lparam);

        public IntPtr Hwnd
        { get { return _hwnd; } }

        public string ClassName
        {
            get 
            {
                //初次获取名，Get一下，之后就无须获取了
                if (_className == null)
                {
                    StringBuilder sb = new StringBuilder();
                    GetClassName(_hwnd, sb, 256);
                    _className = sb.ToString();
                }
                return _className; 
            }
            //set { _className = value; }
        }

        public string WinName
        {
            get
            {
                //初次获取名，Get一下，之后就无须获取了
                if (_winName == null)
                {
                    StringBuilder sb = new StringBuilder();
                    GetWindowText(_hwnd, sb, 256);
                    _winName = sb.ToString();
                }
                return _winName;
            }
        }

        //构造函数：初始附带窗口句柄
        public WindowHandle(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }

        //将IntPtr集合封装成WindowHandle集合
        private static WindowHandle[] ToWindowHandles(IntPtr[] intPtrs)
        {
            WindowHandle[] whs = new WindowHandle[intPtrs.Length];
            for(int i = 0; i < intPtrs.Length; i++) 
            { whs[i] = new WindowHandle(intPtrs[i]); }
            return whs;
        }

        //获取桌面句柄，根句柄
        public static WindowHandle Root
        {
            get
            {
                IntPtr intPtr = GetDesktopWindow();
                return new WindowHandle(intPtr);
            }
        }

        //获取指定类名或窗口名的所有子窗句柄
        public WindowHandle[] FindChildren(string lpClassName, string lpWindowName)
        {
            List<IntPtr> list = new List<IntPtr>();
            IntPtr intPtr = IntPtr.Zero;    //记录上一个找到的子窗句柄
            do
            {
                intPtr = FindWindowEx(_hwnd, intPtr, lpClassName, lpWindowName);
                if (intPtr != IntPtr.Zero) list.Add(intPtr);
            } while (intPtr != IntPtr.Zero);

            if (list.Count <= 0) return null;
            return ToWindowHandles( list.ToArray() );
        }

        //获取所有同类名子窗句柄，忽视窗口名的不同
        public WindowHandle[] FindChildrenByClassName(string lpClassName)
        {
            return FindChildren(lpClassName, null);
        }

        //获取所有同窗口名子窗句柄，忽视类名的不同
        public WindowHandle[] FindChildrenByWinName(string lpWindowName)
        {
            return FindChildren(null, lpWindowName);
        }

        //关闭窗口
        public void Close()
        {
            SendMessage(_hwnd, 0x0010, 0, 0);
        }

        //窗口是否可视
        public bool IsVisible()
        {
            int result = GetWindowLong(_hwnd, -16);
            bool isVisible = ((result & 0x10000000L) != 0);
            return isVisible;
        }
    }
}
