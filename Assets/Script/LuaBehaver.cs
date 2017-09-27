using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BindObj
{
	public string key;
	public GameObject obj;
	public Component component;
}

public class LuaBehaver : MonoBehaviour
{
	//public List<BindObj> m_BindObjList;
	public List<BindObj> m_BindObjList2;
	public List<Vector2> m_BindObjList3;
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
