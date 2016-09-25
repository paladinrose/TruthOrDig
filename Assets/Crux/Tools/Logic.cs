using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Crux{
	public class Logic : MonoBehaviour {

		public LogicTarget[] targets;
		LogicEvent[] currentEvents;



	}

	[System.Serializable]
	public class LogicTarget{
		public Component baseTarget;
		public string targetName;

		string[] subTargets;

		#if UNITY_EDITOR
		bool[] connected;
		#endif

		public string this[int index]{
			get { return subTargets [index]; }
			set { subTargets [index] = value; }
		}
	}

	//We GET info from our InputSource and send it TO our Output source.
	[System.Serializable]
	public class LogicEvent{
		/// <summary>
		/// A binding GETS information from the Input Source.
		/// </summary>
		public int inputSource;

		/// <summary>
		/// A binding SETS information on the Output Source.
		/// </summary>
		public int outputSource;

		/// <summary>
		/// Binding GETS information from an input target, modifies it with the same id slot in Binding Methods, and then SETS it on the same id slot of Output Targets.
		/// </summary>
		public int[]inputTargets;

		/// <summary>
		/// Binding GETS information from an input target, modifies it with the same id slot in Binding Methods, and then SETS it on the same id slot of Output Targets.
		/// </summary>
		public int[]outputTargets;

		/// <summary>
		/// Binding GETS information from an input target, modifies it with the same id slot in Binding Methods, and then SETS it on the same id slot of Output Targets.
		/// </summary>
		public int[] bindingMethods;

		/// <summary>
		/// The event results tell associated nodes or sequences what to do when this event is complete.
		/// </summary>
		int[] eventResults;

		public IEnumerator Invoke(Logic l){
			//Evaluate our inputs into our binding methods and finally into our Output Methods. Any that are an IEnumerator method get set aside for an Omniroutine.
			LogicTarget it = l.targets[inputSource];

			System.Object[] inputObjects = new System.Object[inputTargets.Length], 
			bindingObjects = new System.Object[bindingMethods.Length], 
			outputObjects = new System.Object[outputTargets.Length];

			MemberInfo[] ms, inputInfos = new MemberInfo[inputTargets.Length],
			bindingInfos = new MemberInfo[bindingMethods.Length],
			outputInfos = new MemberInfo[outputTargets.Length];

			eventResults = new int[inputTargets.Length];

			IEnumerable arrayHolder;
			IEnumerator arraySifter;

			for (int id = 0; id < inputTargets.Length; id++) {
				eventResults [id] = -1;
				string[] path = it [inputTargets [id]].Split ("." [0]);
				int i;
				System.Type pathWalker = it.baseTarget.GetType ();

				inputObjects [id] = it.baseTarget;
				string pf = "";


				for (i = 0; i < path.Length - 1; i++) {

					//Check to see if this field is an array. If so, it'll end with ].
					//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
					if (path [i].EndsWith ("]")) {
						string interum = path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
						int fieldArrayID = System.Convert.ToInt32 (interum);
						pf = path [i].Substring (0, path [i].LastIndexOf ("["));
						ms = pathWalker.GetMember (pf);
						if (ms.Length > 0) {
							inputInfos [id] = ms [0];
							if (fieldArrayID >= 0) {
								switch (inputInfos [id].MemberType) {
								case MemberTypes.Field:
									inputObjects [id] = ((FieldInfo)inputInfos [id]).GetValue (inputObjects [id]);
									break;
								case MemberTypes.Property:
									inputObjects [id] = ((PropertyInfo)inputInfos [id]).GetValue (inputObjects [id], null);
									break;
								
								}
								arrayHolder = inputObjects [id] as IEnumerable;
								arraySifter = arrayHolder.GetEnumerator ();
								while (fieldArrayID-- >= 0) {
									arraySifter.MoveNext ();
								}
								inputObjects [id] = RuntimeHelpers.GetObjectValue (arraySifter.Current);
								pathWalker = inputObjects [id].GetType ();
							} else {
								switch (inputInfos [id].MemberType) {
								case MemberTypes.Field:
									inputObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)inputInfos [id]).GetValue (inputObjects [id]));
									pathWalker = inputObjects [id].GetType ();
									break;
								case MemberTypes.Property:
									inputObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)inputInfos [id]).GetValue (inputObjects [id], null));
									pathWalker = inputObjects [id].GetType ();
									break;
								}
							}
						} else {
							pathWalker = null; 
							break;
						}
					} else {
						//Debug.Log ("Getting " + path [i]);
						ms = pathWalker.GetMember (path [i]);
						if (ms.Length > 0) {
							//Debug.Log ("Found " + ms.Length + " members with that name.");
							inputInfos [id] = ms [0];
							switch (inputInfos [id].MemberType) {
							case MemberTypes.Field:
							//Debug.Log ("It was a field.");
								inputObjects [id] = ((FieldInfo)inputInfos [id]).GetValue (inputObjects [id]);
								pathWalker = inputObjects [id].GetType ();
							//Debug.Log ("It was a field: " + inputObjects[id].ToString());
								break;
							case MemberTypes.Property:
							//Debug.Log ("It was a property.");
								inputObjects [id] = ((PropertyInfo)inputInfos [id]).GetValue (inputObjects [id], null);
								pathWalker = inputObjects [id].GetType ();
								break;
							case MemberTypes.Method:
								//Check its return type. It should be non-void. If it's IEnumerator, we need to add it to our list of things to wait on.
								MethodInfo methInf = (MethodInfo)inputInfos [id];
								if (methInf.ReturnType != typeof(void)) {
									inputObjects [id] = methInf.Invoke (inputObjects [id], null);
									pathWalker = inputObjects [id].GetType ();
								}
								break;
							}
						} else {
							pathWalker = null; 
							break;
						}
					}
				}

				//Now, the last one:
				if (pathWalker != null) {
					//Debug.Log ("Getting " + path [path.Length-1]);
					ms = pathWalker.GetMember (path [path.Length - 1]);
					if (ms.Length > 0) {
						//Debug.Log ("Found " + ms.Length + " members with that name.");
						inputInfos [id] = ms [0];

					} 
				} 
			}

			//Now, we use each Binding Method. They take inputObjects in the same line as the input parameter of our binding method.
			for (int id = 0; id < bindingMethods.Length; id++) {
				if(it[bindingMethods[id]] != ""){
					string[] path = it [bindingMethods [id]].Split ("." [0]);
					int i;
					System.Type pathWalker = it.baseTarget.GetType ();

					bindingObjects [id] = it.baseTarget;
					string pf = "";


					for (i = 0; i < path.Length - 1; i++) {

						//Check to see if this field is an array. If so, it'll end with ].
						//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
						if (path [i].EndsWith ("]")) {
							string interum = path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
							int fieldArrayID = System.Convert.ToInt32 (interum);
							pf = path [i].Substring (0, path [i].LastIndexOf ("["));
							ms = pathWalker.GetMember (pf);
							if (ms.Length > 0) {
								bindingInfos [id] = ms [0];
								if (fieldArrayID >= 0) {
									switch (bindingInfos [id].MemberType) {
									case MemberTypes.Field:
										bindingObjects [id] = ((FieldInfo)bindingInfos [id]).GetValue (bindingObjects [id]);
										break;
									case MemberTypes.Property:
										bindingObjects [id] = ((PropertyInfo)bindingInfos [id]).GetValue (bindingObjects [id], null);
										break;

									}
									arrayHolder = bindingObjects [id] as IEnumerable;
									arraySifter = arrayHolder.GetEnumerator ();
									while (fieldArrayID-- >= 0) {
										arraySifter.MoveNext ();
									}
									bindingObjects [id] = RuntimeHelpers.GetObjectValue (arraySifter.Current);
									pathWalker = bindingObjects [id].GetType ();
								} else {
									switch (bindingInfos [id].MemberType) {
									case MemberTypes.Field:
										bindingObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)bindingInfos [id]).GetValue (bindingObjects [id]));
										pathWalker = bindingObjects [id].GetType ();
										break;
									case MemberTypes.Property:
										bindingObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)bindingInfos [id]).GetValue (bindingObjects [id], null));
										pathWalker = bindingObjects [id].GetType ();
										break;
									}
								}
							} else {
								pathWalker = null; 
								break;
							}
						} else {
							//Debug.Log ("Getting " + path [i]);
							ms = pathWalker.GetMember (path [i]);
							if (ms.Length > 0) {
								//Debug.Log ("Found " + ms.Length + " members with that name.");
								bindingInfos [id] = ms [0];
								switch (bindingInfos [id].MemberType) {
								case MemberTypes.Field:
									//Debug.Log ("It was a field.");
									bindingObjects [id] = ((FieldInfo)bindingInfos [id]).GetValue (bindingObjects [id]);
									pathWalker = bindingObjects [id].GetType ();
									//Debug.Log ("It was a field: " + bindingObjects[id].ToString());
									break;
								case MemberTypes.Property:
									//Debug.Log ("It was a property.");
									bindingObjects [id] = ((PropertyInfo)bindingInfos [id]).GetValue (bindingObjects [id], null);
									pathWalker = bindingObjects [id].GetType ();
									break;
								case MemberTypes.Method:
									//Check its return type. It should be non-void. If it's IEnumerator, we need to add it to our list of things to wait on.
									MethodInfo methInf = (MethodInfo)bindingInfos [id];
									if (methInf.ReturnType != typeof(void)) {
										bindingObjects [id] = methInf.Invoke (bindingObjects [id], null);
										pathWalker = bindingObjects [id].GetType ();
									}
									break;
								}
							} else {
								pathWalker = null; 
								break;
							}
						}
					}

					//Now, the last one:
					if (pathWalker != null) {
						//Debug.Log ("Getting " + path [path.Length-1]);
						ms = pathWalker.GetMember (path [path.Length - 1]);
						if (ms.Length > 0) {
							//Debug.Log ("Found " + ms.Length + " members with that name.");
							bindingInfos [id] = ms [0];
							inputObjects [id] = ((MethodInfo)bindingInfos [id]).Invoke (bindingObjects [id], new System.Object[]{ inputObjects [id] });
						} 
					} 
				}
			}

			//And, finally, we use our bindingObjects (converted and in proper form, we assume) to set our outputs. If they're either a field or property, we just set it. 
			//If they're methods, we follow what we did in bindingMethods.
			for (int id = 0; id < outputTargets.Length; id++) {

				string[] path = it [outputTargets [id]].Split ("." [0]);
				int i;
				System.Type pathWalker = it.baseTarget.GetType ();

				outputObjects [id] = it.baseTarget;
				string pf = "";


				for (i = 0; i < path.Length - 1; i++) {

					//Check to see if this field is an array. If so, it'll end with ].
					//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
					if (path [i].EndsWith ("]")) {
						string interum = path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
						int fieldArrayID = System.Convert.ToInt32 (interum);
						pf = path [i].Substring (0, path [i].LastIndexOf ("["));
						ms = pathWalker.GetMember (pf);
						if (ms.Length > 0) {
							outputInfos [id] = ms [0];
							if (fieldArrayID >= 0) {
								switch (outputInfos [id].MemberType) {
								case MemberTypes.Field:
									outputObjects [id] = ((FieldInfo)outputInfos [id]).GetValue (outputObjects [id]);
									break;
								case MemberTypes.Property:
									outputObjects [id] = ((PropertyInfo)outputInfos [id]).GetValue (outputObjects [id], null);
									break;

								}
								arrayHolder = outputObjects [id] as IEnumerable;
								arraySifter = arrayHolder.GetEnumerator ();
								while (fieldArrayID-- >= 0) {
									arraySifter.MoveNext ();
								}
								outputObjects [id] = RuntimeHelpers.GetObjectValue (arraySifter.Current);
								pathWalker = outputObjects [id].GetType ();
							} else {
								switch (outputInfos [id].MemberType) {
								case MemberTypes.Field:
									outputObjects [id] = RuntimeHelpers.GetObjectValue (((FieldInfo)outputInfos [id]).GetValue (outputObjects [id]));
									pathWalker = outputObjects [id].GetType ();
									break;
								case MemberTypes.Property:
									outputObjects [id] = RuntimeHelpers.GetObjectValue (((PropertyInfo)outputInfos [id]).GetValue (outputObjects [id], null));
									pathWalker = outputObjects [id].GetType ();
									break;
								}
							}
						} else {
							pathWalker = null; 
							break;
						}
					} else {
						//Debug.Log ("Getting " + path [i]);
						ms = pathWalker.GetMember (path [i]);
						if (ms.Length > 0) {
							//Debug.Log ("Found " + ms.Length + " members with that name.");
							outputInfos [id] = ms [0];
							switch (outputInfos [id].MemberType) {
							case MemberTypes.Field:
								//Debug.Log ("It was a field.");
								outputObjects [id] = ((FieldInfo)outputInfos [id]).GetValue (outputObjects [id]);
								pathWalker = outputObjects [id].GetType ();
								//Debug.Log ("It was a field: " + outputObjects[id].ToString());
								break;
							case MemberTypes.Property:
								//Debug.Log ("It was a property.");
								outputObjects [id] = ((PropertyInfo)outputInfos [id]).GetValue (outputObjects [id], null);
								pathWalker = outputObjects [id].GetType ();
								break;
							case MemberTypes.Method:
								//Check its return type. It should be non-void. If it's IEnumerator, we need to add it to our list of things to wait on.
								MethodInfo methInf = (MethodInfo)outputInfos [id];
								if (methInf.ReturnType != typeof(void)) {
									outputObjects [id] = methInf.Invoke (outputObjects [id], null);
									pathWalker = outputObjects [id].GetType ();
								}
								break;
							}
						} else {
							pathWalker = null; 
							break;
						}
					}
				}

				//Now, the last one:
				if (pathWalker != null) {
					//Debug.Log ("Getting " + path [path.Length-1]);
					ms = pathWalker.GetMember (path [path.Length - 1]);
					if (ms.Length > 0) {
						//Debug.Log ("Found " + ms.Length + " members with that name.");
						outputInfos [id] = ms [0];
						switch (outputInfos [id].MemberType) {
						case MemberTypes.Field:
							//Debug.Log ("It was a field.");
							((FieldInfo)outputInfos [id]).SetValue (outputObjects [id], inputObjects [id]);
							break;
						case MemberTypes.Property:
							//Debug.Log ("It was a property.");
							((PropertyInfo)outputInfos [id]).SetValue (outputObjects [id],inputObjects[id], null);
							break;
						case MemberTypes.Method:
							//Check its return type. It should be non-void. If it's IEnumerator, we need to add it to our list of things to wait on.
							MethodInfo methInf = (MethodInfo)outputInfos [id];
							if (methInf.ReturnType == typeof(int)) {
								eventResults[id] = (int)methInf.Invoke (outputObjects [id], new System.Object[]{ inputObjects [id] });
							} else {
								methInf.Invoke (outputObjects [id], new System.Object[]{ inputObjects [id] });
							}
							break;
						}
					} 
				} 
			}
			yield return null;
		}
	}


	[System.Serializable]
	public class LogicSequence {
		public LogicNode[] nodes;

		public int rootNode, currentNode;

	}

	[System.Serializable]
	public class LogicNode{
		public ExtensionMethods.TaskSuccess successCondition;
		public LogicEvent[] nodeEvents;

		public int nodeState;

		//When we run a procedure, it runs its bindings.
		//The result of those bindings should be an int which tells us what to do next:
		// -1 = wait.
		// -2 = end.
		//< -2 
		// > -1 = Go To Node

		//Nodes can also run processes that take time. That's why we have a Wait setting.
		//The Logic class should update any running sequences on every update cycle. The whole cycle will return Wait if its on a step that it's waiting on.
		//I should allow for maximum waits that force another step to happen, etc.

	}
}