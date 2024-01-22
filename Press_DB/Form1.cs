using OPCAutomation;
using System.Configuration;
using System.Data.SqlClient;

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
        private int lastSearchValue;
        // [PLC->PC] 각각의 OPCItem 을 List 에 담아서 저장하기 위해 선언한 변수.
        private List<OPCItem> sendItem = new List<OPCItem>();
        // [PC->PLC] 각각의 OPCItem 을 List 에 담아서 저장하기 위해 선언한 변수.
        private List<OPCItem> receiveItem = new List<OPCItem>();
        // Data 들을 저장할 리스트 형태로 저장할 딕셔너리 변수.
        private Dictionary<string, object> itemValues = new Dictionary<string, object>();

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
            ///


            // 왼쪽 맵 읽기.

            // opcItems.AddItem 을 통해 OPC Item 을 읽어 온 뒤 opcItemList 에 불러온 OPCITEM 을 add.
            opcItems.AddItem("[interface]WMS_PLC.PLT_In_Out", 1);
            opcItems.AddItem("[interface]WMS_PLC.Job_Line", 1);
            opcItems.AddItem("[interface]WMS_PLC.Serial_No", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Number", 1);
            opcItems.AddItem("[interface]WMS_PLC.Parts_Count_In_Pallet", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Code", 1);

            //opcItems.AddItem("PLT_TYPE", 1);
            //opcItems.AddItem("Car_Type", 1);
            //opcItems.AddItem("Item", 1);
            //opcItems.AddItem("Spec", 1);
            //opcItems.AddItem("[interface]WMS_PLC.WH_Line", 1);
            //opcItems.AddItem("Counts", 1);


            /* PLT_CODE 사용

            // OPC 아이템 추가
            opcItems.AddItem("PLT_IN_OUT", 1);
            opcItems.AddItem("Job_Line", 1);
            opcItems.AddItem("Serial_No", 1);
            opcItems.AddItem("PLT_Number", 1);
            opcItems.AddItem("PLT_CODE", 1);
            opcItems.AddItem("Parts_count_in_pallet", 1);
            */


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
                    object value = opcItem.Value;

                    // 왼쪽 테이블의 값을 오른쪽 테이블 ItemID를 이용해서 해당 값을 찾아옴. 오른쪽 테이블 ItemID 가 왼쪽 테이블에는 존재 하지 않는 ItemID 면 null 값을 받아옴.
                    object value2 = sendItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // value2 값이 null 이 아닐 때.
                    if (value2 != null)
                    {
                        // 왼쪽 테이블 value2 의 item 값이 존재하지 않고 오른쪽 테이블 value 의 Item값이 존재할 때. 
                        if (value2.ToString() == "" && value.ToString() != value2.ToString())
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

                        // Inreserve 함수를 호출하고 'connection' 객체를 매개변수로 전달합니다.

                        if (Inreserve())
                        {
                            //// if(sendTalbe()){
                            /// 
                            /// }
                            //// else{
                            ///실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                            ///}
                        }
                        else
                        {
                            //// 인설트 실패 시 그리드에 메세지 출력. 로그에 메세지 저장.
                        }
                    }
                    else
                    {
                        //// 
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
                    OutReserveData(cancellationToken);
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

                    // ItemValues 딕셔너리 값을 전부 초기화
                    itemValues.Clear();

                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;

                    /*
                    string code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
                    char codeFirstChar = code.FirstOrDefault()

                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Bank,
                        tc.PLT_CODE
                        tc.State,
                        tc.Cell
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.LEFT(tc.PLT_CODE, 1) >= @codeFirstChar
                        AND tc.State = 'EMPTY'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_In_reserve) -- t_In_reserve 에 tc.Cell 과 같은 Cell 존재하지 않는 것만 검색합니다.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- 이전 검색값 이상, 최대 값 이하인 값들을 검색합니다.
                        OR (tsc.Stk_no <= @lastSearchValue) -- 마지막 검색값 이하인 값들을 검색합니다.
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                    ";
                    */

                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Bank,
                        tc.Cell_type,
                        tc.State,
                        tc.Cell
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Cell_type >= @item
                        AND tc.State = 'EMPTY'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_In_reserve) -- t_In_reserve 에 tc.Cell 과 같은 Cell 존재하지 않는 것만 검색합니다.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- 이전 검색값 이상, 최대 값 이하인 값들을 검색합니다.
                        OR (tsc.Stk_no <= @lastSearchValue) -- 마지막 검색값 이하인 값들을 검색합니다.
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                    ";



                    //// 스테커 상태를 전체 다 읽어온다.
                    ///


                    for (int i = 0; i < 8; i++)
                    {
                        /// 마지막 호기가 6호기 일 때 5호기 부터 조회를 한다.
                        /// 호기 x 2 와 마지막 호기 x 2 - 1 값을 계산한다.


                        /// 스테커 상태가 만족하면 cell 이 구해지면 cell 정보 저장 후 break.
                    }



                    // opcItemList에서 ItemID가 "Item"인 항목을 찾아 해당 값(Value)을 가져옵니다.
                    string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;

                    // SqlCommand 개체를 만들고 쿼리(query)와 연결(connection)을 설정합니다.
                    SqlCommand command = new SqlCommand(query, connection);
                    // @lastSearchValue와 @item 매개변수에 값을 할당합니다.
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    command.Parameters.AddWithValue("@item", item);
                    // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                    SqlDataReader reader = command.ExecuteReader();
                    // SqlDataReader를 반복하여 각 행을 처리합니다.
                    while (reader.Read())
                    {
                        // "Cell" 열의 값을 가져와 문자열로 변환합니다.
                        string cell = reader["Cell"].ToString();

                        // "Stk_state" 열의 값을 가져와 정수로 변환합니다.
                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        // "Stk_no" 열의 값을 가져와 정수로 변환한 후 lastSearchValue 변수에 할당합니다.
                        lastSearchValue = Convert.ToInt32(reader["Stk_no"]);

                        // "Cell" 열의 값을 itemValues라는 딕셔너리나 컬렉션에 할당합니다.
                        itemValues["Cell"] = cell;

                        // stkState의 값이 0인지 확인합니다.
                        if (stkState == 0)
                        {
                            // stkState가 0이면 UpdateListView 함수를 호출하여 특정 매개변수로 ListView 컨트롤을 업데이트합니다.
                            // 셀 정보, "정상 입고"(Normal Incoming), "정상"(Normal), 그리고 현재 날짜와 시간을 특정 형식으로 전달합니다.
                            UpdateListView(cell, "정상 입고", "정상", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }

                    }
                    // 모든 행을 반복한 후 SqlDataReader를 닫습니다.
                    reader.Close();

                    return true;
                }

                catch (Exception ex)
                {
                    // SQL 접속 에러가 날 시 텍스트 색상을 빨간색으로 변경하고 메세지 박스를 통해 에러를 표시합니다.
                    SQLstateTxt.ForeColor = Color.Red;
                    MessageBox.Show(ex.ToString());
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

        private void OutReserveData(CancellationToken cancellationToken)
        {
            /*

           // OPC 아이템 추가
           opcItemList.Add(opcItems.AddItem("PLT_CODE", 1));
           */

            //itemValues["Item"] = opcItems.AddItem("Item", 1);
            //opcItemList.Add(opcItems.AddItem("item", 1));
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    itemValues.Clear();

                    // SQL 에 정상 연결되었다면 텍스트 색상을 파랑색으로 변경
                    SQLstateTxt.ForeColor = Color.Blue;
                    //string Code = opcItemList.Find(pltCode => pltCode.ItemID == "PLT_CODE").ToString();
                    /*
                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
                    SELECT TOP 1
                        tsc.Stk_no,
                        tsc.Stk_state,
                        tc.Cell,
                        tc.Pal_code,
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
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 - 1 OR tc.Bank = tsc.Stk_no * 2)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Pal_code = @Code
                        AND tc.State = 'INCOMP'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_Out_reserve) -- t_Out_reserve 에 tc.Cell 과 같은 Cell 존재하지 않는 것만 검색합니다.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- 이전 검색값 이상, 최대 값 이하인 값들을 검색합니다.
                        OR (tsc.Stk_no <= @lastSearchValue) -- 마지막 검색값 이하인 값들을 검색합니다.
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                ";
                    */
                    string query = @"
                    DECLARE @maxStkNo INT;
                     
                    SELECT
                         @maxStkNo = MAX(Stk_no)
                    FROM 
                        t_SC_state
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
                        tc.Serial_no,
                        tc.Pos
                    FROM
                        t_SC_state tsc
                    LEFT JOIN
                        t_Cell tc ON (tc.Bank = tsc.Stk_no * 2 OR tc.Bank = tsc.Stk_no * 2-1)
                    WHERE
                        tsc.Stk_state = 0
                        AND tc.Cell_type >= 67111
                        AND tc.State = 'INCOMP'
                        AND tc.Cell NOT IN (SELECT Cell FROM t_Out_reserve) -- t_Out_reserve 에 tc.Cell 과 같은 Cell 존재하지 않는 것만 검색합니다.
                        AND (
                            (
                            (tsc.Stk_no <= @maxStkNo AND tsc.Stk_no >= @lastSearchValue) -- 이전 검색값 이상, 최대 값 이하인 값들을 검색합니다.
                            OR (tsc.Stk_no <= @lastSearchValue) -- 마지막 검색값 이하인 값들을 검색합니다.
                            )
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                ";

                    // OPC 항목 리스트에서 ItemID가 "Item"인 값을 찾아 item 변수에 할당합니다.
                    string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;
                    // SqlCommand 개체를 생성하고 쿼리와 연결을 설정합니다.
                    SqlCommand command = new SqlCommand(query, connection);
                    // @lastSearchValue와 @item 매개변수에 값을 할당합니다.
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    // command.Parameters.AddWithValue("@item", item);
                    // 쿼리를 실행하고 SqlDataReader로 결과를 가져옵니다.
                    SqlDataReader reader = command.ExecuteReader();
                    // SqlDataReader를 반복하여 각 행을 처리합니다.
                    while (reader.Read())
                    {
                        // 각 열의 값을 변수에 할당합니다.
                        int stkState = Convert.ToInt32(reader["Stk_state"]);
                        string cell = reader["Cell"].ToString();
                        // "Stk_no" 열의 값을 가져와 정수로 변환한 후 lastSearchValue 변수에 할당합니다.
                        lastSearchValue = Convert.ToInt32(reader["Stk_no"]);


                        // 각 열의 값을 itemValues 딕셔너리에 할당합니다.
                        itemValues["Cell"] = cell;
                        itemValues["item"] = item;
                        itemValues["State"] = reader["State"].ToString();
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

                        //itemValues["Cell"] = cell;
                        //itemValues["PLT_IN_OUT"] = 2;
                        //itemValues["Job_Line"] = 201;
                        //itemValues["Serial_No"] = "23110";
                        //itemValues["Pal_no"] = "6";
                        //itemValues["Pal_type"] = "4";
                        //itemValues["Car_Type"] = "2";
                        //itemValues["Item"] = "67111";
                        //itemValues["Spec"] = "1";
                        //itemValues["LINE"] = "2";
                        //itemValues["Max_qty"] = "2";
                        //itemValues["Counts"] = "3";
                        //itemValues["JobType"] = "OUTAUTO";
                        //itemValues["State"] = "OUTCOMP";
                        //itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                        //itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");


                        // 만약 stkState가 0이면 ListView 컨트롤을 업데이트합니다.
                        if (stkState == 0)
                        {
                            UpdateListView(cell, "정상 출고", "정상", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                    }
                    // SqlDataReader를 닫습니다.
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // SQL 접속 에러가 날 시 텍스트 색상을 빨간색으로 변경하고 메세지 박스를 통해 에러를 표시합니다.
                SQLstateTxt.ForeColor = Color.Red;
                MessageBox.Show(ex.ToString());
            }
        }

        // In_rserve 테이블에 데이터 삽입을 위한 함수 
        private bool Inreserve()
        {
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
            itemValues["Parts_count_int_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_in_pallet")?.Value;
            itemValues["Counts"] = opcItemList.Find(item => item.ItemID == "Counts")?.Value;
            itemValues["JobType"] = "INAUTO";


            // itemValues["PLT_CODE"] = opcItemList.Find(item => item.ItemID == "PLT_CODE")?.Value;
            // itemValues["Parts_count_in_pallet"] = opcItemList.Find(item => item.ItemID == "Parts_count_in_pallet")?.Value;

            //itemValues["PLT_IN_OUT"] = 1;
            //itemValues["Job_Line"] = 201;
            //itemValues["Serial_No"] = "2311010001";
            //itemValues["Pal_no"] = "6";
            //itemValues["Pal_type"] = "4";
            //itemValues["Car_Type"] = "2";
            //itemValues["Item"] = "67111";
            //itemValues["Spec"] = "1";
            //itemValues["LINE"] = "2";
            //itemValues["Max_qty"] = "2";
            //itemValues["Counts"] = "3";
            //itemValues["JobType"] = "INAUTO";
            //itemValues["State"] = "INCOMP";
            //itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
            //itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");


            // t_In_reserve 테이블에 데이터 삽입

            string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', @Udate, @Utime)";
            /* PLT_CODE 처리
             * string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
            */
            // SqlCommand 개체를 만듭니다. 이 개체는 데이터베이스에 대한 쿼리를 실행하는 데 사용됩니다.
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
            // 데이터베이스 연결 닫기
            connection.Close();

            return true;
        }

        // Out_reserve 에 데이터 삽입 하기 위한 함수
        private void OutReserve(SqlConnection connection)
        {

            // t_out_reserve 테이블에 데이터 삽입
            string insertQuery = "INSERT INTO t_Out_reserve (JobType, Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', '', @Serial_no, @Job_line, @Udate, @Utime)";

            /* PLT_CODE 처리
             * string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Parod_time, State, Pos, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
            */

            // SqlCommand 개체를 만듭니다. 이 개체는 데이터베이스에 대한 쿼리를 실행하는 데 사용됩니다.
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