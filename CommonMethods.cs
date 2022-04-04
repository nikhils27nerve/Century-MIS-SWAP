using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using System.Collections.Concurrent;
using System.Reflection;

namespace Century_MIS
{
    class CommonMethods
    {

        static Century_MIS obj_MIS = null;
        static DataSet ds_config = new DataSet();


        /// <summary>
        /// list of all symbols
        /// </summary>
        public static List<CIMTConSymbol> list_Symbols = new List<CIMTConSymbol>();

        /// <summary>
        /// key-groupName | key- Symbol | value- MarginRates  //currently not using
        /// </summary>
        //public static ConcurrentDictionary<string, ConcurrentDictionary<string, MarginRates>> dict_GroupMargin = new ConcurrentDictionary<string, ConcurrentDictionary<string, MarginRates>>();

        /// <summary>
        /// key-LoginId | value - UserName
        /// </summary>
        public static ConcurrentDictionary<ulong, string> dict_UserName = new ConcurrentDictionary<ulong, string>();

        /// <summary>
        /// key- LoginId | key- group (only digits) | value- profile
        /// </summary>
        public static ConcurrentDictionary<ulong, ConcurrentDictionary<int, string>> dict_LoginGroupProfile = new ConcurrentDictionary<ulong, ConcurrentDictionary<int, string>>();

        /// <summary>
        /// key- symbol | value- core spread
        /// </summary>
        public static ConcurrentDictionary<string, double> dict_SymbolCoreSpread = new ConcurrentDictionary<string, double>();


        /// <summary>
        /// key- symbol | key- profile | value- spread 
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, double>> dict_SymbolProfileSpread = new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>();

        /// <summary>
        /// key- GroupId | value- Profile  [temperory use only]
        /// </summary>        
        public static ConcurrentDictionary<int, string> dict_GroupProfile = new ConcurrentDictionary<int, string>();

        /// <summary>
        /// key - PositionID | value - previous day swap
        /// </summary>       
        public static ConcurrentDictionary<ulong, Position_data> dict_PrevDayPositions = new ConcurrentDictionary<ulong, Position_data>();

        public void Initialize(Century_MIS _obj_MIS)
        {
            try
            {
                obj_MIS = _obj_MIS;
                var config_path = Application.StartupPath + "\\Config.xml";
                ds_config.ReadXml(config_path);
            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                obj_MIS.PrintMsg("Unable to read Config file");
            }
        }


        public void HandleException(Exception ee)
        {
            obj_MIS.handleException(ee);

        }


        public static string readFromConfig(string TableName, string ColumnName)
        {
            try
            {
                return ds_config.Tables[TableName].Rows[0][ColumnName].ToString();
            }catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                return " ";
            }
        }

       
        //public static  bool AddToMongo_Symbol(Deal_DATA _Deal)
        //{
        //    try {

        //        var ConnectionString = readFromConfig("MONGO-DB", "CONNECTION-STRING");
        //        var DbName = readFromConfig("MONGO-DB", "DB-NAME");

        //        var client = new MongoClient(ConnectionString);
        //        var db = client.GetDatabase(DbName);

        //        var collection = db.GetCollection<Structure_Symbol>("Symbol");
        //        var deal_date = ConvertFromUnixTimestamp(_Deal.Time).Date;

        //        var SymbolResult = collection.Find(x => x.Symbol.Equals(_Deal.Instrument) && x.Date == ConvertToUnixTimestamp(deal_date)).FirstOrDefault();
        //        if (SymbolResult == null)
        //        {
        //            Structure_Symbol Struct_Symbol = new Structure_Symbol();
        //            Struct_Symbol.Symbol = _Deal.Instrument;
        //            Struct_Symbol.Date = ConvertToUnixTimestamp(deal_date);
        //            Struct_Symbol.Deals.Add(_Deal);

        //            collection.InsertOne(Struct_Symbol);
        //        }
        //        else
        //        {
        //            SymbolResult.Deals.Add(_Deal);
        //            collection.ReplaceOne(x => x.Symbol.Equals(_Deal.Instrument) && x.Date == ConvertToUnixTimestamp(deal_date), SymbolResult);
        //        }
        //        return true;
        //    }
        //    catch (Exception ee)
        //    {
        //        obj_MIS.handleException(ee);
        //        return false;
        //    }
           
        //}


