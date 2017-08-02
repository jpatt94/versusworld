using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JP
{
	public class Event : MonoBehaviour
	{
		private static Dictionary<string, List<MonoBehaviour>> events;

		/**********************************************************/
		// Interface

		public static void Register(MonoBehaviour listener, string eventName)
		{
			if (events == null)
			{
				events = new Dictionary<string, List<MonoBehaviour>>();
			}

			if (!events.ContainsKey(eventName))
			{
				events[eventName] = new List<MonoBehaviour>();
			}

			if (!events[eventName].Contains(listener))
			{
				events[eventName].Add(listener);
			}
		}

		public static void Unregister(MonoBehaviour listener, string eventName)
		{
			if (events != null)
			{
				if (events.ContainsKey(eventName) && events[eventName].Contains(listener))
				{
					events[eventName].Remove(listener);
				}
			}
		}

		public static void UnregisterAll(MonoBehaviour listener)
		{
			if (events != null)
			{
				foreach (KeyValuePair<string, List<MonoBehaviour>> kv in events)
				{
					if (kv.Value.Contains(listener))
					{
						kv.Value.Remove(listener);
					}
				}
			}
		}

		public static void Trigger(string eventName)
		{
			if (events != null && events.ContainsKey(eventName))
			{
				foreach (MonoBehaviour m in events[eventName])
				{
					m.SendMessage(eventName);
				}
			}
		}
	}
}
