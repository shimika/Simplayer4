using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Simplayer4 {
	public class Lyrics {
		private string a2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:SOAP-ENC=\"http://www.w3.org/2003/05/soap-encoding\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns2=\"ALSongWebServer/Service1Soap\" xmlns:ns1=\"ALSongWebServer\" xmlns:ns3=\"ALSongWebServer/Service1Soap12\">";
		private string b2 = "<SOAP-ENV:Body><ns1:GetLyric5><ns1:stQuery><ns1:strChecksum>CheckSum</ns1:strChecksum><ns1:strVersion>2.0 beta2</ns1:strVersion><ns1:strMACAddress></ns1:strMACAddress><ns1:strIPAddress>255.255.255.0</ns1:strIPAddress></ns1:stQuery></ns1:GetLyric5></SOAP-ENV:Body></SOAP-ENV:Envelope>";
		private string c = "<SOAP-ENV:Body><ns1:GetResembleLyric2><ns1:stQuery><ns1:strTitle>TITLE</ns1:strTitle><ns1:strArtistName>ARTIST</ns1:strArtistName><ns1:nCurPage>0</ns1:nCurPage></ns1:stQuery></ns1:GetResembleLyric2></SOAP-ENV:Body></SOAP-ENV:Envelope>";
		private string d = "http://lyrics.alsong.co.kr/alsongwebservice/service1.asmx";
		private string[] e;
		private string[] f;
		private string md5hashcode;

		public string[] LyricLists {
			get {
				return this.e;
			}
		}

		public string[] MakerLists {
			get {
				return this.f;
			}
		}

		public bool GetStreamLyrics(Stream aStream, bool aViewTime) {
			if (!this.a(this.a(aStream, (this.a2 + this.b2).Replace("^^", "'"), this.d), ref this.e, ref this.f) || this.e == null)
				return false;
			if (!aViewTime)
				this.a(ref this.e);
			return true;
		}

		public bool GetLyricsFromFile(string aFilePath, bool aViewTime) {
			if (!this.a(this.a(this.b(aFilePath), (this.a2 + this.b2).Replace("^^", "'"), this.d), ref this.e, ref this.f) || this.e == null)
				return false;
			if (!aViewTime)
				this.a(ref this.e);
			return true;
		}

		public string GetSongMD5FromFile(string aFilePath) {
			return getSongMD5(this.b(aFilePath), (this.a2 + this.b2).Replace("^^", "'"), this.d);
		}

		public bool GetLyricsFromName(string aArtist, string aTitle, bool aViewTime) {
			if (!this.a(this.b(aArtist, aTitle, (this.a2 + this.c).Replace("^^", "'"), this.d), ref this.e, ref this.f) || this.e == null)
				return false;
			if (!aViewTime)
				this.a(ref this.e);
			return true;
		}

		private void a(ref string[] A_0) {
			for (int index1 = 0; index1 < A_0.Length; ++index1) {
				string[] strArray1 = A_0[index1].Replace("\n", "").Split(new char[1]
        {
          Convert.ToChar("\r")
        });
				A_0[index1] = A_0[index1].Remove(0);
				for (int index2 = 0; index2 < strArray1.Length; ++index2) {
					if (strArray1[index2].Length > 9) {
						strArray1[index2] = strArray1[index2].Substring(10, strArray1[index2].Length - 10);
						string[] strArray2;
						IntPtr index3;
						(strArray2 = A_0)[(int)(index3 = (IntPtr)index1)] = strArray2[(int)index3] + strArray1[index2] + "\r\n";
					}
				}
			}
		}

		private XmlDocument b(string A_0, string A_1, string A_2, string A_3) {
			return this.a(A_1, A_0, A_2, A_3);
		}

		private bool a(XmlDocument A_0, ref string[] A_1, ref string[] A_2) {
			if (A_0 == null)
				return false;
			if (A_0.HasChildNodes) {
				XmlNodeList childNodes = A_0.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes;
				XmlNode xmlNode = childNodes[0];
				if (childNodes.Count > 0) {
					if (xmlNode.Name == "GetResembleLyric2Result") {
						int count = xmlNode.ChildNodes.Count;
						if (count <= 0)
							return false;
						A_1 = new string[count];
						A_2 = new string[count];
						for (int index = 0; index < count; ++index) {
							A_2[index] = Convert.ToString(index + 1) + " (by " + xmlNode.ChildNodes[index].ChildNodes[11].InnerText + ")";
							string str = A_0.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[index].ChildNodes[3].InnerText.Replace("<br>", "\r\n");
							A_1[index] = str;
						}
					} else {
						for (int index = 0; index < xmlNode.ChildNodes.Count; ++index) {
							if (xmlNode.ChildNodes[index].Name == "strLyric") {
								A_1 = new string[1];
								A_2 = new string[1];
								A_2[0] = Convert.ToString(1) + " (by " + xmlNode.ChildNodes[index + 6].InnerText + ")";
								string str = xmlNode.ChildNodes[index].InnerText.Replace("<br>", "\r\n");
								A_1[0] = str;
								break;
							}
						}
					}
				}
			}
			return true;
		}

		private Stream b(string A_0) {
			Stream stream = (Stream)null;
			try {
				stream = (Stream)new FileStream(A_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			} catch {
			}
			return stream;
		}

		private XmlDocument a(Stream A_0, string A_1, string A_2) {
			try {
				if (A_0 == null)
					return (XmlDocument)null;
				int A_1_1 = 0;
				A_0.Position = 0L;
				byte[] buffer = new byte[27];
				A_0.Read(buffer, 0, 27);
				if (buffer.Length < 27 || A_0.Position > 50000L)
					return (XmlDocument)null;
				long position = A_0.Position;
				byte[] A_0_1 = new byte[4];
				Array.Copy((Array)buffer, 0, (Array)A_0_1, 0, 4);
				byte[] numArray1 = new byte[3];
				Array.Copy((Array)buffer, 0, (Array)numArray1, 0, 3);
				if (this.a(A_0_1, 0, 0) == "OggS") {
					A_0.Position = position + 4L;
					this.b(A_0);
				} else if (this.a(numArray1, 0, 0) == "ID3") {
					A_0.Position -= 17L;
					byte[] A_3 = new byte[4];
					Array.Copy((Array)buffer, 6, (Array)A_3, 0, 4);
					this.a(this.a(A_0, ref A_1_1, numArray1, A_3));
				} else {
					A_0.Position = 0L;
					this.a(A_0);
				}
				byte[] numArray2 = new byte[163840];
				A_0.Read(numArray2, 0, 163840);
				return this.a(this.a(numArray2), A_1, A_2);
			} catch {
				return (XmlDocument)null;
			}
		}

		private string getSongMD5(Stream A_0, string A_1, string A_2) {
			try {
				if (A_0 == null) { return ""; }
				int A_1_1 = 0;
				A_0.Position = 0L;
				byte[] buffer = new byte[27];
				A_0.Read(buffer, 0, 27);
				if (buffer.Length < 27 || A_0.Position > 50000L) { return ""; }
				long position = A_0.Position;
				byte[] A_0_1 = new byte[4];
				Array.Copy((Array)buffer, 0, (Array)A_0_1, 0, 4);
				byte[] numArray1 = new byte[3];
				Array.Copy((Array)buffer, 0, (Array)numArray1, 0, 3);
				if (this.a(A_0_1, 0, 0) == "OggS") {
					A_0.Position = position + 4L;
					this.b(A_0);
				} else if (this.a(numArray1, 0, 0) == "ID3") {
					A_0.Position -= 17L;
					byte[] A_3 = new byte[4];
					Array.Copy((Array)buffer, 6, (Array)A_3, 0, 4);
					this.a(this.a(A_0, ref A_1_1, numArray1, A_3));
				} else {
					A_0.Position = 0L;
					this.a(A_0);
				}
				byte[] numArray2 = new byte[163840];
				A_0.Read(numArray2, 0, 163840);
				return this.a(numArray2);
			} catch {
				return "";
			}
		}

		private Stream b(Stream A_0) {
			byte[] numArray = new byte[7];
			int num1 = 0;
			while (A_0.Length >= A_0.Position) {
				A_0.Read(numArray, 0, 7);
				byte[] A_0_1 = new byte[7]
        {
          (byte) 5,
          (byte) 118,
          (byte) 111,
          (byte) 114,
          (byte) 98,
          (byte) 105,
          (byte) 115
        };
				if (this.a(numArray, 0, 0) == this.a(A_0_1, 0, 0)) {
					long num2 = A_0.Position - 7L + 11L;
					A_0.Position = num2;
					break;
				} else {
					A_0.Position = (long)num1;
					++num1;
				}
			}
			return A_0;
		}

		private Stream a(Stream A_0, ref int A_1, byte[] A_2, byte[] A_3) {
			int num;
			for (num = 0; num < 500000; ++num) {
				if (((object)this.a(A_2, 0, 0)).ToString() == "ID3") {
					A_1 = 10 + ((int)A_3[0] << 21 | (int)A_3[1] << 14 | (int)A_3[2] << 7 | (int)A_3[3]);
					if (A_0.CanSeek) {
						A_0.Position = (long)A_1;
						break;
					}
				}
			}
			if (num == 500000)
				A_0.Position = 0L;
			return A_0;
		}

		private Stream a(Stream A_0) {
			for (int index = 0; index < 50000; ++index) {
				if (A_0.ReadByte() == (int)byte.MaxValue && A_0.ReadByte() >> 5 == 7) {
					A_0.Position += -2L;
					break;
				}
			}
			return A_0;
		}

		private XmlDocument a(string A_0, string A_1, string A_2) {
			XmlDocument xmlDocument1 = (XmlDocument)null;
			string str = A_1.Replace("^^", "'").Replace("CheckSum", A_0);
			HttpWebRequest httpWebRequest = (HttpWebRequest)null;
			WebResponse webResponse = (WebResponse)null;
			try {
				httpWebRequest = (HttpWebRequest)WebRequest.Create(A_2);
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
				if (httpWebRequest != null)
					((WebRequest)httpWebRequest).GetRequestStream().Close();
				if (webResponse != null)
					webResponse.GetResponseStream().Close();
			}
			return xmlDocument1;
		}

		private string a(string A_0) {
			byte[] bytes = Encoding.UTF8.GetBytes(A_0);
			string str = "";
			foreach (byte num in bytes)
				str = str + string.Format("{0:0x}", (object)Convert.ToString(num, 16));
			return str;
		}

		private XmlDocument a(string A_0, string A_1, string A_2, string A_3) {
			string str = A_2.Replace("^^", "\"").Replace("TITLE", A_0).Replace("ARTIST", A_1);
			WebRequest webRequest = (WebRequest)null;
			WebResponse webResponse = (WebResponse)null;
			try {
				webRequest = WebRequest.Create(A_3);
				webRequest.Method = "POST";
				webRequest.ContentType = "application/soap+xml";
				StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
				streamWriter.WriteLine(str);
				streamWriter.Close();
				Stream responseStream = webRequest.GetResponse().GetResponseStream();
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(responseStream);
				return xmlDocument;
			} finally {
				if (webRequest != null)
					webRequest.GetRequestStream().Close();
				if (webResponse != null)
					webResponse.GetResponseStream().Close();
			}
		}

		private string a(byte[] A_0, int A_1, int A_2) {
			if (A_2 > 0)
				return Encoding.ASCII.GetString(A_0, A_1, A_2);
			else
				return Encoding.ASCII.GetString(A_0);
		}

		private string a(byte[] A_0) {
			MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider();
			cryptoServiceProvider.ComputeHash(A_0);
			byte[] hash = cryptoServiceProvider.Hash;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte num in hash) {
				stringBuilder.Append(string.Format("{0:X2}", (object)num));
			}
			//System.Windows.MessageBox.Show(((object)stringBuilder).ToString());
			md5hashcode = ((object)stringBuilder).ToString();
			return ((object)stringBuilder).ToString();
		}

		private int a(char A_0) {
			int num1 = Convert.ToInt32(A_0);
			if (num1 < 128)
				return num1;
			try {
				Encoding @default = Encoding.Default;
				char[] chars = new char[1] { A_0 };
				if (@default.IsSingleByte) {
					byte[] bytes = new byte[1];
					@default.GetBytes(chars, 0, 1, bytes, 0);
					return (int)bytes[0];
				} else {
					byte[] bytes = new byte[2];
					if (@default.GetBytes(chars, 0, 1, bytes, 0) == 1)
						return (int)bytes[0];
					if (BitConverter.IsLittleEndian) {
						byte num2 = bytes[0];
						bytes[0] = bytes[1];
						bytes[1] = num2;
					}
					return (int)BitConverter.ToInt16(bytes, 0);
				}
			} catch (Exception ex) {
				throw ex;
			}
		}
	}
}
