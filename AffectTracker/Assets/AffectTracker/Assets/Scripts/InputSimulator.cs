using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Affect {
	public class InputSimulator : MonoBehaviour {

		[HelpBox("<size=14><color=#00ee30><b>Input Simulator component</b></color></size>\nSimulate some input from a VR controller. Enable for testing without controller.")]
		[Header("Settings")]
		public bool UseSimulator = false;

		[Header("State")]
		public bool IsTouched = false;
		public Vector2 CurrentLocation = new Vector2();

		Vector2 endVal = new Vector2();
		float randomCounter = 1f;
		AffectMapping mappings;
		InputReader inputReader;

		private void Awake() {
			mappings = GetComponent<AffectMapping>();
			inputReader = GetComponent<InputReader>();
		}

		void FixedUpdate() {
			if (!UseSimulator)
				return;

			if (Vector2.Distance(CurrentLocation, endVal) < 0.1f) {
				endVal = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			}

			CurrentLocation = Vector2.Lerp(CurrentLocation, endVal, Time.fixedDeltaTime);

			randomCounter -= 0.01f;

			if (randomCounter < 0f) {
				randomCounter = IsTouched ? Random.Range(0.5f, 1f) : Random.Range(2f, 6f);
				IsTouched = !IsTouched;
			}

			inputReader.OnFakeInput(CurrentLocation, IsTouched);
		}
	}
}
