using System;
using System.Threading;
using System.Windows.Forms;
using OPCAutomation;

namespace Press_DB
{
    public partial class Form1 : Form
    {
        private OPCServer opcServer; // OPC 서버 객체
        private OPCGroup opcGroup; // OPC 그룹 객체
        private Thread opcThread; // OPC 통신을 처리하는 스레드

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // OPC Server 연결
            opcServer = new OPCServer();
            // OPC 서버 주소값을 받아 연결
            opcServer.Connect("OPCServerIP");

            //OPC 그룹 설정
            opcGroup = opcServer.OPCGroups.Add("OPCGroupName");
            //OPC 그룹 활성화 - 데이터 읽기/쓰기 가능
            opcGroup.IsActive = true;
            // Subscription 활성화 - 데이터 변경 시 클라이언트에 자동 업데이트
            opcGroup.IsSubscribed = true;
            // 업데이트 주기
            opcGroup.UpdateRate = 1000;

            // OPC 아이템 추가 - OPC 아이템 이름으로 추가.
            OPCItem item = opcGroup.OPCItems.AddItem("ItemName", 1);
        }
    }
}