using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Vergil.Utilities;

namespace Vergil.Web {
    /// <summary>
    /// Class for transferring files across an FTP connection.
    /// </summary>
    public class FTP {
        /// <summary>
        /// The URL for this remote server
        /// </summary>
        public string Address { get; private set; }
        private readonly NetworkCredential credentials;

        /// <summary>
        /// Creates a new FTP connection object that can upload or download files from a server. No actual connection is established until a transfer is requested.
        /// </summary>
        /// <param name="address">The FTP address for the connection</param>
        /// <param name="username">The authentication username for this server</param>
        /// <param name="password">The authentication password for this server</param>
        public FTP(string address, string username = "", string password = "") {
            if (!address.Contains("://")) address = "ftp://" + address;
            Address = address;
            credentials = new NetworkCredential(username, password);
        }

        /// <summary>
        /// Uploads a local file to the FTP server.
        /// </summary>
        /// <param name="filepath">The full path of the local file to upload</param>
        /// <param name="remoteFilePath">The path of the desired location of this file on the FTP server</param>
        /// <returns>A description of the response obtained from this transfer request</returns>
        public string Upload(string filepath, string remoteFilePath) {
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create(Address + remoteFilePath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            if (credentials.UserName.Length > 0 || credentials.Password.Length > 0) request.Credentials = credentials;

            if (!File.Exists(filepath)) throw new ArgumentException(filepath + " does not exist.");
            byte[] fileContents = File.ReadAllBytes(filepath);
            
            request.ContentLength = fileContents.Length;
            using(Stream requestStream = request.GetRequestStream()) requestStream.Write(fileContents, 0, fileContents.Length);
            
            FtpWebResponse response = (FtpWebResponse) request.GetResponse();
            return response.StatusDescription;
        }

        /// <summary>
        /// Downloads a remote file 
        /// </summary>
        /// <param name="filepath">The full path of the remote file to download</param>
        /// <param name="localFilePath">The path of the desired location of this file locally</param>
        /// <returns>A description of the response obtained from this transfer request</returns>
        public string Download(string filepath, string localFilePath) {
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create(Address + filepath);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            if (credentials.UserName.Length > 0 || credentials.Password.Length > 0) request.Credentials = credentials;
            FtpWebResponse response = (FtpWebResponse) request.GetResponse();
            using (Stream responseStream = response.GetResponseStream()) {
                FileSystemUtil.CreateDirectories(localFilePath);
                if (File.Exists(localFilePath)) File.Delete(localFilePath);
                using (FileStream localstream = new FileStream(localFilePath, FileMode.OpenOrCreate)) responseStream.CopyTo(localstream);
            }

            return response.StatusDescription;
        }

        /// <summary>
        /// Gets a list of all files on the server in the specified directory
        /// </summary>
        /// <param name="path">The remote path to inspect</param>
        /// <param name="passiveMode">Whether or not to use passive mode in this request. Default: false.</param>
        /// <returns>A list of filenames found in the specified directory</returns>
        public List<string> List(string path = "/", bool passiveMode = false) {
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create(Address + path);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = credentials;
            request.UsePassive = passiveMode;
            request.KeepAlive = false;
            request.Timeout = 10000;

            List<string> result = new List<string>();
            using (FtpWebResponse response = (FtpWebResponse) request.GetResponse()) {
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    while (!reader.EndOfStream) result.Add(reader.ReadLine());
                }
            }
            return result;
        }

        /// <summary>
        /// Checks ability to connect to the FTP server.
        /// </summary>
        /// <returns>True if a connection could be established</returns>
        public bool Ping() {
            try {
                List("/");
                return true;
            } catch (WebException) {
                return false;
            }
        }
    }
}