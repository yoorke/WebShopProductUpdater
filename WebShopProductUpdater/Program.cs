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
                    log("Obrađujem XML podatke...", true);
                    XmlNodeList nodeList = data.DocumentElement.SelectNodes("webdata");
                    int total = nodeList.Count;
                    int i = 1;
                    int status = -1;
                    int[] statuses = new int[2];
                    foreach (XmlNode xmlNode in nodeList)
                    { 
                        //XmlNode xmlNode = nodeList[0];
                        status = updateProduct(xmlNode.SelectSingleNode("barcode").InnerText, xmlNode.SelectSingleNode("naziv") != null ? xmlNode.SelectSingleNode("naziv").InnerText : "none", xmlNode.SelectSingleNode("kol") != null ? xmlNode.SelectSingleNode("kol").InnerText : "-1", xmlNode.SelectSingleNode("pcena").InnerText, total, i++);
                        if (status == 1)
                            statuses[0]++;
                        else if (status == 2)
                            statuses[1]++;
                    }

                    log("Slanje artikala uspešno završeno.");
                    log("Ukupno dodato: " + statuses[0].ToString());
                    log("Ukupno ažurirano:" + statuses[1].ToString());
                    log("Nije pronađeno:" + (nodeList.Count - statuses[0] - statuses[1]).ToString());
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
                log("Učitavam XML fajl...", true);
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

        private static int updateProduct(string barcode, string name, string quantity, string price, int total, int i)
        {
            try
            {
                log("Šaljem podatke za artikal " + barcode + " (" + i.ToString() + "/" + total.ToString() + ") - ");
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
                int status = int.Parse(responseString);
                if (status == 1)
                    log("Uspešno dodat", true);
                else if (status == 2)
                    log("Uspešno ažuriran.", true);
                else if (status == -1)
                    log("Nije pronađen", true);

                return status;
            }
            catch(Exception ex)
            {
                log("Greška prilikom slanja podataka" + Environment.NewLine + ex.Message);
                //Console.WriteLine(ex.Message);
                return -1;
            }
        }

        private static void log(string message, bool newLine = false)
        {
            try
            {
                if (newLine)
                    Console.WriteLine(message);
                else
                    Console.Write(message);

                string filename = string.Format("{0:00}", DateTime.Now.Day) + string.Format("{0:00}", DateTime.Now.Month) + DateTime.Now.Year.ToString() + ".log";
                using (StreamWriter sw = new StreamWriter("log/" + filename, true, Encoding.GetEncoding(65001)))
                {
                    sw.Write(DateTime.Now.ToString());
                    if (newLine)
                    {                        
                        sw.WriteLine(" - " + message);
                    }
                    else
                    { 
                        sw.Write(" - " + message);
                        //sw.WriteLine("----------------------------");
                    }
                }
            }
            catch
            {

            }
        }
    }
}
