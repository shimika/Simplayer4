using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml;

namespace Simplayer4 {
	public class Lyrics {
		private string soapForm = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:SOAP-ENC=\"http://www.w3.org/2003/05/soap-encoding\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns2=\"ALSongWebServer/Service1Soap\" xmlns:ns1=\"ALSongWebServer\" xmlns:ns3=\"ALSongWebServer/Service1Soap12\"><SOAP-ENV:Body><ns1:GetLyric5><ns1:stQuery><ns1:strChecksum>#CheckSum#</ns1:strChecksum><ns1:strVersion>2.0 beta2</ns1:strVersion><ns1:strMACAddress></ns1:strMACAddress><ns1:strIPAddress>255.255.255.0</ns1:strIPAddress></ns1:stQuery></ns1:GetLyric5></SOAP-ENV:Body></SOAP-ENV:Envelope>";
		private string[] lyrList;
		public string[] LyricLists { get { return lyrList; } }

		public bool GetLyricsFromFile(string aFilePath) {
			if (!parseXML(getXMLDocument(getStreamFromFile(aFilePath), soapForm.Replace("^^", "'"), "http://lyrics.alsong.co.kr/alsongwebservice/service1.asmx"), ref lyrList) || lyrList == null) {
				return false;
			}
			return true;
		}

		private bool parseXML(XmlDocument xmlDocs, ref string[] lyrArray) {
			if (xmlDocs == null) { return false; }

			if (xmlDocs.HasChildNodes) {
				XmlNodeList childNodes = xmlDocs.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes;
				XmlNode xmlNode = childNodes[0];
				if (childNodes.Count > 0) {
					if (xmlNode.Name == "GetResembleLyric2Result") {
						int count = xmlNode.ChildNodes.Count;
						if (count <= 0) { return false; }

						lyrArray = new string[count];
						for (int index = 0; index < count; ++index) {
							string str = xmlDocs.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[index].ChildNodes[3].InnerText.Replace("<br>", "\r\n");
							lyrArray[index] = str;
						}
					} else {
						for (int index = 0; index < xmlNode.ChildNodes.Count; ++index) {
							if (xmlNode.ChildNodes[index].Name == "strLyric") {
								lyrArray = new string[1];
								string str = xmlNode.ChildNodes[index].InnerText.Replace("<br>", "\r\n");
								lyrArray[0] = str;
								break;
							}
						}
					}
				}
			}
			return true;
		}

		private Stream getStreamFromFile(string filePath) {
			Stream stream = null;
			try {
				stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			} catch { }
			return stream;
		}

		private XmlDocument getXMLDocument(Stream stream, string soapForm, string url) {
			try {
				string md5 = getSongMD5(stream);
				if (md5 == "") { throw new Exception(); }
				return webRequest(md5, soapForm, url);
			} catch {
				return null;
			}
		}

		private string getSongMD5(Stream stream) {
			try {
				if (stream == null) { return ""; }

				int A_1_1 = 0;
				stream.Position = 0L;
				byte[] buffer = new byte[27];
				stream.Read(buffer, 0, 27);

				if (buffer.Length < 27 || stream.Position > 50000L) { return ""; }

				long position = stream.Position;

				string tag = Encoding.ASCII.GetString(buffer, 0, 3);

				if (tag == "ID3") {
					stream.Position -= 17L;
					byte[] tagSize = new byte[4];
					Array.Copy(buffer, 6, tagSize, 0, 4);
					movePosition(searchPosition(stream, ref A_1_1, tagSize));
				} else {
					stream.Position = 0L;
					movePosition(stream);
				}

				byte[] pureBytes = new byte[163840];
				stream.Read(pureBytes, 0, 163840);

				return getMD5FromByteArray(pureBytes);
			} catch {
				return "";
			}
		}

		private Stream searchPosition(Stream stream, ref int position, byte[] tagSize) {
			int num;
			for (num = 0; num < 500000; ++num) {
				position = 10 + ((int)tagSize[0] << 21 | (int)tagSize[1] << 14 | (int)tagSize[2] << 7 | (int)tagSize[3]);
				if (stream.CanSeek) {
					stream.Position = (long)position;
					break;
				}
			}
			if (num == 500000) {
				stream.Position = 0L;
			}
			return stream;
		}

		private Stream movePosition(Stream stream) {
			for (int index = 0; index < 50000; ++index) {
				if (stream.ReadByte() == (int)byte.MaxValue && stream.ReadByte() >> 5 == 7) {
					stream.Position += -2L;
					break;
				}
			}
			return stream;
		}

		private XmlDocument webRequest(string md5, string soapForm, string url) {
			XmlDocument xmlDocument1 = (XmlDocument)null;
			string str = soapForm.Replace("^^", "'").Replace("#CheckSum#", md5);
			HttpWebRequest httpWebRequest = (HttpWebRequest)null;

			try {
				httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.UserAgent = "gSOAP/2.7";
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "application/soap+xml; charset=utf-8";

				StreamWriter streamWriter = new StreamWriter(((WebRequest)httpWebRequest).GetRequestStream());
				streamWriter.WriteLine(str);
				streamWriter.Close();
				
				Stream responseStream = httpWebRequest.GetResponse().GetResponseStream();

				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(responseStream);
				xmlDocument1 = xmlDocument2;
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			} finally {
				if (httpWebRequest != null) {
					((WebRequest)httpWebRequest).GetRequestStream().Close();
				}

			}
			return xmlDocument1;
		}

		public string GetSongMD5FromFile(string aFilePath) {
			return getSongMD5(getStreamFromFile(aFilePath));
		}

		private string getMD5FromByteArray(byte[] byteArray) {
			MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider();
			cryptoServiceProvider.ComputeHash(byteArray);
			byte[] hash = cryptoServiceProvider.Hash;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte num in hash) {
				stringBuilder.Append(string.Format("{0:X2}", (object)num));
			}
			return stringBuilder.ToString();
		}
	}
}
