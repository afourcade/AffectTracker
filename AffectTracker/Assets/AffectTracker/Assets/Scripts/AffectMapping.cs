using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Affect {

	/// <summary>Remap definitions</summary>
	[System.Serializable]
	public class RemapItem {
		[Serializable]
		public class PropertyValueChanged : UnityEvent<float> { }
		public string description;
		public float minValue;
		public float maxValue;
		public Constants.GridProperties CoordinatesInGrid; // Based on what property
		[SerializeField] public PropertyValueChanged OnRemappedValueUpdate; // Event hook
	}

	public class AffectMapping : MonoBehaviour {

		// Event definitions
		public class RemapVector2ValueChanged : UnityEvent<Vector2> { }
		public class RemapFloatValueChanged : UnityEvent<float> { }

		[HelpBox("<size=14><color=#00ee30><b>Affect Mapping component</b></color></size>\nSettings about the mapping of the cartesian/polar coordinates of the affect grid to low-level visual features. For Flubber feedback.")]
		[HideInInspector]
		public float angle = 0f;
		[HideInInspector]
		public float radius = 0f;

		public List<RemapItem> Settings = new List<RemapItem>();

		// ======================================================================================================================================================

		/// <summary>Calculate values based on new 2D vector data (-1 <> 1)</summary>
		public void UpdateValues(Vector2 obj) {

			// Calculate all values
			angle = (Mathf.Abs(obj.x) < 0.005f && Mathf.Abs(obj.y) < 0.005f) ? 0 : (float)(Math.Atan2((double)obj.x, (double)obj.y) * 180 / Math.PI) + 180;
			radius = Vector2.Distance(new Vector2(0, 0), obj);

			// Calculate all remapping and send events
			foreach (RemapItem r in Settings) {
				switch (r.CoordinatesInGrid) {
					case Constants.GridProperties.X_AXIS:
						r.OnRemappedValueUpdate.Invoke(Constants.remap(obj.x, -1, 1, r.minValue, r.maxValue));
						break;
					case Constants.GridProperties.Y_AXIS:
						r.OnRemappedValueUpdate.Invoke(Constants.remap(obj.y, -1, 1, r.minValue, r.maxValue));
						break;
					case Constants.GridProperties.ANGLE:
						r.OnRemappedValueUpdate.Invoke(Constants.remap(angle, 0, 360, r.minValue, r.maxValue));
						break;
					case Constants.GridProperties.RADIUS:
						r.OnRemappedValueUpdate.Invoke(Constants.remap(radius, 0, 1, r.minValue, r.maxValue));
						break;
				}
			}
		}

		// ======================================================================================================================================================

	}
}