        public static bool AddToMongo_Positions(ConcurrentDictionary<ulong,Position_data> dict_AllPositions)
        {
            try
            {
                var dict_clientWisePositions = dict_AllPositions.Values.GroupBy(x => x.Login).ToDictionary(k => k.Key, v => v.ToList());
                var date = ConvertToUnixTimestamp(DateTime.UtcNow.AddDays(-1).Date);
                var ConnectionString = readFromConfig("MONGO-DB", "CONNECTION-STRING");
                var DbName = readFromConfig("MONGO-DB", "DB-NAME");
                 
                var client = new MongoClient(ConnectionString);
                var db = client.GetDatabase(DbName);

                var collection = db.GetCollection<Structure_Position>("Positions");

                foreach (var ClientPostions in dict_clientWisePositions)
                { 
                    var LoginResult = collection.Find(x => x.Login == ClientPostions.Key && x.Date==date).FirstOrDefault();
                    if (LoginResult == null)
                    {
                        Structure_Position struct_Position = new Structure_Position();
                        struct_Position.Login = ClientPostions.Key;
                        struct_Position.Date = date;
                        struct_Position.Positions = ClientPostions.Value;
                        collection.InsertOne(struct_Position);
                    }
                    else
                    {
                        LoginResult.Positions = ClientPostions.Value;
                        collection.ReplaceOne(x => x.Login == ClientPostions.Key && x.Date == date,LoginResult);
                    }
                }

                return true;

            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                return false;
            }
        }

