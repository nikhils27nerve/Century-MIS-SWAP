using DevExpress.Spreadsheet;
using DevExpress.Spreadsheet.Export;
using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Century_MIS
{
    class CManager : CIMTManagerSink
    {

        uint MT5_CONNECT_TIMEOUT = 30000;

        CIMTManagerAPI m_manager = null;
        CDealSink deal_sink = null;
        CSymbol symbol_sink = null;
        COrder Order_sink = null; 
        Century_MIS obj_MIS = null;

        //public void Dispose()
        //{
        //    Shutdown();
        //}


        public bool Initialize(Century_MIS _obj_MIS)
        {
            try
            {
                obj_MIS = _obj_MIS;
                string message = string.Empty;
                MTRetCode res = MTRetCode.MT_RET_OK_NONE;

                if ((res = SMTManagerAPIFactory.Initialize(null)) != MTRetCode.MT_RET_OK)
                {
                    message = string.Format("Loading manager API failed ({0})", res);

                    obj_MIS.PrintMsg(message);
                    return (false);
                }

                m_manager = SMTManagerAPIFactory.CreateManager(SMTManagerAPIFactory.ManagerAPIVersion, out res);


                if ((res != MTRetCode.MT_RET_OK) || (m_manager == null))
                {
                    SMTManagerAPIFactory.Shutdown();
                    message = string.Format("Creating manager interface failed ({0})", (res == MTRetCode.MT_RET_OK ? "Managed API is null" : res.ToString()));
                    obj_MIS.PrintMsg(message);
                    // _order.printMSG(message);

                    return (false);
                }


                deal_sink = new CDealSink();
                if ((res = deal_sink.Initialize(m_manager,obj_MIS)) != MTRetCode.MT_RET_OK)
                {
                    message = "Dealer: creating deal sink failed";
                    obj_MIS.PrintMsg(message);
                  
                    return (false);
                }

                if ((res = m_manager.DealSubscribe(deal_sink)) != MTRetCode.MT_RET_OK)
                {

                    message = "SMTManagerAPIFactory.DealSubscribe failed";
                    obj_MIS.PrintMsg(message);
                    return (false);

                }

                symbol_sink = new CSymbol();
                if ((res = symbol_sink.Initialize(m_manager, obj_MIS)) != MTRetCode.MT_RET_OK)
                {
                    message = "Dealer: creating Symbol sink failed";
                    obj_MIS.PrintMsg(message);
                  
                    return (false);
                }

                if ((res = m_manager.SymbolSubscribe(symbol_sink)) != MTRetCode.MT_RET_OK)
                {
                    message = "SMTManagerAPIFactory.SymbolSubscribe failed";
                    obj_MIS.PrintMsg(message);
                    return (false);
                }

                Order_sink = new COrder();
                if ((res = Order_sink.Initialize(m_manager, obj_MIS)) != MTRetCode.MT_RET_OK)
                {
                    message = "Dealer: creating order sink failed";
                    obj_MIS.PrintMsg(message);

                    return (false);
                }

                if ((res = m_manager.OrderSubscribe(Order_sink)) != MTRetCode.MT_RET_OK)
                {
                    message = "SMTManagerAPIFactory.OrderSubScribe failed";
                    obj_MIS.PrintMsg(message);
                    return (false);
                }

                return (true);
            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                return false;

            }
           
        }

        public void Shutdown()
        {

            if (m_manager != null)
            {
                m_manager.Dispose();
                m_manager = null;
            }

            SMTManagerAPIFactory.Shutdown();
        }

        public bool Login()
        {
            try
            {
                var Server = CommonMethods.readFromConfig("META-TRADER", "SERVER-URL");
                var Password = CommonMethods.readFromConfig("META-TRADER", "MANAGER-PASSWORD");
                var Login = CommonMethods.readFromConfig("META-TRADER", "MANAGER-ID");

                MTRetCode res = m_manager.Connect(Server,Convert.ToUInt64(Login), Password, null, CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL, MT5_CONNECT_TIMEOUT);
                if (res != MTRetCode.MT_RET_OK)
                {
                    // m_manager.LoggerOut(EnMTLogCode.MTLogErr, "Connection failed ({0})", res);
                    return (false);
                }

                return (true);
            }catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                return false;
            }
        }


        public void LoadAllSymbols()
        {
            try
            {
                var list_Symbols = CommonMethods.list_Symbols;
                for (uint i = 0; i <= m_manager.SymbolTotal(); i++)
                {
                    var _Symbol = m_manager.SymbolCreate();
                    if (m_manager.SymbolNext(i, _Symbol) == MTRetCode.MT_RET_OK)
                    {
                        list_Symbols.Add(_Symbol);
                    }
                }

                obj_MIS.PrintMsg("Symbols Loaded");

            }catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                obj_MIS.PrintMsg("Failed to load Symbols");
            }
        }

        public void GetAllUsers()
        {
            try
            {
                var dict_UserName = CommonMethods.dict_UserName;
                //var dict_LoginGroupProfile = CommonMethods.dict_LoginGroupProfile;
                //var dict_GroupProfile = CommonMethods.dict_GroupProfile;
                for (uint i = 0; i < m_manager.GroupTotal(); i++)
                {
                    var group = m_manager.GroupCreate();
                    if (m_manager.GroupNext(i, group) == MTRetCode.MT_RET_OK)
                    {
                        var Gname = group.Group();
                        var userArr = m_manager.UserCreateArray();
                        if (m_manager.UserRequestArray(Gname, userArr) == MTRetCode.MT_RET_OK)
                        {
                            var GroupId = Regex.Match(Gname, @"\d+\.*\d+").Value.Trim();
                            for (uint j = 0; j < userArr.Total(); j++)
                            {
                                var user = userArr.Next(j);
                                dict_UserName.TryAdd(user.Login(), user.Name());
                                if (!GroupId.Trim().Equals(""))
                                {
                                   // var Profile = dict_GroupProfile[Convert.ToInt32(GroupId)];
                                   // dict_LoginGroupProfile.TryAdd(user.Login(), new ConcurrentDictionary<int, string> { [Convert.ToInt32(GroupId)] = Profile });
                                }
                                
                            }
                        }
                    }
                }   
               
            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
            }
        } 
       
        public ConcurrentDictionary<ulong, Position_data> GetAllPostions()
        {
            ConcurrentDictionary<ulong, CIMTPositionArray > dict_ClientPositions = new ConcurrentDictionary<ulong, CIMTPositionArray>();
            ConcurrentDictionary<ulong, List<Position_data>> dict_ClientWisePositions = new ConcurrentDictionary<ulong, List<Position_data>>();
            ConcurrentDictionary<ulong, Position_data> dict_AllPositions = new ConcurrentDictionary<ulong, Position_data>();
            
            try
            {
                for (uint i = 0; i < m_manager.GroupTotal(); i++)
                {
                    var group = m_manager.GroupCreate();
                    if (m_manager.GroupNext(i, group) == MTRetCode.MT_RET_OK)
                    {
                        var Gname = group.Group();
                        var userArr = m_manager.UserCreateArray();
                        if (m_manager.UserRequestArray(Gname, userArr) == MTRetCode.MT_RET_OK)
                        {
                            var GroupId = Regex.Match(Gname, @"\d+\.*\d+").Value.Trim();

                            for (uint j = 0; j < userArr.Total(); j++)
                            {
                                var user = userArr.Next(j);
                                var Positions = m_manager.PositionCreateArray();
                                var result = m_manager.PositionRequest(user.Login(), Positions);
                                if (result == MTRetCode.MT_RET_OK && Positions.Total()>0)
                                {
                                    dict_ClientPositions.TryAdd(user.Login(), Positions);
                                }
                            }
                        }
                    }
                }
                
                foreach(var KVP in dict_ClientPositions)
                {
                    var arr_Positions = KVP.Value;
                    List<Position_data> lst_Positions = new List<Position_data>();
                    for (uint i = 0; i < arr_Positions.Total(); i++)
                    {
                        var Position = arr_Positions.Next(i);
                        var Unmae = "";
                        CommonMethods.dict_UserName.TryGetValue(Position.Login(), out Unmae);

                        Position_data Pos_data = new Position_data();
                        Pos_data= CommonMethods.SetPositionsValues(Position, Pos_data);
                        Pos_data.Swap = Position.Storage();

                        lst_Positions.Add(Pos_data);

                        dict_AllPositions.TryAdd(Position.Position(), Pos_data);
                    }
                   
                    //dict_ClientWisePositions.TryAdd(KVP.Key, lst_Positions);
                }

            }
            catch (Exception ee)
            {
                obj_MIS.handleException(ee);
            }
            return dict_AllPositions;
        }


       public bool ReleaseManagerAPI()
        {
            try
            {
               m_manager.Release();
            }
            catch (Exception ee)
            {
                obj_MIS.handleException(ee); return false;
            };

            return true;
        }


        //client-> position id -> position 

        public void ReadSpradValues()
        {
            try
            {
                DataTable dt_Profile = new DataTable();
                DataTable dt_Market = new DataTable();
               
                string FullFilePath = CommonMethods.readFromConfig("EXCEL-FILE","FILE-PATH");
                string ProfileSheet_Name = CommonMethods.readFromConfig("EXCEL-FILE", "PROFILE-WORKSHEET-NAME");
                string MasterSheet_Name = CommonMethods.readFromConfig("EXCEL-FILE", "MASTER-WORKSHEET-NAME");
                
                Workbook workbook = new Workbook();
                workbook.LoadDocument(FullFilePath, DocumentFormat.Xlsx);
               
                Worksheet worksheet_Profile = workbook.Worksheets[ProfileSheet_Name];
                var range=  worksheet_Profile.GetDataRange();
                dt_Profile = worksheet_Profile.CreateDataTable(range, true);
                DataTableExporter exporter = worksheet_Profile.CreateDataTableExporter(range, dt_Profile, true);
                exporter.Options.ConvertEmptyCells = false;
                exporter.Options.DefaultCellValueToColumnTypeConverter.EmptyCellValue = true;
                exporter.Options.DefaultCellValueToColumnTypeConverter.SkipErrorValues = true;
                exporter.Export();


                var temp_dict = CommonMethods.dict_GroupProfile;

                foreach (DataRow row in dt_Profile.Rows)
                {
                    try
                    {
                        string GROUP = row[1].ToString();
                        string Profile = row[2].ToString().Trim();
                        if (!Profile.Contains("PROFILE"))
                        {
                            temp_dict.TryAdd(Convert.ToInt32(GROUP), Profile);
                        }
                    }catch(Exception ee)
                    {
                        obj_MIS.handleException(ee);
                        continue;
                    }
                }


                Worksheet worksheet_Market = workbook.Worksheets[MasterSheet_Name];
                var _range = worksheet_Market.GetDataRange();
                dt_Market = worksheet_Market.CreateDataTable(_range, true);
                DataTableExporter _exporter = worksheet_Market.CreateDataTableExporter(_range, dt_Market, true);
                _exporter.Options.ConvertEmptyCells = false;
                _exporter.Options.DefaultCellValueToColumnTypeConverter.EmptyCellValue = true;
                _exporter.Options.DefaultCellValueToColumnTypeConverter.SkipErrorValues = true;
                _exporter.Export();

                var dict_SymbolCoreSpread = CommonMethods.dict_SymbolCoreSpread;
                var dict_SymbolProfileSpread = CommonMethods.dict_SymbolProfileSpread;

                foreach(DataRow row in dt_Market.Rows)
                {
                    try
                    {
                        string Symbol = row[2].ToString().Trim();
                        string CoreSpread = row[6].ToString();

                        if (!Symbol.Contains("SYMBOL"))
                        {
                            //if (Symbol.Equals("FKR"))
                            //{
                            //    var TEST = 0;
                            //}
                            dict_SymbolCoreSpread.TryAdd(Symbol, Convert.ToDouble(CoreSpread));

                            var temp_dictonary = new ConcurrentDictionary<string, double>();

                            temp_dictonary.TryAdd("P1", (row[7].ToString() == "") ? 0 : Convert.ToDouble(row[8]));
                            temp_dictonary.TryAdd("P2", (row[8].ToString() == "") ? 0 : Convert.ToDouble(row[8]));
                            temp_dictonary.TryAdd("P3", (row[9].ToString() == "") ? 0 : Convert.ToDouble(row[9]));
                            temp_dictonary.TryAdd("P4", (row[10].ToString() == "") ? 0 : Convert.ToDouble(row[10]));
                            temp_dictonary.TryAdd("P5", (row[11].ToString() == "") ? 0 : Convert.ToDouble(row[11]));
                            temp_dictonary.TryAdd("P6", (row[12].ToString() == "") ? 0 : Convert.ToDouble(row[12]));
                            temp_dictonary.TryAdd("P7", (row[13].ToString() == "") ? 0 : Convert.ToDouble(row[13]));
                            temp_dictonary.TryAdd("P8", (row[14].ToString() == "") ? 0 : Convert.ToDouble(row[14]));
                            temp_dictonary.TryAdd("P9", (row[15].ToString() == "") ? 0 : Convert.ToDouble(row[15]));

                            dict_SymbolProfileSpread.TryAdd(Symbol, temp_dictonary);

                        }
                    }catch(Exception EE)
                    {
                        // var SYMBOL = row[2].ToString();
                        obj_MIS.handleException(EE);
                        continue;
                    }
                }

            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                obj_MIS.PrintMsg("Error while reading excel file");
            }

        }        
    }

  
   


    class CSymbol : CIMTConSymbolSink
    {
        CIMTManagerAPI m_manager = null;
        Century_MIS obj_MIS = null;
        

        public MTRetCode Initialize(CIMTManagerAPI manager,Century_MIS _Object)
        {
            // _order = _Order;
            if (manager == null)
                return (MTRetCode.MT_RET_ERR_PARAMS);
            m_manager = manager;
            obj_MIS = _Object;
            return (RegisterSink());

        }

        public override void OnSymbolAdd(CIMTConSymbol _Symbol)
        {
            CommonMethods.list_Symbols.Add(_Symbol);
        }
        public override void OnSymbolUpdate(CIMTConSymbol _Symbol)
        {
            try
            {
                var list_Symbols = CommonMethods.list_Symbols;

                for (int i = 0; i < list_Symbols.Count; i++)
                {
                    var symbolName = list_Symbols[i].Symbol();
                    if (symbolName.Equals(_Symbol.Symbol()))
                    {
                        list_Symbols[i] = _Symbol;
                    }

                }
            }catch(Exception ee) {

                obj_MIS.handleException(ee);
            
            }

        }
        public override void OnSymbolDelete(CIMTConSymbol symbol)
        {
            
        }

        public override void OnSymbolSync()
        {
           
        }

    }


    class COrder:CIMTOrderSink
    {
        CIMTManagerAPI m_manager = null;
        Century_MIS obj_MIS = null;

        public MTRetCode Initialize(CIMTManagerAPI manager, Century_MIS _object)
        {
            // _order = _Order;
            if (manager == null)
                return (MTRetCode.MT_RET_ERR_PARAMS);
            m_manager = manager;
            obj_MIS = _object;
            return (RegisterSink());
        }

        public override void OnOrderAdd(CIMTOrder order)
        {
            try
            {
                var _OrderDirection = ((en_OrderDirection)order.Type()).ToString();
                var _OrderType = "";
                var arr_OrderType = _OrderDirection.Split('_');
                if (arr_OrderType.Length == 1)
                    _OrderType = "MARKET";
                else if (arr_OrderType[1] == "LIMIT")
                    _OrderType = "LIMIT";
                else
                    _OrderType = "STOP ENTRY";
            
                var dict_UserName = CommonMethods.dict_UserName;
                var UName = "";
                if (dict_UserName.TryGetValue(order.Login(), out string _Name))
                {
                    UName = _Name;
                }

                Order_data _OrderData = new Order_data();
                _OrderData= CommonMethods.SetOrderValues(order, _OrderData);


                //Order_data _OrderData = new Order_data()
                //{
                //    OrderID = order.Order(),
                //    Login = order.Login(),
                //    Symbol = order.Symbol(), 
                //    Direction = _OrderDirection.Contains("BUY") ? "BUY" : "SELL",
                //    OrderType = _OrderType,
                //    Price = order.PriceOrder(),  //PRICEORDER
                //    SL = order.PriceSL(),
                //    TP = order.PriceTP(),
                //    ContractSize = order.ContractSize(),
                //    Volume = Math.Round((Convert.ToDouble(order.VolumeInitial()) / 10000), 2),
                //    Margin = order.RateMargin(),
                //    Time = CommonMethods.ConvertToUnixTimestamp(DateTime.UtcNow),                   
                //    Flag = en_OrderFlag.NEW.ToString(),
                //};

                var result =CommonMethods.AddToMongo_Order(_OrderData);
                if (result == true)
                {
                    obj_MIS.WriteLog("Order dumped to MongoDB " + order.Print());
                }
                else
                {
                    obj_MIS.WriteLog("Order failed to dump in MongoDB " + order.Print());
                }
            }catch(Exception ee)
            {
                obj_MIS.handleException(ee);
            }
        }

        public override void OnOrderUpdate(CIMTOrder order)
        {
            try
            {
                var _OrderDirection = ((en_OrderDirection)order.Type()).ToString();
                var _OrderType = "";
                var arr_OrderType = _OrderDirection.Split('_');
                if (arr_OrderType.Length == 1)
                    _OrderType = "MARKET";
                else if (arr_OrderType[1] == "LIMIT")
                    _OrderType = "LIMIT";
                else
                    _OrderType = "STOP ENTRY";

                var dict_UserName = CommonMethods.dict_UserName;
                var UName = "";
                if (dict_UserName.TryGetValue(order.Login(), out string _Name))
                {
                    UName = _Name;
                }

                Order_data _OrderData = new Order_data();
                _OrderData = CommonMethods.SetOrderValues(order, _OrderData);

                //Order_data _OrderData = new Order_data()
                //{
                //    OrderID = order.Order(),
                //    Login = order.Login(),
                //    Symbol = order.Symbol(),
                //    Direction = _OrderDirection.Contains("BUY") ? "BUY" : "SELL",
                //    OrderType = _OrderType,
                //    Price = order.PriceOrder(),  //PRICEORDER
                //    SL = order.PriceSL(),
                //    TP = order.PriceTP(),
                //    ContractSize = order.ContractSize(),
                //    Volume = Math.Round((Convert.ToDouble(order.VolumeInitial()) / 10000), 2),
                //    Margin = order.RateMargin(),
                //    Time = CommonMethods.ConvertToUnixTimestamp(DateTime.UtcNow),
                //    //TIME CHECK
                //    Flag = en_OrderFlag.UPDATE.ToString(),
                //};

                var result = CommonMethods.AddToMongo_Order(_OrderData);
                if (result == true)
                {
                    obj_MIS.WriteLog("Order dumped to MongoDB " + order.Print());
                }
                else
                {
                    obj_MIS.WriteLog("Order failed to dump in MongoDB " + order.Print());
                }
            }
            catch (Exception ee)
            {
                obj_MIS.handleException(ee);
            }
        }

        public override void OnOrderDelete(CIMTOrder order)
        {
            try
            {
                var _OrderDirection = ((en_OrderDirection)order.Type()).ToString();
                var _OrderType = "";
                var arr_OrderType = _OrderDirection.Split('_');
                if (arr_OrderType.Length == 1)
                    _OrderType = "MARKET";
                else if (arr_OrderType[1] == "LIMIT")
                    _OrderType = "LIMIT";
                else
                    _OrderType = "STOP ENTRY";

                var dict_UserName = CommonMethods.dict_UserName;
                var UName = "";
                if (dict_UserName.TryGetValue(order.Login(), out string _Name))
                {
                    UName = _Name;
                }

                Order_data _OrderData = new Order_data();
                _OrderData = CommonMethods.SetOrderValues(order, _OrderData);


                //Order_data _OrderData = new Order_data()
                //{
                //    OrderID = order.Order(),
                //    Login = order.Login(),
                //    Symbol = order.Symbol(),
                //    Direction = _OrderDirection.Contains("BUY") ? "BUY" : "SELL",
                //    OrderType = _OrderType,
                //    Price = order.PriceOrder(),
                //    SL = order.PriceSL(),
                //    TP = order.PriceTP(),
                //    ContractSize = order.ContractSize(), 
                //    Volume = Math.Round((Convert.ToDouble(order.VolumeInitial()) / 10000), 2),
                //    Margin = order.RateMargin(),
                //    Time = CommonMethods.ConvertToUnixTimestamp(DateTime.UtcNow),

                //    Flag = en_OrderFlag.DELETE.ToString(),
                //};

                var result = CommonMethods.AddToMongo_Order(_OrderData);
                if (result == true)
                {
                    obj_MIS.WriteLog("Order dumped to MongoDB " + order.Print());
                }
                else
                {
                    obj_MIS.WriteLog("Order failed to dump in MongoDB " + order.Print());
                }
            }
            catch (Exception ee)
            {
                obj_MIS.handleException(ee);
            }
          
        }
     
    }
 
    class CDealSink : CIMTDealSink
    {
        CIMTManagerAPI m_manager = null;
        Century_MIS obj_MIS = null;

        public MTRetCode Initialize(CIMTManagerAPI manager,Century_MIS _object)
        {
           // _order = _Order;
            if (manager == null)
                return (MTRetCode.MT_RET_ERR_PARAMS);
            m_manager = manager;
            obj_MIS = _object;
            return (RegisterSink());

        }

        public override void OnDealAdd(CIMTDeal deal)  ///
        {
            //try
            //{
            //    if (deal != null)
            //    {
            //        //  var _Direction=

            //        var _Direction = deal.Action();

            //        //var _Direction = ((EnDealAction)deal.Action()).ToString();
            //        //_Direction = _Direction.Replace("FIRST", "BUY").Replace("DEAL_", "");

            //        if (_Direction == 0 || _Direction == 1)
            //        {

            //            var dict_LoginGroupProfile = CommonMethods.dict_LoginGroupProfile;
            //            var dict_SymbolProfileSpread = CommonMethods.dict_SymbolProfileSpread;
            //            var dict_SymbolCoreSpread = CommonMethods.dict_SymbolCoreSpread;

            //            //getting username
            //            var dict_UserName = CommonMethods.dict_UserName;
            //            var UName = "";
            //            if (dict_UserName.TryGetValue(deal.Login(), out string _Name))
            //            {
            //                UName = _Name;
            //            }

            //            //getting marginMaintenance
            //            var Symbol = CommonMethods.list_Symbols.Where(x => x.Symbol().Equals(deal.Symbol())).FirstOrDefault();
            //            double _Margin = 0;
            //            if (Symbol != null)
            //            {
            //                _Margin = Math.Round(deal.Price() * deal.RateMargin() * Symbol.MarginRateMaintenance(deal.Action()), 3);
            //            }

            //            //getting OrderType
            //            var _order = m_manager.OrderCreate();
            //            m_manager.OrderGet(deal.Order(), _order);
            //            var _OrderDirection = ((en_OrderDirection)_order.Type()).ToString();

            //            var _OrderType = "";
            //            var arr_OrderType = _OrderDirection.Split('_');
            //            if (arr_OrderType.Length == 1)
            //                _OrderType = "MARKET";
            //            else if (arr_OrderType[1] == "LIMIT")
            //                _OrderType = "LIMIT";
            //            else
            //                _OrderType = "STOP ENTRY";
 
            //            //getting sperad rebate
            //            double _SpreadRebate = 0;

            //            if (dict_LoginGroupProfile.TryGetValue(deal.Login(), out ConcurrentDictionary<int, string> _dict_GroupProfile))
            //            {
            //                var Group = _dict_GroupProfile.Keys.FirstOrDefault();
            //                var Profile = dict_LoginGroupProfile[deal.Login()][Group];
            //                double ProfileSpread = 0;
            //                if (dict_SymbolProfileSpread.TryGetValue(deal.Symbol(), out ConcurrentDictionary<string, double> _dict_ProfileSpread))
            //                {
            //                    ProfileSpread = _dict_ProfileSpread[Profile];
            //                }
            //                var Volume = Math.Round((Convert.ToDouble(deal.Volume()) / 10000), 2);
            //                var ret = deal.RateMargin();
            //                _SpreadRebate = Math.Round(((deal.ContractSize() * ProfileSpread * Volume * deal.RateMargin()) / 2), 8);

            //            }

            //            //getting corespread
            //            double _CoreSpread = 0;
            //            if (dict_SymbolCoreSpread.TryGetValue(deal.Symbol(), out double CSpread))
            //            {
            //                var Volume = Math.Round((Convert.ToDouble(deal.Volume()) / 10000), 2);

            //                _CoreSpread = Math.Round(((deal.ContractSize() * CSpread * Volume * deal.RateMargin()) / 2), 8);

            //            }


            //            var DEAL_CLOSE = deal.VolumeClosed();

            //            Deal_DATA data = new Deal_DATA()
            //            {
            //                Login = deal.Login(),
            //                Name = UName,
            //                Instrument = deal.Symbol(),
            //                OrderID = deal.Order(),
            //                BookingID = deal.Deal(),
            //                Direction = (_Direction==0)?"BUY":"SELL",
            //                Volume = Math.Round((Convert.ToDouble(deal.Volume()) / 10000), 2),
            //                VolumeClosed = Math.Round((Convert.ToDouble(deal.VolumeClosed()) / 10000), 2),
            //                Price = deal.Price(),
            //                SL = deal.PriceSL(),
            //                TP = deal.PriceTP(),
            //                Time = deal.Time(),
            //                Commission = deal.Commission(),
            //                Fee = deal.Fee(),
            //                Margin = _Margin,
            //                Swap = 0,
            //                Instrument_Type = "",
            //                SpreadRebate = _SpreadRebate,
            //                Revenue = 0,
            //                OrderType = _OrderType,
            //                ContractSize = deal.ContractSize(),
            //                CoreSpread = _CoreSpread,
            //            };

            //            var Symbol_result = CommonMethods.AddToMongo_Symbol(data);
            //            var Login_result = true;//CommonMethods.AddToMongo_Login(data);
            //            if (Symbol_result == true && Login_result == true)
            //            {
            //                obj_MIS.PrintMsg("Deal added to MongoDB : " + deal.Print());
            //            }
            //            else
            //            {
            //                obj_MIS.PrintMsg("Falied to add Deal in MongoDB : " + deal.Print());
            //            }

            //        }

            //    }
            //}catch(Exception EE)
            //{
            //    obj_MIS.handleException(EE);
            //}
        }

        public override void OnDealUpdate(CIMTDeal deal)
        {
            if (deal != null)
            {
                string str = deal.Print();

                //  MessageBox.Show("deal updated "+str);
                //   m_manager.LoggerOut(EnMTLogCode.MTLogOK, "{0} deal has been updated", str);
            }
        }

        public override void OnDealDelete(CIMTDeal deal)
        {
            if (deal != null)
            {
                string str = deal.Print();

                //      MessageBox.Show("deal deleted"+str);
                //  m_manager.LoggerOut(EnMTLogCode.MTLogOK, "{0} deal has been deleted", str);
            }
        }

    }



}
