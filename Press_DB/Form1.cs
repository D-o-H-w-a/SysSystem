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
        private OPCServer opcServer; // OPC 서버 연결을 담당하는 객체
        private Thread opcThread; // OPC 통신을 위한 스레드
        private bool stopThread = false; // 스레드 멈추기 위한 플래그

        public Form1()
        {
            InitializeComponent();
            StartOPCThread();
        }

        private void ConnectToOPCServer()
        {
            try
            {
                // OPCServer 객체를 생성하여 OPC 서버에 연결할 준비
               // opcServer = new OPCServer();
                // OPC 서버에 연결
               // opcServer.Connect("OPCServerIP");

                // OPC Server 와 연결 성공 시 item 값 가져오기

                //OPCGroup 생성
               // OPCGroup opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                // OPCGroup에 대한 OPCItems 개체 생성
               // OPCItems opcItems = opcGroup.OPCItems;
                // 원하는 아이템 추가
                //OPCItem opcItem = opcItems.AddItem("YourItem", 1);

                // OPCItem의 값을 저장할 변수
                object value;
                // OPCItem에서 값 읽기
                //opcItem.Read(1, out value, out _, out _);
                value = 71112;
                // 데이터 베이스 연결 및 쿼리 실행
                // SqlConnection 개체 생성
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
                // 데이터베이스 연결 열기
                connection.Open();

                // t_Cell 테이블에서 데이터 가져오기
                SqlCommand command1 = new SqlCommand("SELECT * FROM t_Cell", connection); // t_Cell 테이블에서 모든 열 선택
                SqlDataReader reader1 = command1.ExecuteReader(); // 쿼리 실행 및 결과 읽기

                // 결과 행 반복
                while (reader1.Read())
                {
                    // Cell_type 과 State 값 가져오기
                    int cellType = (int)reader1["Cell_type"]; // Cell_type 열 값 가져오기
                    string state = (string)reader1["State"]; // State 열 값 가져오기

                    // t_SC_sate 테이블에서 Stk_no 컬럼 값들의 Stk_sate 값 가져오기
                    SqlCommand command2 = new SqlCommand("SELECT Stk_state FROM t_SC_state WHERE Stk_no = @StkNo", connection); // Stk_no에 해당하는 Stk_state 값 선택
                    command2.Parameters.AddWithValue("@StkNo", reader1["Stk_no"]); // 파라미터 설정
                    SqlDataReader reader2 = command2.ExecuteReader(); // 쿼리 실행 및 결과 읽기

                    while (reader2.Read()) // 결과 행 반복
                    {
                        int stkState = (int)reader2["Stk_state"]; // Stk_state 열 값 가져오기

                        // 조건 확인 후 ListView에 아이템 추가
                        if (Convert.ToInt32(value) <= cellType && state == "EMPTY" && stkState == 0)
                        {
                            // ListView에 추가할 아이템 생성 및 추가
                            ListViewItem listViewItem = new ListViewItem(reader1["Cell_type"].ToString()); // t_Cell 테이블에서 가져온 값 추가
                            listViewItem.SubItems.Add(reader2["Stk_state"].ToString()); // t_SC_sate 테이블에서 가져온 값 추가
                            // ListView에 추가
                            listView.Items.Add(listViewItem); // ListView에 아이템 추가

                            // t_Cell 테이블을 매게변수 삼아 함수 실행.
                            InsertToDatabase(reader1, connection, Convert.ToString(value));
                            break;
                        }
                        else
                        {
                            if(Convert.ToInt32(value) > cellType)
                            {
                                listView.Items.Add(cellType.ToString(),"Cell의 크기가 item 크기보다 작습니다");
                            }
                        }
                    }
                    // reader2 닫기
                    reader2.Close();
                }
                // reader1 닫기
                reader1.Close();

                // 데이터베이스 연결 닫기
                connection.Close();
            }

            // 예외 처리
            catch (Exception ex)
            {
                // 예외 메시지 표시
                MessageBox.Show("Error : " + ex.Message);
            }
        }
        
        private void StopOPCThread()
        {
            // 스레드 루프를 종료하기 위해 stopThread 변수를 true 로 설정
            stopThread = true;
            // 만약 opcThread가 null이 아니고 동작 중이라면
            //if (opcThread != null && opcThread.IsAlive)
            {
                // 스레드 종료
                StopOPCThread();
            }
        }

        private void StartOPCThread()
        {
            opcThread = new Thread(new ThreadStart(() =>
            {
                while (!stopThread)
                {
                    ConnectToOPCServer();
                    // 0.2초마다 실행
                    Thread.Sleep(200);
                }
            }));
            opcThread.Start();
        }



        private void InsertToDatabase(SqlDataReader reader, SqlConnection connection, string item)
        {
            // t_In_reserve 테이블에 t_Cell의 데이터 삽입
            string insertQuery = "INSERT INTO t_In_reserve (JobType, Cell, Pal_no, Pal_type, Model, Item, Spect, Line, Qty, Max_qty, Quailty, Prod_date, Prod_time, State, Pos, Udate, Utime) " +
                "VALUES (@JobType, @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spect, @Line, @Qty, @Max_qty, @Quailty, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";

            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@JobType", reader["JobType"]);
                insertCommand.Parameters.AddWithValue("@Cell", reader["Cell"]);
                insertCommand.Parameters.AddWithValue("@Pal_no", reader["Pal_no"]);
                insertCommand.Parameters.AddWithValue("@Pal_type", reader["Pal_type"]);
                insertCommand.Parameters.AddWithValue("@Model", reader["Model"]);
                insertCommand.Parameters.AddWithValue("@Item", item);
                insertCommand.Parameters.AddWithValue("@Spect", reader["Spect"]);
                insertCommand.Parameters.AddWithValue("@Line", reader["Line"]);
                insertCommand.Parameters.AddWithValue("@Qty", (int)reader["Qty"]); // int 형태의 Qty 값
                insertCommand.Parameters.AddWithValue("@Max_qty", (int)reader["Max_qty"]); // int 형태의 Max_qty 값
                insertCommand.Parameters.AddWithValue("@Quailty", reader["Quailty"]);
                insertCommand.Parameters.AddWithValue("@Prod_date", reader["Prod_date"]);
                insertCommand.Parameters.AddWithValue("@Prod_time", reader["Prod_time"]);
                insertCommand.Parameters.AddWithValue("@State", "INCOMP");
                insertCommand.Parameters.AddWithValue("@Pos", reader["Pos"]);
                insertCommand.Parameters.AddWithValue("@Udate", DateTime.Now.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@Utime", DateTime.Now.ToString("HH:mm:ss"));
            }
           
        }
    }
}