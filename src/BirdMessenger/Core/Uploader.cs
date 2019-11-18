using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BirdMessenger.Configuration;

namespace BirdMessenger.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Uploader
    {
        public UploadConfig _UploadConfig;

        public Uploader (UploadConfig uploadConfig) => _UploadConfig = uploadConfig;

        /// <summary>
        /// create 
        /// </summary>
        public void Create ()
        {
            Uri uRl = null;
            HttpWebRequest request = WebRequest.Create (_UploadConfig.ServerUrl) as HttpWebRequest;
            request.Method = "POST";
            request.Headers.Add ("Tus-Resumable", "1.0.0");
            request.Headers.Add ("Upload-Length", _UploadConfig.UploadFile.Length.ToString ());
            request.ContentLength = 0;
            request.Headers.Add ("Upload-Metadata", this.CreateMeta ());

            if (_UploadConfig.PreCreateRequest != null)
            {
                _UploadConfig.PreCreateRequest (request);
            }
            if (_UploadConfig.IsCancel)
            {
                throw new Exception ("create request has been canceled");
            }

            HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
            request?.Abort();
            if (response.StatusCode == HttpStatusCode.Created)
            {
                string responseUrl = response.Headers["Location"];
                if (string.IsNullOrEmpty (responseUrl))
                {
                    throw new Exception ("Not found location header");
                }

                if (Uri.TryCreate (responseUrl, UriKind.RelativeOrAbsolute, out uRl))
                {
                    if (!uRl.IsAbsoluteUri)
                    {
                        uRl = new Uri (_UploadConfig.ServerUrl, uRl);
                    }
                }
                else
                {
                    throw new Exception ("Invalid location header");
                }
            }
            else
            {
                throw new Exception ("Create failed");
            }

            _UploadConfig.UploadUrl = uRl;

            //return uRl;

        }

        public void UploadFile ()
        {
            HttpWebRequest request = WebRequest.Create (_UploadConfig.UploadUrl) as HttpWebRequest;
            request.Method = "HEAD";
            request.Headers.Add ("Tus-Resumable", "1.0.0");

            if (_UploadConfig.PreUploadRequest != null)
            {
                _UploadConfig.PreUploadRequest (request);
            }
            if (_UploadConfig.IsCancel)
            {
                throw new Exception ("upload operate has been canceled");
            }

            HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
            long offset = long.Parse (response.Headers["Upload-Offset"]);
            request?.Abort();
            int uploadSize = _UploadConfig.MaxChunkSize;
            using (var fs = new FileStream (_UploadConfig.UploadFile.FullName, FileMode.Open, FileAccess.Read))
            {

                while (true)
                {
                    request = WebRequest.Create (_UploadConfig.UploadUrl) as HttpWebRequest;
                    request.Method = "PATCH";
                    request.Headers.Add ("Tus-Resumable", "1.0.0");
                    request.Headers.Add ("Upload-Offset", "0");
                    request.ContentType = "application/offset+octet-stream";
                    request.KeepAlive = false;
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.Timeout=10*1000;

                    if (fs.Length == offset)
                    {
                        if (_UploadConfig.UploadFinish != null)
                        {
                            _UploadConfig.UploadFinish (_UploadConfig.UploadUrl);
                        }
                        break;
                    }
                    fs.Seek (offset, SeekOrigin.Begin);
                    byte[] buffer = new byte[uploadSize];
                    var readByteCount = fs.Read (buffer, 0, uploadSize);
                    Array.Resize (ref buffer, readByteCount);
                    request.Headers["Upload-Offset"] = offset.ToString ();
                    request.ContentLength = readByteCount;

                    if (_UploadConfig.PreUploadRequest != null)
                    {
                        _UploadConfig.PreUploadRequest (request);
                    }

                    using (var requestStream = request.GetRequestStream ())
                    {
                        requestStream.Write (buffer, 0, readByteCount);
                    }

                    if (_UploadConfig.IsCancel)
                    {
                        throw new Exception ("upload operate has been canceled");
                    }

                    response = (HttpWebResponse) request.GetResponse ();
                    offset = long.Parse (response.Headers["Upload-Offset"]);
                    request?.Abort();

                    if (_UploadConfig.Uploading != null)
                    {
                        _UploadConfig.Uploading (offset, _UploadConfig.UploadFile.Length);
                    }

                }
            }
        }

        public Dictionary<string, string> GetServerInfo ()
        {
            Dictionary<string, string> serverInfo = new Dictionary<string, string> ();
            HttpWebRequest request = WebRequest.Create (_UploadConfig.ServerUrl) as HttpWebRequest;
            request.Method = "OPTIONS";
            HttpWebResponse response = (HttpWebResponse) request.GetResponse ();

            if (response.StatusCode != HttpStatusCode.NoContent || response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception ($"options request failed");
            }

            serverInfo["Tus-Resumable"] = response.Headers["Tus-Resumable"];
            serverInfo["Tus-Version"] = response.Headers["Tus-Version"];
            serverInfo["Tus-Max-Size"] = response.Headers["Tus-Max-Size"];
            serverInfo["Tus-Extension"] = response.Headers["Tus-Extension"];

            return serverInfo;
        }
        private string CreateMeta ()
        {
            string uploadMeta = "";
            _UploadConfig.UploadMeta = _UploadConfig.UploadMeta ?? new Dictionary<string, string> ();

            if (!_UploadConfig.UploadMeta.ContainsKey ("fileName"))
            {
                _UploadConfig.UploadMeta["fileName"] = _UploadConfig.UploadFile.Name;
            }

            List<string> UploadMetaList = new List<string> ();
            foreach (var item in _UploadConfig.UploadMeta)
            {
                string k = item.Key.Replace (" ", "").Replace (",", "");
                string v = Convert.ToBase64String (System.Text.Encoding.UTF8.GetBytes (item.Value));
                UploadMetaList.Add (string.Format ("{0} {1}", k, v));
            }

            uploadMeta = string.Join (",", UploadMetaList.ToArray ());

            return uploadMeta;
        }
    }
}
