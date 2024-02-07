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
        // OPC ���� �ν��Ͻ��� ��Ÿ���� ���� ����
        private OPCServer opcServer;
        // OPC �����۵��� �����ϴ� ��ü ����
        private OPCItems opcItems;
        // OPC �׷���� �����ϴ� ��ü ����
        private OPCGroups opcGroups;
        // OPC �׷��� ��Ÿ���� ���� ����
        private OPCGroup opcGroup;
        // OPC ����� ���� ������ ���� ���� 
        private Thread opcThread;
        // �����带 ������Ű�µ� ���Ǵ� ��ū�� �����ϴ� CancellationTokenSource
        private CancellationTokenSource cancellationTokenSource;
        // tsc.Stk_no �� ���� �˻� ���� ������ ���� ����
        private int searchValue;
        // [PLC->PC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> sendItem = new List<OPCItem>();
        // [PC->PLC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> receiveItem = new List<OPCItem>();
        // Data ���� ������ ����Ʈ ���·� ������ ��ųʸ� ����.
        private Dictionary<string, object> itemValues = new Dictionary<string, object>();
        // ���� �޼����� ��� ���� ����.
        private string errorMsg;
        // Cell_code �� key �� Pal_type �� ������ ������ ���� Dictionary ���� ����.
        private Dictionary<string, string> palType = new Dictionary<string, string>();
        // ���ؿ� CellType ���� ������ ����Ʈ ���� ����.
        private List<string> cellType = new List<string>();
        // ���ؿ� cell ��ȣ���� Ȯ���� ����Ʈ ���� ����.
        private List<string> cell = new List<string>();

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

                // OPC �� ���� ����Ǿ��ٸ� �ؽ�Ʈ�� �޼����� PLC Connect �� ����.
                ShowText(2,"PLC Connect");

                // ������ ����
                opcThread = new Thread(() => opcServerJoin(cancellationToken));
                opcThread.Start();
            }
 
            catch (Exception ex)
            {
                // OPC ���� ������ �� �� �ؽ�Ʈ�� �޼����� PLC Connect Error �� �����ϰ� �޼��� �ڽ��� ���� ������ ǥ���մϴ�.
                ShowText(2, "PLC Connect Error");
                errorMsg = ex.ToString();
            }
        }

        // opc ������ �����Ͽ� ����� �ϸ� ������ ��ü���� ������
        private void opcServerJoin(CancellationToken cancellationToken)
        {
            //// opcItems.AddItem �� while �� ������ ������ �ѹ��� ���� �����ϰ� opcItemList �� read �ؼ� �Ź� ���� �����´�.

            // ���� �� �б�.

            // opcItems.AddItem �� ���� OPC Item �� �о� �� �� opcItemList �� �ҷ��� OPCITEM �� add.
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_In_Out", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Job_Line", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Serial_No", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_Number", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.Parts_Count_In_Pallet", 1));
            sendItem.Add(opcItems.AddItem("[interface]WMS_PLC.PLT_Code", 1));

            // ������ �� �б�.
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
                    // �����ͺ��̽� �� ����.
                    connection.Open();
                    // SQL �� ���� ����Ǿ��ٸ� DBstateTxt �� Text �� DB Connect �� ����.
                    ShowText(1, "DB Connect");

                    // t_Pal_type ���̺� �ִ� Cell_code �� Pal_type �� ���� ����.
                    string query = "SELECT Cell_code, Pal_type FROM t_Pal_type";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // ������ �б� ����.
                            while (reader.Read())
                            {
                                // ���� Cell_code �� cellCode �� ����.
                                string cellCode = reader["Cell_code"].ToString();
                                // ���� Pal_type �� pal_type �� ����.
                                string pal_type = reader["Pal_type"].ToString();

                                // cellCode �� Ű�� pal_type �� ������ �ؼ� ��ųʸ��� ����.
                                palType[cellCode] = pal_type;
                            }

                            //������ ����
                            reader.Close();
                        }

                        // ������ ���̽��� ���� ����.
                        connection.Close();
                        // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ�� "DB Disconnect" �� ����
                        ShowText(1, "DB Disconnect");
                    }
                }
            }
            catch (Exception ex)
            {
                // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� DB Connect Error �� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
            }

            // �����尡 ����ǰ� �ִ� ���� �ݺ�
            while (!cancellationToken.IsCancellationRequested)
            {
                // plcToPC �ִ� �� OPCItem�� ���� �б⸦ �ݺ��մϴ�.
                foreach (OPCItem opcItem in sendItem)
                {
                    // ���� ������ �������� �ʱ�ȭ�մϴ�.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read�� ����Ͽ� OPC �������� �����͸� �о�ɴϴ�.
                    opcItem.Read(1, out value, out quality, out timestamp);
                }

                //// ������ �������̽� �� ������ �б�

                // pcToPLC �ִ� �� OPCItem�� ���� �б⸦ �ݺ��մϴ�.
                foreach (OPCItem opcItem in receiveItem)
                {
                    // ���� ������ �������� �ʱ�ȭ�մϴ�.
                    object value;
                    object quality;
                    object timestamp;
                    // opcItem.Read�� ����Ͽ� OPC �������� �����͸� �о�ɴϴ�.
                    opcItem.Read(2, out value, out quality, out timestamp);
                }


                //// ���� ���̺� PLT_In_Out �����Ͱ� 0 �� �� ������ ���̺� �����Ͱ� ������ ������ ���̺��� �����.
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

                //// ����üũ �Լ� �����
                ///// 1. �԰� ����
                ///
                ///// string ���� pltINout ������ opcItemList ���� PLT_IN_OUT Ű�� ���� callNum �� ����

                int pltInOut = Convert.ToInt32(receiveItem.Find(item => item.ItemID == "[interface]PLC_WMS.PLT_In_Out").Value);

                // ���� �����ϴ��� Ȯ���� �ϱ� ���ؼ� int ���·� ���� ������ ������ ����.
                int pltINout = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_In_Out")?.Value);
                int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);
                int serialNo = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value);
                int pltNumber = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Number")?.Value);
                int palletNum = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Parts_Count_In_Pallet")?.Value);
                int plt_code = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value);

                //PLT_IN_OUT ��û ��ȣ�� ���� int �� ���� callNum
                // callNum ���� 1 �Ͻ� 
                // pltInOut�� 0 �̰� ������ ���鿡 ���� 0�� �ƴ� �ٸ� ���� ���� �� �԰� �Լ� ó��
                if (pltINout == 1 && pltInOut == 0 && jobLine != 0 && serialNo != 0 && pltNumber != 0 && palletNum != 0 && plt_code != 0)
                {
                    // ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� �����ϴ� �Լ�.
                    if (sendData() == 1)
                    {
                        // �����Ϳ� �����ִ��� Ȯ���ϰ� ������ NG Code ���� �ְ� Request_Check ���� 2[ng]��
                        // ������ Request_Check ���� 1[ok] �� �� �Լ�.
                        if (errorCheck() == 1)
                        {
                            // sendData �Լ����� ������ cell ������ ���� ���� ���̺� ���� In_reserve �� ������ ���� �ϱ� ���� �Լ�
                            if (Inreserve())
                            {
                                // ���� ���̺� �����͸� �����ʿ� ���ֱ� ���� �Լ�
                                if (!sendTable())
                                {
                                    ///���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                    // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                    SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                                }
                            }
                            else
                            {
                                //// �μ�Ʈ ���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                ///
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                        // ������ �߻� ��.
                        else if (errorCheck() == -1)
                        {
                            // ���� ���̺� ���� �����ʿ� ����.
                            if (!sendTable())
                            {
                                //// �μ�Ʈ ���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                ///
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                            else
                            {
                                //// errorCheck ���� ������ �˻� ���� �� ������ ������ �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                ///
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
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
                        // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                        SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    }
                }

                //// 1.1 PLT_IN_OUT �� 1���� �� �̰� ������ ���̺� ���� �������� ���� ��
                //// 2. �԰� ������ ������ ũ���� ȣ�� �� ���� �� ���� ��ȸ �� In_reserve ���̺� Insert �ϱ�
                //// 3. Insert �� �����ϸ� ������ ���̺� Write ���ֱ�.

                ///// 2. ��� ����
                ///
                // callNum ���� 2 �Ͻ�
                // pltInOut�� 0 �̰� ������ ���鿡 ���� 0�� �ƴ� �ٸ� ���� ���� �� ��� �Լ� ó��
                else if (pltINout == 2 && pltInOut.ToString() == "0" && jobLine != 0 && serialNo != 0 && pltNumber != 0 && palletNum != 0 && plt_code != 0)
                {
                    if (receiveData() == 1)
                    {
                        // �����Ϳ� �����ִ��� Ȯ���ϰ� ������ NG Code ���� �ְ� Request_Check ���� 2[ng]��
                        // ������ Request_Check ���� 1[ok] �� �� �Լ�.
                        if (errorCheck() == 1)
                        {
                            // ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� �����ϴ� �Լ�.
                            if (OutReserve())
                            {
                                // sendData �Լ����� ������ cell ������ ���� ���� ���̺� ���� In_reserve �� ������ ���� �ϱ� ���� �Լ�
                                if (!sendTable())
                                {
                                    ///���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                    // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                    SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                                }
                            }
                            else
                            {
                                //// �μ�Ʈ ���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                ///
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                        }
                        // ���� �߻���.
                        else if (errorCheck() == -1)
                        {
                            // ���� ���̺� ���� �����ʿ� ����.
                            if (!sendTable())
                            {
                                //// �μ�Ʈ ���� �� �׸��忡 �޼��� ���. �α׿� �޼��� ����.
                                ///
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                                SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            }
                            else
                            {
                                // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
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
                        // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                        SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    }
                }
                //// 2.1 PLT_IN_OUT �� 2���� �� �̰� ������ ���̺� ���� �������� ���� ��
                //// 2. ��� ������ ������ ũ���� ȣ�� �� ���� �� ���� ��ȸ �� Out_reserve ���̺� Insert �ϱ�
                //// 3. Insert �� �����ϸ� ������ ���̺� Write ���ֱ�.

                Thread.Sleep(200);
                ///// 3. �α� �����, �׸��� ȭ�� �����
            }
        }

        // OPC ���� ���� �� ������ ���� �޼���
        private int sendData()
        {
            // connectionString ������ ����� �����ͺ��̽� �ּҸ� ���ؼ� ����
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    itemValues.Clear();
                    // �����ͺ��̽� ����
                    connection.Open();

                    // SQL �� ���� ����Ǿ��ٸ� DBstateTxt �� Text �� DB Connect �� ����.
                    ShowText(1, "DB Connect");
                    //// ����Ŀ ���¸� ��ü �� �о�´�.
                    ///

                    // ��ü ũ������ stkState�� 1���� Ȯ���ϴ� �÷���
                    bool allStkStateOne = true;
                
                    // cell �� cellType �� ���� ���� �ʱ�ȭ.
                    cell.Clear();
                    cellType.Clear();

                    for (int i = 0; i < 8; i++)
                    {
                        searchValue = searchValue - 1;
                        
                        if (searchValue <= 0)
                            searchValue = 8;

                        // Stk_no �� @searchValue ��° ���̸� �ش� ���� ��ġ�� �����ϴ� Stk_state ���� ���ð���.
                        string query = "SELECT Stk_state FROM t_SC_state WHERE Stk_no = @searchValue";

                        // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�.
                        SqlCommand command = new SqlCommand(query, connection);
                        // @searchValue �Ű������� ���� �Ҵ��մϴ�.
                        command.Parameters.AddWithValue("@searchValue", searchValue);
                        // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                        SqlDataReader reader = command.ExecuteReader();
                        // ������ ��� ���� �о�ɴϴ�.
                        if (reader.Read())
                        {
                            // ������ Stk_state �� ���� int���·� ��ȯ�Ͽ� stkState ���� �����մϴ�.
                            int stkState = Convert.ToInt32(reader["Stk_state"]);
                            // stkState �� 0(����) �� ������ ���� ��.
                            if (stkState == 0)
                            {
                                // �ϳ��� Stk_state�� 0�̸� allStkStateOne �������� false�� ����
                                allStkStateOne = false;
                                // reader.Close �� ���Ͽ� �а��ִ� �������� �ݽ��ϴ�.
                                reader.Close();

                                // t_Cell �� Level ���������� �������� Bank ���� searchValue x 2 �� �� �Ǵ� 2�� ���� �� 1�� �� �� �׸��� PLT_CODE ���ڸ� ��ȣ�� �����ϸ�
                                // t_Cell �� State �� 'EMPTY' �̰� In_reserve �� �԰� ��� ���� Cell �� �ƴ� tc.PLT_CODE, tc.State, tc.Cell �� ã�ƿɴϴ�.
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

                                // OPC ����� ���ؼ� Read �Ͽ� ������ PLT_Code ���� code �� ��Ʈ�����·� ����. 
                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value.ToString();
                                // PLT_Code �� �� ���ڸ� ��������.
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

                                // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar �� @searchValue �Ű������� ���� �Ҵ��մϴ�.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);
                                //command.Parameters.AddWithValue("@codeFirstChar", codeFirstChar);
                                // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                                using (SqlDataReader innerreader = command.ExecuteReader())
                                {
                                    // ������ ��� ���� �о�ɴϴ�.
                                    while (innerreader.Read())
                                    {
                                        // �о� �� ���� ���� �� ���� �մϴ�.
                                        if (innerreader.FieldCount > 0)
                                        {
                                            if(bank <= 0)
                                            {
                                               bank = Convert.ToInt32(innerreader["Bank"].ToString());
                                            }
                                            // ���� Cell_type �� Null �� �ƴϰ� ���� �����ϸ�.
                                            if (!string.IsNullOrEmpty(innerreader["Cell_type"].ToString()))
                                            {
                                                // cellType ����Ʈ�� Cell_type �߰�.
                                                cellType.Add(innerreader["Cell_type"].ToString());
                                                // cell �� cell ��ȣ �߰�.
                                                cell.Add(innerreader["Cell"].ToString());
                                            }
                                        }
                                    }
                                    // ������ �б⸦ �����մϴ�.
                                    innerreader.Close();

                                    // PLC ���ؼ� �о�� ������� ���� int ���·� ����.
                                    int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);

                                    // JobLine �� ��ġ�� ���� ������ ���� �� ���ؿ� ũ���� ��ȣ ���� + 1 �Ǵ� + 4�� �ٿ��� ���� Pos �� ǥ��.
                                    if (jobLine == 201 || jobLine == 202 || jobLine == 301)
                                    {
                                        itemValues["Pos"] = "1" + "-" + bank;
                                    }
                                    else if (jobLine == 401 || jobLine == 402 || jobLine == 403 ||
                                         jobLine == 404 || jobLine == 405 || jobLine == 406 || jobLine >= 501)
                                    {
                                        itemValues["Pos"] = "2" + "-" + bank;
                                    }
                                    
                                    // cellType �� Count ��ŭ �ݺ�.
                                    for(int j = 0; j < cellType.Count; j++)
                                    {
                                        // value �� cellType �� j �� ���� ���������� value�� ����.
                                        int value = int.Parse(cellType[j]);

                                        // �ȷ�Ʈ Ÿ���� 3730�̰� pal_type �� value ���� ���� ���� ��.
                                        if (pal_type == 3730 && pal_type == value)
                                        {
                                            // itemValues ��ųʸ��� Cell Ű�� Value ���� j �� ° Cell �� �����մϴ�.
                                            itemValues["Cell"] = cell[j];
                                            // i �� 8�� ���� �༭ ù for���� ������������ ��.
                                            i = 8;
                                            // j for ���� ����.
                                            break;
                                        }
                                        else if (pal_type != 3730 && cellType.Contains(pal_typ) == true && value != 3730)
                                        {
                                            if (value == pal_type)
                                            {
                                                // itemValues ��ųʸ��� Cell Ű�� Value ���� j �� ° Cell �� �����մϴ�.
                                                itemValues["Cell"] = cell[j];
                                                // i �� 8�� ���� �༭ ù for���� ������������ ��.
                                                i = 8;
                                                // j for ���� ����.
                                                break;
                                            }
                                        }
                                        else if(pal_type != 3730 && cellType.Contains(pal_typ) == false && value != 3730)
                                        {
                                            if(value >= pal_type)
                                            {
                                                // itemValues ��ųʸ��� Cell Ű�� Value ���� j �� ° Cell �� �����մϴ�.
                                                itemValues["Cell"] = cell[j];
                                                // i �� 8�� ���� �༭ ù for���� ������������ ��.
                                                i = 8;
                                                // j for ���� ����.
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // ������ �б⸦ �����մϴ�.
                                reader.Close();
                            }
                        }
                        /// ������ ȣ�Ⱑ 6ȣ�� �� �� 5ȣ�� ���� ��ȸ�� �Ѵ�.
                        /// ȣ�� x 2 �� ȣ�� x 2 - 1 ���� ����Ѵ�.


                        /// ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� ���� �� break.
                        /// 
                    }

                    ////// ��� ���� �� NG Code 1 �Է�.
                    ////// ��ǰ ���� �̻��� �� Code 2 �Է�.
                    ////// ������ �̻� �� �� [�ø��� �ѹ� 9�ڸ��� �ƴϰų� ���ų�] Code 3 �Է�.

                    // ������ ���̽��� ���� ����.
                    connection.Close();

                    if (allStkStateOne == true)
                        return -1;


                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ�� "DB Disconnect" �� ����
                    ShowText(1, "DB Disconnect");
                    // ���������� ����� �����Դٸ� return true �� ��ȯ�Ͽ� �Լ��� �����մϴ�.
                    return 1;
                }

                catch (Exception ex)
                {
                    // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� DB Connect Error �� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                    ShowText(1, "DB Connect Error");
                    errorMsg = ex.ToString();
                    // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
                    return 2;
                }
            }
        }

        // �� ����� �۵��Ǵ� �Լ�
        private void Main_FormClosing(object sender, FormClosedEventArgs e)
        {
            // ������ ����
            StopThread();
        }

        private int receiveData()
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    itemValues.Clear();
                    // �����ͺ��̽� ����
                    connection.Open();

                    // SQL �� ���� ����Ǿ��ٸ� DBstateTxt �� Text �� DB Connect �� ����.
                    ShowText(1, "DB Connect");

                    // ��ü ũ������ stkState�� 1���� Ȯ���ϴ� �÷���
                    bool allStkStateOne = true;
                    //// ����Ŀ ���¸� ��ü �� �о�´�.
                    ///

                    // 8�� �ݺ��ؼ� �� 8ȣ�� �� ������ �� �ִ� ������ ã�� for ��.
                    for (int i = 0; i < 8; i++)
                    {
                        // ������ ũ���� ��ȣ�� ������ 1�� �� ���� ������ �����ϰ� �˻��Ѵ�.
                        searchValue = searchValue - 1;

                        // ũ������ ��� �� ��ȣ�� 0������ �� 8�� �����Ѵ�.
                        if (searchValue <= 0)
                            searchValue = 8;

                        // Stk_no �� @searchValue ��° ���̸� �ش� ���� ��ġ�� �����ϴ� Stk_state ���� ���ð���.
                        string query = "SELECT Stk_state FROM t_SC_state WHERE Stk_no = @searchValue";

                        // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�.
                        SqlCommand command = new SqlCommand(query, connection);
                        // @searchValue �Ű������� ���� �Ҵ��մϴ�.
                        command.Parameters.AddWithValue("@searchValue", searchValue);
                        // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                        SqlDataReader reader = command.ExecuteReader();
                        // ������ ��� ���� �о�ɴϴ�.
                        if (reader.Read())
                        {
                            // ������ Stk_state �� ���� int���·� ��ȯ�Ͽ� stkState ���� �����մϴ�.
                            int stkState = Convert.ToInt32(reader["Stk_state"]);
                            // stkState �� 0(����) �� ������ ���� ��.
                            if (stkState == 0)
                            {
                                // �ϳ��� Stk_state�� 0�̸� allStkStateOne �������� false�� ����
                                allStkStateOne = false;

                                // reader.Close �� ���Ͽ� �а��ִ� �������� �ݽ��ϴ�.
                                reader.Close();

                                // t_Cell �� Level ���������� �������� Bank ���� searchValue x 2 �� �� �Ǵ� 2�� ���� �� 1�� �� �� �׸��� PLT_CODE ���ڸ� ��ȣ�� �����ϸ�
                                // t_Cell �� State �� 'INCOMP' �̰� In_reserve �� ��� ��� ���� Cell �� �ƴ� �����͵��� ã�ƿɴϴ�.
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

                                // WMS_PLC �� �ȷ�Ʈ �ڵ��ȣ�� �����ϸ� string ���·� code �� �����Ѵ�.
                                string code = sendItem.Find(pltCode => pltCode.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value.ToString();

                                // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar �� @searchValue �Ű������� ���� �Ҵ��մϴ�.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);

                                // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                                using (SqlDataReader innerreader = command.ExecuteReader())
                                {
                                    // ������ ��� ���� �о�ɴϴ�.
                                    if (innerreader.Read())
                                    {   
                                        // �о� �� ���� ���� �� ���� �մϴ�.
                                        if (innerreader.FieldCount > 0)
                                        {
                                            //////[2024-01-26 �����ʿ�] ���࿡ ���� ���̺� Job_Line �� ���� Pos �� �Է�.
                                            ///
                                            int jobLine = Convert.ToInt32(sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value);
                                            int bank = Convert.ToInt32(innerreader["Bank"]);
                                            // jobLine �� 201 Ȥ�� 202 ����������ũ�� ��ȣ�̰ų� 301 �ڵ�����PLC ��ȣ�̸� Bank �� �ڿ� 2���� �ٿ��� Pos�� �����Ͽ� ��ġ�� �˷��ش�.
                                            if (jobLine == 201 || jobLine == 202 || jobLine == 301)
                                            {
                                                itemValues["Pos"] = "1" + "-" + bank;
                                            }
                                            // jobLine �� 401 ~ 406 ��ü ����ũ�� ��ȣ�̰ų� 501~ ��ü���� ��ȣ �̸� Bank �� �ڿ� 3���� �ٿ��� Pos�� �����Ͽ� ��ġ�� �˷��ش�.
                                            else if (jobLine == 401 || jobLine == 402 || jobLine == 403 ||
                                                 jobLine == 404 || jobLine == 405 || jobLine == 406 || jobLine >= 501)
                                            {
                                                itemValues["Pos"] =  "2" + "-"+ bank;
                                            }
                                            // itemValues ��ųʸ� Ű�� Value ���� ���������� ������ �� �� �ʿ��� ���·� ����ȯ����ģ �� �����մϴ�.
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
                                            // ������ �б⸦ �����մϴ�.
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
                        /// ������ ȣ�Ⱑ 6ȣ�� �� �� 5ȣ�� ���� ��ȸ�� �Ѵ�.
                        /// ȣ�� x 2 �� ȣ�� x 2 - 1 ���� ����Ѵ�.


                        /// ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� ���� �� break.
                        /// 
                    }

                    if(allStkStateOne == true)
                        return -1;

                    // ������ ���̽��� ���� ����.
                    connection.Close();
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ�� "DB Disconnect" �� ����
                    ShowText(1, "DB Disconnect");
                    
                    // ���������� ����� �����Դٸ� return true �� ��ȯ�Ͽ� �Լ��� �����մϴ�.
                    return 1;
                }

                catch (Exception ex)
                {
                    // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� DB Connect Error �� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                    ShowText(1, "DB Connect Error");
                    errorMsg = ex.ToString();
                    // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
                    return 2;
                }
            }
        }

        // In_rserve ���̺� ������ ������ ���� �Լ� 
        private bool Inreserve()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // SQL �� ����.
                    connection.Open();
                    // SQL �� ���� ����Ǿ��ٸ� DBstateTxt �� Text �� DB Connect �� ����.
                    ShowText(1, "DB Connect");

                    // ������ Ű ���� ���� Value ���� sendItem ���� item Value ���� ã�� ����.
                    itemValues["Job_Line"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Job_Line")?.Value;
                    itemValues["Serial_No"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value;
                    itemValues["Pal_no"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Number")?.Value;
                    itemValues["Qty"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Parts_Count_In_Pallet")?.Value;
                    itemValues["Pal_code"] = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.PLT_Code")?.Value;
                    itemValues["JobType"] = "INAUTO";
                    itemValues["State"] = "INCOMP";
                    itemValues["Udate"] = DateTime.Now.ToString("yyyy-MM-dd");
                    itemValues["Utime"] = DateTime.Now.ToString("HH:mm:ss");

                    // ���� ������ ����
                    /*string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Qty, State, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, @Qty,  @State, @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";*/

                    string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality,Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                   "VALUES (@JobType , @Cell, @Pal_no, '', '', '', '', '', @Qty, '', '','', '', @State, @Pos, @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";

                    // SqlCommand ��ü ���� �� ������ ����
                    SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                    // SqlCommand ��ü ���� �� ������ ����
                    foreach (var kvp in itemValues)
                    {
                        // ���� �Ű������� �̹� �߰����� �ʾҴٸ�
                        if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                        {
                            // �Ű������� �߰��� �� ���� (null�� �ƴϸ� �ش� ��, null�̸� ���� ���ڿ��� ����)
                            object valueToInsert = kvp.Value != null ? kvp.Value : " ";
                            // SqlCommand�� Parameters �÷��ǿ� ���ο� �Ű����� �߰�
                            cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                        }
                    }

                    // ���� ����
                    cmdInsert.ExecuteNonQuery();
                    // �����ͺ��̽� ���� �ݱ�
                    connection.Close();
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ�� "DB Disconnect" �� ����
                    ShowText(1, "DB Disconnect");
                }

                // ���������� �����͸� �����Ͽ��ٸ� return ���� true �� ��ȯ.
                return true;
            }

            catch (Exception ex)
            {
                // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� DB Connect Error �� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
                // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
                return false;
            }
        }
        // Out_reserve �� ������ ���� �ϱ� ���� �Լ�
        private bool OutReserve()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // SQL �� ����.
                    connection.Open();
                    // SQL �� ���� ����Ǿ��ٸ� DBstateTxt �� Text �� DB Connect �� ����.
                    ShowText(1, "DB Connect");

                    // ���� ������ ����
                    string insertQuery = "INSERT INTO t_Out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, @Pal_type, @Model,'' , @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_time, @State, @Pos, @Pal_code, @Serial_no, @Job_Line, @Udate, @Utime)";

                    // SqlCommand ��ü ���� �� ������ ����
                    SqlCommand cmdInsert = new SqlCommand(insertQuery, connection);

                    // SqlCommand ��ü ���� �� ������ ����
                    foreach (var kvp in itemValues)
                    {
                        // ���� �Ű������� �̹� �߰����� �ʾҴٸ�
                        if (!cmdInsert.Parameters.Contains("@" + kvp.Key))
                        {
                            // �Ű������� �߰��� �� ���� (null�� �ƴϸ� �ش� ��, null�̸� ���� ���ڿ��� ����)
                            object valueToInsert = kvp.Value != null ? kvp.Value : " ";

                            // SqlCommand�� Parameters �÷��ǿ� ���ο� �Ű����� �߰�
                            cmdInsert.Parameters.AddWithValue("@" + kvp.Key, valueToInsert);
                        }
                    }

                    // ���� ����
                    cmdInsert.ExecuteNonQuery();
                    // �����ͺ��̽� ���� �ݱ�
                    connection.Close();
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ�� "DB Disconnect" �� ����
                    ShowText(1, "DB Disconnect");
                }

                // ���������� �����͸� �����Ͽ��ٸ� return ���� true �� ��ȯ.
                return true;
            }

            catch (Exception ex)
            {
                // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� DB Connect Error �� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                ShowText(1, "DB Connect Error");
                errorMsg = ex.ToString();
                // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
                return false;
            }
        }

        // send ���̺� ���� ���� receive ���̺� ���ִ� �Լ�.
        private bool sendTable()
        {
            try
            {
                // OPCItem �÷��ǿ��� �� �׸� ���� ����
                foreach (OPCItem opcItem in sendItem)
                {
                    // ���� OPCItem�� �� ��������
                    object send = opcItem.Value;

                    // receiveItem �÷��ǿ��� ���� OPCItem�� ������ ItemID�� ���� �׸� ã��
                    object receive = receiveItem.Find(item => item.ItemID.Replace("PLC_WMS", "").Trim() == opcItem.ItemID.Replace("WMS_PLC", "").Trim())?.Value;

                    // ���� receive ���� �����ϰ�, ���� ����ְ� send ���� �ٸ� ���
                    if (receive != null)
                    {
                        if (receive.ToString() == "0" && send.ToString() != receive.ToString())
                        {
                            receiveItem.Find(item => item.ItemID.Replace("PLC_WMS", "").Trim() == opcItem.ItemID.Replace("WMS_PLC", "").Trim())?.Write(send);
                        }
                    }
                }

                // �� �������̽� ���̵���� �����ϸ� �ش� ���� ��ųʸ��� ������ ���������� ���ش�.
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
                // OPC ���� ������ �� �� �ؽ�Ʈ�� �޼����� PLC Connect Error �� �����ϰ� �޼��� �ڽ��� ���� ������ ǥ���մϴ�.
                ShowText(2, "PLC Connect Error");
                errorMsg = ex.ToString();
                return false;
            }
        }

        private void SendErrorMsg(string errorMsg, string errorDate, string errorTime)
        {
            // UI �����尡 �ƴ� ���, UI �����忡�� �۾��ϵ��� ��û
            if (dataGrid.InvokeRequired)
            {
                dataGrid.Invoke((MethodInvoker)delegate
                {
                    // ������ �׸��忡 ���ο� �� �߰�
                    dataGrid.Rows.Add(errorMsg, errorDate, errorTime);
                });
            }
            else // UI �������� ���, ���� �۾� ����
            {
                // ������ �׸��忡 ���ο� �� �߰�
                dataGrid.Rows.Add(errorMsg, errorDate, errorTime);
            }

            // �����޼��� �α� ���� �Լ�ȣ��.
            ErrorMsgAddLog(false);
        }

        // �׸��忡 ���� �����޼����� �α׷� �����ϴ� �Լ�.
        private void ErrorMsgAddLog(bool gridClear)
        {
            // ������� ���� ���� ��θ� ��� ���� Environment Ŭ������ GetFolderPath �޼��� ���
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // ���� ��¥�� ������ �α� ���� �̸� ����
            string logFileName = "ErrorMsg_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            // ���� �α� ���� ��� ����
            string logFilePath = Path.Combine(userDocumentsPath, logFileName);


            try
            {
                // StreamWriter�� ����Ͽ� ���Ͽ� �α� ����
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    // DataGridView�� �� ��� ���� ��ȸ�ϸ� �α׿� ���
                    foreach (DataGridViewRow row in dataGrid.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell != null && cell.Value != null)
                            {
                                sw.Write(cell.Value.ToString() + "\t");
                            }
                        }
                        sw.WriteLine(); // �� ���� ���� �� �ٲ� �߰�
                    }
                }

                if (gridClear == true)
                {
                    // DataGridView�� ��� ���� ����
                    dataGrid.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                // �α� �ۼ� �� ���� �߻� �� ���� ó��
                MessageBox.Show("Error writing to log file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int errorCheck()
        {
            ////// ��� ���� �� NG Code 1 �Է�.
            ////// ��ǰ ���� �̻��� �� Code 2 �Է�.
            ////// ������ �̻� �� �� [�ø��� �ѹ� 9�ڸ��� �ƴϰų� ���ų�] Code 3 �Է�.
           
            // PLC ����� ���ؼ� ���� SerialNo �� serialValue ���� string ���·� ����.
            string serialValue = sendItem.Find(item => item.ItemID == "[interface]WMS_PLC.Serial_No")?.Value.ToString();

            // ���� ��� �����ϴ� Cell �� �����ߴٸ�.
            if (!itemValues.ContainsKey("Cell"))
            {
                // Quality Ű�� ���� 2�������� ng �ڵ带 �־��� ��ųʸ� ����.
                itemValues["Quality"] = "2";
                // NG Code Ű�� ���� 1������ �Ͽ� �������� �˷��� ��ųʸ� ����.
                itemValues["NG_Code"] = "1";

                // ��� �������� �ʽ��ϴ�. + �߻� �ð��� �˷��ִ� ����.
                errorMsg = "Stock does not exist.\r\n" + DateTime.Now.ToString();
                return -1;
            }

            // �ø��� �ѹ��� 9�ڸ��� �ƴϰų� �ø��� �ѹ��� ���ٸ�.
            else if (!string.IsNullOrEmpty(serialValue) && serialValue.Length != 9 || string.IsNullOrEmpty(serialValue))
            {
                // Quality Ű�� ���� 2�������� ng �ڵ带 �־��� ��ųʸ� ����.
                itemValues["Quality"] = "2";
                // NG Code Ű�� ���� 1������ �Ͽ� �������� �˷��� ��ųʸ� ����.
                itemValues["NG_Code"] = "2";

                // �Ϸù�ȣ�� ������ �ƴմϴ�. + �߻� �ð��� �˷��ִ� ����.
                errorMsg = "The serial number is not normal.\r\n" + DateTime.Now.ToString();
                return -1;
            }

            // ���������� ������ �������� �����Ϳ� ������ ���ٸ�.
            else
            {
                // Quality ���� 1���� ���־� ok �� ��Ÿ����.
                itemValues["Quality"] = "1";
                // NG Code �� 0�� �ۼ��Ͽ� ������ ������ �˸���.
                itemValues["NG_Code"] = "0";
            }
            return 1;
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

        private void ShowText(int type, string state)
        {
            // Ÿ�� 1���̸� SQL �ؽ�Ʈ ���¸� ����.
            if (type == 1)
            {
                if (DBstateTxt.InvokeRequired)
                {
                    // SQL ������ ���¿� ���� state ����.
                    DBstateTxt.Invoke(new MethodInvoker(delegate { DBstateTxt.Text = state; }));
                }
                else
                {
                    DBstateTxt.Text = state;
                }
            }

            // Ÿ�� 2���̸� PLC �ؽ�Ʈ ���¸� ����.
            else if(type == 2)
            {
                if (PLCstateTxt.InvokeRequired)
                {
                    // SQL ������ ���¿� ���� state ����.
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