using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;

[System.Serializable]
public class ObjectBinder
{

	public string name;
	public bool active;

	public string[] memberPaths = new string[1];
	public UnityEngine.Object[] baseTargets = new UnityEngine.Object[1];
	public string[] baseTargetLabels = new string[1];
	public bool[] refreshTarget = new bool[1];

	public ExtensionMethods.TaskSuccess ConnectionSuccessCondition, BindAllSuccessCondition;
	public int cscID = 0, bascID = 0;
	public bool[] connectionSuccesses = new bool[0], bindSuccesses = new bool[0];

	public TargetBinding[] bindings = new TargetBinding[0];

	public System.Object[] boundObjects = new System.Object[0];
	public MemberInfo[] boundInfos = new MemberInfo[0];

	#if UNITY_EDITOR
	public float lineHeight, totalLineHeight; 

	public int currentTab, currentTarget, currentBinding, currentModifier, currentConverter, targetListID, bindingListID, modifierListID, converterListID, showTargetType, secondaryListID;
	public bool show;


	public Vector3 hsv;
	public Color primaryC = Color.black, secondaryC, tirtiaryC;

	public Rect drawBox, primaryScrollBar, secondaryScrollBar;


	public FieldInfo[] fieldInfos;
	public PropertyInfo[] propertyInfos;
	public MethodBase[] methodInfos;

	public string[] modifierNames, converterNames, targetNames, targetedMethods;
	#endif


	public bool BindAll(){
		bindSuccesses = new bool[bindings.Length];
		for (int i = 0; i < bindings.Length; i++) {
			if (connectionSuccesses [i]) {
				bindSuccesses [i] = Bind (i);
			} else {
				bindSuccesses [i] = false;
			}
		}
		return ExtensionMethods.CheckTaskSuccess (bindSuccesses, BindAllSuccessCondition, bascID);
	}
	public bool Bind(int id)
	{
		if (active && id >=0 && id < bindings.Length) {

			FieldInfo fieldTo;
			PropertyInfo propertyTo;
			MethodBase methodTo;

			TargetBinding b = bindings [id];
			System.Object primaryOb = null;
			System.Object[] groupObs = new System.Object[b.groupIDs.Length];
			b.successes = new bool[b.groupIDs.Length];

			int i;
			//Get primary:
			//Debug.Log("Primary ID:" + b.primaryID.ToString() + ", Bound Object Length: " + boundObjects.Length);
			if(refreshTarget[b.primaryID]){ Connect(b.primaryID);}
			if (boundObjects [b.primaryID] == null) {
				return false;
			}
			switch (boundInfos [b.primaryID].MemberType) {
			case MemberTypes.Field:
				fieldTo = boundInfos [b.primaryID] as FieldInfo;
				primaryOb = fieldTo.GetValue (boundObjects [b.primaryID]);
				break;
			case MemberTypes.Property:
				propertyTo = boundInfos [b.primaryID] as PropertyInfo;
				primaryOb = propertyTo.GetValue (boundObjects [b.primaryID], null);
				break;
			case MemberTypes.Method:
				methodTo = boundInfos [b.primaryID] as MethodBase;
				primaryOb = methodTo.Invoke (boundObjects [b.primaryID], null);
				break;
			}
			if (primaryOb != null) {
				//Ubiquitous modifier on our primary object:
				if (b.primaryModifier >= 0 && b.primaryModifier < boundObjects.Length) {
					if(refreshTarget[b.primaryModifier]){ Connect (b.primaryModifier);}
					if (boundInfos [b.primaryModifier] != null) {
						methodTo = (MethodBase)boundInfos [b.primaryModifier];
						primaryOb = methodTo.Invoke (boundObjects [b.primaryModifier], new System.Object[]{ primaryOb });
						//primaryOb = methodTo.Invoke(boundObjects[b.primaryModifier],primaryOb);
					}
				}
				//Now apply each group targets modifier.
				for (i = b.groupIDs.Length - 1; i >= 0; i--) {
					if (refreshTarget [b.groupIDs[i]]) {Connect (b.groupIDs [i]);}
					if (b.groupModifiers [i] >= 0 && b.groupModifiers [i] < boundObjects.Length) {
						methodTo = (MethodBase)boundInfos [b.groupModifiers [i]];
						groupObs [i] = methodTo.Invoke (boundObjects[b.groupModifiers[i]], new System.Object[]{ primaryOb });
					}
					if (groupObs [i] == null) {
						groupObs [i] = primaryOb;
					}
				}
				//If we have a converter, we apply it.
				if (b.converter >= 0 && b.converter < boundObjects.Length) {
					methodTo = (MethodBase)boundInfos [b.converter];

					System.Object[] args = new System.Object[]{primaryOb, groupObs};
				
					methodTo.Invoke (boundObjects[b.converter], args);
					primaryOb = args [0];
					groupObs = (System.Object[])args [1];
				} 
				//Apply it to whole group:
				for (i = b.groupIDs.Length - 1; i >= 0; i--) {
					if (boundInfos [b.groupIDs [i]] != null) {
						switch (boundInfos [b.groupIDs [i]].MemberType) {
						case MemberTypes.Field:
							fieldTo = boundInfos [b.groupIDs [i]] as FieldInfo;
							fieldTo.SetValue (boundObjects [b.groupIDs [i]], groupObs [i]);
							break;
						case MemberTypes.Property:
							propertyTo = boundInfos [b.groupIDs [i]] as PropertyInfo;
							propertyTo.SetValue (boundObjects [b.groupIDs [i]], groupObs [i], null);
							break;
						case MemberTypes.Method:
							methodTo = boundInfos [b.groupIDs [i]] as MethodBase;
						//It's likely that this won't work. If it doesn't, I'll need a sub list of groupObs that's the right length. 
							methodTo.Invoke (boundObjects [b.groupIDs [i]], groupObs);
							break;
						}
						b.successes [i] = true;
					} 
				}
				return ExtensionMethods.CheckTaskSuccess (b.successes, b.successCondition);
			}
		} 
		return false;
	}

