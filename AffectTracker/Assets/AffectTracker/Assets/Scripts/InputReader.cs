using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace Affect {

	// Store controller related references in one asset
	[Serializable]
	public class ControllerReferences {
		public XRBaseController XrBasecontroller;
		public InputActionReference inputActionVector2DInput;
		public InputActionReference inputActionTouchInput;
		public InputActionReference inputActionSubmit;
	}

	/// <summary>/// Class that reads in the input from a input device. In our case the XR controller/// </summary>
	public class InputReader : MonoBehaviour {

		[HelpBox("<size=14><color=#00ee30><b>Input Reader component</b></color></size>\nListens to the VR controller input. Settings about the VR controller used for the ratings.")]
		[Header("Settings")]
		public Constants.HapticControllers useHapticDevice = Constants.HapticControllers.RIGHT;

		[Space(20)]
		public ControllerReferences controllerRight = new ControllerReferences();
		public ControllerReferences controllerLeft = new ControllerReferences();

		List<XRBaseController> activeHapticControllers = new List<XRBaseController>();

		[Serializable] public class Input2DValueChanged : UnityEvent<Vector2> { } // event definition to fire
		[Serializable] public class InputTouchValueChanged : UnityEvent<bool> { } // event definition to fire
		[Serializable] public class InputTriggerFired : UnityEvent { } // event definition to fire

		[Space(10)]
		[Header("Advanced settings")]
		public Input2DValueChanged OnVector2InputValueChanged; // event link in inspector
		public InputTouchValueChanged OnInputTouchValueChanged;
		public InputTriggerFired OnInputTriggerFired;

		Vector2 currentValues = new Vector2(0, 0);
		Vector2 newValues = new Vector2(0, 0);
		[HideInInspector]
		public bool isTouched = false;

		private void OnEnable() {

			if (controllerRight.XrBasecontroller == null || controllerLeft.XrBasecontroller == null) {
				Debug.LogError("XRbasecontrollers not referenced in <b>Input Reader! </b>");
				return;
			}

			// Start listening to input from chosen controllers
			if (useHapticDevice == Constants.HapticControllers.RIGHT || useHapticDevice == Constants.HapticControllers.BOTH) {
				activeHapticControllers.Add(controllerRight.XrBasecontroller);
				controllerRight.inputActionTouchInput.action.performed += OnTouchInput;
				controllerRight.inputActionVector2DInput.action.performed += OnNewVector2DInput;
				controllerRight.inputActionSubmit.action.performed += OnTriggerFired;

				var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
				UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

				if (rightHandDevices.Count == 0) {
					//Debug.Log("Right hand controller is asleep");
				}
				else {
					bool touchValue;
					if (rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out touchValue) && touchValue) {
						Debug.Log("touched R");
						isTouched = true;
					}
				}
			}

			if (useHapticDevice == Constants.HapticControllers.LEFT || useHapticDevice == Constants.HapticControllers.BOTH) {
				activeHapticControllers.Add(controllerLeft.XrBasecontroller);
				controllerLeft.inputActionTouchInput.action.performed += OnTouchInput;
				controllerLeft.inputActionVector2DInput.action.performed += OnNewVector2DInput;
				controllerLeft.inputActionSubmit.action.performed += OnTriggerFired;

				var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
				UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

				if (leftHandDevices.Count == 0) {
					//Debug.LogError("Left hand controller is asleep");
				}
				else {
					bool touchValue;
					if (leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out touchValue) && touchValue) {
						Debug.Log("touched L");
						isTouched = true;
					}
				}
			}

			OnInputTouchValueChanged.Invoke(isTouched);
		}

		private void OnDisable() {
			controllerRight.inputActionTouchInput.action.performed -= OnTouchInput;
			controllerRight.inputActionVector2DInput.action.performed -= OnNewVector2DInput;
			controllerRight.inputActionSubmit.action.performed -= OnTriggerFired;

			controllerLeft.inputActionTouchInput.action.performed -= OnTouchInput;
			controllerLeft.inputActionVector2DInput.action.performed -= OnNewVector2DInput;
			controllerLeft.inputActionSubmit.action.performed -= OnTriggerFired;

			currentValues = new Vector2(0, 0);

			isTouched = false;
		}

		private void OnTriggerFired(InputAction.CallbackContext obj) {
			OnInputTriggerFired.Invoke();
		}

		public List<XRBaseController> GetXRBaseControllers() {
			return activeHapticControllers;
		}

		private void OnTouchInput(InputAction.CallbackContext obj) {
			isTouched = obj.ReadValueAsButton();
			OnInputTouchValueChanged.Invoke(obj.ReadValueAsButton());
		}

		public void Reset() {
			currentValues = new Vector2(0, 0);
		}

		void FixedUpdate() {

			if (!isTouched)
				return;

			currentValues = newValues;
			OnVector2InputValueChanged.Invoke(currentValues);
		}

		// ======================================================================================================================================================

		#region Controller input
		void OnNewVector2DInput(InputAction.CallbackContext obj) {

			newValues = obj.ReadValue<Vector2>();

			// Remap input values to square
			newValues.x = Mathf.Clamp(newValues.x / 0.6f, -1, 1);
			newValues.y = Mathf.Clamp(newValues.y / 0.6f, -1, 1);
		}

		public void OnFakeInput(Vector2 input, bool touchstate) {
			if (isTouched != touchstate) {
				isTouched = touchstate;
				OnInputTouchValueChanged.Invoke(isTouched);
			}

			currentValues = new Vector2(Mathf.Clamp(input.x / 0.6f, -1, 1), Mathf.Clamp(input.y / 0.6f, -1, 1));
			OnVector2InputValueChanged.Invoke(currentValues);
		}
		#endregion

	}
}