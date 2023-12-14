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

        // MSSQL 연결 문자열
        //private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

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
                opcServer = new OPCServer();
                // OPC 서버에 연결
                opcServer.Connect("OPCServerIP");

                // OPC Server 와 연결 성공 시 item 값 가져오기

                //OPCGroup 생성
                OPCGroup opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                // OPCGroup에 대한 OPCItems 개체 생성
                OPCItems opcItems = opcGroup.OPCItems;
                // 원하는 아이템 추가
                OPCItem opcItem = opcItems.AddItem("YourItem", 1);

                // OPCItem의 값을 저장할 변수
                object value;
                // OPCItem에서 값 읽기
                opcItem.Read(1, out value, out _, out _);

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
                        if (Convert.ToInt32(value) < cellType && state == "Empty" && stkState == 0)
                        {
                            // ListView에 추가할 아이템 생성 및 추가
                            ListViewItem listViewItem = new ListViewItem(reader1["Cell_type"].ToString()); // t_Cell 테이블에서 가져온 값 추가
                            listViewItem.SubItems.Add(reader2["Stk_state"].ToString()); // t_SC_sate 테이블에서 가져온 값 추가
                            // ListView에 추가
                            ItemView.Items.Add(listViewItem); // ListView에 아이템 추가
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
        /*
        private void Form1_Load(object sender, EventArgs e)
        {
            opcThread = new Thread(ConnectAndReadFromOPCServer); // OPC 통신을 담당하는 스레드 생성
            opcThread.Start(); // 생성한 스레드 시작
        }

        private void ConnectAndReadFromOPCServer()
        {
            try
            {
                // OPC Server 연결
                opcServer = new OPCServer();
                // OPC 서버 주소값을 받아 연결
                opcServer.Connect("OPCServerIP");

                //OPC 그룹(OPC 서버에서 관리하는 데이터 항목들의 모음) 설정
                opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
                //OPC 그룹 활성화 - 데이터 읽기/쓰기 가능
                opcGroup.IsActive = true;
                // Subscription 활성화 - 데이터 변경 시 클라이언트에 자동 업데이트
                opcGroup.IsSubscribed = true;
                // 업데이트 속도 (1초마다)
                opcGroup.UpdateRate = 1000;

                // OPC 그룹에 태그 추가
                OPCItems opcItems = opcGroup.OPCItems;
                // OPC 태그명 입력
                OPCItem opcItem = opcItems.AddItem("YourTagHere", 1);

                while (isRunning)
                {
                    // OPC 아이템에서 값 읽기

                    // 읽은 데이터의 값을 저장하는 배열 변수
                    object itemValue;

                    // 아이템 값 읽기
                    opcItem.Read((short)OPCDataSource.OPCDevice, out itemValue, out _, out _);

                    // cellType 의 값을 받을 변수
                    string cellType;
                    // State 컬럼의 값을 받을 변수
                    string state;
                    //SQL 쿼리 실행을 위한 Command 객체 생성
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        // 데이터베이스 연결 열기
                        connection.Open();
                        // 가져올 데이터베이스 테이블과 컬럼 지정 
                        string sqlQuery = "SELECT Cell_type,State, cranestate FROM t_Cell";


                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        
                        
                        if (Convert.ToInt32(itemValue) > sql)
                        {

                        }
                        // 데이터베이스 연결 끊기
                        connection.Close();
                    }
                }

                // ListView에 항목 추가
                Invoke(new Action(() =>
                {
                }));

                // 0.2초 대기 후 재요청
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("OPC 서버와의 통신 중 오류가 발생했습니다: " + ex.Message);
                }));
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false; // 스레드 종료를 위해 플래그 변경

            // 스레드가 완전히 종료될 때까지 대기
            if (opcThread != null && opcThread.IsAlive)
            {
                // opcThread 스레드가 완전히 종료될 때까지 대기합니다.
                opcThread.Join();
            }
        }
        */
    }
}