	public bool Connect()
	{
		if (baseTargets.Length <=0 || baseTargets.Length != memberPaths.Length){
			return false;
		}
			
		int ml = memberPaths.Length;
		//Debug.Log (ml);
		boundObjects = new System.Object[ml];
		boundInfos = new MemberInfo[ml];

		connectionSuccesses = new bool[ml];

		for (int m = 0; m < ml; m++) {
			if (baseTargets [m] == null) {
				return false;
			}

			string[] path = memberPaths[m].Split ("." [0]);
			int i;
			Type pathWalker = baseTargets[m].GetType ();

			boundObjects [m] = baseTargets[m];
			string pf = "";
			IEnumerable arrayHolder;
			IEnumerator arraySifter;
			MemberInfo[] ms;
			for (i = 0; i < path.Length-1; i++) {
					
				//Check to see if this field is an array. If so, it'll end with ].
				//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
				if (path [i].EndsWith ("]")) {
					string interum = path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
					int fieldArrayID = Convert.ToInt32 (interum);
					pf = path [i].Substring (0, path[i].LastIndexOf ("["));
					ms =  pathWalker.GetMember (pf);
					if (ms.Length >0) {
						boundInfos [m] = ms [0];
						if (fieldArrayID >= 0) {
							switch (boundInfos [m].MemberType) {
							case MemberTypes.Field:
								boundObjects [m] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [m]).GetValue (boundObjects[m]));
								break;
							case MemberTypes.Property:
								boundObjects [m] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [m]).GetValue (boundObjects [m], null));
								break;
							}
							arrayHolder = boundObjects [m] as IEnumerable;
							arraySifter = arrayHolder.GetEnumerator ();
							while (fieldArrayID-- >= 0) {
								arraySifter.MoveNext ();
							}
							boundObjects [m] = RuntimeHelpers.GetObjectValue (arraySifter.Current);
							pathWalker = boundObjects [m].GetType ();
						} else {
							switch (boundInfos [m].MemberType) {
							case MemberTypes.Field:
								boundObjects [m] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [m]).GetValue (boundObjects [m]));
								pathWalker = boundObjects [m].GetType ();
								break;
							case MemberTypes.Property:
								boundObjects [m] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [m]).GetValue (boundObjects [m], null));
								pathWalker = boundObjects [m].GetType ();
								break;
							}
						}
					} else {
						pathWalker = null; 
						break;
					}
				} else {
					//Debug.Log ("Getting " + path [i]);
					ms =  pathWalker.GetMember (path[i]);
					if (ms.Length >0) {
						//Debug.Log ("Found " + ms.Length + " members with that name.");
						boundInfos [m] = ms [0];
						switch (boundInfos [m].MemberType) {
						case MemberTypes.Field:
							//Debug.Log ("It was a field.");
							boundObjects [m] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [m]).GetValue (boundObjects [m]));
							pathWalker = boundObjects [m].GetType ();
							//Debug.Log ("It was a field: " + boundObjects[m].ToString());
							break;
						case MemberTypes.Property:
							//Debug.Log ("It was a property.");
							boundObjects [m] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [m]).GetValue (boundObjects [m], null));
							pathWalker = boundObjects [m].GetType ();
							break;
						}
					} else {
						//Debug.Log ("Cound not find any members with that name.");
						pathWalker = null; 
						break;
					}
				}
			}

			//Now, the last one:
			if (pathWalker != null ) {
				//Debug.Log ("Getting " + path [path.Length-1]);
				ms = pathWalker.GetMember (path [path.Length - 1]);
				if (ms.Length > 0) {
					//Debug.Log ("Found " + ms.Length + " members with that name.");
					boundInfos [m] = ms [0];
					connectionSuccesses [m] = true;
				} else {
					//Debug.Log ("Cound not find any members with that name.");
					connectionSuccesses [m] = false;
				}

			} else {
				//Debug.Log ("No object to check.");
				connectionSuccesses [m] = false;
			}
		}
		return ExtensionMethods.CheckTaskSuccess (connectionSuccesses, ConnectionSuccessCondition, cscID);
	}
	public bool Connect(int id)
	{
		if (id >= 0 && id < baseTargets.Length) {
			if (baseTargets [id] == null) {
				return false;
			}

			string[] path = memberPaths[id].Split ("." [0]);
			int i;
			Type pathWalker = baseTargets[id].GetType ();

			boundObjects [id] = baseTargets[id];
			string pf = "";
			IEnumerable arrayHolder;
			IEnumerator arraySifter;
			MemberInfo[] ms;
			for (i = 0; i < path.Length-1; i++) {

				//Check to see if this field is an array. If so, it'll end with ].
				//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
				if (path [i].EndsWith ("]")) {
					string interum = path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
					int fieldArrayID = Convert.ToInt32 (interum);
					pf = path [i].Substring (0, path[i].LastIndexOf ("["));
					ms =  pathWalker.GetMember (pf);
					if (ms.Length >0) {
						boundInfos [id] = ms [0];
						if (fieldArrayID >= 0) {
							switch (boundInfos [id].MemberType) {
							case MemberTypes.Field:
								boundObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [id]).GetValue (boundObjects[id]));
								break;
							case MemberTypes.Property:
								boundObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [id]).GetValue (boundObjects [id], null));
								break;
							}
							arrayHolder = boundObjects [id] as IEnumerable;
							arraySifter = arrayHolder.GetEnumerator ();
							while (fieldArrayID-- >= 0) {
								arraySifter.MoveNext ();
							}
							boundObjects [id] = RuntimeHelpers.GetObjectValue (arraySifter.Current);
							pathWalker = boundObjects [id].GetType ();
						} else {
							switch (boundInfos [id].MemberType) {
							case MemberTypes.Field:
								boundObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [id]).GetValue (boundObjects [id]));
								pathWalker = boundObjects [id].GetType ();
								break;
							case MemberTypes.Property:
								boundObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [id]).GetValue (boundObjects [id], null));
								pathWalker = boundObjects [id].GetType ();
								break;
							}
						}
					} else {
						pathWalker = null; 
						break;
					}
				} else {
					//Debug.Log ("Getting " + path [i]);
					ms =  pathWalker.GetMember (path[i]);
					if (ms.Length >0) {
						//Debug.Log ("Found " + ms.Length + " members with that name.");
						boundInfos [id] = ms [0];
						switch (boundInfos [id].MemberType) {
						case MemberTypes.Field:
							//Debug.Log ("It was a field.");
							boundObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)boundInfos [id]).GetValue (boundObjects [id]));
							pathWalker = boundObjects [id].GetType ();
							//Debug.Log ("It was a field: " + boundObjects[id].ToString());
							break;
						case MemberTypes.Property:
							//Debug.Log ("It was a property.");
							boundObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)boundInfos [id]).GetValue (boundObjects [id], null));
							pathWalker = boundObjects [id].GetType ();
							break;
						}
					} else {
						//Debug.Log ("Cound not find any members with that name.");
						pathWalker = null; 
						break;
					}
				}
			}

			//Now, the last one:
			if (pathWalker != null ) {
				//Debug.Log ("Getting " + path [path.Length-1]);
				ms = pathWalker.GetMember (path [path.Length - 1]);
				if (ms.Length > 0) {
					//Debug.Log ("Found " + ms.Length + " members with that name.");
					boundInfos [id] = ms [0];
					connectionSuccesses [id] = true;
					return true;
				} else {
					//Debug.Log ("Cound not find any members with that name.");
					connectionSuccesses [id] = false;
				}

			} else {
				//Debug.Log ("No object to check.");
				connectionSuccesses [id] = false;
			}
		}
		return false;
	}
	public void SetTarget(UnityEngine.Object targ, string label){
		for (int i = baseTargets.Length - 1; i >= 0; i--) {
			if (baseTargetLabels [i] == label) {
				baseTargets [i] = targ;
				return;
			}
		}
	}
}

[System.Serializable]
public class TargetBinding{
	public string name = "New Binding";
	public int primaryID = -1, primaryModifier = -1, converter = -1;
	public int[] groupIDs = new int[1], groupModifiers = new int[]{-1};
	public ExtensionMethods.TaskSuccess successCondition = ExtensionMethods.TaskSuccess.All;
	public int scID;
	public bool[] successes;
}


