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
            StartThread();
        }

        // 데이터베이스 연결 문자열
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private string opcServerIP = ConfigurationManager.AppSettings["OPCServerIP"];

        // OPC 서버 연결 및 데이터 수집 메서드
        private void ConnectToOPCServer(CancellationToken cancellationToken)
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


            // OPC 아이템 추가
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

            while (!cancellationToken.IsCancellationRequested) {
                // 데이터베이스 연결
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string cell = new string("");
                    // 데이터 베이스 연결을 열어옴
                    connection.Open();

                    // t_Cell 테이블에서 Cell_type 및 State 가져오기
                    SqlCommand cmdCell = new SqlCommand("SELECT Cell_type, State, Cell FROM t_Cell", connection);
                    // 셀 정보를 가져오기 위한 쿼리 실행
                    SqlDataReader readerCell = cmdCell.ExecuteReader();

                    // t_Cell 테이블 레코드 반복
                    while (readerCell.Read())
                    {
                        // Cell_type 의 값을 가져옴
                        string cellType = readerCell["Cell_type"].ToString();
                        // State 의 값을 가져옴
                        string state = readerCell["State"].ToString();

                        cell = Convert.ToString(readerCell["Cell"]);
                        int value = 71112;// Convert.ToInt32(opcItemList.Find(item => item.ItemID == "Item"));

                        if (!string.IsNullOrEmpty(cellType))
                        {
                            if (value <= Convert.ToInt32(cellType) && state == "EMPTY")
                            {
                                // t_Cell 리더 닫기
                                readerCell.Close();
                                break;
                            }

                            else if (value > int.Parse(cellType))
                            {
                                UpdateListView(cell, "셀 공간이 적합하지 않음");
                            }

                            else if (state != "Emtpy")
                            {
                                switch (state)
                                {
                                    case "INRUN":
                                        UpdateListView(cell, "입고 진행 중");
                                        break;
                                    case "OUTRUN":
                                        UpdateListView(cell, "출고 진행 중");
                                        break;
                                    case "INCOMP":
                                        UpdateListView(cell, "입고 완료");
                                        break;
                                    case "OUTCOMP":
                                        UpdateListView(cell, "출고 완료");
                                        break;
                                }
                            }
                        }
                    }

                    // t_SC_state 테이블에서 Stk_state 값 가져오기
                    SqlCommand cmdSCState = new SqlCommand("SELECT Stk_mode FROM t_Run_mode ORDER BY Stk_no", connection);
                    // SC_state 정보를 가져오기 위한 쿼리 실행
                    SqlDataReader readerSCState = cmdSCState.ExecuteReader();

                    // t_SC_state 테이블 레코드 반복
                    while (readerSCState.Read())
                    {
                        // Stk_state 의 값을 가져옴
                        int stkState = Convert.ToInt32(readerSCState["Stk_mode"]);
                        //OPC 아이템 값 가져오기

                        // 조건 검사 및 처리
                        // OPC 아이템 값과 데이터베이스에서 가져온 값들을 조건으로 검사
                        if (stkState == 0)
                        {
                            // 조건에 맞을 경우 데이터베이스에 삽입
                            // t_SC_state 리더 닫기
                            readerSCState.Close();
                            break;
                        }
                        else
                        {
                            if (stkState != 1)
                            {
                                UpdateListView(cell, stkState + "번 크레인 Error 상태");
                            }
                        }
                    }

                    UpdateListView(cell, "데이터 정상 저장");
                    //InsertToDatabase(connection, cell);
                    
                    // 스레드 종료
                    StopThread();
                }
            }
        }

        void UpdateListView(string stateCell, string numCell)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke((MethodInvoker)delegate
                {
                    cellState.Text = stateCell;
                    cellNum.Text = numCell;
                    // ListView 업데이트 등 UI 작업 수행
                    listView.Items.Add(cellState.Text);
                });
            }
            else
            {
                cellState.Text = stateCell;
                cellNum.Text = numCell;
                // 현재 UI 스레드에서 실행 중인 경우
                listView.Items.Add(cellState.Text);
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
            itemValues["PLT_TYPE"] = opcItemList.Find(item => item.ItemID == "PLT_TYPE")?.Value;
            itemValues["Car_Type"] = opcItemList.Find(item => item.ItemID == "Car_Type")?.Value;
            itemValues["Item"] = opcItemList.Find(item => item.ItemID == "Item")?.Value;
            itemValues["Spec"] = opcItemList.Find(item => item.ItemID == "Spec")?.Value;
            itemValues["LINE"] = opcItemList.Find(item => item.ItemID == "LINE")?.Value;
            itemValues["Parts_count_int_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_int_pallet")?.Value;
            itemValues["Counts"] = opcItemList.Find(item => item.ItemID == "Counts")?.Value;

            // t_In_reserve 테이블에 데이터 삽입
            string insertQuery = "INSERT INTO t_In_reserve (Cell, PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_TYPE, Car_Type, Item, Spec, LINE, Parts_count_int_pallet, Counts)" +
                                 "VALUES (@Cell, @PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_TYPE, @Car_Type, @Item, @Spec, @LINE, @Parts_count_int_pallet, @Counts)";

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

        // 스레드 시작부분
        private void StartThread()
        {
            // CancellationTokenSource 생성
            cancellationTokenSource = new CancellationTokenSource();

            // CancellationTokenSource에서 Token 가져오기
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // 스레드 시작
            opcThread = new Thread(() => ConnectToOPCServer(cancellationToken));
            opcThread.Start();
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
    }
}