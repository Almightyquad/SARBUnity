﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InputIp : MonoBehaviour {

	public void onEdit()
	{
		NetworkClient.instance.updateHostName(gameObject.transform.FindChild ("Text").GetComponent<Text>().text);
	}
}
