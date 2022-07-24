using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

public class Strutture
{
    // Some constant used in ApiLatoken
    public const string ask = "ASK";
    public const string bid = "BID";
    public const string baseUrl = "https://api.latoken.com";
    public const string m_quoteCurrency = "0c3a106d-bde3-4c13-a26e-3fd2394529e5";
    public const string m_tickerCurrency = "USDT";
    public System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo()
    {
        NumberDecimalSeparator = "."
    };
    // Structures used to get the json response from the server side
    public class Order
    {
        public class Returneds
        {
            public string id { get; set; }
            public string status { get; set; }
            public string side { get; set; }
            public string condition { get; set; }
            public string type { get; set; }
            public string baseCurrency { get; set; }
            public string quoteCurrency { get; set; }
            public string clientOrderId { get; set; }
            public string price { get; set; }
            public string quantity { get; set; }
            public string cost { get; set; }
            public string filled { get; set; }
            public string trader { get; set; }           
            public string creator { get; set; }
            public string creatorId { get; set; }
            public object timestamp { get; set; }
        }
        public class SingleOrder
        {
            public string id { get; set; }
            public string status { get; set; }
            public string side { get; set; }
            public string condition { get; set; }
            public string type { get; set; }
            public string baseCurrency { get; set; }
            public string quoteCurrency { get; set; }
            public string clientOrderId { get; set; }
            public string price { get; set; }
            public string quantity { get; set; }
            public string cost { get; set; }
            public string filled { get; set; }
            public string trader { get; set; }
            public object timestamp { get; set; }
            public string creator { get; set; }
            public string creatorId { get; set; }
        }


    }
    public class CancellOrder
    {
        public class Errors
        {
            public string property1 { get; set; }
            public string property2 { get; set; }
        }

        public class Response
        {
            public string message { get; set; }
            public string status { get; set; }
            public string error { get; set; }
            public Errors errors { get; set; }
        }
    }
    public class BalancesResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public long timestamp { get; set; }
        public string currency { get; set; }
        public string available { get; set; }
        public string blocked { get; set; }

    }
    public class OrderBook
    {
        public class Ask
        {
            public string price { get; set; }
            public string quantity { get; set; }
            public string cost { get; set; }
            public string accumulated { get; set; }
        }

        public class Bid
        {
            public string price { get; set; }
            public string quantity { get; set; }
            public string cost { get; set; }
            public string accumulated { get; set; }
        }

        public class Response
        {
            public List<Ask> ask { get; set; }
            public List<Bid> bid { get; set; }
            public string totalAsk { get; set; }
            public string totalBid { get; set; }
        }
    }
    public class OrderSubmitted
    {

        public class Errors
        {
            public string property1 { get; set; }
            public string property2 { get; set; }
        }

        public class Response
        {
            public string id { get; set; }
            public string message { get; set; }
            public string status { get; set; }
            public string error { get; set; }
            public Errors errors { get; set; }
        }

    }
}

