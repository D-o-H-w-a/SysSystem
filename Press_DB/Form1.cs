using System;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using OPCAutomation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        private Thread opcThread; // OPC ����� ����� ������
        private OPCServer opcServer; // OPC �������� ������ �����ϴ� ��ü
        private OPCGroup opcGroup; // OPC �����κ��� �����͸� ������ �׷�
        private bool isRunning; // ������ ���� ���θ� �����ϴ� �÷���

        // MSSQL ���� ���ڿ�
        //private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            IntializeOPC();
        }

        private void IntializeOPC()
        {
            opcServer = new OPCServer();
            opcServer.Connect("OPCServerIP");

            opcGroup = opcServer.OPCGroups.Add("OPCGroupName");

        }
        /*
        private void Form1_Load(object sender, EventArgs e)
        {
            opcThread = new Thread(ConnectAndReadFromOPCServer); // OPC ����� ����ϴ� ������ ����
            opcThread.Start(); // ������ ������ ����
        }

        private void ConnectAndReadFromOPCServer()
        {
            try
            {
                // OPC Server ����
                opcServer = new OPCServer();
                // OPC ���� �ּҰ��� �޾� ����
                opcServer.Connect("OPCServerIP");

                //OPC �׷�(OPC �������� �����ϴ� ������ �׸���� ����) ����
                opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                //OPC �׷� Ȱ��ȭ - ������ �б�/���� ����
                opcGroup.IsActive = true;
                // Subscription Ȱ��ȭ - ������ ���� �� Ŭ���̾�Ʈ�� �ڵ� ������Ʈ
                opcGroup.IsSubscribed = true;
                // ������Ʈ �ӵ� (1�ʸ���)
                opcGroup.UpdateRate = 1000;

                // OPC �׷쿡 �±� �߰�
                OPCItems opcItems = opcGroup.OPCItems;
                // OPC �±׸� �Է�
                OPCItem opcItem = opcItems.AddItem("YourTagHere", 1);

                while (isRunning)
                {
                    // OPC �����ۿ��� �� �б�

                    // ���� �������� ���� �����ϴ� �迭 ����
                    object itemValue;

                    // ������ �� �б�
                    opcItem.Read((short)OPCDataSource.OPCDevice, out itemValue, out _, out _);

                    // cellType �� ���� ���� ����
                    string cellType;
                    // State �÷��� ���� ���� ����
                    string state;
                    //SQL ���� ������ ���� Command ��ü ����
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        // �����ͺ��̽� ���� ����
                        connection.Open();
                        // ������ �����ͺ��̽� ���̺�� �÷� ���� 
                        string sqlQuery = "SELECT Cell_type,State, cranestate FROM t_Cell";


                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        
                        
                        if (Convert.ToInt32(itemValue) > sql)
                        {

                        }
                        // �����ͺ��̽� ���� ����
                        connection.Close();
                    }
                }

                // ListView�� �׸� �߰�
                Invoke(new Action(() =>
                {
                }));

                // 0.2�� ��� �� ���û
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("OPC �������� ��� �� ������ �߻��߽��ϴ�: " + ex.Message);
                }));
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false; // ������ ���Ḧ ���� �÷��� ����

            // �����尡 ������ ����� ������ ���
            if (opcThread != null && opcThread.IsAlive)
            {
                // opcThread �����尡 ������ ����� ������ ����մϴ�.
                opcThread.Join();
            }
        }
        */
    }
}