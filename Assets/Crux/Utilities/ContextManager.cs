using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ContextManager : ScriptableObject {

	static ContextManager contextManager;

	public static ContextManager GetInstance(){
		if (contextManager == null) {
			contextManager = FindObjectOfType<ContextManager> ();
			if (contextManager == null) {
				contextManager = ScriptableObject.CreateInstance<ContextManager> ();
			}
		}
		return contextManager;
	}


	List<string> tagNames = new List<string>();
	List<ContextTags> contextTags = new List<ContextTags>();

	public string this[int index]{
		get { return tagNames [index];}
	}

	public string[] GetAllTags()
	{
		return tagNames.ToArray ();
	}

	public string[] GetTags(int[] ids)
	{
		string[] t = new string[ids.Length];
		for (int i = ids.Length - 1; i >= 0; i--) {
			t [i] = tagNames [ids [i]];
		}
		return t;
	}
	public int Length { get { return tagNames.Count; } }

	public int Add(string s)
	{
		for (int i = tagNames.Count - 1; i >= 0; i--) {
			if (s == tagNames [i]) {
				return i;
			}
		}
		tagNames.Add (s);
		return tagNames.Count - 1;
	}

	public void Remove(int t)
	{
		if (t >= 0 && t < tagNames.Count) {
			int[] newIDs = new int[this.Length];
			int i;
			for (i = 0; i < newIDs.Length; i++) {
				if (i < t) {
					newIDs [i] = i;
				} else if (i >t){
					newIDs [i] = i - 1;
				} else {
					newIDs [i] = -1;
				}
			}
			tagNames.RemoveAt (t);
			for (i = contextTags.Count - 1; i >= 0; i--) {
				contextTags [i].Restructure (newIDs);
			}
		}
	}

	public void AddContextTags(ContextTags c)
	{
		int id = contextTags.IndexOf(c);
		if (id < 0) {
			contextTags.Add (c);
		}
	}

	public void RemoveContextTags(ContextTags c)
	{
		contextTags.Remove (c);
	}

	public void RemoveContextTags(int c)
	{
		contextTags.RemoveAt (c);
	}
	public ContextTags NewContextTags(){
		ContextTags c = new ContextTags ();
		contextTags.Add (c);
		c.SetupTags (this);
		return c;
	}

	public AffinityTags<int> NewAffinityTags(int s){
		AffinityTags<int> c = new AffinityTags<int> (s);
		contextTags.Add (c);
		c.SetupTags (this);
		return c;
	}

}
