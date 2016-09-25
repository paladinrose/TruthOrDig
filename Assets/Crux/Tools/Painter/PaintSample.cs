using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Crux {
	public class PaintSample : MonoBehaviour {

		public int palletID, sampleID;
		public Painter painter;
		public UnityEvent onPaint, onRemove;
		public LODGroup levelsOfDetail;
		public void Remove(){
			painter.pallet [palletID].Remove (sampleID);
		}
	}
}