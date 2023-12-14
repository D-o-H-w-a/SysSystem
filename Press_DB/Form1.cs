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
        private OPCServer opcServer; // OPC ���� ������ ����ϴ� ��ü
        private Thread opcThread; // OPC ����� ���� ������
        private bool stopThread = false; // ������ ���߱� ���� �÷���

        // MSSQL ���� ���ڿ�
        //private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            StartOPCThread();
        }

        private void ConnectToOPCServer()
        {
            try
            {
                // OPCServer ��ü�� �����Ͽ� OPC ������ ������ �غ�
                opcServer = new OPCServer();
                // OPC ������ ����
                opcServer.Connect("OPCServerIP");

                // OPC Server �� ���� ���� �� item �� ��������

                //OPCGroup ����
                OPCGroup opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                // OPCGroup�� ���� OPCItems ��ü ����
                OPCItems opcItems = opcGroup.OPCItems;
                // ���ϴ� ������ �߰�
                OPCItem opcItem = opcItems.AddItem("YourItem", 1);

                // OPCItem�� ���� ������ ����
                object value;
                // OPCItem���� �� �б�
                opcItem.Read(1, out value, out _, out _);

                // ������ ���̽� ���� �� ���� ����
                // SqlConnection ��ü ����
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
                // �����ͺ��̽� ���� ����
                connection.Open();

                // t_Cell ���̺��� ������ ��������
                SqlCommand command1 = new SqlCommand("SELECT * FROM t_Cell", connection); // t_Cell ���̺��� ��� �� ����
                SqlDataReader reader1 = command1.ExecuteReader(); // ���� ���� �� ��� �б�

                // ��� �� �ݺ�
                while (reader1.Read())
                {
                    // Cell_type �� State �� ��������
                    int cellType = (int)reader1["Cell_type"]; // Cell_type �� �� ��������
                    string state = (string)reader1["State"]; // State �� �� ��������

                    // t_SC_sate ���̺��� Stk_no �÷� ������ Stk_sate �� ��������
                    SqlCommand command2 = new SqlCommand("SELECT Stk_state FROM t_SC_state WHERE Stk_no = @StkNo", connection); // Stk_no�� �ش��ϴ� Stk_state �� ����
                    command2.Parameters.AddWithValue("@StkNo", reader1["Stk_no"]); // �Ķ���� ����
                    SqlDataReader reader2 = command2.ExecuteReader(); // ���� ���� �� ��� �б�

                    while (reader2.Read()) // ��� �� �ݺ�
                    {
                        int stkState = (int)reader2["Stk_state"]; // Stk_state �� �� ��������

                        // ���� Ȯ�� �� ListView�� ������ �߰�
                        if (Convert.ToInt32(value) < cellType && state == "Empty" && stkState == 0)
                        {
                            // ListView�� �߰��� ������ ���� �� �߰�
                            ListViewItem listViewItem = new ListViewItem(reader1["Cell_type"].ToString()); // t_Cell ���̺��� ������ �� �߰�
                            listViewItem.SubItems.Add(reader2["Stk_state"].ToString()); // t_SC_sate ���̺��� ������ �� �߰�
                            // ListView�� �߰�
                            ItemView.Items.Add(listViewItem); // ListView�� ������ �߰�
                        }
                    }
                    // reader2 �ݱ�
                    reader2.Close();
                }
                // reader1 �ݱ�
                reader1.Close();

                // �����ͺ��̽� ���� �ݱ�
                connection.Close();
            }

            // ���� ó��
            catch (Exception ex)
            {
                // ���� �޽��� ǥ��
                MessageBox.Show("Error : " + ex.Message);
            }
        }
        
        private void StartOPCThread()
        {
            opcThread = new Thread(new ThreadStart(() =>
            {
                while (!stopThread)
                {
                    ConnectToOPCServer();
                    // 0.2�ʸ��� ����
                    Thread.Sleep(200);
                }
            }));
            opcThread.Start();
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