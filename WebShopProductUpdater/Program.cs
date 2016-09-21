using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Net.Http;
using System.Net;
using System.Text;

namespace WebShopProductUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            getProductsData();
        }

        private static void getProductsData()
        {
            try
            { 
                XmlDocument data = getXmlData();
                if(data != null)
                {
                    log("Obrađujem XML podatke...");
                    XmlNodeList nodeList = data.DocumentElement.SelectNodes("webdata");
                    foreach (XmlNode xmlNode in nodeList)
                    { 
                        //XmlNode xmlNode = nodeList[0];
                        updateProduct(xmlNode.SelectSingleNode("barcode").InnerText, xmlNode.SelectSingleNode("naziv") != null ? xmlNode.SelectSingleNode("naziv").InnerText : "none", xmlNode.SelectSingleNode("kol") != null ? xmlNode.SelectSingleNode("kol").InnerText : "-1", xmlNode.SelectSingleNode("pcena").InnerText);
                    }
                }
            }
            catch(Exception ex)
            {
                log("Greška prilikom obrade XML fajla" + Environment.NewLine + ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }

        private static XmlDocument getXmlData()
        {
            try
            {
                log("Učitavam XML fajl...");
                XmlDocument productsData = new XmlDocument();
                

                //productsData.Load(ConfigurationManager.AppSettings["sourceFileLocation"]);
                productsData.LoadXml(File.ReadAllText(ConfigurationManager.AppSettings["sourceFileLocation"]));
                return productsData;
            }
            catch(Exception ex)
            {
                log("Greška prilikom učivatanja fajla " + ConfigurationManager.AppSettings["sourceFileLocation"] + Environment.NewLine + ex.Message);
                //Console.WriteLine(ex.Message);
                return null;
            }
        }

        //private static async void updateProduct(string barcode, string name, string quantity, string price)
        //{
            //using (var client = new HttpClient())
            //{
            //var client = new HttpClient();
                //var data = new Dictionary<string, string>
                //{
                    //{"barcode", barcode },
                    //{ "name", name },
                    //{ "quantity", quantity },
                    //{ "price", price }
                    //{"value", "32423423" }
                //};
                //var content = new FormUrlEncodedContent(data);
            //var response = await client.PostAsync(ConfigurationManager.AppSettings["webUrl"], content);
            //var response = await client.PostAsync("http://localhost:62592/api/values", content);
                //var responseString = await response.Content.ReadAsStringAsync();
            //}
                
        //}

        private static void updateProduct(string barcode, string name, string quantity, string price)
        {
            try
            {
                log("Šaljem podatke za artikal " + barcode);
                var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["webUrl"]);
                //var request = (HttpWebRequest)WebRequest.Create("http://localhost:63480/api/product");
                var postData = "barcode=" + barcode;
                postData += "&name=" + name;
                postData += "&quantity=" + quantity;
                postData += "&price=" + price;
                var data = Encoding.UTF8.GetBytes(postData); 

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.MediaType = "json";

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch(Exception ex)
            {
                log("Greška prilikom slanja podataka" + Environment.NewLine + ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }

        private static void log(string message)
        {
            try
            {
                Console.WriteLine(message);
                string filename = string.Format("{0:00}", DateTime.Now.Day) + string.Format("{0:00}", DateTime.Now.Month) + DateTime.Now.Year.ToString() + ".log";
                using (StreamWriter sw = new StreamWriter("log/" + filename, true, Encoding.GetEncoding(65001)))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(message);
                    sw.WriteLine("----------------------------");
                }
            }
            catch
            {

            }
        }
    }
}
