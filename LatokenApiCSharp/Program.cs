using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {

        string apiKey = "YourApiKey";
        string apiSecret = "YouArpiSecret";
        string idCointToTrade = "56ea7d5d-e233-4a71-9f6d-0b264d50dd07"; //PBT COIN
        string tickerCoinToTrade = "PBT"; //TICKER PBT
        string quantityLImitOrder = "0.02";
        string name = "Name of your account";
        

        ClientLatoken client = new ClientLatoken(apiKey, apiSecret, tickerCoinToTrade, idCointToTrade, quantityLImitOrder, name);

        //ORDER BOOK RESPECT TO /USDT
        ClientLatoken.OrderBook.Response book = client.getBook(null, null);


    }
}


