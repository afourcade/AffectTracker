using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Affect {
	public class RatingLogger : MonoBehaviour {

		[HelpBox("<size=14><color=#00ee30><b>Rating Logger component</b></color></size>\nSettings about the recording/logging/saving of ratings.")]
		[Header("Settings")]
		public Constants.RatingType RatingType = Constants.RatingType.CR;
		[Tooltip("In Hz")]
		public float SamplingFrequency = 20f;
		[Tooltip("Auto start logging (CR mode only)")]
		public bool AutoStart = false;

		[Space(20)]
		[Tooltip("Auto save to disk")]
		public bool AutoSaveToDisk = false;
		[Tooltip("Interval (in seconds) between savings to disk")]
		public float SaveToDiskInterval = 30f;
		[Tooltip("File will be saved in default ApplicationPath")]
		public string Filename = "AffectTrackerLogfile.txt";

		List<string[]> inputValuesLog = new List<string[]>();
		Coroutine LoggingRoutine = null;

		bool isRunning = false;
		bool isRecording = false;
		Vector2 currentValues = new Vector2();
		InputReader inputReader = null;

		// ======================================================================================================================================================

		private void Awake() {
			inputReader = GetComponent<InputReader>();
		}

		private void Start() {
			if (AutoStart && RatingType == Constants.RatingType.CR)
				Init();
		}

		/// <summary>Set ratingtype [0=CR][1=SR] and interval, starts logging if CR</summary>
		public void Init() {

			Reset();

			inputValuesLog.Clear();

			// If it is CR, start logging routine
			if (RatingType == Constants.RatingType.CR) {
				// Check touch state
				isRecording = inputReader.isTouched;

				StartCRlogging();
			}
		}

        private void Reset() {
			isRunning = false;
			isRecording = false;
			if (LoggingRoutine != null) StopCoroutine(LoggingRoutine); else LoggingRoutine = null;
		}
		// ======================================================================================================================================================
		#region LOGGING
		/// <summary>
		/// Logs one entry in the list.
		/// </summary>
		public void SubmitSingleLogEntry() {
			if (RatingType != Constants.RatingType.SR)
				return;

			LogEntry();
			LoggingEnd();
		}

		/// <summary>
		/// Starts the CR logging routine
		/// </summary>
		public void StartCRlogging() {
			isRunning = true;

			if (LoggingRoutine != null) StopCoroutine(LoggingRoutine); else LoggingRoutine = null;
			LoggingRoutine = StartCoroutine(Logging());
		}

        /// <summary>Logs entries based on frequency</summary>
        private IEnumerator Logging() {
			Stopwatch timer = Stopwatch.StartNew();

			while (isRunning) {

				LogEntry();
				yield return new WaitForSecondsRealtime(1/SamplingFrequency);

				if (!AutoSaveToDisk)
					continue;

				if (timer.Elapsed.TotalSeconds > SaveToDiskInterval) {
					SavingIntervalReached();
					timer = Stopwatch.StartNew();
				}
			}

			timer.Stop();
			LoggingEnd();
		}

		private void LogEntry() {

			inputValuesLog.Add(new string[]
			{   Time.time.ToString(),
				(isRecording ? currentValues.x.ToString() : "NA"),
				(isRecording ? currentValues.y.ToString() : "NA")
			});
		}

		private void LoggingEnd() {
			SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Stops the logging routine
		/// </summary>
		public void StopLogging() {
			isRunning = false;

			if (RatingType == Constants.RatingType.SR)
				LoggingEnd();
		}

        private void SavingIntervalReached() { 
			//UnityEngine.Debug.Log("Auto saved logfile to disk");

			string writeString = "";

			foreach (var entry in inputValuesLog) {
				writeString += entry[0].ToString() + "," + entry[1].ToString() + "," + entry[2].ToString() + "\n";
			}

			FileManager.WriteString(Filename, writeString, true);
		}

		#endregion
		// -------------------------------------------------------------------------------------------------------------------------------
		#region OUT
		/// <summary>
		/// Gets logging data in memory
		/// </summary>
		/// <returns></returns>
		public List<string[]> GetRatingData() {
			return inputValuesLog;
		}

		/// <summary>
		/// Gets logging data and empties list in memory
		/// </summary>
		/// <returns></returns>
		public List<string[]> GetAndClearRatingData() {
			List<string[]> tmp = new List<string[]>();
			tmp.AddRange(inputValuesLog);
			inputValuesLog.Clear();

			return tmp;
		}

		#endregion
		// -------------------------------------------------------------------------------------------------------------------------------
		#region IN

		/// <summary>Updates local values</summary>
		public void UpdateValues(Vector2 newValues) {
			currentValues = newValues;
		}

		/// <summary>Joystick touched or not</summary>
		public void UpdateTouchState(bool isTouched) {

			if (isTouched) { // touched and is allowed
				isRecording = true; // 

			}
			else {
				isRecording = false;
			}
		}

		#endregion
		// -------------------------------------------------------------------------------------------------------------------------------
		#region DEBUG

		public void OverruleRecordingSetting(bool onOff) {
			isRecording = onOff;
		}

		#endregion 
	}
}