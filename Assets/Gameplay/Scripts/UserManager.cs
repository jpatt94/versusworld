using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class UserManager : MonoBehaviour
{
	#region CreateUser
	public void CreateUser(string username, string password, string email)
	{
		StartCoroutine(CreateUserWeb(username, password, email));
	}

	private IEnumerator CreateUserWeb(string username, string password, string email)
	{
		string url = "http://www.versusworldgame.com/users/create";
		WWWForm form = new WWWForm();
		form.AddField("username", username);
		form.AddField("password", password);
		form.AddField("email", email);
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		yield return www.Send();

		if (www.isNetworkError)
		{
			GetComponent<CreateAccountMenu>().OnCreateAccountError(www.error);
			Debug.Log(www.error);
		}
		else
		{
			// Add Callback To Give Back The User Id
			GetComponent<CreateAccountMenu>().OnCreateAccountResponse(www.downloadHandler.text);

			Debug.Log(www.downloadHandler.text);
		}
	}
	#endregion

	#region LoginUser
	public void LoginUser(string username, string password)
	{
		StartCoroutine(LoginUserWeb(username, password));
	}

	private IEnumerator LoginUserWeb(string username, string password)
	{
		string url = "http://www.versusworldgame.com/users/login";
		WWWForm form = new WWWForm();
		form.AddField("username", username);
		form.AddField("password", password);
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		yield return www.Send();

		if (www.isNetworkError)
		{
			GetComponent<LogInMenu>().OnLogInError(www.error);
			Debug.Log(www.error);
		}
		else
		{
			// Add Callback To Give Back The User Id
			GetComponent<LogInMenu>().OnLogInResponse(www.downloadHandler.text);

			Debug.Log(www.downloadHandler.text);
		}
	}
	#endregion
}