using System;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using OPCAutomation;

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
        private void ConnectToOPCServer()
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

            // 데이터베이스 연결
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // 데이터 베이스 연결을 열어옴
                connection.Open();

                // t_Cell 테이블에서 Cell_type 및 State 가져오기
                SqlCommand cmdCell = new SqlCommand("SELECT Cell_type, State FROM t_Cell", connection);
                // 셀 정보를 가져오기 위한 쿼리 실행
                SqlDataReader readerCell = cmdCell.ExecuteReader();

                // t_Cell 테이블 레코드 반복
                while (readerCell.Read())
                {
                    // Cell_type 의 값을 가져옴
                    int cellType = Convert.ToInt32(readerCell["Cell_type"]);
                    // State 의 값을 가져옴
                    string state = readerCell["State"].ToString();

                    // t_SC_state 테이블에서 Stk_state 값 가져오기
                    SqlCommand cmdSCState = new SqlCommand("SELECT Stk_state FROM t_SC_state", connection);
                    // SC_state 정보를 가져오기 위한 쿼리 실행
                    SqlDataReader readerSCState = cmdSCState.ExecuteReader();

                    // t_SC_state 테이블 레코드 반복
                    while (readerSCState.Read())
                    {
                        // Stk_state 의 값을 가져옴
                        int stkState = Convert.ToInt32(readerSCState["Stk_state"]);
                        //OPC 아이템 값 가져오기
                        int value = Convert.ToInt32(opcItemList.Find(item => item.ItemID == "Item"));
                        // 조건 검사 및 처리
                        // OPC 아이템 값과 데이터베이스에서 가져온 값들을 조건으로 검사
                        if (value < cellType && state == "Empty" && stkState == 0)
                        {
                            // 조건에 맞을 경우 데이터베이스에 삽입
                            InsertToDatabase(connection);
                        }
                    }
                    // t_SC_state 리더 닫기
                    readerSCState.Close();
                }

                // t_Cell 리더 닫기
                readerCell.Close();
                // 데이터베이스 연결 닫기
                connection.Close();
                // 스레드 종료
                StopThread();
            }
        }

        private void InsertToDatabase(SqlConnection connection)
        {
            // 아이템 이름과 값의 딕셔너리 생성
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            // 각 아이템의 값을 딕셔너리에 추가
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
            string insertQuery = "INSERT INTO t_In_reserve (PLT_IN_OUT, Job_Line, Serial_No, PLT_Number, PLT_TYPE, Car_Type, Item, Spec, LINE, Parts_count_int_pallet, Counts) " +
                                 "VALUES (@PLT_IN_OUT, @Job_Line, @Serial_No, @PLT_Number, @PLT_TYPE, @Car_Type, @Item, @Spec, @LINE, @Parts_count_int_pallet, @Counts)";

            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // 딕셔너리의 값들을 SQL 매개변수에 추가
            foreach (var kvp in itemValues)
            {
                cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
            }

            // 쿼리 실행
            cmdInsert.ExecuteNonQuery();
        }

        // 스레드 시작부분
        private void StartThread()
        {
            // ConnectToOPCServer 메서드를 실행하기 위한 새로운 스레드를 생성
            opcThread = new Thread(new ThreadStart(ConnectToOPCServer));
            // 생성한 스레드를 시작
            opcThread.Start();
        }

        // 스레드 종료 메서드
        private void StopThread()
        {
            // opcThread 값이 null 아니며 opcThread 가 동작중일 시
            if (opcThread != null && opcThread.IsAlive)
            { 
                // 스레드 종료 요청
                opcThread.Abort();
            }
        }

    }
}