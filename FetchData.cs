using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatewayUpload
{
    public class FetchData
    {
        public static List<string> listItems { get; set; }
        

        public static void CallString(string refFrom, int count,  out string sResult)
        {
            sResult = "Invalid nType";
            if (refFrom == "last" || refFrom == "from")
            {
                //OK
            }
            else
            {
                MessageBox.Show("Invalid parameter");
                goto ExitHere;
            }

            string url = "http://34.122.10.49:8080/";
            url = url + refFrom;
            url = url + "/" + count;
            //url = url + "/" + count + "/all";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            httpWebRequest.PreAuthenticate = true;

            httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("dale:liebenberg"));
            HttpWebResponse responseObj = null;
            httpWebRequest.Timeout = 6000;


            //try
            //{
            //    responseObj = (HttpWebResponse)httpWebRequest.GetResponse();
            //}
            //catch (Exception e)
            //{
            //    sResult = e.ToString();
                
            //    goto ExitHere;
            //}

            try
            {
                responseObj = (HttpWebResponse) httpWebRequest.GetResponse();

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    sResult = "Abort";
                    goto ExitHere;
                }
            }
            catch (Exception e2)
            {
                sResult = "Abort";
                goto ExitHere;
            }

            if (responseObj == null)
            {
                sResult = "Abort";
                goto ExitHere;
            }
            sResult = null;
            try
            {
                using (Stream stream = responseObj.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    sResult = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e3)
            {
                sResult = "null";
                goto ExitHere;
            }
            

            ExitHere:;
           

        }
    }
}
