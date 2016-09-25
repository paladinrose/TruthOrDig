using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class intRange {

	public int min, max, critical, value;
	public bool lockMin, lockMax;

	public intRange(int value, int min, int max, int critical )
	{
		this.max = max; this.min = min; this.critical = critical; this.value = value;
		if (min < value) {lockMin = true;} else {lockMin = false;}
		if (max > value) {lockMax = true;} else {lockMax = false;}
	}

	public bool Critical(){
		if (value <= critical) {
			return true;
		}
		return false;
	}

	public static implicit operator intRange(int i)
	{
		return new intRange (i, i - 1, i + 1, i);
	}

	public static implicit operator int(intRange ir)
	{
		return ir.value;
	}

	public static bool operator ==(intRange r1, intRange r2)
	{
		if (r1.min == r2.min &&
			r1.max == r2.max &&
			r1.critical == r2.critical &&
			r1.value == r2.value) {
			return true;
		} 
		return false;
	}
	public static bool operator ==(intRange r1, int r2)
	{
		if (r1.value == r2) {
			return true;
		} 
		return false;
	}
	public static bool operator !=(intRange r1, intRange r2)
	{
		if (r1.min != r2.min ||
			r1.max != r2.max ||
			r1.critical != r2.critical ||
			r1.value != r2.value) {
			return true;
		} 
		return false;
	}
	public static bool operator !=(intRange r1, int r2)
	{
		if (r1.value != r2) {
			return true;
		} 
		return false;
	}

	public static intRange operator +(intRange r1, intRange r2)
	{
		r1.value += r2.value;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}
	public static intRange operator +(intRange r1, int r2)
	{
		r1.value += r2;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}
	public static intRange operator ++(intRange r1)
	{
		r1.value ++;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}

	public static intRange operator -(intRange r1, intRange r2)
	{
		r1.value -= r2.value;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static intRange operator -(intRange r1, int r2)
	{
		r1.value -= r2;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static intRange operator --(intRange r1)
	{
		r1.value --;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}

	public static intRange operator *(intRange r1, intRange r2)
	{
		r1.value *= r2.value;
		if (r1.lockMax && r1.value > r1.max){
			r1.value = r1.max;
		}
		return r1;
	}
	public static intRange operator *(intRange r1, int r2)
	{
		r1.value *= r2;
		if (r1.lockMax && r1.value > r1.max){
			r1.value = r1.max;
		}
		return r1;
	}

	public static intRange operator /(intRange r1, intRange r2)
	{
		r1.value /= r2.value;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static intRange operator /(intRange r1, int r2)
	{
		r1.value /= r2;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}

	public override bool Equals(object o)
	{
		try{
			return (this == (intRange) o);
		}catch{
			return false;
		}
	}
	public override int GetHashCode ()
	{
		return value;
	}
}

[System.Serializable]
public class floatRange {

	public float min, max, critical, value;
	public bool lockMin, lockMax;

	public floatRange(float value, float min, float max, float critical )
	{
		this.max = max; this.min = min; this.critical = critical; this.value = value;
		if (min < value) {lockMin = true;} else {lockMin = false;}
		if (max > value) {lockMax = true;} else {lockMax = false;}
	}

	public bool Critical(){
		if (value <= critical) {
			return true;
		}
		return false;
	}

	public static implicit operator floatRange(float i)
	{
		return new floatRange (i, i - 1, i + 1, i);
	}

	public static implicit operator float(floatRange ir)
	{
		return ir.value;
	}

	public static bool operator ==(floatRange r1, floatRange r2)
	{
		if (r1.min == r2.min &&
		   r1.max == r2.max &&
		   r1.critical == r2.critical &&
		   r1.value == r2.value) {
			return true;
		} 
		return false;
	}
	public static bool operator ==(floatRange r1, float r2)
	{
		if (r1.value == r2) {
			return true;
		} 
		return false;
	}
	public static bool operator !=(floatRange r1, floatRange r2)
	{
		if (r1.min != r2.min ||
			r1.max != r2.max ||
			r1.critical != r2.critical ||
			r1.value != r2.value) {
			return true;
		} 
		return false;
	}
	public static bool operator !=(floatRange r1, float r2)
	{
		if (r1.value != r2) {
			return true;
		} 
		return false;
	}
		
	public static floatRange operator +(floatRange r1, floatRange r2)
	{
		r1.value += r2.value;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}
	public static floatRange operator +(floatRange r1, float r2)
	{
		r1.value += r2;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}
	public static floatRange operator ++(floatRange r1)
	{
		r1.value ++;
		if (r1.lockMax && r1.value > r1.max) {
			r1.value = r1.max;
		}
		return r1;
	}

	public static floatRange operator -(floatRange r1, floatRange r2)
	{
		r1.value -= r2.value;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static floatRange operator -(floatRange r1, float r2)
	{
		r1.value -= r2;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static floatRange operator --(floatRange r1)
	{
		r1.value --;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}

	public static floatRange operator *(floatRange r1, floatRange r2)
	{
		r1.value *= r2.value;
		if (r1.lockMax && r1.value > r1.max){
			r1.value = r1.max;
		}
		return r1;
	}
	public static floatRange operator *(floatRange r1, float r2)
	{
		r1.value *= r2;
		if (r1.lockMax && r1.value > r1.max){
			r1.value = r1.max;
		}
		return r1;
	}

	public static floatRange operator /(floatRange r1, floatRange r2)
	{
		r1.value /= r2.value;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}
	public static floatRange operator /(floatRange r1, float r2)
	{
		r1.value /= r2;
		if (r1.lockMin && r1.value < r1.min) {
			r1.value = r1.min;
		}
		return r1;
	}

	public override bool Equals(object o)
	{
		try{
			return (this == (floatRange) o);
		}catch{
			return false;
		}
	}
	public override int GetHashCode ()
	{
		return Mathf.RoundToInt(value);
	}
}


[System.Serializable]
public class ContextTags
{
	public static ContextManager contextManager;

	int[] tags;

	public ContextTags(){
		ResetTags (0);
	}
	public void SetupTags(ContextManager c = null){
		if (c == null) {
			contextManager = ContextManager.GetInstance ();
		} else {
			contextManager = c;
		}
		contextManager.AddContextTags (this);
	}
	public void ResetTags(int l){
		tags = new int [l];
	}
		
	public int this[int index]{
		get { return tags [index];}
	}

	public int Length{
		get { return tags.Length; }
	}

	public string[] GetTags()
	{
		string[] t = contextManager.GetTags (tags);
		return t;
	}
	public virtual int Add(string s)
	{
		return Add(contextManager.Add (s));
	}
	public virtual int Add(int s)
	{
		if (s < contextManager.Length) {
			for (int i = tags.Length - 1; i >= 0; i--) {
				if (s == tags [i]) {
					return i;
				}
			}
			List<int> newtags = new List<int> (tags);
			newtags.Add (s);
			tags = newtags.ToArray ();
			return tags.Length - 1;
		} else {
			return -1;
		}
	}

	public virtual void Restructure(int[] ids)
	{
		List<int> toRemove = new List<int> ();
		for (int i = tags.Length - 1; i >= 0; i--) {
			if(ids[tags[i]] >=0){
				tags [i] = ids [tags [i]];
			} else {
				toRemove.Add (i);
			}
		}
		if (toRemove.Count > 0) {
			List<int> newTags = new List<int> (tags);
			for (int tr = 0; tr < toRemove.Count; tr++) {
				newTags.RemoveAt (toRemove [tr]);
			}
			tags = newTags.ToArray ();
		}
	}

	public virtual int[] Compare(ContextTags c)
	{
		//Returns all the ids on THIS list, that the corresponding list has.
		List<int> ret = new List<int>();
		for (int i = Length; i >= 0; i--) {
			bool has = false;
			for (int j = c.Length; j >= 0; j--) {
				if (c [j] == this [i]) {
					has = true; break;
				}
			}
			if (has) {
				ret.Add (i);
			}
		}
		return ret.ToArray ();
	}

}
[System.Serializable]
public class AffinityTags<T> : ContextTags
{
	public T[] affinity;

	public AffinityTags()
	{
		if (contextManager == null) {
			contextManager = ContextManager.GetInstance ();
		}
		ResetTags (0);
		affinity = new T[0];
		contextManager.AddContextTags (this);
	}
	public AffinityTags(int s)
	{
		if (contextManager == null) {
			contextManager = ContextManager.GetInstance ();
		}
		ResetTags (s);
		affinity = new T[s];
	}
	public override int Add (int s)
	{
		return -1;
	}

	public override int Add (string s)
	{
		return -1;
	}

	public int Add(int s, T t)
	{
		int i = base.Add (s);
		if (i < affinity.Length) {
			affinity [i] = t;
			return i;
		}
		List<T> newAffinity = new List<T> (affinity);
		newAffinity.Add (t);
		affinity = newAffinity.ToArray ();
		return affinity.Length - 1;
	}

	public int Add(string s, T t)
	{
		int i = base.Add (s);
		if (i < affinity.Length) {
			affinity [i] = t;
			return i;
		}

		List<T> newAffinity = new List<T> (affinity);
		newAffinity.Add (t);
		affinity = newAffinity.ToArray ();
		return affinity.Length - 1;
	}

	public override void Restructure(int[] ids)
	{
		List<int> toRemove = new List<int> ();
		T[] newaff = new T[affinity.Length];
		for (int i = affinity.Length - 1; i >= 0; i--) {
			if (this[i] < ids.Length) {
				newaff [i] = affinity[ids[this[i]]];
			} else {
				toRemove.Add (i);
			}
		}
		if (toRemove.Count > 0) {
			List<T> newaffinity = new List<T> (newaff);
			for (int tr = 0; tr < toRemove.Count; tr++) {
				newaffinity.RemoveAt (toRemove [tr]);
			}
			affinity = newaffinity.ToArray ();
		}
		base.Restructure (ids);
	}
}