        //todo order time done and swap sign - or +
        public static bool AddToMongo_Order(Order_data _Order) 
        {
            try
            {
                var ConnectionString = readFromConfig("MONGO-DB", "CONNECTION-STRING");
                var DbName = readFromConfig("MONGO-DB", "DB-NAME");

                var client = new MongoClient(ConnectionString);
                var db = client.GetDatabase(DbName);

                var collection = db.GetCollection<Structure_Order>("Orders");
                var order_date = ConvertToUnixTimestamp(DateTime.UtcNow.Date);

                var LoginResult = collection.Find(x => x.Login == _Order.Login && x.Date == order_date).FirstOrDefault();
                if (LoginResult == null)
                {
                    Structure_Order struct_Order = new Structure_Order();
                    struct_Order.Login = _Order.Login;
                    struct_Order.Date = order_date;
                    struct_Order.Orders.Add(_Order);
                    collection.InsertOne(struct_Order);
                }
                else
                {
                    LoginResult.Orders.Add(_Order);
                    collection.ReplaceOne(x => x.Login == _Order.Login && x.Date ==order_date, LoginResult);
                }

                return true;
            }catch(Exception ee)
            {
                obj_MIS.handleException(ee);
                return false;
            }
      
        }
        
        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - origin;
            return Convert.ToInt64(diff.TotalSeconds);
        }

        public static T CopyPropertiesFrom<T>(CIMTOrder FromObj, T ToObj)
        {
            foreach (PropertyInfo propTo in ToObj.GetType().GetProperties())
            {
                PropertyInfo propFrom = FromObj.GetType().GetProperty(propTo.Name);
                if (propFrom != null && propFrom.CanWrite)
                    propTo.SetValue(ToObj, propFrom.GetValue(FromObj, null), null);
            }

            return ToObj;
        }


        public static Order_data SetOrderValues(CIMTOrder FromOrd,Order_data ToOrd) 
        {
            ToOrd.ActivationFlags = FromOrd.ActivationFlags();
            ToOrd.ActivationMode = FromOrd.ActivationMode();
            ToOrd.ActivationPrice = FromOrd.ActivationPrice();
            ToOrd.ActivationTime = FromOrd.ActivationTime();
            ToOrd.Comment = FromOrd.Comment().ToString();
            ToOrd.ContractSize = FromOrd.ContractSize();
            ToOrd.Dealer = FromOrd.Dealer();
            ToOrd.Digits = FromOrd.Digits();
            ToOrd.DigitsCurrency = FromOrd.DigitsCurrency();
            ToOrd.ExpertID = FromOrd.ExpertID();
            ToOrd.ExternalID = FromOrd.ExternalID();
            ToOrd.Login = FromOrd.Login();
            ToOrd.ModificationFlags = FromOrd.ModificationFlags();
            ToOrd.Order = FromOrd.Order();
            ToOrd.PositionByID = FromOrd.PositionByID();
            ToOrd.PositionID = FromOrd.PositionID();
            ToOrd.PriceCurrent = FromOrd.PriceCurrent();
            ToOrd.PriceOrder = FromOrd.PriceOrder();
            ToOrd.PriceSL = FromOrd.PriceSL();
            ToOrd.PriceTP = FromOrd.PriceTP();
            ToOrd.PriceTrigger = FromOrd.PriceTrigger();
            ToOrd.Print = FromOrd.Print();
            ToOrd.RateMargin = FromOrd.RateMargin();
            ToOrd.Reason = FromOrd.Reason();
            ToOrd.State = FromOrd.State();
            ToOrd.Symbol = FromOrd.Symbol();
            ToOrd.TimeDone = FromOrd.TimeDone();
            ToOrd.TimeDoneMsc = FromOrd.TimeDoneMsc();
            ToOrd.TimeExpiration = FromOrd.TimeExpiration();
            ToOrd.TimeSetup = FromOrd.TimeSetup();
            ToOrd.TimeSetupMsc = FromOrd.TimeSetupMsc();
            ToOrd.Type = FromOrd.Type();
            ToOrd.TypeFill = FromOrd.TypeFill();
            ToOrd.TypeTime = FromOrd.TypeTime();
            ToOrd.VolumeCurrent = Math.Round((Convert.ToDouble(FromOrd.VolumeCurrent())) / 10000, 2); 
            ToOrd.VolumeCurrentExt = Math.Round((Convert.ToDouble(FromOrd.VolumeCurrentExt())) / 10000, 2);
            ToOrd.VolumeInitial = Math.Round((Convert.ToDouble(FromOrd.VolumeInitial())) / 10000, 2);
            ToOrd.VolumeInitialExt = Math.Round((Convert.ToDouble(FromOrd.VolumeInitialExt())) / 10000, 2);

            return ToOrd;
        }


        public static Position_data SetPositionsValues(CIMTPosition FromPos , Position_data ToPos)
        {
            ToPos.ActivationMode = FromPos.ActivationMode();
            ToPos.ActivationPrice = FromPos.ActivationPrice();
            ToPos.ActivationTime = FromPos.ActivationTime();
            ToPos.Comment = FromPos.Comment();
            ToPos.ContractSize = FromPos.ContractSize();
            ToPos.Dealer = FromPos.Dealer();
            ToPos.Digits = FromPos.Digits();
            ToPos.DigitsCurrency = FromPos.DigitsCurrency();
            ToPos.ExpertID = FromPos.ExpertID();
            ToPos.ExpertPositionID = FromPos.ExpertPositionID();
            ToPos.ExternalID = FromPos.ExternalID();
            ToPos.Login = FromPos.Login();
            ToPos.ModificationFlags = FromPos.ModificationFlags();
            ToPos.ObsoleteValue = FromPos.ObsoleteValue();
            ToPos.Position = FromPos.Position();
            ToPos.PriceCurrent = FromPos.PriceCurrent();
            ToPos.PriceOpen = FromPos.PriceOpen();
            ToPos.PriceSL = FromPos.PriceSL();
            ToPos.PriceTP = FromPos.PriceTP();
            ToPos.Print = FromPos.Print();
            ToPos.Profit = FromPos.Profit();
            ToPos.RateMargin = FromPos.RateMargin();
            ToPos.RateProfit = FromPos.RateProfit();
            ToPos.Reason = FromPos.Reason();
            
            ToPos.Storage = FromPos.Storage();
            ToPos.Symbol = FromPos.Symbol();
            ToPos.TimeCreate = FromPos.TimeCreate();
            ToPos.TimeCreateMsc = FromPos.TimeCreateMsc();
            ToPos.TimeUpdate = FromPos.TimeUpdate();
            ToPos.TimeUpdateMsc = FromPos.TimeUpdateMsc();
            ToPos.Volume = Math.Round((Convert.ToDouble(FromPos.Volume())) / 10000, 2); //FromPos.Volume();
            ToPos.VolumeExt = FromPos.VolumeExt();
          
            return ToPos;
        }



        public static T CopyPropertiesFromPosition<T>(CIMTPosition FromObj, T ToObj)
        {
            try
            {
                foreach (PropertyInfo propTo in ToObj.GetType().GetProperties())
                {
                    
                    PropertyInfo propFrom = FromObj.GetType().GetProperty(propTo.Name);
                    propTo.SetValue(ToObj, FromObj, null);
                    if (propFrom != null && propFrom.CanWrite)
                        propTo.SetValue(ToObj, propFrom.GetValue(FromObj, null), null);
                }
            }
            catch(Exception ee)
            {
                obj_MIS.handleException(ee);
            }
            

            return ToObj;
        }



    }
}

