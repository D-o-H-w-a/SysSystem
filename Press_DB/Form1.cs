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
        // OPC 아이템들을 담을 리스트 선언
        private List<OPCItem> opcItemList = new List<OPCItem>();
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

            // reserve 값이 0 일시 입고 함수 처리
            if (reserve == 0)
            {
                InReserveData(cancellationToken);
            }
            // reserve 값이 1 일시 출고 함수 처리
            else if (reserve == 1)
            {

            }
        }

        // OPC 서버 연결 및 데이터 수집 메서드
        private void InReserveData(CancellationToken cancellationToken)
        {
            /*
            
            // OPC 아이템 추가
            opcItemList.Add(opcItems.AddItem("PLT_IN_OUT", 1));
            opcItemList.Add(opcItems.AddItem("Job_Line", 1));
            opcItemList.Add(opcItems.AddItem("Serial_No", 1));
            opcItemList.Add(opcItems.AddItem("PLT_Number", 1));
            opcItemList.Add(opcItems.AddItem("PLT_CODE", 1));
            opcItemList.Add(opcItems.AddItem("Parts_count_in_pallet", 1));
            */

            testitem.Add("PLT_IN_OUT");
            testitem.Add("Job_Line");
            testitem.Add("Serial_No");
            testitem.Add("PLT_Number");
            testitem.Add("PLT_CODE");
            testitem.Add("Parts_count_in_pallet");

            while (!cancellationToken.IsCancellationRequested)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string Code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
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
                        string cellType = reader["Cell_type"].ToString();
                        string state = reader["State"].ToString();
                        string cell = reader["Cell"].ToString();
                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        if (stkState == 0)
                        {
                            UpdateListView(cell, "정상 입고", "정상", DateTime.Now.ToString("yyyy-MM-dd"),DateTime.Now.ToString("hh:mm:ss"));
                        }
                    }

                    reader.Close();

                    opcThread.Join();
                }
            }
        }

        private void InsertToDatabase(SqlConnection connection, string cell)
        {
            // 아이템 이름과 값의 딕셔너리 생성
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            // 각 아이템의 값을 딕셔너리에 추가
            itemValues["Cell"] = cell;
            itemValues["PLT_IN_OUT"] = opcItemList.Find(item => item.ItemID == "PLT_IN_OUT")?.Value;
            itemValues["Job_Line"] = opcItemList.Find(item => item.ItemID == "Job_Line")?.Value;
            itemValues["Serial_No"] = opcItemList.Find(item => item.ItemID == "Serial_No")?.Value;
            itemValues["PLT_Number"] = opcItemList.Find(item => item.ItemID == "PLT_Number")?.Value;
            itemValues["PLT_CODE"] = opcItemList.Find(item => item.ItemID == "PLT_CODE")?.Value;
            itemValues["Parts_count_in_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_in_pallet")?.Value;

            // t_In_reserve 테이블에 데이터 삽입
            string insertQuery = "INSERT INTO t_In_reserve (Cell, PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_CODE, Parts_count_in_pallet)" +
                                 "VALUES (@Cell, @PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_CODE, @Parts_count_in_pallet)";

            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // 딕셔너리의 값들을 SQL 매개변수에 추가
            foreach (var kvp in itemValues)
            {
                cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
            }

            // 쿼리 실행
            cmdInsert.ExecuteNonQuery();

            // 데이터베이스 연결 닫기
            connection.Close();
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
            
            testitem.Add("PLT_CODE");

            while (!cancellationToken.IsCancellationRequested)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string Code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
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
                        AND tc.State = 'INCOMP'
                    ORDER BY
                        tsc.Stk_no DESC
                ";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string cellType = reader["Cell_type"].ToString();
                        string state = reader["State"].ToString();
                        string cell = reader["Cell"].ToString();
                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        if (stkState == 0)
                        {
                            UpdateListView(cell, "정상 입고", "정상", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss"));
                        }
                    }

                    reader.Close();

                    opcThread.Join();
                }
            }
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
                dataGrid.Invoke((MethodInvoker)delegate {
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
    }
}