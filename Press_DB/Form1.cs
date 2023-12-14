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
        // OPC ���� ���� �� ��ü ����
        private OPCServer opcServer;
        private OPCItems opcItems;
        private List<OPCItem> opcItemList = new List<OPCItem>();

        public Form1()
        {
            InitializeComponent();
            StartThread();
        }

        // �����ͺ��̽� ���� ���ڿ�
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        // OPC ���� ���� �� ������ ���� �޼���
        private void ConnectToOPCServer()
        {
            // OPC ���� ����
            opcServer = new OPCServer();
            opcServer.Connect("OPCServerIP");

            // OPC ������ �߰� (����)
            opcItems = opcServer.OPCItems;
            opcItemList.Add(opcItems.AddItem("PLT_IN_OUT", 1));
            opcItemList.Add(opcItems.AddItem("Job_Line", 1));
            opcItemList.Add(opcItems.AddItem("Serial_No"), 1);
            opcItemList.Add(opcItems.AddItem("PLT_Number"), 1);
            opcItemList.Add(opcItems.AddItem("PLT_TYPE"), 1);
            opcItemList.Add(opcItems.AddItem("Car_Type"), 1);
            opcItemList.Add(opcItems.AddItem("Item"), 1);
            opcItemList.Add(opcItems.AddItem("Spec"), 1);
            opcItemList.Add(opcItems.AddItem("LINE"), 1);
            opcItemList.Add(opcItems.AddItem("Parts_count_int_pallet"), 1);
            opcItemList.Add(opcItems.AddItem("Counts"), 1);

            // �ʿ��� ������ OPC ������ �߰�

            // �����ͺ��̽� ����
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // t_Cell ���̺��� Cell_type �� State ��������
                SqlCommand cmdCell = new SqlCommand("SELECT Cell_type, State FROM t_Cell", connection);
                SqlDataReader readerCell = cmdCell.ExecuteReader();

                while (readerCell.Read())
                {
                    int cellType = Convert.ToInt32(readerCell["Cell_type"]);
                    string state = readerCell["State"].ToString();

                    // t_SC_state ���̺��� Stk_state �� ��������
                    SqlCommand cmdSCState = new SqlCommand("SELECT Stk_state FROM t_SC_state", connection);
                    SqlDataReader readerSCState = cmdSCState.ExecuteReader();

                    while (readerSCState.Read())
                    {
                        int stkState = Convert.ToInt32(readerSCState["Stk_state"]);
                        
                        int value = Convert.ToInt32(opcItemList.Find(item => item.ItemName == "Item"));
                        // ���� �˻� �� ó��
                        // opcItemList ����Ʈ�� ���ϴ� �ε����� ����Ͽ� OPC ������ ���� �����Ͽ� ���� �˻�
                        if (value < cellType && state == "Empty" && stkState == 0)
                        {
                            // In_reserve �� ����
                            // �� �κп��� In_reserve ���� �����ϴ� ����� ���� ������ ó���� �߰��ϼ���.
                            // ��: SqlCommand�� ����Ͽ� �����ͺ��̽��� ���� �����ϴ� ��� ��
                        }
                    }

                    readerSCState.Close();
                }

                readerCell.Close();
                connection.Close();
            }

            // ������ ����
            // �ʿ��� ��� ������ ���� ó�� �߰�
        }

        private void InsertToDatabase(SqlConnection connection)
        {
            // ������ �̸��� ���� ��ųʸ� ����
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            // �� �������� ���� ��ųʸ��� �߰�
            itemValues["PLT_IN_OUT"] = opcItemList.Find(item => item.ItemName == "PLT_IN_OUT")?.Value;
            itemValues["Job_Line"] = opcItemList.Find(item => item.ItemName == "Job_Line")?.Value;
            itemValues["Serial_No"] = opcItemList.Find(item => item.ItemName == "Serial_No")?.Value;
            itemValues["PLT_Number"] = opcItemList.Find(item => item.ItemName == "PLT_Number")?.Value;
            itemValues["PLT_TYPE"] = opcItemList.Find(item => item.ItemName == "PLT_TYPE")?.Value;
            itemValues["Car_Type"] = opcItemList.Find(item => item.ItemName == "Car_Type")?.Value;
            itemValues["Item"] = opcItemList.Find(item => item.ItemName == "Item")?.Value;
            itemValues["Spec"] = opcItemList.Find(item => item.ItemName == "Spec")?.Value;
            itemValues["LINE"] = opcItemList.Find(item => item.ItemName == "LINE")?.Value;
            itemValues["Parts_count_int_pallet"] = opcItemList.Find(item => item.ItemName == "Parts_count_int_pallet")?.Value;
            itemValues["Counts"] = opcItemList.Find(item => item.ItemName == "Counts")?.Value;

            // t_In_reserve ���̺� ������ ����
            string insertQuery = "INSERT INTO t_In_reserve (PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_TYPE, Car_Type, Item, Spec, LINE, Parts_count_int_pallet, Counts) " +
                                 "VALUES (@PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_TYPE, @Car_Type, @Item, @Spec, @LINE, @Parts_count_int_pallet, @Counts)";

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
            Thread opcThread = new Thread(new ThreadStart(ConnectToOPCServer));
            opcThread.Start();
        }
    }
}