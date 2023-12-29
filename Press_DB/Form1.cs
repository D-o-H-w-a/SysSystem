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
        // OPC 서버 인스턴스를 나타내는 변수 선언
        private OPCServer opcServer;
        // OPC 아이템들을 관리하는 객체 선언
        private OPCItems opcItems;
        // OPC 그룹들을 관리하는 객체 선언
        private OPCGroups opcGroups;
        // OPC 그룹을 나타내는 변수 선언
        private OPCGroup opcGroup;
        // OPC 아이템을 나타내는 변수 선언
        private OPCItem opcItem;
        // OPC 통신을 위한 스레드 변수 선언 
        private Thread opcThread;
        // 스레드를 중지시키는데 사용되는 토큰을 생성하는 CancellationTokenSource
        private CancellationTokenSource cancellationTokenSource;

        // 데이터베이스 테스트용
        private List<object> testitem = new List<object>();

        public Form1()
        {
            InitializeComponent();
            // 스레드 시작 메소드 호출
        }

        // 데이터베이스 연결 문자열
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private string opcServerIP = ConfigurationManager.AppSettings["OPCServerIP"];

        // 스레드 시작부분
        private void StartThread(int reserve)
        {
            // CancellationTokenSource 생성
            cancellationTokenSource = new CancellationTokenSource();

            // CancellationTokenSource에서 Token 가져오기
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // 스레드 시작
            opcThread = new Thread(() => opcServerJoin(reserve, cancellationToken));
            opcThread.Start();
        }

        private void opcServerJoin(int reserve, CancellationToken cancellationToken)
        {
            /*
            // OPC 서버 연결
            opcServer = new OPCServer();
            // OPC 서버에 연결
            opcServer.Connect(opcServerIP);

            // OPC 그룹 생성 및 설정
            opcGroups = opcServer.OPCGroups; // opc 서버에서 그룹을 관리하는 객체를 가져옴
            // 이름에 맞는 OPC 그룹을 생성
            opcGroup = opcGroups.Add("YourGroup");
            // OPC 그룹을 활성화
            opcGroup.IsActive = true;
            // OPC 그룹을 구독 모드로 설정하여 실시간 데이터 수집
            opcGroup.IsSubscribed = true;
            // OPC 아이템들을 관리하는 객체를 가져옴
            opcItems = opcGroup.OPCItems;
            */

            // reserve 값이 0 일시 입고 함수 처리
            if (reserve == 0)
            {
                InReserveData(cancellationToken);
            }
            // reserve 값이 1 일시 출고 함수 처리
            else if (reserve == 1)
            {
                OutReserveData(cancellationToken);
            }
        }

        // OPC 서버 연결 및 데이터 수집 메서드
        private void InReserveData(CancellationToken cancellationToken)
        {
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            /* item 사용
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

            /* PLT_CODE 사용
            
            // OPC 아이템 추가
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
                                UpdateListView(cell, "정상 입고", "정상", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss"));
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

        // 폼 종료시 작동되는 함수
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // 스레드 종료
            StopThread();
        }

        private void OutReserveData(CancellationToken cancellationToken)
        {
            /*

           // OPC 아이템 추가
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
                            UpdateListView(cell, "정상 출고", "정상", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss"));
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
            // insert 가 0 이면 Inreserv 에 데이터 삽입
            if (insert == 0)
            {
                // 각 아이템의 값을 딕셔너리에 추가
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

                // t_In_reserve 테이블에 데이터 삽입

                string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', @Udate, @Utime)";
                /* PLT_CODE 처리
                 * string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
                */

                SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                // 딕셔너리의 값들을 SQL 매개변수에 추가
                foreach (var kvp in itemValues)
                {
                    if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                    {
                        object valueToInsert = kvp.Value != null ? kvp.Value : " "; // 키가 없거나 값이 null인 경우 공백 문자열로 대체

                        cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                    }
                }

                // 쿼리 실행
                cmdInsert.ExecuteNonQuery();
            }

            // insert 가 1 이면 outreserv 에 데이터 삽입
            else if (insert == 1)
            {
                // itemValues["PLT_CODE"] = opcItemList.Find(item => item.ItemID == "PLT_CODE")?.Value;

                // t_out_reserve 테이블에 데이터 삽입
                string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";

                /* PLT_CODE 처리
                 * string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Parod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
                */
                SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                // 딕셔너리의 값들을 SQL 매개변수에 추가
                foreach (var kvp in itemValues)
                {
                    cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                }

                // 쿼리 실행
                cmdInsert.ExecuteNonQuery();
            }
            // 데이터베이스 연결 닫기
            connection.Close();
        }

        // 스레드 종료 메서드
        private void StopThread()
        {
            // opcThread 값이 null 아니며 opcThread 가 동작중일 시
            if (opcThread != null && opcThread.IsAlive)
            {
                // 스레드 종료 요청
                cancellationTokenSource.Cancel();
            }
        }

        void UpdateListView(string cell, string cellState, string scState, string Date, string Time)
        {
            // UI 스레드가 아닌 경우, UI 스레드에서 작업하도록 요청
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate
                {
                    // 데이터 그리드에 새로운 행 추가
                    dataGrid.Rows.Add(cell, cellState, scState, Date, Time);
                });
            }
            else // UI 스레드인 경우, 직접 작업 수행
            {
                // 데이터 그리드에 새로운 행 추가
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