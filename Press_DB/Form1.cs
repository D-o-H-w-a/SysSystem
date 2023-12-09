using System;
using System.Threading;
using System.Windows.Forms;
using OPCAutomation;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        private OPCServer opcServer; // 클라이언트와 서버간의 통신을 위해서 필요한 것들을 가져올 interface 변수.
        private OPCGroup opcGround; // 서버에 저장된 OPCGroup을 클라이언트에 담아줄 interface 변수.
        private Thread opcThread; // 
        private bool isRunning;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}