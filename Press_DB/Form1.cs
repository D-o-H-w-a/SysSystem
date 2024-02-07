using OPCAutomation;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using WinRT;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

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
        // Cell_code 를 key 로 Pal_type 을 값으로 가지고 있을 Dictionary 변수 선언.
        private Dictionary<string, string> palType = new Dictionary<string, string>();
        // 구해온 CellType 들을 저장할 리스트 변수 선언.
        private List<string> cellType = new List<string>();
        // 구해온 cell 번호들을 확인할 리스트 변수 선언.
        private List<string> cell = new List<string>();

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

                // OPC 에 정상 연결되었다면 텍스트의 메세지를 PLC Connect 로 변경.
                ShowText(2,"PLC Connect");

                // 스레드 시작
                opcThread = new Thread(() => opcServerJoin(cancellationToken));
                opcThread.Start();
            }
 
            catch (Exception ex)
            {
                // OPC 접속 에러가 날 시 텍스트의 메세지를 PLC Connect Error 로 변경하고 메세지 박스를 통해 에러를 표시합니다.
                ShowText(2, "PLC Connect Error");
                errorMsg = ex.ToString();
            }
        }

        // opc 서버와 연결하여 통신을 하며 아이템 객체들을 가져옴
        private void opcServerJoin(CancellationToken cancellationToken)
        {
            //// opcItems.AddItem 을 while 문 밖으로 꺼내어 한번만 값을 생성하고 opcItemList 를 read 해서 매번 값을 가져온다.

            // 왼쪽 맵 읽기.

            // opcItems.AddItem 을 통해 OPC Item 을 읽어 온 뒤 opcItemList 에 불러온 OPCITEM 을 add.
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_In_Out", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Job_Line", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Serial_No", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_Number", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Parts_Count_In_Pallet", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_Code", 1));

            // 오른쪽 맵 읽기.
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.Job_Line", 2));
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.PLT_In_Out", 2));
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.Serial_No", 2));
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.PLT_Number", 2));
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.PLT_Code", 2));
            receiveItem.Add(opcItems.AddItem("[interface]PLC_WMS.Parts_Count_In_Pallet", 2));

            receiveItem.Add(opcItems.AddItem("[interface]ACS1_WH_01_01.AGV_Call_PLT_Out_Adc", 2)); // WH_LINE
            receiveItem.Add(opcItems.AddItem("[interface]ACS1_WH_01_01.AGV_Call_PLT_Out_Rework", 2)); //Request_Check
            receiveItem.Add(opcItems.AddItem("[interface]ACS1_WH_01_01.AGV_Lift_Down_Status", 2)); // NG Code
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 데이터베이스 에 접속.
                    connection.Open();
                    // SQL 에 정상 연결되었다면 DBstateTxt 의 Text 를 DB Connect 로 변경.
                    ShowText(1, "DB Connect");

                    // t_Pal_type 테이블에 있는 Cell_code 와 Pal_type 을 전부 들고옴.
                    string query = "SELECT Cell_code, Pal_type FROM t_Pal_type";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // 데이터 읽기 시작.
                            while (reader.Read())
                            {
                                // 읽은 Cell_code 를 cellCode 에 대입.
                                string cellCode = reader["Cell_code"].ToString();
                                // 읽은 Pal_type 을 pal_type 에 대입.
                                string pal_type = reader["Pal_type"].ToString();

                                // cellCode 를 키로 pal_type 을 변수로 해서 딕셔너리에 저장.
                                palType[cellCode] = pal_type;
                            }

                            //쿼리문 종료
                            reader.Close();
                        }

                        // 데이터 베이스에 접속 종료.
                        connection.Close();
                        // SQL 연결이 종료 되었다면 텍스트를 "DB Disconnect" 로 변경
                        ShowText(1, "DB Disconnect");
                    }
                }
            }
            catch (Exception ex)
            {
                // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트를 DB Connect Error 로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
            }

            // 스레드가 실행되고 있는 동안 반복
            while (!cancellationToken.IsCancellationRequested)
            {
                // plcToPC 있는 각 OPCItem에 대해 읽기를 반복합니다.
                foreach (OPCItem opcItem in sendItem)
                {
                    // 값을 저장할 변수들을 초기화합니다.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read를 사용하여 OPC 서버에서 데이터를 읽어옵니다.
                    opcItem.Read(1, out value, out quality, out timestamp);
                }

                //// 프레스 인터페이스 맵 오른쪽 읽기

                // pcToPLC 있는 각 OPCItem에 대해 읽기를 반복합니다.
                foreach (OPCItem opcItem in receiveItem)
                {
                    // 값을 저장할 변수들을 초기화합니다.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read를 사용하여 OPC 서버에서 데이터를 읽어옵니다.
                    opcItem.Read(2, out value, out quality, out timestamp);
                }


                //// 왼쪽 테이블에 PLT_In_Out 데이터가 0 일 때 오른쪽 테이블에 데이터가 있으면 오른쪽 테이블을 지운다.
                object send = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out")?.Value;
                if(send.ToString() == "0")
                {
                    Thread.Sleep(1000);

                    for (int i = 0; i < receiveItem.Count; i++)
                    {
                        if (receiveItem[i].Value.ToString() != "0")
                        {
                            receiveItem[i].Write("0");
                        }
                    }
                }

                //// 조건체크 함수 만들기
                ///// 1. 입고 조건
                ///
                ///// string 형태 pltINout 변수에 opcItemList 에서 PLT_IN_OUT 키의 값을 callNum 에 전달

                int pltInOut = Convert.ToInt32(receiveItem.Find(item => item.ItemID == "[interface]PLC_WMS.PLT_In_Out").Value);

                // 값이 존재하는지 확인을 하기 위해서 int 형태로 값을 가져올 변수들 생성.
                int pltINout = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out")?.Value);
                int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);
                int serialNo = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value);
                int pltNumber = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Number")?.Value);
                int palletNum = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Parts_Count_In_Pallet")?.Value);
                int plt_code = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value);

                //PLT_IN_OUT 요청 번호를 받을 int 형 변수 callNum
                // callNum 값이 1 일시 
                // pltInOut의 0 이고 나머지 값들에 값이 0이 아닌 다른 값이 있을 시 입고 함수 처리
                if (pltINout == 1 && pltInOut == 0 && jobLine != 0 && serialNo != 0 && pltNumber != 0 && palletNum != 0 && plt_code != 0)
                {
                    // 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장하는 함수.
                    if (sendData() == 1)
                    {
                        // 데이터에 문제있는지 확인하고 있으면 NG Code 값을 주고 Request_Check 값을 2[ng]로
                        // 없으면 Request_Check 값을 1[ok] 로 줄 함수.
                        if (errorCheck() == 1)
                        {
                            // sendData 함수에서 가져온 cell 정보를 토대로 왼쪽 테이블 값을 In_reserve 에 데이터 삽입 하기 위한 함수
                            if (Inreserve())
                            {
                                // 왼쪽 테이블 데이터를 오른쪽에 써주기 위한 함수
                                if (!sendTable())
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
                        // 에러가 발생 시.
                        else if (errorCheck() == -1)
                        {
                            // 왼쪽 테이블 값을 오른쪽에 써줌.
                            if (!sendTable())
                            {
                                //// 인설트 실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                                ///
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                            else
                            {
                                //// errorCheck 에서 데이터 검사 과정 중 문제가 있으면 그리드에 메세지 출력. 로그에 메세지 저장.
                                ///
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                    }
                    else if (sendData() == -1)
                    {
                        continue;
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
                // callNum 값이 2 일시
                // pltInOut의 0 이고 나머지 값들에 값이 0이 아닌 다른 값이 있을 시 출고 함수 처리
                else if (pltINout == 2 && pltInOut.ToString() == "0" && jobLine != 0 && serialNo != 0 && pltNumber != 0 && palletNum != 0 && plt_code != 0)
                {
                    if (receiveData() == 1)
                    {
                        // 데이터에 문제있는지 확인하고 있으면 NG Code 값을 주고 Request_Check 값을 2[ng]로
                        // 없으면 Request_Check 값을 1[ok] 로 줄 함수.
                        if (errorCheck() == 1)
                        {
                            // 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장하는 함수.
                            if (OutReserve())
                            {
                                // sendData 함수에서 가져온 cell 정보를 토대로 왼쪽 테이블 값을 In_reserve 에 데이터 삽입 하기 위한 함수
                                if (!sendTable())
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
                        // 에러 발생시.
                        else if (errorCheck() == -1)
                        {
                            // 왼쪽 테이블 값을 오른쪽에 써줌.
                            if (!sendTable())
                            {
                                //// 인설트 실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                                ///
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                            else
                            {
                                // 에러 메세지, 발생 날짜, 발생 시각을 매게변수 삼아 함수 호출.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                    }
                    else if (receiveData() == -1)
                    {
                        continue;
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

                Thread.Sleep(200);
                ///// 3. 로그 남기기, 그리드 화면 지우기
            }
        }

        // OPC 서버 연결 및 데이터 수집 메서드
        private int sendData()
        {
            // connectionString 변수에 저장된 데이터베이스 주소를 통해서 연결
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    itemValues.Clear();
                    // 데이터베이스 접속
                    connection.Open();

                    // SQL 에 정상 연결되었다면 DBstateTxt 의 Text 를 DB Connect 로 변경.
                    ShowText(1, "DB Connect");
                    //// 스테커 상태를 전체 다 읽어온다.
                    ///

                    // 전체 크레인의 stkState가 1인지 확인하는 플래그
                    bool allStkStateOne = true;
                
                    // cell 과 cellType 이 가진 값들 초기화.
                    cell.Clear();
                    cellType.Clear();

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
                                // 하나라도 Stk_state가 0이면 allStkStateOne 변수값을 false로 설정
                                allStkStateOne = false;
                                // reader.Close 를 통하여 읽고있는 쿼리문을 닫습니다.
                                reader.Close();

                                // t_Cell 의 Level 오름차순을 기준으로 Bank 값이 searchValue x 2 한 값 또는 2를 곱한 후 1을 뺀 값 그리고 PLT_CODE 앞자리 번호가 동일하며
                                // t_Cell 의 State 가 'EMPTY' 이고 In_reserve 에 입고 대기 중인 Cell 이 아닌 tc.PLT_CODE, tc.State, tc.Cell 을 찾아옵니다.
                                query = @"
                                    SELECT tc.State, tc.Cell, tc.Bank, tc.Cell_type
                                    FROM t_Cell tc
                                    LEFT JOIN t_In_reserve tr ON tc.Cell = tr.Cell
                                    WHERE 
                                        (tc.Bank = @searchValue * 2 OR tc.Bank = @searchValue * 2 - 1)
                                        AND tc.State = 'EMPTY'
                                        AND tr.Cell IS NULL
                                    ORDER BY tc.Level ASC;
                                    ";

                                // OPC 통신을 통해서 Read 하여 가져온 PLT_Code 값을 code 에 스트링형태로 대입. 
                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value.ToString();
                                // PLT_Code 의 앞 두자리 가져오기.
                                string cell_code = code?.Length >= 2 ? code.Substring(0, 2) : null;
                                int pal_type = 0;
                                string pal_typ = "";
                                int bank = 0;
                                if (!string.IsNullOrEmpty(cell_code))
                                {
                                    pal_type = int.Parse(palType[cell_code]);
                                    pal_typ = palType[cell_code];
                                }
                                //char codeFirstChar = code.FirstOrDefault();

                                // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar 와 @searchValue 매개변수에 값을 할당합니다.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);
                                //command.Parameters.AddWithValue("@codeFirstChar", codeFirstChar);
                                // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                                using (SqlDataReader innerreader = command.ExecuteReader())
                                {
                                    // 가져온 결과 값을 읽어옵니다.
                                    while (innerreader.Read())
                                    {
                                        // 읽어 온 값이 있을 때 접근 합니다.
                                        if (innerreader.FieldCount > 0)
                                        {
                                            if(bank <= 0)
                                            {
                                               bank = Convert.ToInt32(innerreader["Bank"].ToString());
                                            }
                                            // 읽은 Cell_type 이 Null 이 아니고 값이 존재하면.
                                            if (!string.IsNullOrEmpty(innerreader["Cell_type"].ToString()))
                                            {
                                                // cellType 리스트에 Cell_type 추가.
                                                cellType.Add(innerreader["Cell_type"].ToString());
                                                // cell 에 cell 번호 추가.
                                                cell.Add(innerreader["Cell"].ToString());
                                            }
                                        }
                                    }
                                    // 쿼리문 읽기를 종료합니다.
                                    innerreader.Close();

                                    // PLC 통해서 읽어온 잡라인을 대입 int 형태로 대입.
                                    int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);

                                    // JobLine 이 위치에 따른 조건이 맞을 때 구해온 크레인 번호 옆에 + 1 또는 + 4를 붙여서 값을 Pos 에 표기.
                                    if (jobLine == 201 || jobLine == 202 || jobLine == 301)
                                    {
                                        itemValues["Pos"] = "1" + "-" + bank;
                                    }
                                    else if (jobLine == 401 || jobLine == 402 || jobLine == 403 ||
                                         jobLine == 404 || jobLine == 405 || jobLine == 406 || jobLine >= 501)
                                    {
                                        itemValues["Pos"] = "2" + "-" + bank;
                                    }
                                    
                                    // cellType 의 Count 만큼 반복.
                                    for(int j = 0; j < cellType.Count; j++)
                                    {
                                        // value 에 cellType 의 j 번 값을 정수형으로 value에 선언.
                                        int value = int.Parse(cellType[j]);

                                        // 팔레트 타입이 3730이고 pal_type 과 value 값이 동일 시할 때.
                                        if (pal_type == 3730 && pal_type == value)
                                        {
                                            // itemValues 딕셔너리의 Cell 키의 Value 값에 j 번 째 Cell 을 대입합니다.
                                            itemValues["Cell"] = cell[j];
                                            // i 를 8로 값을 줘서 첫 for문을 빠져나가도록 함.
                                            i = 8;
                                            // j for 문을 종료.
                                            break;
                                        }
                                        else if (pal_type != 3730 && cellType.Contains(pal_typ) == true && value != 3730)
                                        {
                                            if (value == pal_type)
                                            {
                                                // itemValues 딕셔너리의 Cell 키의 Value 값에 j 번 째 Cell 을 대입합니다.
                                                itemValues["Cell"] = cell[j];
                                                // i 를 8로 값을 줘서 첫 for문을 빠져나가도록 함.
                                                i = 8;
                                                // j for 문을 종료.
                                                break;
                                            }
                                        }
                                        else if(pal_type != 3730 && cellType.Contains(pal_typ) == false && value != 3730)
                                        {
                                            if(value >= pal_type)
                                            {
                                                // itemValues 딕셔너리의 Cell 키의 Value 값에 j 번 째 Cell 을 대입합니다.
                                                itemValues["Cell"] = cell[j];
                                                // i 를 8로 값을 줘서 첫 for문을 빠져나가도록 함.
                                                i = 8;
                                                // j for 문을 종료.
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // 쿼리문 읽기를 종료합니다.
                                reader.Close();
                            }
                        }
                        /// 마지막 호기가 6호기 일 때 5호기 부터 조회를 한다.
                        /// 호기 x 2 와 호기 x 2 - 1 값을 계산한다.


                        /// 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장 후 break.
                        /// 
                    }

                    ////// 재고가 없을 시 NG Code 1 입력.
                    ////// 제품 정보 이상일 시 Code 2 입력.
                    ////// 데이터 이상 일 시 [시리얼 넘버 9자리가 아니거나 없거나] Code 3 입력.

                    // 데이터 베이스에 접속 종료.
                    connection.Close();

                    if (allStkStateOne == true)
                        return -1;


                    // SQL 연결이 종료 되었다면 텍스트를 "DB Disconnect" 로 변경
                    ShowText(1, "DB Disconnect");
                    // 정상적으로 결과를 가져왔다면 return true 를 반환하여 함수를 종료합니다.
                    return 1;
                }

                catch (Exception ex)
                {
                    // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트를 DB Connect Error 로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                    ShowText(1, "DB Connect Error");
                    errorMsg = ex.ToString();
                    // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
                    return 2;
                }
            }
        }

        // 폼 종료시 작동되는 함수
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // 스레드 종료
            StopThread();
        }

        private int receiveData()
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    itemValues.Clear();
                    // 데이터베이스 접속
                    connection.Open();

                    // SQL 에 정상 연결되었다면 DBstateTxt 의 Text 를 DB Connect 로 변경.
                    ShowText(1, "DB Connect");

                    // 전체 크레인의 stkState가 1인지 확인하는 플래그
                    bool allStkStateOne = true;
                    //// 스테커 상태를 전체 다 읽어온다.
                    ///

                    // 8번 반복해서 총 8호기 중 가져올 수 있는 셀값을 찾을 for 문.
                    for (int i = 0; i < 8; i++)
                    {
                        // 마지막 크레인 번호의 값에서 1을 뺀 값을 변수에 저장하고 검사한다.
                        searchValue = searchValue - 1;

                        // 크레인을 계산 할 번호가 0이하일 때 8로 조정한다.
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
                                // 하나라도 Stk_state가 0이면 allStkStateOne 변수값을 false로 설정
                                allStkStateOne = false;

                                // reader.Close 를 통하여 읽고있는 쿼리문을 닫습니다.
                                reader.Close();

                                // t_Cell 의 Level 오름차순을 기준으로 Bank 값이 searchValue x 2 한 값 또는 2를 곱한 후 1을 뺀 값 그리고 PLT_CODE 앞자리 번호가 동일하며
                                // t_Cell 의 State 가 'INCOMP' 이고 In_reserve 에 출고 대기 중인 Cell 이 아닌 데이터들을 찾아옵니다.
                                query = @"
                                    SELECT TOP 1
                                            tc.Pal_code, tc.State, tc.Cell,tc.Pal_no,tc.Pal_type,tc.Model,tc.Spec, tc.Line,
                                            tc.Qty,tc.Max_qty,tc.Quality,tc.Prod_time,tc.Pos,tc.Bank,tc.Serial_no,tc.Job_line
                                    FROM t_Cell tc
                                    LEFT JOIN t_Out_reserve tr ON tc.Cell = tr.Cell
                                    WHERE 
                                        (tc.Bank = @searchValue * 2 OR tc.Bank = @searchValue * 2 - 1)
                                        AND LEFT(tc.Pal_code, 2) = LEFT(@code, 2)
                                        AND tc.State = 'INCOMP'
                                        AND tr.Cell IS NULL
                                    ORDER BY tc.Level ASC, tc.Udate ASC, tc.Utime ASC;
                                    ";

                                // WMS_PLC 의 팔레트 코드번호가 존재하면 string 형태로 code 에 대입한다.
                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value.ToString();

                                // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar 와 @searchValue 매개변수에 값을 할당합니다.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);

                                // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                                using (SqlDataReader innerreader = command.ExecuteReader())
                                {
                                    // 가져온 결과 값을 읽어옵니다.
                                    if (innerreader.Read())
                                    {   
                                        // 읽어 온 값이 있을 때 접근 합니다.
                                        if (innerreader.FieldCount > 0)
                                        {
                                            //////[2024-01-26 수정필요] 만약에 왼쪽 테이블에 Job_Line 에 따라서 Pos 를 입력.
                                            ///
                                            int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);
                                            int bank = Convert.ToInt32(innerreader["Bank"]);
                                            // jobLine 이 201 혹은 202 프레스리워크장 번호이거나 301 자동적재PLC 번호이면 Bank 값 뒤에 2번을 붙여서 Pos에 대입하여 위치를 알려준다.
                                            if (jobLine == 201 || jobLine == 202 || jobLine == 301)
                                            {
                                                itemValues["Pos"] = "1" + "-" + bank;
                                            }
                                            // jobLine 이 401 ~ 406 차체 리워크장 번호이거나 501~ 차체렉방 번호 이면 Bank 값 뒤에 3번을 붙여서 Pos에 대입하여 위치를 알려준다.
                                            else if (jobLine == 401 || jobLine == 402 || jobLine == 403 ||
                                                 jobLine == 404 || jobLine == 405 || jobLine == 406 || jobLine >= 501)
                                            {
                                                itemValues["Pos"] =  "2" + "-"+ bank;
                                            }
                                            // itemValues 딕셔너리 키의 Value 값에 쿼리문에서 가져온 값 을 필요한 형태로 형변환을거친 후 대입합니다.
                                            itemValues["Cell"] = innerreader["Cell"].ToString();
                                            itemValues["Pal_code"] = code;
                                            itemValues["State"] = "OUTCOMP";
                                            itemValues["Pal_no"] = innerreader["Pal_no"];
                                            itemValues["Pal_type"] = innerreader["Pal_type"].ToString();
                                            itemValues["Model"] = innerreader["Model"].ToString();
                                            itemValues["Spec"] = innerreader["Spec"].ToString();
                                            itemValues["Line"] = innerreader["Line"].ToString();
                                            itemValues["Qty"] = Convert.ToInt32(innerreader["Qty"]);
                                            itemValues["Max_qty"] = Convert.ToInt32(innerreader["Max_qty"]);
                                            itemValues["Prod_time"] = innerreader["Prod_time"].ToString();
                                            itemValues["Serial_no"] = innerreader["Serial_no"].ToString();
                                            itemValues["JobType"] = "OUTAUTO";
                                            itemValues["Job_Line"] = innerreader["Job_line"].ToString();
                                            itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                                            itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");
                                            // 쿼리문 읽기를 종료합니다.
                                            innerreader.Close();
                                            break;
                                        }
                                        else
                                        {
                                            innerreader.Close();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                reader.Close();
                            }
                        }
                        /// 마지막 호기가 6호기 일 때 5호기 부터 조회를 한다.
                        /// 호기 x 2 와 호기 x 2 - 1 값을 계산한다.


                        /// 스테커 상태가 만족하면 cell 을 구해서 구해지면 cell 정보 저장 후 break.
                        /// 
                    }

                    if(allStkStateOne == true)
                        return -1;

                    // 데이터 베이스에 접속 종료.
                    connection.Close();
                    // SQL 연결이 종료 되었다면 텍스트를 "DB Disconnect" 로 변경
                    ShowText(1, "DB Disconnect");
                    
                    // 정상적으로 결과를 가져왔다면 return true 를 반환하여 함수를 종료합니다.
                    return 1;
                }

                catch (Exception ex)
                {
                    // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트를 DB Connect Error 로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                    ShowText(1, "DB Connect Error");
                    errorMsg = ex.ToString();
                    // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
                    return 2;
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
                    // SQL 에 정상 연결되었다면 DBstateTxt 의 Text 를 DB Connect 로 변경.
                    ShowText(1, "DB Connect");

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
                    /*string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Qty, State, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, @Qty,  @State, @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";*/

                    string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality,Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                   "VALUES (@JobType , @Cell, @Pal_no, '', '', '', '', '', @Qty, '', '','', '', @State, @Pos, @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";

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
                    // SQL 연결이 종료 되었다면 텍스트를 "DB Disconnect" 로 변경
                    ShowText(1, "DB Disconnect");
                }

                // 정상적으로 데이터를 저장하였다면 return 값을 true 로 반환.
                return true;
            }

            catch (Exception ex)
            {
                // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트를 DB Connect Error 로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
                // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
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
                    // SQL 에 정상 연결되었다면 DBstateTxt 의 Text 를 DB Connect 로 변경.
                    ShowText(1, "DB Connect");

                    // 삽입 쿼리문 정의
                    string insertQuery = "INSERT INTO t_Out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model,'' , @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_time, @State, @Pos, @Pal_code, @Serial_no, @Job_Line, @Udate, @Utime)";

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
                    // SQL 연결이 종료 되었다면 텍스트를 "DB Disconnect" 로 변경
                    ShowText(1, "DB Disconnect");
                }

                // 정상적으로 데이터를 저장하였다면 return 값을 true 로 반환.
                return true;
            }

            catch (Exception ex)
            {
                // SQL 에 접속 하는 중 에러가 발생할 시 SQL 접속 상태 텍스트를 DB Connect Error 로 변경하고 에러메세지를 string 형태로 형변환 후 errorMsg 변수에 대입합니다.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
                // false 를 return 시켜 접속이 되지 않았음을 알려줍니다.
                return false;
            }
        }

        // send 테이블에 읽은 값을 receive 테이블에 써주는 함수.
        private bool sendTable()
        {
            try
            {
                // OPCItem 컬렉션에서 각 항목에 대한 루프
                foreach (OPCItem opcItem in sendItem)
                {
                    // 현재 OPCItem의 값 가져오기
                    object send = opcItem.Value;

                    // receiveItem 컬렉션에서 현재 OPCItem과 동일한 ItemID를 가진 항목 찾기
                    object receive = receiveItem.Find(item => item.ItemID.Replace("PLC_WMS", "").Trim() == opcItem.ItemID.Replace("WMS_PLC", "").Trim())?.Value;

                    // 만약 receive 값이 존재하고, 값이 비어있고 send 값과 다른 경우
                    if (receive != null)
                    {
                        if (receive.ToString() == "0" && send.ToString() != receive.ToString())
                        {
                            receiveItem.Find(item => item.ItemID.Replace("PLC_WMS", "").Trim() == opcItem.ItemID.Replace("WMS_PLC", "").Trim())?.Write(send);
                        }
                    }
                }

                // 각 인터페이스 아이디들이 존재하면 해당 값을 딕셔너리에 저장한 벨류값으로 써준다.
                OPCItem writeItem = receiveItem.Find(item => item.ItemID == "[interface]ACS1_WH_01_01.AGV_Call_PLT_Out_Adc"); // WH_LINE
                if (writeItem != null)
                {
                    if (itemValues.ContainsKey("Pos"))
                    {
                        string whLine = itemValues["Pos"].ToString();

                        whLine = whLine.Replace("-", "");
                        writeItem.Write(whLine);
                    }
                }

                writeItem = receiveItem.Find(item => item.ItemID == "[interface]ACS1_WH_01_01.AGV_Call_PLT_Out_Rework"); //Request_Check
                if (writeItem != null)
                {
                    writeItem.Write(itemValues["Quality"].ToString());
                }

                writeItem = receiveItem.Find(item => item.ItemID == "[interface]ACS1_WH_01_01.AGV_Lift_Down_Status"); // NG Code
                if (writeItem != null)
                {
                    writeItem.Write(itemValues["NG_Code"].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                // OPC 접속 에러가 날 시 텍스트의 메세지를 PLC Connect Error 로 변경하고 메세지 박스를 통해 에러를 표시합니다.
                ShowText(2, "PLC Connect Error");
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
            // 사용자의 문서 폴더 경로를 얻기 위해 Environment 클래스의 GetFolderPath 메서드 사용
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // 현재 날짜를 포함한 로그 파일 이름 생성
            string logFileName = "ErrorMsg_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            // 최종 로그 파일 경로 생성
            string logFilePath = Path.Combine(userDocumentsPath, logFileName);


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
                            if (cell != null && cell.Value != null)
                            {
                                sw.Write(cell.Value.ToString() + "\t");
                            }
                        }
                        sw.WriteLine(); // 각 행의 끝에 줄 바꿈 추가
                    }
                }

                if (gridClear == true)
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

        private int errorCheck()
        {
            ////// 재고가 없을 시 NG Code 1 입력.
            ////// 제품 정보 이상일 시 Code 2 입력.
            ////// 데이터 이상 일 시 [시리얼 넘버 9자리가 아니거나 없거나] Code 3 입력.
           
            // PLC 통신을 통해서 얻은 SerialNo 를 serialValue 값에 string 형태로 대입.
            string serialValue = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value.ToString();

            // 만약 재고가 존재하는 Cell 을 못구했다면.
            if (!itemValues.ContainsKey("Cell"))
            {
                // Quality 키의 값을 2번값으로 ng 코드를 넣어줄 딕셔너리 생성.
                itemValues["Quality"] = "2";
                // NG Code 키의 값을 1번으로 하여 재고없음을 알려줄 딕셔너리 생성.
                itemValues["NG_Code"] = "1";

                // 재고가 존재하지 않습니다. + 발생 시간을 알려주는 변수.
                errorMsg = "Stock does not exist.\r\n" + DateTime.Now.ToString();
                return -1;
            }

            // 시리얼 넘버가 9자리가 아니거나 시리얼 넘버가 없다면.
            else if (!string.IsNullOrEmpty(serialValue) && serialValue.Length != 9 || string.IsNullOrEmpty(serialValue))
            {
                // Quality 키의 값을 2번값으로 ng 코드를 넣어줄 딕셔너리 생성.
                itemValues["Quality"] = "2";
                // NG Code 키의 값을 1번으로 하여 재고없음을 알려줄 딕셔너리 생성.
                itemValues["NG_Code"] = "2";

                // 일련번호가 정상이 아닙니다. + 발생 시간을 알려주는 변수.
                errorMsg = "The serial number is not normal.\r\n" + DateTime.Now.ToString();
                return -1;
            }

            // 정상적으로 셀값이 구해졌고 데이터에 문제가 없다면.
            else
            {
                // Quality 값에 1번을 써주어 ok 를 나타낸다.
                itemValues["Quality"] = "1";
                // NG Code 를 0을 작성하여 에러가 없음을 알린다.
                itemValues["NG_Code"] = "0";
            }
            return 1;
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

        private void ShowText(int type, string state)
        {
            // 타입 1번이면 SQL 텍스트 상태를 변경.
            if (type == 1)
            {
                if (DBstateTxt.InvokeRequired)
                {
                    // SQL 연결이 상태에 따라 state 변경.
                    DBstateTxt.Invoke(new MethodInvoker(delegate { DBstateTxt.Text = state; }));
                }
                else
                {
                    DBstateTxt.Text = state;
                }
            }

            // 타입 2번이면 PLC 텍스트 상태를 변경.
            else if(type == 2)
            {
                if (PLCstateTxt.InvokeRequired)
                {
                    // SQL 연결이 상태에 따라 state 변경.
                    PLCstateTxt.Invoke(new MethodInvoker(delegate { PLCstateTxt.Text = state; }));
                }
                else
                {
                    PLCstateTxt.Text = state;
                }
            }
        }

        private void outBtn_Click(object sender, EventArgs e)
        {
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out").Write("0");
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line").Write("0");
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No").Write("0");
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Number").Write("0");
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Parts_Count_In_Pallet").Write("0");
            sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Code").Write("0");
        }

        private void inBtn_Click(object sender, EventArgs e)
        {
            foreach (OPCItem opcItem in sendItem)
            {
                switch (opcItem.ItemID)
                {
                    case "[interface]WMS_PLC.PLT_In_Out":
                        opcItem.Write("1");
                        break;
                    case "[interface]WMS_PLC.Job_Line":
                        opcItem.Write("401");
                        break;
                    case "[interface]WMS_PLC.Serial_No":
                        opcItem.Write("411010001");
                        break;
                    case "[interface]WMS_PLC.PLT_Number":
                        opcItem.Write("345");
                        break;
                    case "[interface]WMS_PLC.Parts_Count_In_Pallet":
                        opcItem.Write("8");
                        break;
                    case "[interface]WMS_PLC.PLT_Code":
                        opcItem.Write("1101");
                        break;
                }
            }
        }
    }
}