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
               // opcServer = new OPCServer();
                // OPC ������ ����
               // opcServer.Connect("OPCServerIP");

                // OPC Server �� ���� ���� �� item �� ��������

                //OPCGroup ����
               // OPCGroup opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                // OPCGroup�� ���� OPCItems ��ü ����
               // OPCItems opcItems = opcGroup.OPCItems;
                // ���ϴ� ������ �߰�
                //OPCItem opcItem = opcItems.AddItem("YourItem", 1);

                // OPCItem�� ���� ������ ����
                object value;
                // OPCItem���� �� �б�
                //opcItem.Read(1, out value, out _, out _);
                value = 71112;
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
                        if (Convert.ToInt32(value) <= cellType && state == "EMPTY" && stkState == 0)
                        {
                            // ListView�� �߰��� ������ ���� �� �߰�
                            ListViewItem listViewItem = new ListViewItem(reader1["Cell_type"].ToString()); // t_Cell ���̺��� ������ �� �߰�
                            listViewItem.SubItems.Add(reader2["Stk_state"].ToString()); // t_SC_sate ���̺��� ������ �� �߰�
                            // ListView�� �߰�
                            listView.Items.Add(listViewItem); // ListView�� ������ �߰�

                            // t_Cell ���̺��� �ŰԺ��� ��� �Լ� ����.
                            InsertToDatabase(reader1, connection, Convert.ToString(value));
                            break;
                        }
                        else
                        {
                            if(Convert.ToInt32(value) > cellType)
                            {
                                listView.Items.Add(cellType.ToString(),"Cell�� ũ�Ⱑ item ũ�⺸�� �۽��ϴ�");
                            }
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
        
        private void StopOPCThread()
        {
            // ������ ������ �����ϱ� ���� stopThread ������ true �� ����
            stopThread = true;
            // ���� opcThread�� null�� �ƴϰ� ���� ���̶��
            //if (opcThread != null && opcThread.IsAlive)
            {
                // ������ ����
                StopOPCThread();
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



        private void InsertToDatabase(SqlDataReader reader, SqlConnection connection, string item)
        {
            // t_In_reserve ���̺� t_Cell�� ������ ����
            string insertQuery = "INSERT INTO t_In_reserve (JobType, Cell, Pal_no, Pal_type, Model, Item, Spect, Line, Qty, Max_qty, Quailty, Prod_date, Prod_time, State, Pos, Udate, Utime) " +
                "VALUES (@JobType, @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spect, @Line, @Qty, @Max_qty, @Quailty, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";

            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@JobType", reader["JobType"]);
                insertCommand.Parameters.AddWithValue("@Cell", reader["Cell"]);
                insertCommand.Parameters.AddWithValue("@Pal_no", reader["Pal_no"]);
                insertCommand.Parameters.AddWithValue("@Pal_type", reader["Pal_type"]);
                insertCommand.Parameters.AddWithValue("@Model", reader["Model"]);
                insertCommand.Parameters.AddWithValue("@Item", item);
                insertCommand.Parameters.AddWithValue("@Spect", reader["Spect"]);
                insertCommand.Parameters.AddWithValue("@Line", reader["Line"]);
                insertCommand.Parameters.AddWithValue("@Qty", (int)reader["Qty"]); // int ������ Qty ��
                insertCommand.Parameters.AddWithValue("@Max_qty", (int)reader["Max_qty"]); // int ������ Max_qty ��
                insertCommand.Parameters.AddWithValue("@Quailty", reader["Quailty"]);
                insertCommand.Parameters.AddWithValue("@Prod_date", reader["Prod_date"]);
                insertCommand.Parameters.AddWithValue("@Prod_time", reader["Prod_time"]);
                insertCommand.Parameters.AddWithValue("@State", "INCOMP");
                insertCommand.Parameters.AddWithValue("@Pos", reader["Pos"]);
                insertCommand.Parameters.AddWithValue("@Udate", DateTime.Now.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@Utime", DateTime.Now.ToString("HH:mm:ss"));
            }
           
        }
    }
}