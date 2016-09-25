using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Crux;
public class TruthOrDigLevelWizard : ScriptableWizard {

	[MenuItem ("Truth Or Dig/New Level")]
	static void CreateWizard(){
		TruthOrDigLevelWizard wiz = ScriptableWizard.DisplayWizard<TruthOrDigLevelWizard> ("New Level", "Create", "Cancel");
		DirectoryInfo dirInfo = new System.IO.DirectoryInfo (Application.dataPath + "/Environment/Graphics");
		DirectoryInfo[] subDirs = dirInfo.GetDirectories ();
		wiz.levelTypes = new string[subDirs.Length];
		wiz.levelTypeSelection = new bool[subDirs.Length];
		for (int i = subDirs.Length - 1; i >= 0; i--) {
			wiz.levelTypes [i] = subDirs [i].Name;
		}
	}

	public string levelName = "New Level";

	public string[] levelTypes;
	public bool[] levelTypeSelection;
	bool showLevelTypes;
	int menuState = 0;
	string loadingMessage = "Loading...";
	bool waitingOnSaveScreen = false;

	override protected bool DrawWizardGUI(){
		
		switch(menuState){
		case 0:
			//default.
			levelName = EditorGUILayout.TextField ("Level Name", levelName);
			showLevelTypes = EditorGUILayout.ToggleLeft ("Level Types", showLevelTypes);
			if (showLevelTypes) {
				for (int i = 0; i < levelTypes.Length; i++) {
					levelTypeSelection [i] = GUILayout.Toggle (levelTypeSelection [i], levelTypes [i]);
				}	
			}

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (createButtonName)) {
				menuState = 1;
			}
			if (GUILayout.Button (otherButtonName)) {
				Close ();
			}
			GUILayout.EndHorizontal ();
			break;
		case 1:
			//Ask if they want to save the current scene.
			GUILayout.Label ("Do you want to save the current scene?");
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Yes")) {
				menuState = 2;
				UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes ();
				waitingOnSaveScreen = true;
				loadingMessage = "Saving";
			}
			if (GUILayout.Button ("No")) {
				menuState = 2;
				EditorApplication.update += CreateNewLevel;
			}
			GUILayout.EndHorizontal ();
			break;
		case 2:
			//Loading new scene.
			GUILayout.Label (loadingMessage);
			if (waitingOnSaveScreen) {
				EditorApplication.update += CreateNewLevel;
				waitingOnSaveScreen = false;
				loadingMessage = "Loading...";
				Repaint ();
			}
			break;
		case 3:
			//New scene loaded. Finalize.
			Close();
			break;
		}
		return true;
	}

	IEnumerator creatingLvl = null;
	void CreateNewLevel()
	{
		if (creatingLvl == null) {
			UnityEditor.SceneManagement.EditorSceneManager.NewScene (UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
			creatingLvl = CreatingLevel ();
		} else {
			if (!creatingLvl.MoveNext ()) {
				creatingLvl = null;
				EditorApplication.update -= CreateNewLevel;
			}
		}
	}
	IEnumerator CreatingLevel(){

		ParallaxCameraController cam = ((GameObject)PrefabUtility.InstantiatePrefab (
		AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Environment/Prefabs/2D Parallax Camera"))).GetComponent<ParallaxCameraController> ();
		yield return null;

		Designer levelDesigner = ((GameObject)PrefabUtility.InstantiatePrefab (
		AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Environment/Prefabs/Level Designer"))).GetComponent<Designer> ();
		yield return null;

		List<PalletItem> pallet = new List<PalletItem> ();
		List<PalletSet> sets = new List<PalletSet> ();
		for (int i = 0; i < levelTypes.Length; i++) {
			if (levelTypeSelection [i]) {
				List<int> palletSet = new List<int> ();
				DirectoryInfo dirInfo = new System.IO.DirectoryInfo (Application.dataPath + "/Environment/Graphic/" + levelTypes[i]);
				FileInfo[] files = dirInfo.GetFiles ();
				for (int j = 0; j < files.Length; j++) {
					PalletItem p = new PalletItem ();
					p.baseItem = ((GameObject)AssetDatabase.LoadAssetAtPath <GameObject>("Assets/Environment/Graphics/" + levelTypes [i] + "/" + files [j].Name)).GetComponent<PaintSample>();
					if (p.baseItem) {
						palletSet.Add (pallet.Count);
						pallet.Add (p);
					}
				}
				if (palletSet.Count > 0) {
					PalletSet ps = new PalletSet ();
					ps.name = levelTypes [i];
					ps.ids = palletSet.ToArray ();
					sets.Add (ps);
				}
			}
			yield return null;
		}

		//Now, I want to load the starting and ending zones
		//
		levelDesigner.painter.pallet = pallet.ToArray ();
		levelDesigner.painter.palletSets = sets.ToArray ();
		menuState = 3;
	}
}
