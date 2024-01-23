using OPCAutomation;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.AxHost;

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
        // tsc.Stk_no 의 최종 검색 값을 저장할 변수 선언
        private int searchValue;
        // [PLC->PC] 각각의 OPCItem 을 List 에 담아서 저장하기 위해 선언한 변수.
        private List<OPCItem> sendItem = new List<OPCItem>();
        // [PC->PLC] 각각의 OPCItem 을 List 에 담아서 저장하기 위해 선언한 변수.
        private List<OPCItem> receiveItem = new List<OPCItem>();
        // Data 들을 저장할 리스트 형태로 저장할 딕셔너리 변수.
        private Dictionary<string, object> itemValues = new Dictionary<string, object>();
        // 에러 메세지를 담고 있을 변수.
        private string errorMsg;
        /*
        // opc item 값을 담아올 List  변수 선언
        private List<OPCItem> opcItemList = new List<OPCItem>();
        */

        public Form1()
        {
            InitializeComponent();
            // 스레드 시작 메소드 호출
            StartThread();
        }

        // 데이터베이스 연결 문자열
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        // 스레드 시작부분
        private void StartThread()
        {
            try
            {
                // CancellationTokenSource 생성
                cancellationTokenSource = new CancellationTokenSource();

                // CancellationTokenSource에서 Token 가져오기
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                string opcGroupName = ConfigurationManager.AppSettings["OPCGroupName"];

                // OPC 서버 연결
                opcServer = new OPCServer();
                // OPC 서버에 연결
                opcServer.Connect("RSLinx OPC Server");

                // OPC 그룹 생성 및 설정
                opcGroups = opcServer.OPCGroups;

                // opc 서버에서 그룹을 관리하는 객체를 가져옴
                // 이름에 맞는 OPC 그룹을 생성
                opcGroup = opcGroups.Add("interface");
                // OPC 그룹을 활성화
                opcGroup.IsActive = true;
                // OPC 그룹을 구독 모드로 설정하여 실시간 데이터 수집
                opcGroup.IsSubscribed = true;
                // OPC 아이템들을 관리하는 객체를 가져옴
                opcItems = opcGroup.OPCItems;

                // OPC 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                OPCstateTxt.ForeColor = Color.Blue;

                // 스레드 시작
                opcThread = new Thread(() => opcServerJoin(cancellationToken));
                opcThread.Start();
            }

            catch (Exception ex)
            {
                // OPC 접속 에러가 날 시 텍스트 색상을 빨간색으로 변경하고 메세지 박스를 통해 에러를 표시합니다.
                OPCstateTxt.ForeColor = Color.Red;
                MessageBox.Show(ex.ToString());
            }
        }

        // opc 서버와 연결하여 통신을 하며 아이템 객체들을 가져옴
        private void opcServerJoin(CancellationToken cancellationToken)
        {
            //// opcItems.AddItem 을 while 문 밖으로 꺼내어 한번만 값을 생성하고 opcItemList 를 read 해서 매번 값을 가져온다.

            // 왼쪽 맵 읽기.

            // opcItems.AddItem 을 통해 OPC Item 을 읽어 온 뒤 opcItemList 에 불러온 OPCITEM 을 add.
            opcItems.AddItem("[interface]WMS_PLC.PLT_In_Out", 1);
            opcItems.AddItem("[interface]WMS_PLC.Job_Line", 1);
            opcItems.AddItem("[interface]WMS_PLC.Serial_No", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Number", 1);
            opcItems.AddItem("[interface]WMS_PLC.Parts_Count_In_Pallet", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Code", 1);

            // 오른쪽 맵 읽기.
            opcItems.AddItem("[interface]PLC_WMS.Job_Line", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_In_Out", 2);
            opcItems.AddItem("[interface]PLC_WMS.Serial_No", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_Number", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_Code", 2);
            opcItems.AddItem("[interface]PLC_WMS.Parts_Count_In_pallet", 2);

            //opcItems.AddItem("[interface]PLC_WMS.WH_LINE", 2);
            //opcItems.AddItem("[interface]PLC_WMS.Request_Check", 2);
            //opcItems.AddItem("[interface]PLC_WMS.NG Code", 2);

            // 스레드가 실행되고 있는 동안 반복
            while (!cancellationToken.IsCancellationRequested)
            {
                // plcToPC 있는 각 OPCItem에 대해 읽기를 반복합니다.
                foreach (OPCItem opcItem in opcItems)
                {
                    // 값을 저장할 변수들을 초기화합니다.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read를 사용하여 OPC 서버에서 데이터를 읽어옵니다.
                    opcItem.Read(1, out value, out quality, out timestamp);

                    // string 형태로 변환한 object quality 의 값이 "0"(Good) 일 때 sendItem List 형태의 OPCITEM 에 opcitem 저장.
                    if (quality.ToString() == "0")
                    {
                        sendItem.Add(opcItem);
                    }
                }

                //// 프레스 인터페이스 맵 오른쪽 읽기

                // pcToPLC 있는 각 OPCItem에 대해 읽기를 반복합니다.
                foreach (OPCItem opcItem in opcItems)
                {
                    // 값을 저장할 변수들을 초기화합니다.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read를 사용하여 OPC 서버에서 데이터를 읽어옵니다.
                    opcItem.Read(2, out value, out quality, out timestamp);

                    // string 형태로 변환한 object quality 의 값이 "0"(Good) 일 때 receiveItem List 형태의 OPCITEM 에 opcitem 저장.
                    if (quality.ToString() == "0")
                    {
                        receiveItem.Add(opcItem);
                    }
                }


                //// 왼쪽 테이블에 데이터가 존재하지 않을 때 오른쪽 테이블에 데이터가 있을 때 오른쪽 테이블을 지운다.
                foreach (OPCItem opcItem in receiveItem)
                {
                    // 왼쪽 테이블의 opcItem value 값을 object value 에 저장.
                    object receive = opcItem.Value;

                    // 왼쪽 테이블의 값을 오른쪽 테이블 ItemID를 이용해서 해당 값을 찾아옴. 오른쪽 테이블 ItemID 가 왼쪽 테이블에는 존재 하지 않는 ItemID 면 null 값을 받아옴.
                    object send = sendItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // value2 값이 null 이 아닐 때.
                    if (send != null)
                    {
                        // 왼쪽 테이블 value2 의 item 값이 존재하지 않고 오른쪽 테이블 value 의 Item값이 존재할 때. 
                        if (send.ToString() == "" && receive.ToString() != send.ToString())
                        {
                            // opcItem.Write 를 이용해서 value2 값으로 초기화 시켜준다.
                            opcItem.Write("");
                        }
                    }
                }

                //// 조건체크 함수 만들기
                ///// 1. 입고 조건
                ///
                ///// string 형태 pltINout 변수에 opcItemList 에서 PLT_IN_OUT 키의 값을 callNum 에 전달
                string pltINout = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out")?.Value;

                //PLT_IN_OUT 요청 번호를 받을 int 형 변수 callNum
                // callNum 값이 1 일시 입고 함수 처리
                if (int.Parse(pltINout) == 1)
                {
                    if (sendData(cancellationToken))
                    {

                        if (Inreserve())
                        {
                            if (sendTalbe())
                            {

                            }
                            else
                            {
                                ///실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                        else
                        {
                            //// 인설트 실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                            ///
                            // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                            SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                    }
                    else
                    {
                        // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                        SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    }
                }

                //// 1.1 PLT_IN_OUT 이 1번일 때 이고 오른쪽 테이블 값이 존재하지 않을 때
                //// 2. 입고 조건이 맞으면 크레인 호기 와 상태 셀 상태 조회 후 In_reserve 테이블에 Insert 하기
                //// 3. Insert 를 성공하면 오른쪽 테이블에 Write 해주기.

                ///// 2. 출고 조건
                ///
                // callNum 값이 2 일시 출고 함수 처리
                else if (int.Parse(pltINout) == 2)
                {
                    if (receiveData(cancellationToken))
                    {

                        if (OutReserve())
                        {
                            if (sendTalbe())
                            {

                            }
                            else
                            {
                                ///실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                        else
                        {
                            //// 인설트 실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                            ///
                            // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                            SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                    }
                    else
                    {
                        // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                        SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    }
                }
                //// 2.1 PLT_IN_OUT 이 2번일 때 이고 오른쪽 테이블 값이 존재하지 않을 때
                //// 2. 출고 조건이 맞으면 크레인 호기 와 상태 셀 상태 조회 후 Out_reserve 테이블에 Insert 하기
                //// 3. Insert 를 성공하면 오른쪽 테이블에 Write 해주기.




                ///// 3. 로그 남기기, 그리드 화면 지우기




                Thread.Sleep(200);
            }
        }

        // OPC 서버 연결 및 데이터 수집 메서드
        private bool sendData(CancellationToken cancellationToken)
        {
            // connectionString 변수에 저장된 데이터베이스 주소를 통해서 연결
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // 데이터베이스 접속
                    connection.Open();

                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;

                    //// 스테커 상태를 전체 다 읽어온다.
                    ///

                    for (int i = 0; i < 8; i++)
                    {
                        searchValue = searchValue - 1;

                        if (searchValue <= 0)
                            searchValue = 8;

                        // Stk_no 는 @searchValue 번째 값이며 해당 값의 위치에 존재하는 Stk_state 값을 들고올것임.
                        string query = "SELECT Stk_state FROM t_SC_state WHERE Stk_no = @searchValue";

                        // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다.
                        SqlCommand command = new SqlCommand(query, connection);
                        // @searchValue 매개변수에 값을 할당합니다.
                        command.Parameters.AddWithValue("@searchValue", searchValue);
                        // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                        SqlDataReader reader = command.ExecuteReader();
                        // 가져온 결과 값을 읽어옵니다.
                        if (reader.Read())
                        {
                            // 가져온 Stk_state 의 값을 int형태로 변환하여 stkState 값에 대입합니다.
                            int stkState = Convert.ToInt32(reader["Stk_state"]);
                            // stkState 가 0(정상) 인 조건이 성립 시.
                            if (stkState == 0)
                            {
                                // reader.Close 를 통하여 읽고있는 쿼리문을 닫습니다.
                                reader.Close();

                                // t_Cell 의 Level 오름차순을 기준으로 Bank 값이 searchValue x 2 한 값 또는 2를 곱한 후 1을 뺀 값 그리고 PLT_CODE 앞자리 번호가 동일하며
                                // t_Cell 의 State 가 'EMPTY' 이고 In_reserve 에 입고 대기 중인 Cell 이 아닌 tc.PLT_CODE, tc.State, tc.Cell 을 찾아옵니다.
                                query = @"
                                    SELECT TOP 1 tc.Pal_code, tc.State, tc.Cell
                                    FROM t_Cell tc
                                    LEFT JOIN t_In_reserve tr ON tc.Cell = tr.Cell
                                    WHERE 
                                        (tc.Bank = @searchValue * 2 OR tc.Bank = @searchValue * 2 - 1)
                                        AND tc.LEFT(tc.Pal_code, 1) = @codeFirstChar
                                        AND tc.State = 'EMPTY'
                                        AND tr.Cell IS NULL
                                    ORDER BY tc.Level ASC;
                                    ";

                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code").ToString();
                                char codeFirstChar = code.FirstOrDefault();

                                // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar 와 @searchValue 매개변수에 값을 할당합니다.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@codeFirstChar", codeFirstChar);

                                // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                                reader = command.ExecuteReader();
                                // 가져온 결과 값을 읽어옵니다.
                                if (reader.Read())
                                {
                                    // 읽어 온 값이 있을 때 접근 합니다.
                                    if(reader.FieldCount > 0)
                                    {
                                        // itemValues 딕셔너리의 Cell 키의 Value 값에 쿼리문에서 가져온 Cell 을 string 형태로 형변환을 거친 후 대입합니다.
                                        itemValues["Cell"] = reader["Cell"].ToString();
                                        // 쿼리문 읽기를 종료합니다.
                                        reader.Close();
                                        break;
                                    }
                                }
                            }
                        }
                        /// 마지막 호기가 6호기 일 때 5호기 부터 조회를 한다.
                        /// 호기 x 2 와 호기 x 2 - 1 값을 계산한다.


                        /// 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장 후 break.
                        /// 
                    }

                    // 데이터 베이스에 접속 종료.
                    connection.Close();
                    // SQL 연결이 종료 되었다면 텍스트 색상을 검정색으로 변경
                    SQLstateTxt.ForeColor = Color.Black;
                    // 정상적으로 결과를 가져왔다면 return true 를 반환하여 함수를 종료합니다.
                    return true;
                }

                catch (Exception ex)
                {
                    // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트의 색상을 빨간색으로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                    SQLstateTxt.ForeColor = Color.Red;
                    errorMsg = ex.ToString();
                    // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
                    return false;
                }
            }
        }

        // 폼 종료시 작동되는 함수
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // 스레드 종료
            StopThread();
        }

        private bool receiveData(CancellationToken cancellationToken)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // 데이터베이스 접속
                    connection.Open();

                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;

                    //// 스테커 상태를 전체 다 읽어온다.
                    ///

                    for (int i = 0; i < 8; i++)
                    {
                        searchValue = searchValue - 1;

                        if (searchValue <= 0)
                            searchValue = 8;

                        // Stk_no 는 @searchValue 번째 값이며 해당 값의 위치에 존재하는 Stk_state 값을 들고올것임.
                        string query = "SELECT Stk_state FROM t_SC_state WHERE Stk_no = @searchValue";

                        // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다.
                        SqlCommand command = new SqlCommand(query, connection);
                        // @searchValue 매개변수에 값을 할당합니다.
                        command.Parameters.AddWithValue("@searchValue", searchValue);
                        // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                        SqlDataReader reader = command.ExecuteReader();
                        // 가져온 결과 값을 읽어옵니다.
                        if (reader.Read())
                        {
                            // 가져온 Stk_state 의 값을 int형태로 변환하여 stkState 값에 대입합니다.
                            int stkState = Convert.ToInt32(reader["Stk_state"]);
                            // stkState 가 0(정상) 인 조건이 성립 시.
                            if (stkState == 0)
                            {
                                // reader.Close 를 통하여 읽고있는 쿼리문을 닫습니다.
                                reader.Close();
                         
                                // t_Cell 의 Level 오름차순을 기준으로 Bank 값이 searchValue x 2 한 값 또는 2를 곱한 후 1을 뺀 값 그리고 PLT_CODE 앞자리 번호가 동일하며
                                // t_Cell 의 State 가 'EMPTY' 이고 In_reserve 에 입고 대기 중인 Cell 이 아닌 tc.PLT_CODE, tc.State, tc.Cell 을 찾아옵니다.
                                query = @"
                                    SELECT TOP 1 tc.Pal_code, tc.State, tc.Cell,tc.Pal_no,tc.Pal_type,tc.Model,tc.Spec, tc.Line,
                                    tc.Qty,tc.Max_qty,tc.Quality,tc.Prod_date,tc.Prod_time,tc.Prod_time,tc.Pos
                                    FROM t_Cell tc
                                    LEFT JOIN t_Out_reserve tr ON tc.Cell = tr.Cell
                                    WHERE 
                                        (tc.Bank = @searchValue * 2 OR tc.Bank = @searchValue * 2 - 1)
                                        AND tc.Pal_code = @code
                                        AND tc.State = 'INCOMP'
                                        AND tr.Cell IS NULL
                                    ORDER BY tc.Level ASC;
                                    ";

                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code").ToString();

                                // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar 와 @searchValue 매개변수에 값을 할당합니다.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);

                                // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                                reader = command.ExecuteReader();
                                // 가져온 결과 값을 읽어옵니다.
                                if (reader.Read())
                                {
                                    // 읽어 온 값이 있을 때 접근 합니다.
                                    if (reader.FieldCount > 0)
                                    {
                                        // itemValues 딕셔너리 키의 Value 값에 쿼리문에서 가져온 값 을 필요한 형태로 형변환을거친 후 대입합니다.
                                        itemValues["Cell"] = reader["Cell"].ToString();
                                        itemValues["Pal_code"] = code;
                                        itemValues["State"] = "OUTCOMP";
                                        itemValues["Pal_no"] = reader["Pal_no"].ToString();
                                        itemValues["Pal_type"] = reader["Pal_type"].ToString();
                                        itemValues["Model"] = reader["Model"].ToString();
                                        itemValues["Spec"] = reader["Spec"].ToString();
                                        itemValues["Line"] = reader["Line"].ToString();
                                        itemValues["Qty"] = Convert.ToInt32(reader["Qty"]);
                                        itemValues["Max_qty"] = Convert.ToInt32(reader["Max_qty"]);
                                        itemValues["Quality"] = reader["Quality"].ToString();
                                        itemValues["Prod_date"] = reader["Prod_date"].ToString();
                                        itemValues["Prod_time"] = reader["Prod_time"].ToString();
                                        itemValues["Pos"] = reader["Pos"].ToString();
                                        itemValues["Serial_no"] = Convert.ToDouble(reader["Serial_no"]);
                                        itemValues["JobType"] = "OUTAUTO";
                                        itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                                        itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");
                                        // 쿼리문 읽기를 종료합니다.
                                        reader.Close();
                                        break;
                                    }
                                }
                            }
                        }
                        /// 마지막 호기가 6호기 일 때 5호기 부터 조회를 한다.
                        /// 호기 x 2 와 호기 x 2 - 1 값을 계산한다.


                        /// 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장 후 break.
                        /// 
                    }

                    // 데이터 베이스에 접속 종료.
                    connection.Close();
                    // SQL 연결이 종료 되었다면 텍스트 색상을 검정색으로 변경
                    SQLstateTxt.ForeColor = Color.Black;
                    // 정상적으로 결과를 가져왔다면 return true 를 반환하여 함수를 종료합니다.
                    return true;
                }

                catch (Exception ex)
                {
                    // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트의 색상을 빨간색으로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                    SQLstateTxt.ForeColor = Color.Red;
                    errorMsg = ex.ToString();
                    // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
                    return false;
                }
            }
        }

        // In_rserve 테이블에 데이터 삽입을 위한 함수 
        private bool Inreserve()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // SQL 에 접속.
                    connection.Open();
                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;

                    // 각각의 키 값이 가진 Value 값에 sendItem 에서 item Value 값을 찾아 대입.
                    itemValues["Job_Line"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value;
                    itemValues["Serial_No"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value;
                    itemValues["Pal_no"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Number")?.Value;
                    itemValues["Qty"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Parts_Count_In_Pallet")?.Value;
                    itemValues["Pal_code"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value;
                    itemValues["JobType"] = "INAUTO";
                    itemValues["State"] = "INCOMP";
                    itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                    itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");

                    // 삽입 쿼리문 정의
                    string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, '', '', '', '', '', @Qty '', '', '', '', @State, '', @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";

                    // SqlCommand 객체 생성 및 쿼리문 설정
                    SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                    // SqlCommand 객체 생성 및 쿼리문 설정
                    foreach (var kvp in itemValues)
                    {
                        // 만약 매개변수가 이미 추가되지 않았다면
                        if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                        {
                            // 매개변수에 추가할 값 설정 (null이 아니면 해당 값, null이면 공백 문자열로 설정)
                            object valueToInsert = kvp.Value != null ? kvp.Value : " ";

                            // SqlCommand의 Parameters 컬렉션에 새로운 매개변수 추가
                            cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                        }
                    }

                    // 쿼리 실행
                    cmdInsert.ExecuteNonQuery();
                    // 데이터베이스 연결 닫기
                    connection.Close();
                    // SQL 연결이 종료 되었다면 텍스트 색상을 검정색으로 변경
                    SQLstateTxt.ForeColor = Color.Black;
                }

                // 정상적으로 데이터를 저장하였다면 return 값을 true 로 반환.
                return true;
            }

            catch(Exception ex)
            {
                // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트의 색상을 빨간색으로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                SQLstateTxt.ForeColor = Color.Red;
                errorMsg = ex.ToString();
                return false;
            }
        }
        // Out_reserve 에 데이터 삽입 하기 위한 함수
        private bool OutReserve()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // SQL 에 접속.
                    connection.Open();
                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;

                    // 삽입 쿼리문 정의
                    string insertQuery = "INSERT INTO t_Out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @PLT_Number, @Pal_type, @Model,'' , @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Pal_code, @Serial_no, @Job_Line, @Udate, @Utime)";

                    // SqlCommand 객체 생성 및 쿼리문 설정
                    SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                    // SqlCommand 객체 생성 및 쿼리문 설정
                    foreach (var kvp in itemValues)
                    {
                        // 만약 매개변수가 이미 추가되지 않았다면
                        if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                        {
                            // 매개변수에 추가할 값 설정 (null이 아니면 해당 값, null이면 공백 문자열로 설정)
                            object valueToInsert = kvp.Value != null ? kvp.Value : " ";

                            // SqlCommand의 Parameters 컬렉션에 새로운 매개변수 추가
                            cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                        }
                    }

                    // 쿼리 실행
                    cmdInsert.ExecuteNonQuery();
                    // 데이터베이스 연결 닫기
                    connection.Close();
                    // SQL 연결이 종료 되었다면 텍스트 색상을 검정색으로 변경
                    SQLstateTxt.ForeColor = Color.Black;
                }

                // 정상적으로 데이터를 저장하였다면 return 값을 true 로 반환.
                return true;
            }

            catch (Exception ex)
            {
                // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트의 색상을 빨간색으로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                SQLstateTxt.ForeColor = Color.Red;
                errorMsg = ex.ToString();
                return false;
            }
        }

        // send 테이블에 읽은 값을 receive 테이블에 써주는 함수.
        private bool sendTalbe()
        {
            try
            {
                // OPCItem 컬렉션에서 각 항목에 대한 루프
                foreach (OPCItem opcItem in sendItem)
                {
                    // 현재 OPCItem의 값 가져오기
                    object send = opcItem.Value;

                    // receiveItem 컬렉션에서 현재 OPCItem과 동일한 ItemID를 가진 항목 찾기
                    object receive = receiveItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // 만약 receive 값이 존재하고, 값이 비어있고 send 값과 다른 경우
                    if (receive != null)
                    {
                        if (receive.ToString() == "" && send.ToString() != receive.ToString())
                        {
                            receiveItem.Find(item => item.ItemID == opcItem.ItemID)?.Write(send);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
                return false;
            }
        }

        private void SendErrorMsg(string errorMsg, string errorDate, string errorTime)
        {
            // UI 스레드가 아닌 경우, UI 스레드에서 작업하도록 요청
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate
                {
                    // 데이터 그리드에 새로운 행 추가
                    dataGrid.Rows.Add(errorMsg, errorDate, errorTime);
                });
            }
            else // UI 스레드인 경우, 직접 작업 수행
            {
                // 데이터 그리드에 새로운 행 추가
                dataGrid.Rows.Add(errorMsg, errorDate, errorTime);
            }
            
            // 에러메세지 로그 저장 함수호출.
            ErrorMsgAddLog(false);
        }

        // 그리드에 적힌 에러메세지를 로그로 저장하는 함수.
        private void ErrorMsgAddLog(bool gridClear)
        {
            // 로그 파일 경로 설정
            string logFilePath = "C:\\Path\\To\\ErrorMsg.txt";

            try
            {
                // StreamWriter를 사용하여 파일에 로그 쓰기
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    // DataGridView의 각 행과 열을 순회하며 로그에 기록
                    foreach (DataGridViewRow row in dataGrid.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            sw.Write(cell.Value.ToString() + "\t");
                        }
                        sw.WriteLine(); // 각 행의 끝에 줄 바꿈 추가
                    }

                    // 로그 작성이 완료되면 메시지 출력
                    MessageBox.Show("DataGridView contents saved to log file.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if(gridClear == true)
                {
                    // DataGridView의 모든 행을 제거
                    dataGrid.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                // 로그 작성 중 오류 발생 시 예외 처리
                MessageBox.Show("Error writing to log file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void outBtn_Click(object sender, EventArgs e)
        {
            //StartThread(2);
        }

        private void inBtn_Click(object sender, EventArgs e)
        {
            //StartThread(1);
        }
    }
}