using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DevEnvLibrary
{
    public class MultipartParser
    {
        public void ParseSimple(Stream stream, Encoding encoding)
        {
            this.Success = false;

            // Read the stream into a byte array
            byte[] data = ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            //first line with deliminiter
            string delimiter = content.Substring(0, content.IndexOf("\r\n"));

            //search all delimiters
            List<int> delimiterIndexes = new List<int>();
            int delIndex = content.IndexOf(delimiter);
            while (delIndex >= 0)
            {
                delimiterIndexes.Add(delIndex);
                delIndex = content.IndexOf(delimiter, delIndex + delimiter.Length);
            }

            for (int i = 0; i < delimiterIndexes.Count() - 1 ; i++)
            {
                string tempContent = content.Substring(delimiterIndexes[i], delimiterIndexes[i + 1] - delimiterIndexes[i] - "\r\n".Length);

                // Look for name (userID)
                Regex re = new Regex(@"(?<=name\=\"")(.*?)(?=\"")");
                Match name = re.Match(tempContent);

                switch (name.Value)
                {
                    case "username":
                        {
                            this.Username = tempContent.Substring(tempContent.IndexOf("name=\"username\"") + ("name=\"username\"").Length + "\r\n\r\n".Length);

                            //check session token
                            CommonFunc.CheckSessionToken(this.Username);

                            break;
                        }
                    case "upload":
                        {
                            // Look for Content-Type
                            re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                            Match contentTypeMatch = re.Match(content);

                            // Look for filename
                            re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                            Match filenameMatch = re.Match(content);

                            // Did we find the required values?
                            if (contentTypeMatch.Success && filenameMatch.Success)
                            {
                                // Set properties
                                this.ContentType = contentTypeMatch.Value.Trim();
                                this.Filename = filenameMatch.Value.Trim();

                                // Get the start & end indexes of the file contents
                                int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                                byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                                int endIndex = IndexOf(data, delimiterBytes, startIndex);

                                int contentLength = endIndex - startIndex;

                                // Extract the file contents from the byte array
                                byte[] fileData = new byte[contentLength];

                                Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                                this.FileContents = fileData;
                                this.Success = true;
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Index of searched text.
        /// </summary>
        /// <param name="searchWithin">Search within.</param>
        /// <param name="serachFor">Search for.</param>
        /// <param name="startIndex">Start index.</param>
        /// <returns>Index.</returns>
        private int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Converts Stream to ByteArray.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <returns>Output ByteArray.</returns>
        private byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[2048];
            using (MemoryStream ms = new MemoryStream())
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                while (read > 0)
                {
                    ms.Write(buffer, 0, read);
                    read = stream.Read(buffer, 0, buffer.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Username.
        /// </summary>
        public string Username
        {
            get;
            private set;
        }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Parse success.
        /// </summary>
        public bool Success
        {
            get;
            private set;
        }

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType
        {
            get;
            private set;
        }

        /// <summary>
        /// File name.
        /// </summary>
        public string Filename
        {
            get;
            private set;
        }

        /// <summary>
        /// File contents.
        /// </summary>
        public byte[] FileContents
        {
            get;
            private set;
        }
    }
}