//public static bool AddToMongoDb(Deal_DATA _Deal)
//{
//    try
//    {
//        var ConnectionString = readFromConfig("MONGO-DB", "CONNECTION-STRING");
//        var DbName = readFromConfig("MONGO-DB", "DB-NAME");

//        var client = new MongoClient(ConnectionString);
//        var db = client.GetDatabase(DbName);


//        //adding in symbol
//        var collection = db.GetCollection<Structure_Symbol>("Symbol");

//        var deal_date = ConvertFromUnixTimestamp(_Deal.Time).Date;

//        var symbolResult = collection.Find(x => x.Symbol.Equals(_Deal.Symbol) && x.Date == ConvertToUnixTimestamp(deal_date)).Limit(1).First();

//        if (symbolResult == null || symbolResult.Count == 0)
//        {

//            Symbol_data sy_data = new Symbol_data();
//            sy_data.Date = ConvertToUnixTimestamp(deal_date);
//            sy_data.Deals.Add(_Deal);

//            Structure_Symbol struct_symbol = new Structure_Symbol()
//            {
//                Symbol = _Deal.Symbol,
//                Dates = new List<Symbol_data> { sy_data },
//            };
//            collection.InsertOne(struct_symbol);
//        }
//        else
//        {
//            var Dates = symbolResult[0].Dates;
//            var deal_date = ConvertFromUnixTimestamp(_Deal.Time).Date;  //new Datetime(2021,12,12);

//            var Date = Dates.Where(x => x.Date == ConvertToUnixTimestamp(deal_date)).FirstOrDefault();

//            if (Date != null)
//            {
//                Dates.Remove(Date);
//                Date.Deals.Add(_Deal);
//                Dates.Add(Date);

//            }
//            else
//            {
//                Symbol_data sy_data = new Symbol_data();
//                sy_data.Date = ConvertToUnixTimestamp(deal_date);
//                sy_data.Deals.Add(_Deal);
//                Dates.Add(sy_data);
//            }

//            symbolResult[0].Dates = Dates;
//            collection.ReplaceOne(x => x.Symbol.Equals(_Deal.Symbol), symbolResult[0]);
//        }

//        return true;

//    }
//    catch (Exception ee)
//    {
//        MessageBox.Show(ee.Message);

//        return false;
//    }

//}

//public static bool AddToMongo_Login(Deal_DATA _Deal)
//{
//    try
//    {
//        var ConnectionString = readFromConfig("MONGO-DB", "CONNECTION-STRING");
//        var DbName = readFromConfig("MONGO-DB", "DB-NAME");

//        var client = new MongoClient(ConnectionString);
//        var db = client.GetDatabase(DbName);


//        //adding in symbol
//        var collection = db.GetCollection<Structure_Login>("Login");

//        var LoginResult = collection.Find(x => x.Login == _Deal.Login).Limit(1).ToList();

//        if (LoginResult == null || LoginResult.Count == 0)
//        {
//            var deal_date = ConvertFromUnixTimestamp(_Deal.Time).Date;
//            Login_Data lo_data = new Login_Data();
//            lo_data.Date = ConvertToUnixTimestamp(deal_date);
//            lo_data.Deals.Add(_Deal);

//            Structure_Login struct_login = new Structure_Login()
//            {
//                Login = _Deal.Login,
//                Dates = new List<Login_Data> { lo_data },
//            };

//            collection.InsertOne(struct_login);

//        }
//        else
//        {
//            var Dates = LoginResult[0].Dates;
//            var deal_date = ConvertFromUnixTimestamp(_Deal.Time).Date;  //new Datetime(2021,12,12);

//            var Date = Dates.Where(x => x.Date == ConvertToUnixTimestamp(deal_date)).FirstOrDefault();

//            if (Date != null)
//            {
//                Dates.Remove(Date);
//                Date.Deals.Add(_Deal);
//                Dates.Add(Date);

//            }
//            else
//            {
//                Login_Data lo_data = new Login_Data();
//                lo_data.Date = ConvertToUnixTimestamp(deal_date);
//                lo_data.Deals.Add(_Deal);
//                Dates.Add(lo_data);
//            }

//            LoginResult[0].Dates = Dates;
//            collection.ReplaceOne(x => x.Login == _Deal.Login, LoginResult[0]);
//        }

//        return true;

//    }
//    catch (Exception ee)
//    {
//        obj_MIS.handleException(ee);
//        return false;
//    }


//}
