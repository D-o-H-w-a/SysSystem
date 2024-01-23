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
        private int searchValue;
        // [PLC->PC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> sendItem = new List<OPCItem>();
        // [PC->PLC] ������ OPCItem �� List �� ��Ƽ� �����ϱ� ���� ������ ����.
        private List<OPCItem> receiveItem = new List<OPCItem>();
        // Data ���� ������ ����Ʈ ���·� ������ ��ųʸ� ����.
        private Dictionary<string, object> itemValues = new Dictionary<string, object>();
        // ���� �޼����� ��� ���� ����.
        private string errorMsg;
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

            // ���� �� �б�.

            // opcItems.AddItem �� ���� OPC Item �� �о� �� �� opcItemList �� �ҷ��� OPCITEM �� add.
            opcItems.AddItem("[interface]WMS_PLC.PLT_In_Out", 1);
            opcItems.AddItem("[interface]WMS_PLC.Job_Line", 1);
            opcItems.AddItem("[interface]WMS_PLC.Serial_No", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Number", 1);
            opcItems.AddItem("[interface]WMS_PLC.Parts_Count_In_Pallet", 1);
            opcItems.AddItem("[interface]WMS_PLC.PLT_Code", 1);

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
                    object receive = opcItem.Value;

                    // ���� ���̺��� ���� ������ ���̺� ItemID�� �̿��ؼ� �ش� ���� ã�ƿ�. ������ ���̺� ItemID �� ���� ���̺��� ���� ���� �ʴ� ItemID �� null ���� �޾ƿ�.
                    object send = sendItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // value2 ���� null �� �ƴ� ��.
                    if (send != null)
                    {
                        // ���� ���̺� value2 �� item ���� �������� �ʰ� ������ ���̺� value �� Item���� ������ ��. 
                        if (send.ToString() == "" && receive.ToString() != send.ToString())
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

                        if (Inreserve())
                        {
                            if (sendTalbe())
                            {

                            }
                            else
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
                // callNum ���� 2 �Ͻ� ��� �Լ� ó��
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
                    else
                    {
                        // ���� �޼���, �߻� ��¥, �߻� �ð��� �ŰԺ��� ��� �Լ� ȣ��.
                        SendErrorMsg(errorMsg, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    }
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

                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
                    SQLstateTxt.ForeColor = Color.Blue;

                    //// ����Ŀ ���¸� ��ü �� �о�´�.
                    ///

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
                                // reader.Close �� ���Ͽ� �а��ִ� �������� �ݽ��ϴ�.
                                reader.Close();

                                // t_Cell �� Level ���������� �������� Bank ���� searchValue x 2 �� �� �Ǵ� 2�� ���� �� 1�� �� �� �׸��� PLT_CODE ���ڸ� ��ȣ�� �����ϸ�
                                // t_Cell �� State �� 'EMPTY' �̰� In_reserve �� �԰� ��� ���� Cell �� �ƴ� tc.PLT_CODE, tc.State, tc.Cell �� ã�ƿɴϴ�.
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

                                // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar �� @searchValue �Ű������� ���� �Ҵ��մϴ�.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@codeFirstChar", codeFirstChar);

                                // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                                reader = command.ExecuteReader();
                                // ������ ��� ���� �о�ɴϴ�.
                                if (reader.Read())
                                {
                                    // �о� �� ���� ���� �� ���� �մϴ�.
                                    if(reader.FieldCount > 0)
                                    {
                                        // itemValues ��ųʸ��� Cell Ű�� Value ���� ���������� ������ Cell �� string ���·� ����ȯ�� ��ģ �� �����մϴ�.
                                        itemValues["Cell"] = reader["Cell"].ToString();
                                        // ������ �б⸦ �����մϴ�.
                                        reader.Close();
                                        break;
                                    }
                                }
                            }
                        }
                        /// ������ ȣ�Ⱑ 6ȣ�� �� �� 5ȣ�� ���� ��ȸ�� �Ѵ�.
                        /// ȣ�� x 2 �� ȣ�� x 2 - 1 ���� ����Ѵ�.


                        /// ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� ���� �� break.
                        /// 
                    }

                    // ������ ���̽��� ���� ����.
                    connection.Close();
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ ������ ���������� ����
                    SQLstateTxt.ForeColor = Color.Black;
                    // ���������� ����� �����Դٸ� return true �� ��ȯ�Ͽ� �Լ��� �����մϴ�.
                    return true;
                }

                catch (Exception ex)
                {
                    // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� ������ ���������� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                    SQLstateTxt.ForeColor = Color.Red;
                    errorMsg = ex.ToString();
                    // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
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

        private bool receiveData(CancellationToken cancellationToken)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // �����ͺ��̽� ����
                    connection.Open();

                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
                    SQLstateTxt.ForeColor = Color.Blue;

                    //// ����Ŀ ���¸� ��ü �� �о�´�.
                    ///

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
                                // reader.Close �� ���Ͽ� �а��ִ� �������� �ݽ��ϴ�.
                                reader.Close();
                         
                                // t_Cell �� Level ���������� �������� Bank ���� searchValue x 2 �� �� �Ǵ� 2�� ���� �� 1�� �� �� �׸��� PLT_CODE ���ڸ� ��ȣ�� �����ϸ�
                                // t_Cell �� State �� 'EMPTY' �̰� In_reserve �� �԰� ��� ���� Cell �� �ƴ� tc.PLT_CODE, tc.State, tc.Cell �� ã�ƿɴϴ�.
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

                                // SqlCommand ��ü�� ����� ����(query)�� ����(connection)�� �����մϴ�
                                command = new SqlCommand(query, connection);
                                // @codeFirstChar �� @searchValue �Ű������� ���� �Ҵ��մϴ�.
                                command.Parameters.AddWithValue("@searchValue", searchValue);
                                command.Parameters.AddWithValue("@code", code);

                                // ������ �����ϰ� SqlDataReader�� ����� �����ɴϴ�.
                                reader = command.ExecuteReader();
                                // ������ ��� ���� �о�ɴϴ�.
                                if (reader.Read())
                                {
                                    // �о� �� ���� ���� �� ���� �մϴ�.
                                    if (reader.FieldCount > 0)
                                    {
                                        // itemValues ��ųʸ� Ű�� Value ���� ���������� ������ �� �� �ʿ��� ���·� ����ȯ����ģ �� �����մϴ�.
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
                                        // ������ �б⸦ �����մϴ�.
                                        reader.Close();
                                        break;
                                    }
                                }
                            }
                        }
                        /// ������ ȣ�Ⱑ 6ȣ�� �� �� 5ȣ�� ���� ��ȸ�� �Ѵ�.
                        /// ȣ�� x 2 �� ȣ�� x 2 - 1 ���� ����Ѵ�.


                        /// ����Ŀ ���°� �����ϸ� cell �� ���ؼ� �������� cell ���� ���� �� break.
                        /// 
                    }

                    // ������ ���̽��� ���� ����.
                    connection.Close();
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ ������ ���������� ����
                    SQLstateTxt.ForeColor = Color.Black;
                    // ���������� ����� �����Դٸ� return true �� ��ȯ�Ͽ� �Լ��� �����մϴ�.
                    return true;
                }

                catch (Exception ex)
                {
                    // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� ������ ���������� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                    SQLstateTxt.ForeColor = Color.Red;
                    errorMsg = ex.ToString();
                    // false �� return ���� ������ ���� �ʾ����� �˷��ݴϴ�.
                    return false;
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
                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
                    SQLstateTxt.ForeColor = Color.Blue;

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
                    string insertQuery = "INSERT INTO t_In_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @Pal_no, '', '', '', '', '', @Qty '', '', '', '', @State, '', @Pal_code, @Serial_No, @Job_Line, @Udate, @Utime)";

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
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ ������ ���������� ����
                    SQLstateTxt.ForeColor = Color.Black;
                }

                // ���������� �����͸� �����Ͽ��ٸ� return ���� true �� ��ȯ.
                return true;
            }

            catch(Exception ex)
            {
                // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� ������ ���������� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                SQLstateTxt.ForeColor = Color.Red;
                errorMsg = ex.ToString();
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
                    // SQL �� ���� ����Ǿ��ٸ� �ؽ�Ʈ ������ �Ķ������� ����
                    SQLstateTxt.ForeColor = Color.Blue;

                    // ���� ������ ����
                    string insertQuery = "INSERT INTO t_Out_reserve (JobType ,Cell, Pal_no, Pal_type, Model, Item, Spec, Line, Qty, Max_qty, Quality, Prod_date, Prod_time, State, Pos, Pal_code, Serial_no, Job_line, Udate, Utime)" +
                                       "VALUES (@JobType , @Cell, @PLT_Number, @Pal_type, @Model,'' , @Spec, @Line, @Qty, @Max_qty, @Quality, @Prod_date, @Prod_time, @State, @Pos, @Pal_code, @Serial_no, @Job_Line, @Udate, @Utime)";

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
                    // SQL ������ ���� �Ǿ��ٸ� �ؽ�Ʈ ������ ���������� ����
                    SQLstateTxt.ForeColor = Color.Black;
                }

                // ���������� �����͸� �����Ͽ��ٸ� return ���� true �� ��ȯ.
                return true;
            }

            catch (Exception ex)
            {
                // SQL �� ���� �ϴ� �� ������ �߻��� �� SQL ���� ���� �ؽ�Ʈ�� ������ ���������� �����ϰ� �����޼����� string ���·� ����ȯ �� errorMsg ������ �����մϴ�.
                SQLstateTxt.ForeColor = Color.Red;
                errorMsg = ex.ToString();
                return false;
            }
        }

        // send ���̺� ���� ���� receive ���̺� ���ִ� �Լ�.
        private bool sendTalbe()
        {
            try
            {
                // OPCItem �÷��ǿ��� �� �׸� ���� ����
                foreach (OPCItem opcItem in sendItem)
                {
                    // ���� OPCItem�� �� ��������
                    object send = opcItem.Value;

                    // receiveItem �÷��ǿ��� ���� OPCItem�� ������ ItemID�� ���� �׸� ã��
                    object receive = receiveItem.Find(item => item.ItemID == opcItem.ItemID)?.Value;

                    // ���� receive ���� �����ϰ�, ���� ����ְ� send ���� �ٸ� ���
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
            // �α� ���� ��� ����
            string logFilePath = "C:\\Path\\To\\ErrorMsg.txt";

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
                            sw.Write(cell.Value.ToString() + "\t");
                        }
                        sw.WriteLine(); // �� ���� ���� �� �ٲ� �߰�
                    }

                    // �α� �ۼ��� �Ϸ�Ǹ� �޽��� ���
                    MessageBox.Show("DataGridView contents saved to log file.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if(gridClear == true)
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