using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace EikonProcessing
{
    public class DataToTransfer
    {
        // https://github.com/eikontx/eikon-spt/blob/5e1e9de04c43cb9b37be1f5c27e7f4273cad8a65/pipeline-tooling/README.md
        public string barcode = "";
        // public string cell_line = "";
        // public string target = "";
        public string picklist_file_name = "";

        public class ScopeMetadata
        {
            public string name = "";
            public int frame_rate = 100;
            //     public string pixel_size = "";
        }
        public ScopeMetadata scope_metadata = new ScopeMetadata();
        // public string nd2_dir = "";
    }

    public static class ProcessingServer
    {
        public static string SendToServer(string url, object o)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "/plates");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(o);
                streamWriter.Write(json);
            }

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (WebException e)
            {
                response = ((HttpWebResponse)e.Response);
            }

            string result = "";
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> desobj = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            foreach (var kvp in desobj)
            {
                if (sb.Length != 0)
                    sb.Append(", ");

                sb.Append(kvp.Key + ": " + kvp.Value);
            }

            string output = "";
            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:  //201
                                              // Success
                    output = "Server successfully created record. ";
                    break;
                case HttpStatusCode.BadRequest: //400
                case HttpStatusCode.MethodNotAllowed: //405
                case HttpStatusCode.InternalServerError: //500
                    output = "Server sent: " + response.StatusDescription + ", ";
                    break;
                default:
                    output = "Server reported unexpected error code: " + response.StatusDescription + ", ";
                    // Shouldn't get here
                    break;
            }

            output += sb.ToString();

            return output;

            /*
            var json = JsonConvert.SerializeObject(o);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, data);

            return response.Content.ReadAsStringAsync().Result;*/
        }
    }
}
