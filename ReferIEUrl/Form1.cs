using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReferIEUrl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Win 32 API
        [DllImport("user32.dll")]
        static extern int EnumWindows(CallbackDef callback, int lParam);

        delegate bool CallbackDef(int hWnd, int lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(int hWnd, StringBuilder text, int count);


        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);


        [DllImport("USER32.DLL")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int nMaxCount);
        
        const int WM_KEYDOWN = 0x100;
        
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        //所有 IE 的Handler
        private List<int> IEIntPtrs = new List<int>();




        /// <summary>
        /// 取的該Handler 下的子Handler
        /// </summary>
        /// <param name="targetHandler"></param>
        /// <returns></returns>
        public List<IntPtr> GetAllChildHandles(IntPtr targetHandler)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);

                EnumChildWindows(targetHandler, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }


        /// <summary>
        /// 找尋所有開啟的IE
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool SearchIE(int hWnd, int lParam)
        {

            string mystring;

            StringBuilder text = new StringBuilder(255);
            GetWindowText(hWnd, text, 255);

            mystring = text.ToString();

            //確認抓到IE
            if (mystring.Contains("Internet Explorer"))
            {
                IEIntPtrs.Add(hWnd);
            }
            return true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            IEIntPtrs.Clear();
            CallbackDef callback = new CallbackDef(SearchIE);
            EnumWindows(callback, 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var ieHandler in IEIntPtrs)
            {
                //取的抓到IE下 所有的Handler
                var childHandlers = GetAllChildHandles((IntPtr)ieHandler);

                foreach (var childHandler in childHandlers)
                {

                    StringBuilder objType = new StringBuilder(255);
                    GetClassName(childHandler, objType, 255);
                    if (objType.ToString() == "Edit")
                    {
                      
                        SetForegroundWindow((IntPtr)childHandler);
                        SendKeys.Send("http://no2don.blogspot.com\r\n");
                    }

                }
            }
        }
    }
}
