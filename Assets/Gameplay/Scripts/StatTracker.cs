using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class StatTracker : MonoBehaviour
{
	#region AddKill
	public void AddKill(int victimID, int shooterID, int assistID, Vector3 killPosition, Vector3 deathPosition, int killType, int mapID, int gameID)
	{
		StartCoroutine(AddKillWeb(victimID, shooterID, assistID, killPosition, deathPosition, killType, mapID, gameID));
	}

	private IEnumerator AddKillWeb(int victimID, int shooterID, int assistID, Vector3 killPosition, Vector3 deathPosition, int killType, int mapID, int gameID)
	{
		string url = "http://www.versusworldgame.com/stats/add-kill";
		WWWForm form = new WWWForm();
		form.AddField("victim", victimID.ToString());
		form.AddField("shooter", shooterID.ToString());
		form.AddField("assister", assistID.ToString());
		form.AddField("killX", killPosition.x.ToString());
		form.AddField("killY", killPosition.y.ToString());
		form.AddField("killZ", killPosition.z.ToString());
		form.AddField("deathX", deathPosition.x.ToString());
		form.AddField("deathY", deathPosition.y.ToString());
		form.AddField("deathZ", deathPosition.z.ToString());
		form.AddField("killType", killType);
		form.AddField("map", mapID);
		form.AddField("game", gameID);
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		yield return www.Send();

		if (www.isError)
		{
			Debug.Log(www.error);
		}
		else
		{
			// Add Callback To Give Back The User Id
			Debug.Log(www.downloadHandler.text);
		}
	}
	#endregion

	#region AddMedal
	public void AddMedal(int playerID, int medalType, int gameID, float medalTime)
	{
		StartCoroutine(AddMedalWeb(playerID, medalType, gameID, medalTime));
	}

	private IEnumerator AddMedalWeb(int playerID, int medalType, int gameID, float medalTime)
	{
		string url = "http://www.versusworldgame.com/stats/add-medal";
		WWWForm form = new WWWForm();
		form.AddField("player", playerID.ToString());
		form.AddField("medal", medalType.ToString());
		form.AddField("game", gameID.ToString());
		form.AddField("medalTime", medalTime.ToString());
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		yield return www.Send();

		if (www.isError)
		{
			Debug.Log(www.error);
		}
		else
		{
			// Add Callback To Give Back The User Id
			Debug.Log(www.downloadHandler.text);
		}
	}
	#endregion
}