using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILastUpdater
{
	void LastUpdate();
}

public class LastUpdateManager : MonoBehaviour
{
	[HideInInspector] public readonly List<ILastUpdater> LastUpdateList = new List<ILastUpdater>();
	
	void LateUpdate ()
	{
		foreach (ILastUpdater updater in LastUpdateList)
		{
			updater.LastUpdate();
		}
	}
}
