using System.Text.Json;
using static System.Console;

// Разработать функцию по работе с API Binance которая получает URL, описанный в документации
// по адресу https://binance-docs.github.io/apidocs/spot/en/#kline-candlestick-data.
//
// Запрос производиться методом GET с относительной uri /api/v3/klines при этом нужно использовать параметры,
// которые конкретизируют какие данные нам нужны, для этого добавляем  ?symbol=BTCUSDT&interval=1d в конец
// нашего Uri, для формирования ссылки такого вида используйте UriBuilder. Полученные данные Сериализуйте
// в файл используя Класс с полями соответствующие следующим данным,
// представленными в документации, ниже пример:
//    1499040000000,      // Kline open time
//    "0.01634790",       // Open price
//    "0.80000000",       // High price
//    "0.01575800",       // Low price
//    "0.01577100",       // Close price
//    "148976.11427815",  // Volume
//    1499644799999,      // Kline Close time
//    "2434.19055334",    // Quote asset volume
//    308,                // Number of trades
//    "1756.87402397",    // Taker buy base asset volume
//    "28.46694368",      // Taker buy quote asset volume
//    "0"                 // Unused field, ignore.
//
// Цель собрать эти данные и Сериализовать их в файл формата *.json.

public class KlineCandlestickData
{
    public long OpenTime { get; set; }
    public required string OpenPrice { get; set; }
    public required string HighPrice { get; set; }
    public required string LowPrice { get; set; }
    public required string ClosePrice { get; set; }
    public required string Volume { get; set; }
    public long CloseTime { get; set; }
    public required string QuoteAssetVolume { get; set; }
    public int NumberOfTrades { get; set; }
    public required string TakerBuyBaseAssetVolume { get; set; }
    public required string TakerBuyQuoteAssetVolume { get; set; }
    public required string UnusedField { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class BinanceApiClient
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<List<KlineCandlestickData>> GetKlineCandlestickData(string symbol, string interval)
    {
        List<KlineCandlestickData> klineDataList = new List<KlineCandlestickData>();

        try
        {
            UriBuilder builder = new UriBuilder("https://api.binance.com/api/v3/klines");
            builder.Query = $"symbol={symbol}&interval={interval}";

            HttpResponseMessage response = await client.GetAsync(builder.ToString());

            if (response.IsSuccessStatusCode)
            {
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (JsonDocument doc = await JsonDocument.ParseAsync(responseStream))
                    {
                        foreach (JsonElement element in doc.RootElement.EnumerateArray())
                        {
                            KlineCandlestickData data = new KlineCandlestickData
                            {
                                OpenTime = element[0].GetInt64(),
                                OpenPrice = element[1].GetString(),
                                HighPrice = element[2].GetString(),
                                LowPrice = element[3].GetString(),
                                ClosePrice = element[4].GetString(),
                                Volume = element[5].GetString(),
                                CloseTime = element[6].GetInt64(),
                                QuoteAssetVolume = element[7].GetString(),
                                NumberOfTrades = element[8].GetInt32(),
                                TakerBuyBaseAssetVolume = element[9].GetString(),
                                TakerBuyQuoteAssetVolume = element[10].GetString(),
                                UnusedField = element[11].GetString()
                            };

                            klineDataList.Add(data);
                        }
                    }
                }
            }

            return klineDataList;
        }
        catch (Exception ex)
        {
            WriteLine("Произошла ошибка: " + ex.Message);
            return null;
        }
    }

    public static async Task SerializeDataToFile(List<KlineCandlestickData> data, string filePath)
    {
        try
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                foreach (var item in data)
                {
                    await sw.WriteLineAsync(item.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            WriteLine("Произошла ошибка при записи в файл: " + ex.Message);
        }
    }
}

class Program
{
    static async Task Main()
    {
        BinanceApiClient binanceApiClient = new BinanceApiClient();

        string symbol = "BTCUSDT";
        string interval = "1d";

        List<KlineCandlestickData> klineData = await BinanceApiClient.GetKlineCandlestickData(symbol, interval);

        string filePath = "binance_kline_data.json";
        await BinanceApiClient.SerializeDataToFile(klineData, filePath);

        WriteLine("Данные были получены и сериализованы в файл: " + filePath);
    }
}