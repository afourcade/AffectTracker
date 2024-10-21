using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Affect {

	[RequireComponent(typeof(InputReader))]
	public class ControllerHaptics : MonoBehaviour {

		[HelpBox("<size=14><color=#00ee30><b>Controller haptic component</b></color></size>\nSettings to enable/disable vibrations of the VR controller. E.g., can be used to remind the user to rate continuously.")]
		[Header("Haptics")]
		[Tooltip("Enable haptics")]
		public bool UseHaptics = true;
		[Tooltip("Time between haptic pulses, in seconds")]
		public float PulseInterval = 2f;
		[Tooltip("Duration of haptic pulses, in seconds")]
		public float PulseDuration = 0.5f;
		[Tooltip("Value between 0-1")]
		public float PulseStrength = 0.5f;

		float currentDuration = 0.5f;
		float currentStrength = 0.5f;

		bool isActive = false;
		Coroutine HapticRoutine = null;
		List<XRBaseController> activeHapticControllers = new List<XRBaseController>();

		// ======================================================================================================================================================

		void Start() {
			activeHapticControllers.AddRange(GetComponent<InputReader>().GetXRBaseControllers());

			currentDuration = PulseDuration;
			currentStrength = PulseStrength;

			if (UseHaptics)
				EnableHaptics(true);
		}

		void Reset() {
			if (HapticRoutine != null) StopCoroutine(HapticRoutine); else HapticRoutine = null;
		}

		private void OnDisable() { Reset(); }
		private void OnDestroy() { Reset(); }

		// ======================================================================================================================================================

		/// <summary>
		/// Controls if haptics are being used
		/// </summary>
		/// <param name="onOff">True or false</param>
		public void EnableHaptics(bool onOff) {
			if (onOff) {
				isActive = true;
				if (HapticRoutine != null) StopCoroutine(HapticRoutine);
				HapticRoutine = StartCoroutine(HapticPulseRoutine());
			}
			else {
				if (HapticRoutine != null) StopCoroutine(HapticRoutine);
				HapticRoutine = null;
				isActive = false;
			}
		}
		IEnumerator HapticPulseRoutine() {
			while (isActive) {
				SendHapticPulse();
				yield return new WaitForSecondsRealtime(PulseInterval);
			}
		}

		void SendHapticPulse() {
			foreach (XRBaseController c in activeHapticControllers)
				c.SendHapticImpulse(currentStrength, currentDuration);
		}


		/// <summary>
		/// Delay between the pulses
		/// </summary>
		/// <param name="value">Duration in seconds</param>
		public void UpdatePulseInterval(float value) {
			PulseInterval = value;
		}
		/// <summary>
		/// Update duration of one pulse
		/// </summary>
		/// <param name="value">New Duration</param>
		public void UpdatePulseDuration(float value) {
			currentDuration = value;
		}
		/// <summary>
		/// Update the strength value of the pulse
		/// </summary>
		/// <param name="value">New strength</param>
		public void UpdatePulseStrength(float value) {
			currentStrength = value;
		}
		/// <summary>
		/// Update the touched state
		/// </summary>
		/// <param name="isTouched">True or false</param>
		public void UpdateTouchState(bool isTouched) {
			// Intentially commented out:
			//isActive = isTouched;
		}

	}
}
