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
        // OPC 관련 변수 및 객체 선언
        private OPCServer opcServer;
        private OPCItems opcItems;
        private List<OPCItem> opcItemList = new List<OPCItem>();

        public Form1()
        {
            InitializeComponent();
            StartThread();
        }

        // 데이터베이스 연결 문자열
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        // OPC 서버 연결 및 데이터 수집 메서드
        private void ConnectToOPCServer()
        {
            // OPC 서버 연결
            opcServer = new OPCServer();
            opcServer.Connect("OPCServerIP");

            // OPC 아이템 추가 (예시)
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

            // 필요한 나머지 OPC 아이템 추가

            // 데이터베이스 연결
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // t_Cell 테이블에서 Cell_type 및 State 가져오기
                SqlCommand cmdCell = new SqlCommand("SELECT Cell_type, State FROM t_Cell", connection);
                SqlDataReader readerCell = cmdCell.ExecuteReader();

                while (readerCell.Read())
                {
                    int cellType = Convert.ToInt32(readerCell["Cell_type"]);
                    string state = readerCell["State"].ToString();

                    // t_SC_state 테이블에서 Stk_state 값 가져오기
                    SqlCommand cmdSCState = new SqlCommand("SELECT Stk_state FROM t_SC_state", connection);
                    SqlDataReader readerSCState = cmdSCState.ExecuteReader();

                    while (readerSCState.Read())
                    {
                        int stkState = Convert.ToInt32(readerSCState["Stk_state"]);
                        
                        int value = Convert.ToInt32(opcItemList.Find(item => item.ItemName == "Item"));
                        // 조건 검사 및 처리
                        // opcItemList 리스트의 원하는 인덱스를 사용하여 OPC 아이템 값에 접근하여 조건 검사
                        if (value < cellType && state == "Empty" && stkState == 0)
                        {
                            // In_reserve 값 저장
                            // 이 부분에서 In_reserve 값을 저장하는 방법에 따라 적절한 처리를 추가하세요.
                            // 예: SqlCommand를 사용하여 데이터베이스에 값을 삽입하는 방법 등
                        }
                    }

                    readerSCState.Close();
                }

                readerCell.Close();
                connection.Close();
            }

            // 스레드 종료
            // 필요한 경우 스레드 종료 처리 추가
        }

        private void InsertToDatabase(SqlConnection connection)
        {
            // 아이템 이름과 값의 딕셔너리 생성
            Dictionary<string, object> itemValues = new Dictionary<string, object>();

            // 각 아이템의 값을 딕셔너리에 추가
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
            Thread opcThread = new Thread(new ThreadStart(ConnectToOPCServer));
            opcThread.Start();
        }
    }
}