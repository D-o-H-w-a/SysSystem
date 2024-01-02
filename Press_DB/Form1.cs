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
        // opc item ���� ��ƿ� List  ���� ����
        private List<OPCItem> opcItemList = new List<OPCItem>();
        // tsc.Stk_no �� ���� �˻� ���� ������ ���� ����
        int lastSearchValue;

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

            // ������ ����
            opcThread = new Thread(() => opcServerJoin(reserve, cancellationToken));
            opcThread.Start();
        }

        // opc ������ �����Ͽ� ����� �ϸ� ������ ��ü���� ������
        private void opcServerJoin( int reserve ,CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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
                */

                /*
                // item ���
                opcItemList.Add(opcItems.AddItem("PLT_IN_OUT", 1));
                opcItemList.Add(opcItems.AddItem("Job_Line", 1));
                opcItemList.Add(opcItems.AddItem("Serial_No", 1));
                opcItemList.Add(opcItems.AddItem("PLT_Number", 1));
                opcItemList.Add(opcItems.AddItem("PLT_TYPE", 1));
                opcItemList.Add(opcItems.AddItem("Car_Type", 1));
                opcItemList.Add(opcItems.AddItem("Item", 1));
                opcItemList.Add(opcItems.AddItem("Spec", 1));
                opcItemList.Add(opcItems.AddItem("LINE", 1));
                opcItemList.Add(opcItems.AddItem("Parts_count_in_pallet", 1));
                opcItemList.Add(opcItems.AddItem("Counts", 1));
                */

                /* PLT_CODE ���

                // OPC ������ �߰�
                opcItemList.Add(opcItems.AddItem("PLT_IN_OUT", 1));
                opcItemList.Add(opcItems.AddItem("Job_Line", 1));
                opcItemList.Add(opcItems.AddItem("Serial_No", 1));
                opcItemList.Add(opcItems.AddItem("PLT_Number", 1));
                opcItemList.Add(opcItems.AddItem("PLT_CODE", 1));
                opcItemList.Add(opcItems.AddItem("Parts_count_in_pallet", 1));
                */

                string callNum = opcItemList.Find(item => item.ItemID == "PLT_IN_OUT")?.Value;


                if (reserve == 1)
                {
                    InReserveData(cancellationToken);
                }
                // callNum ���� 2 �Ͻ� ��� �Լ� ó��
                else if (reserve == 2)
                {
                    OutReserveData(cancellationToken);
                }


                // PLT_IN_OUT ��û ��ȣ�� ���� int �� ���� callNum
                //if (!string.IsNullOrEmpty(callNum))
                //{
                //    // callNum ���� 1 �Ͻ� �԰� �Լ� ó��
                //    if ( int.Parse(callNum) == 1)
                //    {
                //        InReserveData(cancellationToken);
                //    }
                //    // callNum ���� 2 �Ͻ� ��� �Լ� ó��
                //    else if (int.Parse(callNum) == 2)
                //    {
                //        OutReserveData(cancellationToken);
                //    }
                //}
            }
        }

        // OPC ���� ���� �� ������ ���� �޼���
        private void InReserveData(CancellationToken cancellationToken)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLstateTxt.ForeColor = Color.Blue;

                    //string code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
                    //char codeFirstChar = code.FirstOrDefault();

                    /*                   string query = @"
                                       SELECT TOP 1
                                           tsc.Stk_no,
                                           tsc.Stk_state,
                                           tc.Cell,
                                           tc.State,
                                           tc.PLT_CODE
                                       FROM
                                           t_SC_state tsc
                                       LEFT JOIN
                                           t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                                       WHERE
                                           tsc.Stk_state = 0
                                           AND tc.LEFT(tc.PLT_CODE, 1) >= @codeFirstChar
                                           AND tc.State = 'EMPTY'
                                       ORDER BY
                                           tsc.Stk_no DESC
                                   ";
                    */

                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Bank,
                        tc.Cell_type,
                        tc.State,
                        tc.Cell
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Cell_type >= 71112
                        AND tc.State = 'EMPTY'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_In_reserve) -- t_In_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                        OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
                        )
                    ORDER BY
                        tsc.Stk_no DESC
                    ";

                    //string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    //command.Parameters.AddWithValue("@item", item);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        string cell = reader["Cell"].ToString();

                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        lastSearchValue = Convert.ToInt32(reader["Stk_no"]);

                        if (stkState == 0)
                        {
                            UpdateListView(cell, "���� �԰�", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                        //InsertToDatabase(connection, 0, itemValues);
                    }
                    reader.Close();
                    opcThread.Join();
                }

                catch (Exception ex)
                {
                    SQLstateTxt.ForeColor = Color.Red;
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        // �� ����� �۵��Ǵ� �Լ�
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // ������ ����
            StopThread();
        }

        private void OutReserveData(CancellationToken cancellationToken)
        {
            /*

           // OPC ������ �߰�
           opcItemList.Add(opcItems.AddItem("PLT_CODE", 1));
           */

            //itemValues["Item"] = opcItems.AddItem("Item", 1);
            //opcItemList.Add(opcItems.AddItem("item", 1));
            //testitem.Add("PLT_CODE");

            while (!cancellationToken.IsCancellationRequested)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //string Item = itemValues["Item"].ToString();
                    connection.Open();
                    //string Code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
                    //string Code = opcItemList.Find(pltCode => pltCode.ItemID == "item").ToString();
                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Cell,
                        tc.item,
                        tc.State,
                        tc.Pal_no,
                        tc.Pal_type,
                        tc.Model,
                        tc.Spec,
                        tc.Line,
                        tc.Qty,
                        tc.Max_qty,
                        tc.Quality,
                        tc.Prod_date,
                        tc.Prod_time,
                        tc.Pos
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Cell_type >= 67111
                        AND tc.State = 'INCOMP'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_Out_reserve) -- t_Out_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                        OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
                        )
                    ORDER BY
                        tsc.Stk_no DESC
                ";
                    //string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    //command.Parameters.AddWithValue("@item", item);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int stkState = Convert.ToInt32(reader["Stk_state"]);
                        string cell = reader["Cell"].ToString();

                        if (stkState == 0)
                        {
                            UpdateListView(cell, "���� ���", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                    }
                    reader.Close();

                    //InsertToDatabase(connection, 1, itemValues);
                }
            }
        }

        private void InsertToDatabase(SqlConnection connection, int insert, Dictionary<string, object> itemValues)
        {
            // insert �� 0 �̸� Inreserv �� ������ ����
            if (insert == 0)
            {
                // �� �������� ���� ��ųʸ��� �߰�
                /*
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
                itemValues["Parts_count_int_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_in_pallet")?.Value;
                itemValues["Counts"] = opcItemList.Find(item => item.ItemID == "Counts")?.Value;
                */

                // itemValues["PLT_CODE"] = opcItemList.Find(item => item.ItemID == "PLT_CODE")?.Value;
                // itemValues["Parts_count_in_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_in_pallet")?.Value;

                // t_In_reserve ���̺� ������ ����

                string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', @Udate, @Utime)";
                /* PLT_CODE ó��
                 * string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
                */

                SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                // ��ųʸ��� ������ SQL �Ű������� �߰�
                foreach (var kvp in itemValues)
                {
                    if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                    {
                        object valueToInsert = kvp.Value != null ? kvp.Value : " "; // Ű�� ���ų� ���� null�� ��� ���� ���ڿ��� ��ü

                        cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                    }
                }

                // ���� ����
                cmdInsert.ExecuteNonQuery();
            }

            // insert �� 1 �̸� outreserv �� ������ ����
            else if (insert == 1)
            {
                // itemValues["PLT_CODE"] = opcItemList.Find(item => item.ItemID == "PLT_CODE")?.Value;

                // t_out_reserve ���̺� ������ ����
                string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";

                /* PLT_CODE ó��
                 * string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Parod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
                */
                SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                // ��ųʸ��� ������ SQL �Ű������� �߰�
                foreach (var kvp in itemValues)
                {
                    cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                }

                // ���� ����
                cmdInsert.ExecuteNonQuery();
            }
            // �����ͺ��̽� ���� �ݱ�
            connection.Close();
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

        void UpdateListView(string cell, string cellState, string scState, string Date, string Time)
        {
            // UI �����尡 �ƴ� ���, UI �����忡�� �۾��ϵ��� ��û
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate
                {
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

        private void inBtn_Click(object sender, EventArgs e)
        {
            StartThread(1);
        }

        private void outBtn_Click(object sender, EventArgs e)
        {
            StartThread(2);
        }
    }
}