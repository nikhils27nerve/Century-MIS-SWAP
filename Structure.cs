using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Century_MIS
{


    //AAPL
    // 12-NOV-2021
    //DEAL1
    //DEAL2
    // 13-NOV-2021
    //DEAL1
    //DEAL2

    //EURUSD
    // 12-NOV-2021
    //DEAL1
    //DEAL2 
    //13-NOV-2021
    //DEAL1
    //DEAL2

    //40000
    //12-NOV-2021
    //DEAL1
    //DEAL2  
    //13-NOV-2021
    //DEAL1
    //DEAL2

    //50000
    //12-NOV-2021
    //DEAL1
    //DEAL2
    //13-NOV-201
    //DEAL1
    //DEAL2




    //new coloumn- 
    //contract size
    //Symbol-Instrument
    //dealId- BookingID
    //Name
    //spread-rebate
    //revenue
    //Ordertypes
    //instruemnt types
     

    //[MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    //class Structure_Symbol
    //{
    //    public string Symbol { get; set; }
    //    public long Date { get; set; }
    //    public List<Deal_DATA> Deals = new List<Deal_DATA>();
        
    //}

    //[MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    //class Structure_Login
    //{
    //    public ulong Login { get; set; }
    //    public long Date { get; set; }
    //    public List<Deal_DATA> Deals = new List<Deal_DATA>();
    //}

    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class Structure_Order
    {
        public ulong Login { get; set; }
        public long Date { get; set; }
        public List<Order_data> Orders = new List<Order_data>();
    }

    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class Structure_Position
    {
        public ulong Login { get; set; }
        public long Date { get; set; }

        public List<Position_data> Positions = new List<Position_data>();
    }

   

    public class Position_data
    {
        public uint Action { get; set; }

        public uint ActivationFlags { get; set; }
        public uint ActivationMode { get; set; }

        public double ActivationPrice { get; set; }

        public long ActivationTime { get; set; }

        public string Comment { get; set; }

        public double ContractSize { get; set; }

        public ulong Dealer { get; set; }

        public uint Digits { get; set; }

        public uint DigitsCurrency { get; set; }

        public ulong ExpertID { get; set; }

        public ulong ExpertPositionID { get; set; }
        public string ExternalID { get; set; }

        public ulong Login { get; set; }

        public uint ModificationFlags { get; set; }

        public double ObsoleteValue { get; set; }
        public ulong Position { get; set; }
        public double PriceCurrent { get; set; }

        public double PriceOpen { get; set; }
        public double PriceSL { get; set; }

        public double PriceTP { get; set; }
        public string Print { get; set; }
        public double Profit { get; set; }

        public double RateMargin { get; set; }

        public double RateProfit { get; set; }

        public uint Reason { get; set; }

        public double Storage { get; set; }

        public string Symbol { get; set; }

        public long TimeCreate { get; set; }
        public long TimeCreateMsc { get; set; }

        public long TimeUpdate { get; set; }

        public long TimeUpdateMsc { get; set; }

        public double Volume { get; set; }

        public ulong VolumeExt { get; set; }

        public double Swap { get; set; }
    }

    public class Order_data
    {
        public uint ActivationFlags { get; set; }

        public uint ActivationMode { get; set; }

        public double ActivationPrice { get; set; }

        public long ActivationTime { get; set; }

        public string Comment { get; set; }

        public double ContractSize { get; set; }

        public ulong Dealer { get; set; }

        public uint Digits { get; set; }

        public uint DigitsCurrency { get; set; }

      
        public ulong ExpertID { get; set; }

        public string ExternalID { get; set; }
        
        public ulong Login { get; set; }

        public uint ModificationFlags { get; set; }
        public ulong Order { get; set; }

        public ulong PositionByID { get; set; }

        public ulong PositionID { get; set; }

        public double PriceCurrent { get; set; }

        public double PriceOrder { get; set; }
        public double PriceSL { get; set; }

        public double PriceTP { get; set; }

        public double PriceTrigger { get; set; }
        public string Print { get; set; }
        public double RateMargin { get; set; }

        public uint Reason { get; set; }

      
        public uint State { get; set; }

        public string Symbol { get; set; }

        public long TimeDone { get; set; }

        public long TimeDoneMsc { get; set; }
        public long TimeExpiration { get; set; }

        public long TimeSetup { get; set; }

        public long TimeSetupMsc { get; set; }

        public uint Type { get; set; }
        public uint TypeFill { get; set; }

        public uint TypeTime { get; set; }

        public double VolumeCurrent { get; set; }

        public double VolumeCurrentExt { get; set; }

        public double VolumeInitial { get; set; }
        public double VolumeInitialExt { get; set; }
    }

  

    //public class Order_data
    //{
    //    public ulong Login { get; set; }
    //    public string Symbol { get; set; }  //changed symbol-instrument
    //    public long Time { get; set; }
    //    public ulong OrderID { get; set; }
    //    public string Direction { get; set; }
    //    public string OrderType { get; set; }  //new added 
    //    public double Volume { get; set; }
    //    public double ContractSize { get; set; }   //new added
    //    public double Margin { get; set; }
    //    public double Price { get; set; }
    //    public double SL { get; set; }
    //    public double TP { get; set; }
    //    public string Flag { get; set; }
    //}



    //class Deal_DATA
    //{
    //    public ulong Login { get; set; }
    //    public string Name { get; set; }    //new addded
    //    public string Instrument { get; set; }  //changed symbol-instrument
    //    public long Time { get; set; }
    //    public ulong OrderID { get; set; }

    //    public ulong BookingID { get; set; }    //changed dealid-booking id

    //    public string Direction { get; set; }
    //    public string OrderType { get; set; }  //new added 

    //    public double Volume { get; set; }
    //    public double VolumeClosed { get; set; }
    //    public double ContractSize { get; set; }   //new added
    //    public double Margin { get; set; }


    //    public double Price { get; set; }
    //    public double SL { get; set; }
    //    public double TP { get; set; }

    //    public double Commission { get; set; }
    //    public double Fee { get; set; }
    //    public double Swap { get; set; }


    //    public string Instrument_Type { get; set; }   //new added 
    //    public double SpreadRebate { get; set; }  //new added 
    //    public double CoreSpread { get; set; }
    //    public double Revenue { get; set; }    //new added 

    //}



    //public class MarginRates
    //{
    //    public string Symbol { get; set; }
    //    public double BUY { get; set; }
    //    public double SELL { get; set; }
    //}

    public enum en_OrderFlag
    {
        NEW=0,
        UPDATE=1,
        DELETE=2,
    }


    public enum en_OrderDirection
    {
        BUY = 0,
        SELL = 1,
        BUY_LIMIT = 2,
        SELL_LIMIT = 3,
        BUY_STOP_ENTRY = 4,
        SELL_STOP_ENTRY = 5,
    }


    //class Symbol_data
    //{
    //    public long Date { get; set; }
    //    public List<Deal_DATA> Deals = new List<Deal_DATA>();
    //}

    //class Login_Data
    //{
    //    public long Date { get; set; }
    //    public List<Deal_DATA> Deals = new List<Deal_DATA>();
    //}


  

}
