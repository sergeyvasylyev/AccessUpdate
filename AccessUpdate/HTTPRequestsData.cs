using System;
using System.Text;
using System.Net;
using System.IO;

namespace AccessUpdate
{
    public static class HTTPRequestsData
    {
        public static string SendRequest(string UriArg, string PostData, string ContentType, string AccessUpdate= "SavePrincipal", Boolean ?AuthorizationDefault=true, string CookieData = "")
        {            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriArg);

            if (AccessUpdate == "SavePrincipal")
            {
                request.Headers.Add("SOAPAction", "http://tempuri.org/IPrincipalService/SavePrincipal");
            }
            else if (AccessUpdate == "RemovePrincipal")
            {
                request.Headers.Add("SOAPAction", "http://tempuri.org/IPrincipalService/RemovePrincipal");
            }
            else if (AccessUpdate == "GetCurrentPrincipal")
            {
                request.Headers.Add("SOAPAction", "http://tempuri.org/IPrincipalService/GetCurrentPrincipal");
            }

            if (AuthorizationDefault == true)
            {
                request.UseDefaultCredentials = true;
                // Set credentials to use for this request.            
                ICredentials credentials = CredentialCache.DefaultCredentials;
                request.Credentials = credentials;
            }
            else
            {
                //request.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                //NetworkCredential cred = new System.Net.NetworkCredential();
                //request.Credentials = cred;
                request.UseDefaultCredentials = true;
                if (CookieData != "")
                {
                    request.Headers.Add("Cookie", CookieData);
                }
            }

            request.Method = "POST";
            request.ContentType = ContentType;

            string postData = PostData;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();                

            ///////////////////////////////response
            string ResponseBody = "";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Get the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                    Char[] read = new Char[256];

                    // Read 256 charcters at a time.    
                    int count = readStream.Read(read, 0, 256);
                    //Console.WriteLine("HTML...\r\n");

                    while (count > 0)
                    {
                        // Dump the 256 characters on a string and display the string onto the console.
                        String str = new String(read, 0, count);
                        ResponseBody = ResponseBody + str;
                        count = readStream.Read(read, 0, 256);
                    }

                    Console.WriteLine("Response stream received.");
                    Console.WriteLine(readStream.ReadToEnd());
                    response.Close();
                    readStream.Close();
                }
            }
            catch (WebException ex)
            {
                string WebException = ex.Message;
                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        WebException = reader.ReadToEnd();
                    }
                }
                return WebException;
            }

            catch (Exception ex)
            {
                return ex.Message;
            }

            return ResponseBody;
            
        }
    }
}
