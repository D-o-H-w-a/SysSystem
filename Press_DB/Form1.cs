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
        private Thread opcThread; // OPC 통신을 담당할 스레드
        private OPCServer opcServer; // OPC 서버와의 연결을 관린하는 객체
        private OPCGroup opcGroup; // OPC 서버로부터 데이터를 수집할 그룹
        private bool isRunning; // 스레드 실행 여부를 관리하는 플래그

        // MSSQL 연결 문자열
        //private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            IntializeOPC();
        }

        private void IntializeOPC()
        {
            opcServer = new OPCServer();
            opcServer.Connect("OPCServerIP");

            opcGroup = opcServer.OPCGroups.Add("OPCGroupName");

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