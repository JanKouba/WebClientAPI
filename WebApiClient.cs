using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Data;
using System.Web;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Zradelna
{
    public class WebApiClient
    {
        string cookie = string.Empty;

        private string login_id;
        private string boarder;
        private string workstation;
        private string acc_id;

        private double accountBalance;

        public string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
        public string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36 OPR/81.0.4196.60";

        public string Login_id { get => login_id; set => login_id = value; }
        public string Boarder { get => boarder; set => boarder = value; }
        public string Workstation { get => workstation; set => workstation = value; }
        public string Acc_id { get => acc_id; set => acc_id = value; }
        public double AccountBalance { get => accountBalance; }

        /// <summary>
        /// Open homepage to set session ID and get in the cookie
        /// </summary>
        public void OpenHomepage()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://172.15.27.236:8080/isis/objednavkytouch/index");

            request.Accept = Accept;
            request.Method = "GET";
            request.ContentType = "text/html; charset=utf-8";
            request.UserAgent = UserAgent;

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            cookie = response.Headers[HttpResponseHeader.SetCookie]; //Get cookie
        }

        /// <summary>
        /// Call login function using cookie
        /// </summary>
        /// <param name="cardreader">Card ID - personal indetifier</param>
        /// <returns></returns>
        public void Login(string cardreader)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://172.15.27.236:8080/isis/objednavkytouch/loginon");

            var postData = "{\"cardreader\": \"" + cardreader + "\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Accept = Accept;
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.UserAgent = UserAgent;
            request.Referer = "http://172.15.27.236:8080/isis/objednavkytouch/index";

            request.Headers[HttpRequestHeader.Cookie] = cookie; //Set cookie

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            GetLoginInfo( responseString);
        }

        /// <summary>
        /// Get login info from response string while logged in
        /// </summary>
        /// <param name="response"></param>
        public void GetLoginInfo(string response)
        {
            //Get login info
            string json = new Regex("a_data=(.*)").Match(response).Groups[1].Value; //Extract desired JSON data
            DataTable dtLoginInfo = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject("[" + json + "]", (typeof(DataTable))); //Convert to DataTable

            login_id = dtLoginInfo.Rows[0]["login_id"].ToString();
            boarder = dtLoginInfo.Rows[0]["boarder"].ToString();
            workstation = dtLoginInfo.Rows[0]["workstation"].ToString();
            acc_id = dtLoginInfo.Rows[0]["acc_id"].ToString();
        }

        /// <summary>
        /// Get current account balance for active login
        /// </summary>
        public void GetAccountBalance()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://172.15.27.236:8080/isis/objednavky/AccountList");
            
            var postData = "{\"login_id\": \""  + login_id + "\",";
            postData += "\"boarder\": \""       + boarder + "\",";
            postData += "\"accounts\": \""      + "" + "\",";
            postData += "\"workstation\": \""   + workstation + "\",";
            postData += "\"acc_id\": \""        + acc_id + "\",";
            postData += "\"dest_id\": \""       + acc_id + "\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Accept = Accept;
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.UserAgent = UserAgent;
            request.Referer = "http://172.15.27.236:8080/isis/objednavkytouch/index";

            request.Headers[HttpRequestHeader.Cookie] = cookie; //Set cookie

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            string stringBalance = new Regex("<b>(.*) Kč</b>").Match(responseString).Groups[1].Value; //Extract desired data

            Double.TryParse(stringBalance, out accountBalance);
        }


    }
}
