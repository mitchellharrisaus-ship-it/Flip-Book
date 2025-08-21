using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace Flipbook_App.Pages;

public class CanvasModel : PageModel
{
	//private static string EncryptPassword(string plainPassword)
	//{
	//	var key = Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23");
	//	var formattedIV = "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC".ToLower().Replace("-", "").Substring(5, 16);
	//	var iv = Encoding.ASCII.GetBytes(formattedIV);

	//	var dataToEncrypt = Encoding.Unicode.GetBytes(plainPassword);

	//	//Encrypt the data.
	//	using (var algorithm = new RijndaelManaged())
	//	using (var encryptor = algorithm.CreateEncryptor(key, iv))
	//	using (var transformationStream = new MemoryStream())
	//	using (var encryptStream = new CryptoStream(transformationStream, encryptor, CryptoStreamMode.Write))
	//	{
	//		//Write all data to the crypto stream and flush it.
	//		encryptStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
	//		encryptStream.FlushFinalBlock();

	//		return Convert.ToBase64String(transformationStream.ToArray());
	//	}
	//}

	public void OnGet()
    {
		//var password = "M9MGqeau7m4xFEzOvsBlQEQzKHByw0RnM9MGqeau7m4xFEzOvsBlQEQzKHByw0Rn";
		//var base64Password = "TTlNR3FlYXU3bTR4RkV6T3ZzQmxRRVF6S0hCeXcwUm5NOU1HcWVhdTdtNHhGRXpPdnNCbFFFUXpLSEJ5dzBSbg==";
		//var encryptedPassword = EncryptPassword(password);
		//Console.WriteLine(encryptedPassword);
	}
}
