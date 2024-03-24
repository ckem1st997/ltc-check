using NBitcoin;
using NBitcoin.Altcoins;
using System.Diagnostics;

namespace ConsoleApp3
{
    internal class Program
    {


        // Biến để cache dữ liệu
        private static List<string> cachedData = new List<string>();
        private static HashSet<string> addData = new HashSet<string>();
        private static string currentDirectory = Environment.CurrentDirectory;
        static async Task Main(string[] args)
        {
            string filePath2 = Path.Combine(currentDirectory, "btc-check.txt");
            int lengthFile = 2048;
            string filePath1 = Path.Combine(currentDirectory, "words_alpha.txt");
            List<string> data = await GetDataAsync(filePath1);

            List<string> rd = new List<string>();
            int count = 0;
            Console.WriteLine($"Số dòng trong file: {data.Count}");

            string mnemonicWords = "";
            int seedNum = 12;

            Random random = new Random();
            while (true)
            {

                rd = new List<string>();
                var listRd = new List<int>();
                mnemonicWords = string.Empty;
                for (int i = 0; i < seedNum; i++)
                {
                    bool b = true;
                    while (b)
                    {
                        int randomIndex = random.Next(lengthFile);
                        var check = listRd.Where(x => x == randomIndex);
                        if ((check == null || !check.Any()))
                        {
                            rd.Add(randomIndex.ToString());
                            listRd.Add(randomIndex);
                            mnemonicWords = mnemonicWords + " " + data[randomIndex];
                            b = false;
                        }
                    }

                }
                mnemonicWords = mnemonicWords.Trim();
                if (!(!string.IsNullOrEmpty(mnemonicWords) && (mnemonicWords.Split(" ").Length == 12 || mnemonicWords.Split(" ").Length == 24))) continue;
                try
                {
                    var listAddress = new List<string>();
                    // Khai báo mạng Dogecoin
                    var network = Litecoin.Instance.Mainnet;

                    ExtKey masterKey = CreateMasterPrivateKey(mnemonicWords, network);

                    // Tạo địa chỉ ví từ khóa chính
                    var keyPath = new KeyPath("m/44'/3'/0'/0/0");
                    ExtKey key = masterKey.Derive(keyPath);
                    BitcoinAddress address = key.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, network);
                    if (address != null)
                        listAddress.Add(address.ToString());
                    count++;
                    // Tạo và kiểm tra các loại địa chỉ khác nhau
                    //  Stopwatch stopwatch = Stopwatch.StartNew();
                    await DeriveAndCheckBalance(listAddress, filePath2, mnemonicWords);
                    //  stopwatch.Stop();
                    Console.WriteLine($"[{count}]-{seedNum}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            static ExtKey CreateMasterPrivateKey(string seedWords, Network network)
            {
                // Chuyển đổi từ seed words thành khóa chính
                Mnemonic mnemonic = new Mnemonic(seedWords);
                ExtKey masterKey = mnemonic.DeriveExtKey();
                return masterKey;
            }
            async Task DeriveAndCheckBalance(List<string> listAddress, string csvFilePath, string mnemonicWords)
            {
                try
                {
                    // Tạo địa chỉ từ master key và key path
                    // Kiểm tra xem địa chỉ có trong file CSV không
                    bool addressFound = await AddressExistsInCsv(listAddress, csvFilePath);

                    if (addressFound)
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }


            async Task<bool> AddressExistsInCsv(List<string> listAddress, string csvFilePath)
            {
                string? line = "";
                if (addData.Count < 1)
                {
                    Console.WriteLine("begin aync data !");
                    using (var reader = new StreamReader(csvFilePath))
                    {
                        // Đọc từng dòng trong tệp
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                                addData.Add(line);
                        }
                    }
                    Console.WriteLine("end aync data !");
                }
                foreach (var VARIABLE in listAddress)
                {
                    if (addData.Contains(VARIABLE))
                        return true;
                }
                return false;
            }
        }

        static async Task<List<string>> GetDataAsync(string filePath)
        {
            // Nếu dữ liệu đã được cache, trả về dữ liệu từ cache
            if (cachedData != null && cachedData.Count > 0)
            {
                Console.WriteLine("Lấy dữ liệu từ cache.");
                return cachedData;
            }

            // Nếu chưa có dữ liệu trong cache, đọc từ file
            Console.WriteLine("Đọc dữ liệu từ file và cache nó.");
            cachedData = new List<string>();

            // Kiểm tra xem file có tồn tại không
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại.");
                return cachedData;
            }

            // Đọc file và lưu vào cache
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cachedData.Add(line);
                }
            }

            return cachedData;
        }

    }
}
