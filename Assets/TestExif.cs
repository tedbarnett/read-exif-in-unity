using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Unity 2019 update to TestUnityExif (from https://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data)
// ExifReader described here: http://www.takenet.or.jp/~ryuuji/minisoft/exifread/english/ 


public class TestExif : MonoBehaviour {

	/// <summary>
	/// Sample file online at "https://lh3.googleusercontent.com/-1c82FJozx0s/U67jGIfUKXI/AAAAAAAAFyM/sdYWzujT9GA/s0-U-I/IMG_1939.JPG";
	/// </summary>

	public RawImage imageHolder; // defined in Unity Inspector
	public Text ExifData;
	public InputField newImageFileInputField;
	public Button newImageFileButton;
	private Texture2D texture = null;
	private string imagePath;

	void Awake() {
		newImageFileButton.onClick.AddListener(newImageFile);
		newImageFile();
		Debug.Log("Persistent path = " + Application.persistentDataPath);	//If you have permissions issues, put your image file here to find it!

	}


	public void newImageFile()
	{
		imagePath = newImageFileInputField.text;
		StartCoroutine(LoadTexture());
	}

	IEnumerator LoadTexture() {
		yield return StartCoroutine(GetImage(this.imagePath));
		if (imageHolder) {
			imageHolder.texture = this.texture;
		}
	}


	/// <summary>
	/// ExifLib - http://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data
	/// </summary>
	IEnumerator GetImage(string url) {
		WWW www = new WWW(url);
		Debug.Log("Fetching image " + url);
		yield return www;
		if (!System.String.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
			this.texture = null;
		} else {
			Debug.Log("Finished Getting Image -> SIZE: " + www.bytes.Length.ToString());
			ExifLib.JpegInfo jpi = ExifLib.ExifReader.ReadJpeg(www.bytes, "Sample File");


			double[] Latitude = jpi.GpsLatitude;
			double[] Longitude = jpi.GpsLongitude;

			ExifData.text = "<b>Exif Data:</b>";
			ExifData.text = ExifData.text + "\n" + "FileName: " + jpi.FileName;
			ExifData.text = ExifData.text + "\n" + "DateTime: " + jpi.DateTime;
			ExifData.text = ExifData.text + "\n" + "GpsLatitude: " + Latitude[0] + " " + Latitude[1] + " " + Latitude[2];
			ExifData.text = ExifData.text + "\n" + "GpsLongitude: " + Longitude[0] + " " + Longitude[1] + " " + Longitude[2];
			ExifData.text = ExifData.text + "\n" + "Description: " + jpi.Description;
			ExifData.text = ExifData.text + "\n" + "Height: " + jpi.Height;
			ExifData.text = ExifData.text + "\n" + "Width: " + jpi.Width;
			ExifData.text = ExifData.text + "\n" + "ResolutionUnit: " + jpi.ResolutionUnit;
			ExifData.text = ExifData.text + "\n" + "UserComment: " + jpi.UserComment;
			ExifData.text = ExifData.text + "\n" + "Software: " + jpi.Software;
			ExifData.text = ExifData.text + "\n" + "Model: " + jpi.Model;
			ExifData.text = ExifData.text + "\n" + "Orientation: " + jpi.Orientation.ToString();
			this.texture = www.texture;
		}
	}
}
