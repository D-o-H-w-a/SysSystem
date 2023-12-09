using System;
using System.Threading;
using System.Windows.Forms;
using OPCAutomation;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        private OPCServer opcServer; // OPC ���� ��ü
        private OPCGroup opcGroup; // OPC �׷� ��ü
        private Thread opcThread; // OPC ����� ó���ϴ� ������

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // OPC Server ����
            opcServer = new OPCServer();
            // OPC ���� �ּҰ��� �޾� ����
            opcServer.Connect("OPCServerIP");

            //OPC �׷� ����
            opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
            //OPC �׷� Ȱ��ȭ - ������ �б�/���� ����
            opcGroup.IsActive = true;
            // Subscription Ȱ��ȭ - ������ ���� �� Ŭ���̾�Ʈ�� �ڵ� ������Ʈ
            opcGroup.IsSubscribed = true;
            // ������Ʈ �ֱ�
            opcGroup.UpdateRate = 1000;

            // OPC ������ �߰� - OPC ������ �̸����� �߰�.
            OPCItem item = opcGroup.OPCItems.AddItem("ItemName", 1);
        }
    }
}