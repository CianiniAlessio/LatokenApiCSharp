using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
public class ClientLatoken : Strutture
{
	private string apiKey;
	private string apiSecret;	
	private string m_quantityLimitOrder;
	private string m_tickerCoin;
	private string m_idCoin;
	private string m_NameAccount;
	private List<OrderSubmitted.Response> m_OrderResponsesLimitSell;
	private List<OrderSubmitted.Response> m_OrderResponsesMarketSell;
	private List<OrderSubmitted.Response> m_OrderResponsesLimitBuy;
	private List<OrderSubmitted.Response> m_OrderResponsesMarketBuy; 


	/// <summary>
	/// Create you client that will trade the coin that you pass
	/// </summary>
	public ClientLatoken(string apiKey, string apiSecret,string TickerCoin,string idCoin,string quantityLimitOrder, string holderAccount)
    {
		this.apiKey = apiKey;	
		this.apiSecret = apiSecret;	
		this.m_idCoin = idCoin;	
		this.m_tickerCoin = TickerCoin;	
		this.m_quantityLimitOrder = quantityLimitOrder;
        this.m_OrderResponsesLimitSell = new List<OrderSubmitted.Response>();
        this.m_OrderResponsesMarketSell= new List<OrderSubmitted.Response>();
        this.m_OrderResponsesLimitBuy= new List<OrderSubmitted.Response>();
		this.m_OrderResponsesMarketBuy = new List<OrderSubmitted.Response>();
		this.m_NameAccount = holderAccount;

	}
	/// <summary>
	/// Function in which I pass a Response for a submitted order and it print the results and the name of the client that sent the order
	/// </summary>
	/// <param name="response"></param>
	/// <param name="type"></param>
	private void LoggerResponseSelfTrade(OrderSubmitted.Response response, string type)
    {
		Console.WriteLine($"{m_NameAccount}---ORDINE {type.ToUpper()}: [MESSAGE: {response.message}. ERRORS 1: {response.errors}, " +
			$"ERRORS 2: {response.error}, STATUS: {response.status} ]\n");
	}
	/// <summary>
	/// Get all the balances of your account
	/// </summary>
	/// <returns></returns>
	public List<BalancesResponse> getBalances()
    {
			
		var endpoint = "/v2/auth/account/";
		var method = HttpMethod.Get.ToString();

		var parameters = new Dictionary<string, string>();
		parameters.Add("currency", m_idCoin);
		var queryParams = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + queryParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint + "?" + queryParams);
		request.Method = method;
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = JsonConvert.DeserializeObject<List<BalancesResponse>>(
				new StreamReader(response.GetResponseStream()).ReadToEnd()
				);
				
