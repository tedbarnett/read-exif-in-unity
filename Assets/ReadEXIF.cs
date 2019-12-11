using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

// Unity 2019 update to TestUnityExif (from https://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data)
// ExifReader described here: http://www.takenet.or.jp/~ryuuji/minisoft/exifread/english/ 


public class ReadEXIF : MonoBehaviour
{

	/// <summary>
	/// Sample file online at "https://lh3.googleusercontent.com/-1c82FJozx0s/U67jGIfUKXI/AAAAAAAAFyM/sdYWzujT9GA/s0-U-I/IMG_1939.JPG";
	/// </summary>

	public RawImage imageHolder; // defined in Unity Inspector
	public Text ExifData;
	public InputField newImageFileInputField;
	public Button newImageFileButton;
	public Button rotateButton;
	private Texture2D texture = null;
	private string imagePath;
	private Texture2D newTexture;
	private string orientationString;

	void Awake()
	{
		newImageFileButton.onClick.AddListener(newImageFile);
		rotateButton.onClick.AddListener(Rotate90Clockwise);
		newImageFile();
		Debug.Log("Persistent path = " + Application.persistentDataPath);   //If you have permissions issues, put your image file here to find it!

	}


	public void newImageFile()
	{
		imagePath = newImageFileInputField.text;
		StartCoroutine(LoadTexture());
	}

	IEnumerator LoadTexture()
	{
		yield return StartCoroutine(LoadByteArrayIntoTexture(this.imagePath));
		if (imageHolder)
		{
			imageHolder.texture = this.texture;
			CorrectRotation();
			imageHolder.SizeToParent(); // see CanvasExtensions.cs for this code
		}
	}


	/// <summary>
	/// ExifLib - http://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data
	/// </summary>


IEnumerator LoadByteArrayIntoTexture(string url)
	{
	UnityWebRequest www = UnityWebRequest.Get(url);
	yield return www.SendWebRequest();

	if (www.isNetworkError || www.isHttpError)
	{
		Debug.Log(www.error);
	}
	else
	{
		// retrieve results as binary data
		byte[] results = www.downloadHandler.data;

			Debug.Log("Finished Getting Image -> SIZE: " + results.Length.ToString());
			ExifLib.JpegInfo jpi = ExifLib.ExifReader.ReadJpeg(results, "Sample File");


			double[] Latitude = jpi.GpsLatitude;
			double[] Longitude = jpi.GpsLongitude;
			orientationString = jpi.Orientation.ToString();

			ExifData.text = "<b>Exif Data:</b>" + "<color=white>";
			ExifData.text = ExifData.text + "\n" + "FileName: " + jpi.FileName;
			ExifData.text = ExifData.text + "\n" + "DateTime: " + jpi.DateTime;
			ExifData.text = ExifData.text + "\n" + "GpsLatitude: " + Latitude[0] + "° " + Latitude[1] + "' " + Latitude[2] + '"';
			ExifData.text = ExifData.text + "\n" + "GpsLongitude: " + Longitude[0] + "° " + Longitude[1] + "' " + Longitude[2] + '"';
			ExifData.text = ExifData.text + "\n" + "Description: " + jpi.Description;
			ExifData.text = ExifData.text + "\n" + "Height: " + jpi.Height + " pixels";
			ExifData.text = ExifData.text + "\n" + "Width: " + jpi.Width + " pixels";
			ExifData.text = ExifData.text + "\n" + "ResolutionUnit: " + jpi.ResolutionUnit;
			ExifData.text = ExifData.text + "\n" + "UserComment: " + jpi.UserComment;
			ExifData.text = ExifData.text + "\n" + "Make: " + jpi.Make;
			ExifData.text = ExifData.text + "\n" + "Model: " + jpi.Model;
			ExifData.text = ExifData.text + "\n" + "Software: " + jpi.Software;
			ExifData.text = ExifData.text + "\n" + "Orientation: " + orientationString;
			ExifData.text = ExifData.text + "</color>";

			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(results);
			newTexture = tex;

			// Not sure why, but many images come in flipped 180 degrees
			//newTexture = rotateTexture(newTexture, true); // Rotate clockwise 90 degrees
			//newTexture = rotateTexture(newTexture, true); // Rotate clockwise 90 degrees (again, to flip it)
			this.texture = newTexture;
		}
}



	public void CorrectRotation()
	{
		// tries to use the jpi.Orientation to rotate the image properly
		newTexture = (Texture2D)imageHolder.texture;

		switch (orientationString)
		{
			case "TopRight": // Rotate clockwise 90 degrees
				newTexture = rotateTexture(newTexture, true);
				break;
			case "TopLeft": // Rotate 0 degrees...
				break;
			case "BottomRight": // Rotate clockwise 180 degrees
				newTexture = rotateTexture(newTexture, true);
				newTexture = rotateTexture(newTexture, true);
				break;
			case "BottomLeft": // Rotate clockwise 270 degrees (I think?)...
				newTexture = rotateTexture(newTexture, true);
				newTexture = rotateTexture(newTexture, true);
				break;
			default:
				break;
		}


		imageHolder.texture = newTexture;
		imageHolder.SizeToParent(); // see CanvasExtensions.cs for this code

	}

	public void Rotate90Clockwise()
	{
		newTexture = (Texture2D)imageHolder.texture;
		newTexture = rotateTexture(newTexture, true); // Rotate clockwise 90 degrees
		imageHolder.texture = newTexture;
		imageHolder.SizeToParent(); // see CanvasExtensions.cs for this code

	}

	Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
	{
		Color32[] original = originalTexture.GetPixels32();
		Color32[] rotated = new Color32[original.Length];
		int w = originalTexture.width;
		int h = originalTexture.height;

		int iRotated, iOriginal;

		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				iRotated = (i + 1) * h - j - 1;
				iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
				rotated[iRotated] = original[iOriginal];
			}
		}

		Texture2D rotatedTexture = new Texture2D(h, w);
		rotatedTexture.SetPixels32(rotated);
		rotatedTexture.Apply();
		return rotatedTexture;
	}
}
