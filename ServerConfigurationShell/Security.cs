using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigurationShell
{
	public class Security
	{
		private static readonly string encryptkey = "!@#$%^&*()";
		private static string tokenIssuer = "self";
		public static readonly TimeSpan TokenExpiredHours = new TimeSpan(2, 0, 0);
		public static String Encrypt(String strText)
		{

			Byte[] byKey = { };

			Byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

			try
			{

				byKey = System.Text.Encoding.UTF8.GetBytes(encryptkey.Substring(0, 8));

				DESCryptoServiceProvider des = new DESCryptoServiceProvider();

				Byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);

				MemoryStream ms = new MemoryStream();

				CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);

				cs.Write(inputByteArray, 0, inputByteArray.Length);

				cs.FlushFinalBlock();

				return Convert.ToBase64String(ms.ToArray());

			}

			catch (Exception ex)
			{

				return null;

			}

		}


		public static String Decrypt(String strText)
		{

			Byte[] byKey = { };

			Byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

			Byte[] inputByteArray = new byte[strText.Length];

			try
			{

				byKey = System.Text.Encoding.UTF8.GetBytes(encryptkey.Substring(0, 8));

				DESCryptoServiceProvider des = new DESCryptoServiceProvider();

				inputByteArray = Convert.FromBase64String(strText);

				MemoryStream ms = new MemoryStream();

				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);

				cs.Write(inputByteArray, 0, inputByteArray.Length);

				cs.FlushFinalBlock();

				System.Text.Encoding encoding = System.Text.Encoding.UTF8;

				return encoding.GetString(ms.ToArray());

			}

			catch (Exception ex)
			{

				return null;

			}
		}
	}
}
