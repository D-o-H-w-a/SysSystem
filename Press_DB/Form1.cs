using OPCAutomation;
using System.Configuration;
using System.Data.SqlClient;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        // OPC ���� �ν��Ͻ��� ��Ÿ���� ���� ����
        private OPCServer opcServer;
        // OPC �����۵��� �����ϴ� ��ü ����
        private OPCItems opcItems;
        // OPC �׷���� �����ϴ� ��ü ����
        private OPCGroups opcGroups;
        // OPC �׷��� ��Ÿ���� ���� ����
        private OPCGroup opcGroup;
        // OPC �������� ��Ÿ���� ���� ����
        private OPCItem opcItem;
        // OPC ����� ���� ������ ���� ���� 
        private Thread opcThread;
        // �����带 ������Ű�µ� ���Ǵ� ��ū�� �����ϴ� CancellationTokenSource
        private CancellationTokenSource cancellationTokenSource;
        // tsc.Stk_no �� ���� �˻� ���� ������ ���� ����
        private int lastSearchValue;
        // [PLC->PC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> sendItem = new List<OPCItem>();
        // [PC->PLC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> receiveItem = new List<OPCItem>();
        // Data ���� ������ ����Ʈ ���·� ������ ��ųʸ� ����.
        private Dictionary<string, object> itemValues = new Dictionary<string, object>();

        /*
        // opc item ���� ��ƿ� List  ���� ����
        private List<OPCItem> opcItemList = new List<OPCItem>();
        */

        public Form1()
        {
            InitializeComponent();
            // ������ ���� �޼ҵ� ȣ��
            StartThread();
        }

        // �����ͺ��̽� ���� ���ڿ�
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        // ������ ���ۺκ�
        private void StartThread()
        {
            try
            {
                // CancellationTokenSource ����
                cancellationTokenSource = new CancellationTokenSource();

                // CancellationTokenSource���� Token ��������
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                string opcGroupName = ConfigurationManager.AppSettings["OPCGroupName"];

                // OPC ���� ����
                opcServer = new OPCServer();
                // OPC ������ ����
                opcServer.Connect("RSLinx OPC Server");

                // OPC �׷� ���� �� ����
                opcGroups = opcServer.OPCGroups;

                // opc �������� �׷��� �����ϴ� ��ü�� ������
                // �̸��� �´� OPC �׷��� ����
                opcGroup = opcGroups.Add("interface");
                // OPC �׷��� Ȱ��ȭ
                opcGroup.IsActive = true;
                // OPC �׷��� ���� ���� �����Ͽ� �ǽð� ������ ����
                opcGroup.IsSubscribed = true;
                // OPC �����۵��� �����ϴ� ��ü�� ������
                opcItems = opcGroup.OPCItems;

                // OPC �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
                OPCstateTxt.ForeColor = Color.Blue;

                // ������ ����
                opcThread = new Thread(() => opcServerJoin(cancellationToken));
                opcThread.Start();
            }

            catch (Exception ex)
            {
                // OPC ���� ������ �� �� �ؽ�Ʈ ������ ���������� �����ϰ� �޼��� �ڽ��� ���� ������ ǥ���մϴ�.
                OPCstateTxt.ForeColor = Color.Red;
                MessageBox.Show(ex.ToString());
            }
        }

        // opc ������ �����Ͽ� ����� �ϸ� ������ ��ü���� ������
        private void opcServerJoin(CancellationToken cancellationToken)
        {
            //// opcItems.AddItem �� while �� ������ ������ �ѹ��� ���� �����ϰ� opcItemList �� read �ؼ� �Ź� ���� �����´�.
            ///


            // ���� �� �б�.

            // opcItems.AddItem �� ���� OPC Item �� �о� �� �� opcItemList �� �ҷ��� OPCITEM �� add.
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


            /* PLT_CODE ���

            // OPC ������ �߰�
            opcItems.AddItem("PLT_IN_OUT", 1);
            opcItems.AddItem("Job_Line", 1);
            opcItems.AddItem("Serial_No", 1);
            opcItems.AddItem("PLT_Number", 1);
            opcItems.AddItem("PLT_CODE", 1);
            opcItems.AddItem("Parts_count_in_pallet", 1);
            */


            // ������ �� �б�.
            opcItems.AddItem("[interface]PLC_WMS.Job_Line", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_In_Out", 2);
            opcItems.AddItem("[interface]PLC_WMS.Serial_No", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_Number", 2);
            opcItems.AddItem("[interface]PLC_WMS.PLT_Code", 2);
            opcItems.AddItem("[interface]PLC_WMS.Parts_Count_In_pallet", 2);

            //opcItems.AddItem("[interface]PLC_WMS.WH_LINE", 2);
            //opcItems.AddItem("[interface]PLC_WMS.Request_Check", 2);
            //opcItems.AddItem("[interface]PLC_WMS.NG Code", 2);

            // �����尡 ����ǰ� �ִ� ���� �ݺ�
            while (!cancellationToken.IsCancellationRequested)
            {
                // plcToPC �ִ� �� OPCItem�� ���� �б⸦ �ݺ��մϴ�.
                foreach (OPCItem opcItem in opcItems)
                {
                    // ���� ������ �������� �ʱ�ȭ�մϴ�.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read�� ����Ͽ� OPC �������� �����͸� �о�ɴϴ�.
                    opcItem.Read(1, out value, out quality, out timestamp);

                    // string ���·� ��ȯ�� object quality �� ���� "0"(Good) �� �� sendItem List ������ OPCITEM �� opcitem ����.
                    if (quality.ToString() == "0")
                    {
                        sendItem.Add(opcItem);
                    }
                }

                //// ������ �������̽� �� ������ �б�

                // pcToPLC �ִ� �� OPCItem�� ���� �б⸦ �ݺ��մϴ�.
                foreach (OPCItem opcItem in opcItems)
                {
                    // ���� ������ �������� �ʱ�ȭ�մϴ�.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read�� ����Ͽ� OPC �������� �����͸� �о�ɴϴ�.
                    opcItem.Read(2, out value, out quality, out timestamp);

                    // string ���·� ��ȯ�� object quality �� ���� "0"(Good) �� �� receiveItem List ������ OPCITEM �� opcitem ����.
                    if (quality.ToString() == "0")
                    {
                        receiveItem.Add(opcItem);
                    }
                }


                //// ���� ���̺� �����Ͱ� �������� ���� �� ������ ���̺� �����Ͱ� ���� �� ������ ���̺��� �����.
                foreach (OPCItem opcItem in receiveItem)
                {
                    // ���� ���̺��� opcItem value ���� object value �� ����.
                    object value = opcItem.Value;

                    // ���� ���̺��� ���� ������ ���̺� ItemID�� �̿��ؼ� �ش� ���� ã�ƿ�. ������ ���̺� ItemID �� ���� ���̺��� ���� ���� �ʴ� ItemID �� null ���� �޾ƿ�.
                    object value2 = sendItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // value2 ���� null �� �ƴ� ��.
                    if (value2 != null)
                    {
                        // ���� ���̺� value2 �� item ���� �������� �ʰ� ������ ���̺� value �� Item���� ������ ��. 
                        if (value2.ToString() == "" && value.ToString() != value2.ToString())
                        {
                            // opcItem.Write �� �̿��ؼ� value2 ������ �ʱ�ȭ �����ش�.
                            opcItem.Write("");
                        }
                    }
                }

                //// ����üũ �Լ� �����
                ///// 1. �԰� ����
                ///
                ///// string ���� pltINout ������ opcItemList ���� PLT_IN_OUT Ű�� ���� callNum �� ����
                string pltINout = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out")?.Value;

                //PLT_IN_OUT ��û ��ȣ�� ���� int �� ���� callNum
                // callNum ���� 1 �Ͻ� �԰� �Լ� ó��
                if (int.Parse(pltINout) == 1)
                {
                    if (sendData(cancellationToken))
                    {

                        // Inreserve �Լ��� ȣ���ϰ� 'connection' ��ü�� �Ű������� �����մϴ�.

                        if (Inreserve())
                        {
                            //// if(sendTalbe()){
                            /// 
                            /// }
                            //// else{
                            ///���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                            ///}
                        }
                        else
                        {
                            //// �μ�Ʈ ���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                        }
                    }
                    else
                    {
                        //// 
                    }
                }

                //// 1.1 PLT_IN_OUT �� 1���� �� �̰� ������ ���̺� ���� �������� ���� ��
                //// 2. �԰� ������ ������ ũ���� ȣ�� �� ���� �� ���� ��ȸ �� In_reserve ���̺� Insert �ϱ�
                //// 3. Insert �� �����ϸ� ������ ���̺� Write ���ֱ�.

                ///// 2. ��� ����
                ///
                // callNum ���� 2 �Ͻ� ��� �Լ� ó��
                else if (int.Parse(pltINout) == 2)
                {
                    OutReserveData(cancellationToken);
                }
                //// 2.1 PLT_IN_OUT �� 2���� �� �̰� ������ ���̺� ���� �������� ���� ��
                //// 2. ��� ������ ������ ũ���� ȣ�� �� ���� �� ���� ��ȸ �� Out_reserve ���̺� Insert �ϱ�
                //// 3. Insert �� �����ϸ� ������ ���̺� Write ���ֱ�.




                ///// 3. �α� �����, �׸��� ȭ�� �����




                Thread.Sleep(200);
            }
        }

        // OPC ���� ���� �� ������ ���� �޼���
        private bool sendData(CancellationToken cancellationToken)
        {
            // connectionString ������ ����� �����ͺ��̽� �ּҸ� ���ؼ� ����
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // �����ͺ��̽� ����
                    connection.Open();

                    // ItemValues ��ųʸ� ���� ���� �ʱ�ȭ
                    itemValues.Clear();

                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
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
                        AND tc.Cell NOT IN (SELECT Cell FROM t_In_reserve) -- t_In_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                        OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
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
                        AND tc.Cell NOT IN (SELECT Cell FROM t_In_reserve) -- t_In_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                        OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                    ";



                    //// ����Ŀ ���¸� ��ü �� �о�´�.
                    ///


                    for (int i = 0; i < 8; i++)
                    {
                        /// ������ ȣ�Ⱑ 6ȣ�� �� �� 5ȣ�� ���� ��ȸ�� �Ѵ�.
                        /// ȣ�� x 2 �� ������ ȣ�� x 2 - 1 ���� ����Ѵ�.


                        /// ����Ŀ ���°� �����ϸ� cell �� �������� cell ���� ���� �� break.
                    }



                    // opcItemList���� ItemID�� "Item"�� �׸��� ã�� �ش� ��(Value)�� �����ɴϴ�.
                    string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;

                    // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�.
                    SqlCommand command = new SqlCommand(query, connection);
                    // @lastSearchValue�� @item �Ű������� ���� �Ҵ��մϴ�.
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    command.Parameters.AddWithValue("@item", item);
                    // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                    SqlDataReader reader = command.ExecuteReader();
                    // SqlDataReader�� �ݺ��Ͽ� �� ���� ó���մϴ�.
                    while (reader.Read())
                    {
                        // "Cell" ���� ���� ������ ���ڿ��� ��ȯ�մϴ�.
                        string cell = reader["Cell"].ToString();

                        // "Stk_state" ���� ���� ������ ������ ��ȯ�մϴ�.
                        int stkState = Convert.ToInt32(reader["Stk_state"]);

                        // "Stk_no" ���� ���� ������ ������ ��ȯ�� �� lastSearchValue ������ �Ҵ��մϴ�.
                        lastSearchValue = Convert.ToInt32(reader["Stk_no"]);

                        // "Cell" ���� ���� itemValues��� ��ųʸ��� �÷��ǿ� �Ҵ��մϴ�.
                        itemValues["Cell"] = cell;

                        // stkState�� ���� 0���� Ȯ���մϴ�.
                        if (stkState == 0)
                        {
                            // stkState�� 0�̸� UpdateListView �Լ��� ȣ���Ͽ� Ư�� �Ű������� ListView ��Ʈ���� ������Ʈ�մϴ�.
                            // �� ����, "���� �԰�"(Normal Incoming), "����"(Normal), �׸��� ���� ��¥�� �ð��� Ư�� �������� �����մϴ�.
                            UpdateListView(cell, "���� �԰�", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }

                    }
                    // ��� ���� �ݺ��� �� SqlDataReader�� �ݽ��ϴ�.
                    reader.Close();

                    return true;
                }

                catch (Exception ex)
                {
                    // SQL ���� ������ �� �� �ؽ�Ʈ ������ ���������� �����ϰ� �޼��� �ڽ��� ���� ������ ǥ���մϴ�.
                    SQLstateTxt.ForeColor = Color.Red;
                    MessageBox.Show(ex.ToString());
                    return false;
                }
            }
        }

        // �� ����� �۵��Ǵ� �Լ�
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // ������ ����
            StopThread();
        }

        private void OutReserveData(CancellationToken cancellationToken)
        {
            /*

           // OPC ������ �߰�
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

                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
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
                        AND tc.Cell NOT IN (SELECT Cell FROM t_Out_reserve) -- t_Out_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                        (tsc.Stk_no >= @lastSearchValue AND tsc.Stk_no <= @maxStkNo) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                        OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
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
                        AND tc.Cell NOT IN (SELECT Cell FROM t_Out_reserve) -- t_Out_reserve �� tc.Cell �� ���� Cell �������� �ʴ� �͸� �˻��մϴ�.
                        AND (
                            (
                            (tsc.Stk_no <= @maxStkNo AND tsc.Stk_no >= @lastSearchValue) -- ���� �˻��� �̻�, �ִ� �� ������ ������ �˻��մϴ�.
                            OR (tsc.Stk_no <= @lastSearchValue) -- ������ �˻��� ������ ������ �˻��մϴ�.
                            )
                        )
                    ORDER BY
                        tc.Bank DESC,
                        tsc.Stk_no DESC
                ";

                    // OPC �׸� ����Ʈ���� ItemID�� "Item"�� ���� ã�� item ������ �Ҵ��մϴ�.
                    string item = opcItemList.Find(item => item.ItemID == "Item")?.Value;
                    // SqlCommand ��ü�� �����ϰ� ������ ������ �����մϴ�.
                    SqlCommand command = new SqlCommand(query, connection);
                    // @lastSearchValue�� @item �Ű������� ���� �Ҵ��մϴ�.
                    command.Parameters.AddWithValue("@lastSearchValue", lastSearchValue);
                    // command.Parameters.AddWithValue("@item", item);
                    // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                    SqlDataReader reader = command.ExecuteReader();
                    // SqlDataReader�� �ݺ��Ͽ� �� ���� ó���մϴ�.
                    while (reader.Read())
                    {
                        // �� ���� ���� ������ �Ҵ��մϴ�.
                        int stkState = Convert.ToInt32(reader["Stk_state"]);
                        string cell = reader["Cell"].ToString();
                        // "Stk_no" ���� ���� ������ ������ ��ȯ�� �� lastSearchValue ������ �Ҵ��մϴ�.
                        lastSearchValue = Convert.ToInt32(reader["Stk_no"]);


                        // �� ���� ���� itemValues ��ųʸ��� �Ҵ��մϴ�.
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


                        // ���� stkState�� 0�̸� ListView ��Ʈ���� ������Ʈ�մϴ�.
                        if (stkState == 0)
                        {
                            UpdateListView(cell, "���� ���", "����", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        }
                    }
                    // SqlDataReader�� �ݽ��ϴ�.
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // SQL ���� ������ �� �� �ؽ�Ʈ ������ ���������� �����ϰ� �޼��� �ڽ��� ���� ������ ǥ���մϴ�.
                SQLstateTxt.ForeColor = Color.Red;
                MessageBox.Show(ex.ToString());
            }
        }

        // In_rserve ���̺� ������ ������ ���� �Լ� 
        private bool Inreserve()
        {
            // �� �������� ���� ��ųʸ��� �߰�

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


            // t_In_reserve ���̺� ������ ����

            string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                                "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', @Udate, @Utime)";
            /* PLT_CODE ó��
             * string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
            */
            // SqlCommand ��ü�� ����ϴ�. �� ��ü�� �����ͺ��̽��� ���� ������ �����ϴ� �� ���˴ϴ�.
            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // ��ųʸ��� ������ SQL �Ű������� �߰�
            foreach (var kvp in itemValues)
            {
                if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                {
                    object valueToInsert = kvp.Value != null ? kvp.Value : " "; // Ű�� ���ų� ���� null�� ��� ���� ���ڿ��� ��ü

                    cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                }
            }

            // ���� ����
            cmdInsert.ExecuteNonQuery();
            // �����ͺ��̽� ���� �ݱ�
            connection.Close();

            return true;
        }

        // Out_reserve �� ������ ���� �ϱ� ���� �Լ�
        private void OutReserve(SqlConnection connection)
        {

            // t_out_reserve ���̺� ������ ����
            string insertQuery = "INSERT INTO t_Out_reserve (JobType, Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, '', @Item, @Spec, @Line, '', @Max_qty, '', '', '', @State, '', '', @Serial_no, @Job_line, @Udate, @Utime)";

            /* PLT_CODE ó��
             * string insertQuery = "INSERT INTO t_out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Parod_time, State, Pos, Udate, Utime)" +
                            "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model, @Item, @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Udate, @Utime)";
            */

            // SqlCommand ��ü�� ����ϴ�. �� ��ü�� �����ͺ��̽��� ���� ������ �����ϴ� �� ���˴ϴ�.
            SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

            // ��ųʸ��� ������ SQL �Ű������� �߰�
            foreach (var kvp in itemValues)
            {
                cmdInsert.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
            }

            // ���� ����
            cmdInsert.ExecuteNonQuery();
            // �����ͺ��̽� ���� �ݱ�
            connection.Close();
        }

        // ������ ���� �޼���
        private void StopThread()
        {
            // opcThread ���� null �ƴϸ� opcThread �� �������� ��
            if (opcThread != null && opcThread.IsAlive)
            {
                // ������ ���� ��û
                cancellationTokenSource.Cancel();
            }
        }

        void UpdateListView(string cell, string cellState, string scState, string Date, string Time)
        {
            // UI �����尡 �ƴ� ���, UI �����忡�� �۾��ϵ��� ��û
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate
                {
                    // ������ �׸��忡 ���ο� �� �߰�
                    dataGrid.Rows.Add(cell, cellState, scState, Date, Time);
                });
            }
            else // UI �������� ���, ���� �۾� ����
            {
                // ������ �׸��忡 ���ο� �� �߰�
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