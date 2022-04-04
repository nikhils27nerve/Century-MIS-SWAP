using DevExpress.XtraEditors;
using NerveLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Century_MIS
{
    public partial class Century_MIS : DevExpress.XtraEditors.XtraForm
    {
        NerveLogger logger = new NerveLogger();
        CManager m_mangerAPI = null;

        public Century_MIS()
        {
            InitializeComponent();
            InitialiseLogger(out Exception _Error);

            CommonMethods METHODS = new CommonMethods();
            METHODS.Initialize(this);
            m_mangerAPI = new CManager();
        }

        private bool InitialiseLogger(out Exception _Error)
        {
            try
            {
                _Error = null;
                string _AppPath = Application.StartupPath;
                logger = new NerveLogger(true, true, ApplicationName: "CenturyMIS Order Dump");
                logger.Initialize(_AppPath);

            }
            catch (Exception ee) { _Error = ee; return false; }

            return true;
        }

        private void Century_MIS_Load(object sender, EventArgs e)
        {
            try
            {
                var Server = CommonMethods.readFromConfig("META-TRADER", "SERVER-URL");
                var Password = CommonMethods.readFromConfig("META-TRADER", "MANAGER-PASSWORD");
                var Login = CommonMethods.readFromConfig("META-TRADER", "MANAGER-ID");


                //var DATE = CommonMethods.ConvertFromUnixTimestamp(1648944000);

                txt_ServerURL.Text = Server;
                txt_Username.Text = Login;
                txt_Password.Text = Password;

                txt_Password.Enabled = false;
                txt_Username.Enabled = false;
                txt_ServerURL.Enabled = false;
                btn_Connect.Enabled = false;

                if (m_mangerAPI.Initialize(this))
                {
                    if(m_mangerAPI.Login()){                      
                        PrintMsg("Manager connected to " + Server);                       
                        m_mangerAPI.LoadAllSymbols();
                        m_mangerAPI.GetAllUsers();
                    }
                    else
                    {
                        PrintMsg("Manager failed to connect " + Server);
                    }
                }
                else
                {
                    PrintMsg("Manager failed to connect " + Server);
                }
                //BeforeSwapProcess();
                Task.Run(() => CalculateSwap());   //on
                Task.Run(() => RestartApp());

            }catch(Exception EE)
            {
                handleException(EE);
            }
        }

        private void CalculateSwap()
        {
            var SwapCalculationTime = CommonMethods.readFromConfig("OTHER", "SWAP-CALCULATE-TIME");
            if (SwapCalculationTime != "-")
            {
                try
                {
                    var dte_SwapTime = DateTime.Parse(SwapCalculationTime);
                    var BeforeConfTime = (dte_SwapTime - DateTime.Now).TotalSeconds;

                    if (BeforeConfTime < 0)
                    {
                        BeforeConfTime = (dte_SwapTime.AddDays(1) - DateTime.Now).TotalSeconds;
                    }
                    WriteLog("Thread sleeped for " + BeforeConfTime + " seconds");
                    Thread.Sleep(Convert.ToInt32(BeforeConfTime * 1000));
                    WriteLog("Before-configuration-time Process started");
                    
                    BeforeSwapProcess();
                    CalculateSwap();
                }
                catch (Exception ee)
                {
                    handleException(ee);
                }
            }
        }

        private void BeforeSwapProcess()
        {
            try
            {
                //var BeforePositions = m_mangerAPI.GetAllPostions();
                //Thread.Sleep(10 *60* 1000);
                //WriteLog("After-configuration-time Process started");
                var CurrDayPositions = m_mangerAPI.GetAllPostions();
                setSwap(CurrDayPositions);
            }
            catch (Exception ee)
            {
                handleException(ee);
            } 
        }


        private void setSwap(ConcurrentDictionary<ulong, Position_data> dict_CurrDayPositions)
        {
            try
            {
               double PrevdaySwap = 0;
               var dict_PrevDaySwap = CommonMethods.dict_PrevDayPositions;
               ConcurrentDictionary<ulong, Position_data> dict_AllPositions = new ConcurrentDictionary<ulong, Position_data>();

               foreach (var KVP in dict_CurrDayPositions)
               {
                    var Position = KVP.Value;
                    if (dict_PrevDaySwap.TryGetValue(KVP.Key,out Position_data PrevDayPosition))
                    {
                        PrevdaySwap = PrevDayPosition.Storage;
                    }

                    var swap = Math.Round(PrevdaySwap - Position.Swap,2);
                    Position.Swap = swap;
                    dict_AllPositions.TryAdd(KVP.Key, Position);
               }

               CommonMethods.dict_PrevDayPositions.Clear();
               CommonMethods.dict_PrevDayPositions = dict_AllPositions;
               CommonMethods.AddToMongo_Positions(dict_AllPositions);
               PrintMsg("Positions dumped to Mongodb");

            }
            catch(Exception ee)
            {
                handleException(ee);
            }
        }

        private void SetSwap(ConcurrentDictionary<ulong, Position_data> dict_BeforeSwapPositions, ConcurrentDictionary<ulong, Position_data> dict_AfterSwapPositions)
        {
            try
            {
               ConcurrentDictionary<ulong, Position_data> dict_AllPositions = new ConcurrentDictionary<ulong, Position_data>();
              
               foreach(var KVP in dict_AfterSwapPositions)
               {
                    var Position = KVP.Value;
                    double OldSwapValue = 0;
                    if (dict_BeforeSwapPositions.TryGetValue(KVP.Key, out Position_data BeforePosition))
                    {
                        OldSwapValue = BeforePosition.Swap;
                    }

                    var swap = Math.Round(OldSwapValue - Position.Swap, 2);   //old swap - new swap 
                    Position.Swap = swap;
                    dict_AllPositions.TryAdd(KVP.Key, Position);
               } 
                
               //foreach(var KVP in dict_BeforeSwapPositions)
               //{
               //     var Position = KVP.Value;

               //     double newSwapValue = 0;
               //     if(dict_AfterSwapPositions.TryGetValue(KVP.Key,out Position_data AfterPosition))
               //     {
               //         newSwapValue = AfterPosition.Swap;
               //     }
                   
               //     var swap = Math.Round(Position.Swap - newSwapValue, 2);   //old swap - new swap 
               //     Position.Swap = swap;
               //     dict_AllPositions.TryAdd(KVP.Key, Position);
               //}

                CommonMethods.AddToMongo_Positions(dict_AllPositions);
                PrintMsg("Positions dumped to Mongodb");

            }catch(Exception ee)
            {
                handleException(ee);
            }
        }

        private void RestartApp()
        {
            var _AutoReloadTime = CommonMethods.readFromConfig("OTHER", "AUTO-RELOAD-TIME");
            if (_AutoReloadTime != "-")
            {
                try
                {
                    var dte_AutoReload = DateTime.Parse(_AutoReloadTime);
                    double waitSeconds = (dte_AutoReload - DateTime.Now).TotalSeconds;

                    if (waitSeconds < 0)
                        waitSeconds = (dte_AutoReload.AddDays(1) - DateTime.Now).TotalSeconds;

                    Thread.Sleep(Convert.ToInt32(waitSeconds * 1000));
                    m_mangerAPI.ReleaseManagerAPI();
                    ClearAllDictonary();
                    ReconnectManagerApi();
                    RestartApp();
                    
                }
                catch (Exception ee) {

                    handleException(ee);  
                }  
            }
        }


        private void ReconnectManagerApi()
        {
            try
            {

                var Server = CommonMethods.readFromConfig("META-TRADER", "SERVER-URL");
                var Password = CommonMethods.readFromConfig("META-TRADER", "MANAGER-PASSWORD");
                var Login = CommonMethods.readFromConfig("META-TRADER", "MANAGER-ID");

                if (m_mangerAPI.Initialize(this))
                {
                    if (m_mangerAPI.Login())
                    {
                        PrintMsg("Manager connected to " + Server);
                        m_mangerAPI.LoadAllSymbols();
                        m_mangerAPI.GetAllUsers();
                    }
                    else
                    {
                        PrintMsg("Manager failed to connect " + Server);
                    }
                }
                else
                {
                    PrintMsg("Manager failed to connect " + Server);
                }
            }
            catch(Exception ee)
            {
                PrintMsg("Error while tried to connect with ManagerAPI");
                handleException(ee);
            }
        }

        private void ClearAllDictonary()
        {
            try
            {
                //CommonMethods.dict_GroupMargin.Clear();
                //CommonMethods.dict_GroupProfile.Clear();
                //CommonMethods.dict_LoginGroupProfile.Clear();
                //CommonMethods.dict_SymbolCoreSpread.Clear();
                //CommonMethods.dict_SymbolProfileSpread.Clear();
                CommonMethods.dict_UserName.Clear();
                CommonMethods.list_Symbols.Clear();
  
            }catch(Exception ee)
            {
                handleException(ee);
            }
        }


        public void handleException(Exception ex)
        {
            logger.Error(ex);
        }

        public void WriteLog(string message)
        {
            logger.Debug(message);
            
        }


        public void PrintMsg(string message)
        {
            try
            {
                // lst_messages.Items.Insert(0, DateTime.Now.ToString() + " | " + message);
                if (IsHandleCreated)
                {
                    if (InvokeRequired)
                        Invoke((MethodInvoker)(() => { lst_messages.Items.Insert(0, DateTime.Now.ToString() + " | " + message); }));
                    else
                        lst_messages.Items.Insert(0, DateTime.Now.ToString() + " | " + message);

                    Application.DoEvents();
                }

            }
            catch(Exception ee)
            {
                handleException(ee);
            }
        }
    }
    
}


//// var DICT = CommonMethods.dict_LoginGroupProfile;
//var _DICT = CommonMethods.dict_SymbolCoreSpread;
//var DCT = CommonMethods.dict_SymbolProfileSpread;