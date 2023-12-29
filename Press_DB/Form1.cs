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

            // ������ ����
            opcThread = new Thread(() => opcServerJoin(reserve, cancellationToken));
            opcThread.Start();
        }

        private void opcServerJoin(int reserve, CancellationToken cancellationToken)
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

            // reserve ���� 0 �Ͻ� �԰� �Լ� ó��
            if (reserve == 0)
            {
                InReserveData(cancellationToken);
            }
            // reserve ���� 1 �Ͻ� ��� �Լ� ó��
            else if (reserve == 1)
            {
                OutReserveData(cancellationToken);
            }
        }

        // OPC ���� ���� �� ������ ���� �޼���
        private void InReserveData(CancellationToken cancellationToken)
        {
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            /* item ���
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

            while (!cancellationToken.IsCancellationRequested)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        SQLstateTxt.ForeColor = Color.Blue;
                        /*
                        itemValues["PLT_IN_OUT"] = opcItems.AddItem("PLT_IN_OUT", 1);
                        itemValues["Job_Line"] = opcItems.AddItem("Job_Line", 1);
                        itemValues["Serial_No"] = opcItems.AddItem("Serial_No", 1);
                        itemValues["Pal_no"] = opcItems.AddItem("PLT_Number", 1);
                        itemValues["Pal_type"] = opcItems.AddItem("PLT_TYPE", 1);
                        itemValues["Car_Type"] = opcItems.AddItem("Car_Type", 1);
                        itemValues["Item"] = opcItems.AddItem("Item", 1);
                        itemValues["Spec"] = opcItems.AddItem("Spec", 1);
                        itemValues["LINE"] = opcItems.AddItem("LINE", 1);
                        itemValues["Max_qty"] = opcItems.AddItem("Parts_count_in_pallet", 1);
                        itemValues["Counts"] = opcItems.AddItem("Counts", 1);
                        */

                        itemValues["PLT_IN_OUT"] = 1;
                        itemValues["Job_Line"] = 201;
                        itemValues["Serial_No"] = "2311010001";
                        itemValues["Pal_no"] = "6";
                        itemValues["Pal_type"] = "4";
                        itemValues["Car_Type"] = "2";
                        itemValues["Item"] = "67111";
                        itemValues["Spec"] = "1";
                        itemValues["LINE"] = "2";
                        itemValues["Max_qty"] = 2;
                        itemValues["Counts"] = "3";

                        string item = itemValues["Item"].ToString();
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
                                               t_Cell tc ON LEFT(tc.Cell, 2) = tsc.Stk_no
                                           WHERE
                                               tsc.Stk_state = 0
                                               AND tc.LEFT(tc.PLT_CODE, 1) >= @codeFirstChar
                                               AND tc.State = 'EMPTY'
                                           ORDER BY
                                               tsc.Stk_no DESC
                                       ";
                        */

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
                        AND tc.Cell_type >= @item
                        AND tc.State = 'EMPTY'
                    ORDER BY
                        tsc.Stk_no DESC
                ";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@item", item);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            string cell = reader["Cell"].ToString();


                            itemValues["JobType"] = "INAUTO";
                            itemValues["Cell"] = cell;
                            itemValues["State"] = "INCOMP";
                            itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                            itemValues["Utime"] = DateTime.Now.ToString("hh:mm:ss");

                            int stkState = Convert.ToInt32(reader["Stk_state"]);

                            if (stkState == 0)
                            {
                                UpdateListView(cell, "���� �԰�", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss"));
                            }

                            reader.Close();

                            InsertToDatabase(connection, 0, itemValues);

                            opcThread.Join();
                        }
                    }

                    catch (Exception ex)
                    {
                        SQLstateTxt.ForeColor = Color.Red;
                        MessageBox.Show(ex.ToString());
                    }
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

            Dictionary<string, object> itemValues = new Dictionary<string, object>();
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
                        t_Cell tc ON LEFT(tc.Cell, 2) = tsc.Stk_no
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.item = '67111'
                        AND tc.State = 'INCOMP'
                    ORDER BY
                        CONVERT(datetime, tc.Udate + ' ' + Utime) ASC,
                        tsc.Stk_no DESC;
                ";

                    SqlCommand command = new SqlCommand(query, connection);
                    //command.Parameters.AddWithValue("@Item", Item);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int stkState = Convert.ToInt32(reader["Stk_state"]);
                        string cell = reader["Cell"].ToString();

                        itemValues["JobType"] = "OUTAUTO";
                        itemValues["Cell"] = cell;
                        itemValues["Pal_no"] = reader["Pal_no"].ToString();
                        itemValues["Pal_type"] = reader["Pal_type"].ToString();
                        itemValues["Model"] = reader["Model"].ToString();
                        itemValues["Item"] = reader["Item"].ToString();
                        itemValues["Spec"] = reader["Spec"].ToString();
                        itemValues["Line"] = reader["Line"].ToString();
                        itemValues["Qty"] = reader["Qty"].ToString();
                        itemValues["Max_qty"] = reader["Max_qty"].ToString();
                        itemValues["Quality"] = reader["Quality"].ToString();
                        itemValues["Prod_date"] = reader["Prod_date"].ToString();
                        itemValues["Prod_time"] = reader["Prod_time"].ToString();
                        itemValues["State"] = "OUTCOMP";
                        itemValues["Pos"] = reader["Pos"].ToString();
                        itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                        itemValues["Utime"] = DateTime.Now.ToString("hh:mm:ss");


                        if (stkState == 0)
                        {
                            UpdateListView(cell, "���� ���", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss"));
                        }
                    }
                    reader.Close();

                    InsertToDatabase(connection, 1, itemValues);

                    opcThread.Join();
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
            StartThread(0);
        }

        private void outBtn_Click(object sender, EventArgs e)
        {
            StartThread(1);
        }
    }
}