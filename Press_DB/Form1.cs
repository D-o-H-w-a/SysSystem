using System;
using System.Threading;
using System.Windows.Forms;
using OPCAutomation;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        private OPCServer opcServer; // Ŭ���̾�Ʈ�� �������� ����� ���ؼ� �ʿ��� �͵��� ������ interface ����.
        private OPCGroup opcGround; // ������ ����� OPCGroup�� Ŭ���̾�Ʈ�� ����� interface ����.
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