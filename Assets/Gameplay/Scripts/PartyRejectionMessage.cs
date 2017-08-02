using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PartyRejectionMessage : MessageBase
{
	private PartyRejectionReason reason;

	public PartyRejectionMessage()
	{
	}

	public PartyRejectionMessage(PartyRejectionReason reason)
	{
		this.reason = reason;
	}

	/**********************************************************/
	// Accessors/Mutators

	public PartyRejectionReason Reason
	{
		get
		{
			return reason;
		}
	}
}