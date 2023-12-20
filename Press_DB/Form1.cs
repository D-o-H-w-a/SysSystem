using System;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using OPCAutomation;
using static System.Windows.Forms.AxHost;
using System.Diagnostics;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        // OPC ���� �ν��Ͻ��� ��Ÿ���� ���� ����
        private OPCServer opcServer;
        // OPC �����۵��� �����ϴ� ��ü ����
        private OPCItems opcItems;
        // OPC �����۵��� ���� ����Ʈ ����
        private List<OPCItem> opcItemList = new List<OPCItem>();
        // OPC �׷���� �����ϴ� ��ü ����
        private OPCGroups opcGroups;
        // OPC �׷��� ��Ÿ���� ���� ����
        private OPCGroup opcGroup;
        // OPC �������� ��Ÿ���� ���� ����
        private OPCItem opcItem;
        // OPC ����� ���� ������ ���� ���� 
        private Thread opcThread;
        // �����带 ������Ű�µ� ���Ǵ� ��ū�� �����ϴ� CancellationTokenSource
        private CancellationTokenSource cancellationTokenSource;

        // �����ͺ��̽� �׽�Ʈ��
        private List<object> testitem = new List<object>();

        public Form1()
        {
            InitializeComponent();
            // ������ ���� �޼ҵ� ȣ��
        }

        // �����ͺ��̽� ���� ���ڿ�
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private string opcServerIP = ConfigurationManager.AppSettings["OPCServerIP"];

        // ������ ���ۺκ�
        private void StartThread(int reserve)
        {
            // CancellationTokenSource ����
            cancellationTokenSource = new CancellationTokenSource();

            // CancellationTokenSource���� Token ��������
            CancellationToken cancellationToken = cancellationTokenSource.Token;


            if (reserve == 0)
            {
                // ������ ����
                opcThread = new Thread(() => inReserveData(cancellationToken));
                opcThread.Start();
            }
        }

        // OPC ���� ���� �� ������ ���� �޼���
        private void inReserveData(CancellationToken cancellationToken)
        {
            /*
            // OPC ���� ����
            opcServer = new OPCServer();
            // OPC ������ ����
            opcServer.Connect(opcServerIP);

            // OPC �׷� ���� �� ����
            opcGroups = opcServer.OPCGroups; // opc �������� �׷��� �����ϴ� ��ü�� ������
            // �̸��� �´� OPC �׷��� ����
            opcGroup = opcGroups.Add("YourGroup");
            // OPC �׷��� Ȱ��ȭ
            opcGroup.IsActive = true;
            // OPC �׷��� ���� ���� �����Ͽ� �ǽð� ������ ����
            opcGroup.IsSubscribed = true;
            // OPC �����۵��� �����ϴ� ��ü�� ������
            opcItems = opcGroup.OPCItems;


            // OPC ������ �߰�
            opcItemList.Add(opcItems.AddItem("PLT_IN_OUT", 1));
            opcItemList.Add(opcItems.AddItem("Job_Line", 1));
            opcItemList.Add(opcItems.AddItem("Serial_No", 1));
            opcItemList.Add(opcItems.AddItem("PLT_Number", 1));
            opcItemList.Add(opcItems.AddItem("PLT_TYPE", 1));
            opcItemList.Add(opcItems.AddItem("Car_Type", 1));
            opcItemList.Add(opcItems.AddItem("Item", 1));
            opcItemList.Add(opcItems.AddItem("Spec", 1));
            opcItemList.Add(opcItems.AddItem("LINE", 1));
            opcItemList.Add(opcItems.AddItem("Parts_count_int_pallet", 1));
            opcItemList.Add(opcItems.AddItem("Counts", 1));
            */

            testitem.Add("PLT_IN_OUT");
            testitem.Add("Job_Line");
            testitem.Add("Serial_No");
            testitem.Add("PLT_Number");
            testitem.Add("PLT_TYPE");
            testitem.Add("Car_Type");
            testitem.Add("Item");
            testitem.Add("Spec");
            testitem.Add("LINE");
            testitem.Add("Parts_count_int_pallet");
            testitem.Add("Counts");

            while (!cancellationToken.IsCancellationRequested)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Cell,
                        tc.Cell_type,
                        tc.State
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON LEFT(tc.Cell, 2) = tsc.Stk_no
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Cell_type >= '67111'
                        AND tc.State = 'EMPTY'
                    ORDER BY
                        tsc.Stk_no DESC
                ";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int value = 67111;// Convert.ToInt32(opcItemList.Find(item => item.ItemID == "Item"));
                        string cellType = reader["Cell_type"].ToString();
                        string state = reader["State"].ToString();
                        string cell = reader["Cell"].ToString();
                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        if (stkState == 0)
                        {
                            UpdateListView(cell, "���� �԰�", "����", DateTime.Now.ToString("yyyy-MM-dd"),DateTime.Now.ToString("hh:mm:ss"));
                        }
                    }

                    reader.Close();

                    opcThread.Join();
                }
            }
        }

        void UpdateListView(string cell, string cellState, string scState, string Date, string Time)
        {
            // UI �����尡 �ƴ� ���, UI �����忡�� �۾��ϵ��� ��û
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate {
                    // ������ �׸��忡 ���ο� �� �߰�
                    dataGrid.Rows.Add(cell, cellState, scState, Date, Time);
                });
            }
            else // UI �������� ���, ���� �۾� ����
            {
                // ������ �׸��忡 ���ο� �� �߰�
                dataGrid.Rows.Add(cell, cellState, scState, Date, Time);
            }
        }

        private void InsertToDatabase(SqlConnection connection, string cell)
        {
            // ������ �̸��� ���� ��ųʸ� ����
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            // �� �������� ���� ��ųʸ��� �߰�
            itemValues["Cell"] = cell;
            itemValues["PLT_IN_OUT"] = opcItemList.Find(item => item.ItemID == "PLT_IN_OUT")?.Value;
            itemValues["Job_Line"] = opcItemList.Find(item => item.ItemID == "Job_Line")?.Value;
            itemValues["Serial_No"] = opcItemList.Find(item => item.ItemID == "Serial_No")?.Value;
            itemValues["PLT_Number"] = opcItemList.Find(item => item.ItemID == "PLT_Number")?.Value;
            itemValues["PLT_TYPE"] = opcItemList.Find(item => item.ItemID == "PLT_TYPE")?.Value;
            itemValues["Car_Type"] = opcItemList.Find(item => item.ItemID == "Car_Type")?.Value;
            itemValues["Item"] = opcItemList.Find(item => item.ItemID == "Item")?.Value;
            itemValues["Spec"] = opcItemList.Find(item => item.ItemID == "Spec")?.Value;
            itemValues["LINE"] = opcItemList.Find(item => item.ItemID == "LINE")?.Value;
            itemValues["Parts_count_int_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_int_pallet")?.Value;
            itemValues["Counts"] = opcItemList.Find(item => item.ItemID == "Counts")?.Value;

            // t_In_reserve ���̺� ������ ����
            string insertQuery = "INSERT INTO t_In_reserve (Cell, PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_TYPE, Car_Type, Item, Spec, LINE, Parts_count_int_pallet, Counts)" +
                                 "VALUES (@Cell, @PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_TYPE, @Car_Type, @Item, @Spec, @LINE, @Parts_count_int_pallet, @Counts)";

            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // ��ųʸ��� ������ SQL �Ű������� �߰�
            foreach (var kvp in itemValues)
            {
                cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
            }

            // ���� ����
            cmdInsert.ExecuteNonQuery();

            // �����ͺ��̽� ���� �ݱ�
            connection.Close();
        }

        // �� ����� �۵��Ǵ� �Լ�
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // ������ ����
            StopThread();
        }

        // ������ ���� �޼���
        private void StopThread()
        {
            // opcThread ���� null �ƴϸ� opcThread �� �������� ��
            if (opcThread != null && opcThread.IsAlive)
            {
                // ������ ���� ��û
                cancellationTokenSource.Cancel();
            }
        }

        private void inBtn_Click(object sender, EventArgs e)
        {
            StartThread(0);
        }
    }
}