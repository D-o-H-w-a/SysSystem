using System;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using OPCAutomation;
using static System.Windows.Forms.AxHost;

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

        // �����ͺ��̽� �׽�Ʈ��
        private List<object> testitem = new List<object>();

        public Form1()
        {
            InitializeComponent();
            // ������ ���� �޼ҵ� ȣ��
            StartThread();
        }

        // �����ͺ��̽� ���� ���ڿ�
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private string opcServerIP = ConfigurationManager.AppSettings["OPCServerIP"];

        // OPC ���� ���� �� ������ ���� �޼���
        private void ConnectToOPCServer()
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

            // �����ͺ��̽� ����
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string cell = new string("");
                // ������ ���̽� ������ �����
                connection.Open();

                // t_Cell ���̺��� Cell_type �� State ��������
                SqlCommand cmdCell = new SqlCommand("SELECT Cell_type, State, Cell FROM t_Cell", connection);
                // �� ������ �������� ���� ���� ����
                SqlDataReader readerCell = cmdCell.ExecuteReader();

                // t_Cell ���̺� ���ڵ� �ݺ�
                while (readerCell.Read())
                {
                    // Cell_type �� ���� ������
                    string cellType = readerCell["Cell_type"].ToString();
                    // State �� ���� ������
                    string state = readerCell["State"].ToString();

                    int value = 71112;// Convert.ToInt32(opcItemList.Find(item => item.ItemID == "Item"));

                    if (!string.IsNullOrEmpty(cellType))
                    {
                        if (value <= Convert.ToInt32(cellType) && state == "EMPTY")
                        {
                            cell = Convert.ToString(readerCell["Cell"]);
                            // t_Cell ���� �ݱ�
                            readerCell.Close();
                            break;
                        }

                        else if (value > int.Parse(cellType))
                        {
                            UpdateListView("�� ������ �������� ����");
                        }

                        else if (state != "Emtpy")
                        {
                            switch (state)
                            {
                                case "INRUN":
                                    UpdateListView("�԰� ���� ��");
                                    break;
                                case "OUTRUN":
                                    UpdateListView("��� ���� ��");
                                    break;
                                case "INCOMP":
                                    UpdateListView("�԰� �Ϸ�");
                                    break;
                                case "OUTCOMP":
                                    UpdateListView("��� �Ϸ�");
                                    break;
                            }
                        }
                    }
                }

                // t_SC_state ���̺��� Stk_state �� ��������
                SqlCommand cmdSCState = new SqlCommand("SELECT Stk_mode FROM t_Run_mode ORDER BY Stk_no", connection);
                // SC_state ������ �������� ���� ���� ����
                SqlDataReader readerSCState = cmdSCState.ExecuteReader();

                // t_SC_state ���̺� ���ڵ� �ݺ�
                while (readerSCState.Read())
                {
                    // Stk_state �� ���� ������
                    int stkState = Convert.ToInt32(readerSCState["Stk_mode"]);
                    //OPC ������ �� ��������
                    
                    // ���� �˻� �� ó��
                    // OPC ������ ���� �����ͺ��̽����� ������ ������ �������� �˻�
                    if (stkState == 0)
                    {
                        // ���ǿ� ���� ��� �����ͺ��̽��� ����
                        // t_SC_state ���� �ݱ�
                        readerSCState.Close();
                        break;
                    }
                    else
                    {
                        if (stkState != 1)
                        {
                            UpdateListView(stkState + "�� ũ���� Error ����");
                        }
                    }
                }

                UpdateListView("������ ���� ����");
                //InsertToDatabase(connection, cell);
                // �����ͺ��̽� ���� �ݱ�
                connection.Close();
                // ������ ����
                StopThread();
            }
        }

        void UpdateListView(string text)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke((MethodInvoker)delegate {
                    // ListView ������Ʈ �� UI �۾� ����
                    listView.Items.Add(text);
                });
            }
            else
            {
                // ���� UI �����忡�� ���� ���� ���
                listView.Items.Add(text);
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
            string insertQuery = "INSERT INTO t_In_reserve (Cell, PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_TYPE, Car_Type, Item, Spec, LINE, Parts_count_int_pallet, Counts) " +
                                 "VALUES (@Cell, @PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_TYPE, @Car_Type, @Item, @Spec, @LINE, @Parts_count_int_pallet, @Counts)";

            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // ��ųʸ��� ������ SQL �Ű������� �߰�
            foreach (var kvp in itemValues)
            {
                cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
            }

            // ���� ����
            cmdInsert.ExecuteNonQuery();
        }

        // ������ ���ۺκ�
        private void StartThread()
        {
            // ConnectToOPCServer �޼��带 �����ϱ� ���� ���ο� �����带 ����
            opcThread = new Thread(new ThreadStart(ConnectToOPCServer));
            // ������ �����带 ����
            opcThread.Start();
        }

        // ������ ���� �޼���
        private void StopThread()
        {
            // opcThread ���� null �ƴϸ� opcThread �� �������� ��
            if (opcThread != null && opcThread.IsAlive)
            { 
                // ������ ���� ��û
                opcThread.Join();
            }
        }

    }
}