			return responseString;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			Console.WriteLine(responseString);
			return null;
		}

	}
	/// <summary>
	/// Get the all the levels of the order book, if you pass a tickercoin and quotecurrency it will override the ones of the client. A client is needed to use this function
	/// </summary>
	/// <param name="tickerCoin"></param>
	/// <param name="quoteCurrency"></param>
	/// <returns></returns>
	public OrderBook.Response getBook(string? tickerCoin, string? quoteCurrency )
    {
		tickerCoin = (tickerCoin == null) ?  m_tickerCoin: tickerCoin;
		quoteCurrency = (quoteCurrency == null) ?	m_quoteCurrency : quoteCurrency;

		var endpoint = "/v2/book/";
		var method = "GET";

		var parameters = new Dictionary<string, string>(){
		{"currency", tickerCoin},
		{"quote", m_tickerCurrency}
	};
		var queryParams = string.Join("/", parameters.Select(kvp =>  kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + queryParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint  + queryParams);
		request.Method = method;
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var book = JsonConvert.DeserializeObject<OrderBook.Response>(
				responseString
				);

			return book;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			Console.WriteLine(responseString);
			return null;
		}


	}
	/// <summary>
	/// Return first bid and first ask data of the orderbook
	/// </summary>
	/// <returns></returns>
	public Dictionary<string,double> FirstLevelOrderBook()
    {
		Dictionary<string,double> result = new Dictionary<string, double>();
		OrderBook.Response response = getBook(null,null);
		result.Add(ask, Math.Round(Convert.ToDouble(response.ask[0].price,nfi),8));
		result.Add(bid, Math.Round(Convert.ToDouble(response.bid[0].price,nfi),8));
		return result;

	}
	/// <summary>
	/// Send a limit order
	/// </summary>
	/// <param name="side"></param>
	/// <param name="type"></param>
	/// <param name="price"></param>
	/// <returns></returns>
	public OrderSubmitted.Response SendOrder(string side, string type, string price)
    {
		var endpoint = "/v2/auth/order/place";
		var method = "POST";

		/* form parameters */
		var parameters = new Dictionary<string, string>(){
			{"baseCurrency", m_idCoin},
			{"quoteCurrency", m_quoteCurrency},
			{"side", side},
			{"condition", "GOOD_TILL_CANCELLED"},
			{"type", type},
			{"clientOrderId", "myCustomOrde3"},
			{"price", price},
			{"quantity", m_quantityLimitOrder},
			{"timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}
		};
		var bodyContent = "{" + string.Join(",", parameters.Select(kvp => "\"" + kvp.Key + "\": \"" + kvp.Value + "\"").ToArray()) + "}";
		var signableParams = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + signableParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint);
		request.Method = method;
		request.ContentType = "application/json";
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);
		request.ContentLength = bodyContent.Length;
		var body = request.GetRequestStream();
		body.Write(Encoding.ASCII.GetBytes(bodyContent), 0, bodyContent.Length);
		body.Close();

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<OrderSubmitted.Response>(
				responseString
				);

			return orderResponse;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<OrderSubmitted.Response>(
				responseString
				);

			return orderResponse;
		}
	}
	/// <summary>
	/// Send a market Order
	/// </summary>
	/// <param name="side"></param>
	/// <returns></returns>
	public OrderSubmitted.Response SendOrder(string side)
	{
		var endpoint = "/v2/auth/order/place";
		var method = "POST";

		var parameters = new Dictionary<string, string>(){
			{"baseCurrency", m_idCoin},
			{"quoteCurrency", m_quoteCurrency},
			{"side", side},
			{"condition", "GOOD_TILL_CANCELLED"},
			{"type", "MARKET"},
			{"clientOrderId", "myCustomOrde3"},
			{"quantity", m_quantityLimitOrder},
			{"timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}
		};
		var bodyContent = "{" + string.Join(",", parameters.Select(kvp => "\"" + kvp.Key + "\": \"" + kvp.Value + "\"").ToArray()) + "}";
		var signableParams = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + signableParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint);
		request.Method = method;
		request.ContentType = "application/json";
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);
		request.ContentLength = bodyContent.Length;
		var body = request.GetRequestStream();
		body.Write(Encoding.ASCII.GetBytes(bodyContent), 0, bodyContent.Length);
		body.Close();

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<OrderSubmitted.Response>(
				responseString
				);

			return orderResponse;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<OrderSubmitted.Response>(
				responseString
				);

			return orderResponse;
		}
	}
	/// <summary>
	/// Send Buy limit order with the quantity passed in the client 
	/// </summary>
	/// <param name="price"></param>
	/// <returns></returns>
	private OrderSubmitted.Response SendBuyOrderLimit(string price)
    {
		OrderSubmitted.Response response = SendOrder("BUY", "LIMIT", price);
		m_OrderResponsesLimitBuy.Add(response);
		return response;

	}
	/// <summary>
	/// Send Sell limit order with the quantity passed in the client 
	/// </summary>
	/// <param name="price"></param>
	/// <returns></returns>
	private OrderSubmitted.Response SendSellOrderLimit(string price)
	{ 
		OrderSubmitted.Response response = SendOrder("SELL", "LIMIT", price);
		m_OrderResponsesLimitSell.Add(response);
		return response;
	}
	/// <summary>
	/// Send Buy market order with the quantity passed in the client 
	/// </summary>
	/// <param name="price"></param>
	/// <returns></returns>
	private OrderSubmitted.Response SendBuyOrderMarket()
	{
		OrderSubmitted.Response response = SendOrder("BUY");
		m_OrderResponsesMarketBuy.Add(response);
		return response;
	}
	/// <summary>
	/// Send Sell market order with the quantity passed in the client 
	/// </summary>
	/// <param name="price"></param>
	/// <returns></returns>
	private OrderSubmitted.Response SendSellOrderMarket()
	{
		OrderSubmitted.Response response = SendOrder("SELL");
		m_OrderResponsesMarketSell.Add(response);
		return response;
		
	}
	/// <summary>
	/// Get all the orders for the account
	/// </summary>
	/// <returns></returns>
	public List<Order.Returneds> GetOrders()
    {
		var endpoint = "/v2/auth/order";
		var method = "GET";

		var parameters = new Dictionary<string, string>();
		var queryParams = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + queryParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint + "?" + queryParams);
		request.Method = method;
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<List<Order.Returneds>>(
				responseString
				);

			return orderResponse;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			var orderResponse = JsonConvert.DeserializeObject<List<Order.Returneds>>(
				responseString
				);

			return orderResponse;
		}

	}
	/// <summary>
	/// Get all the orders by Id
	/// </summary>
	/// <param name="orderId"></param>
	/// <returns></returns>
	public Order.SingleOrder GetOrderById(string orderId)
    {
		var endpoint = "/v2/auth/order/getOrder/";
		var method = "GET";

		var parameters = new Dictionary<string, string>(){
			{"id", orderId}
		};
		var queryParams = string.Join("", parameters.Select(kvp =>kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + queryParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint + "" + queryParams);
		request.Method = method;
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);

		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var singleOrder = JsonConvert.DeserializeObject<Order.SingleOrder>(
				responseString
				);

			return singleOrder;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			var singleOrder = JsonConvert.DeserializeObject<Order.SingleOrder>(
				responseString
				);

			return singleOrder;
		}
	}
	/// <summary>
	/// Cancell Order By Id
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>

	public CancellOrder.Response CancellOrderById(string id)
    {
		var endpoint = "/v2/auth/order/cancel";
		var method = "POST";

		var parameters = new Dictionary<string, string>(){
			{"id", id}
		};
		var bodyContent = "{" + string.Join(",", parameters.Select(kvp => "\"" + kvp.Key + "\": \"" + kvp.Value + "\"").ToArray()) + "}";
		var signableParams = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());

		HMAC crypto = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
		byte[] signBytes = crypto.ComputeHash(Encoding.ASCII.GetBytes(method + endpoint + signableParams));
		string signature = string.Empty;
		signBytes.ToList().ForEach(b => signature += b.ToString("x2"));

		var request = HttpWebRequest.CreateHttp(baseUrl + endpoint);
		request.Method = method;
		request.ContentType = "application/json";
		request.Headers.Add("X-LA-APIKEY", apiKey);
		request.Headers.Add("X-LA-SIGNATURE", signature);
		request.ContentLength = bodyContent.Length;
		var body = request.GetRequestStream();
		body.Write(Encoding.ASCII.GetBytes(bodyContent), 0, bodyContent.Length);
		body.Close();
		try
		{
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			var singleOrder = JsonConvert.DeserializeObject<CancellOrder.Response>(
				responseString
				);

			return singleOrder;
		}
		catch (WebException we)
		{
			var errorResponse = we.Response as HttpWebResponse;
			var responseString = new StreamReader(errorResponse.GetResponseStream()).ReadToEnd();
			var singleOrder = JsonConvert.DeserializeObject<CancellOrder.Response>(
				responseString
				);

			return singleOrder;
		}
	}
}

