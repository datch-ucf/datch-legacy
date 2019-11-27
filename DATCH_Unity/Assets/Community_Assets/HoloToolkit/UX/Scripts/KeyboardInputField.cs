// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using HoloToolkit.Examples.GazeRuler;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HoloToolkit.UI.Keyboard
{
    /// <summary>
    /// Class that when placed on an input field will enable keyboard on click
    /// </summary>
    public class KeyboardInputField : InputField
    {
        /// <summary>
        /// Internal field for overriding keyboard spawn point
        /// </summary>
        [Header("Keyboard Settings")]
        public Transform KeyboardSpawnPoint;
        public bool reposition = false;

        /// <summary>
        /// Internal field for overriding keyboard spawn point
        /// </summary>
        [HideInInspector]
        public Keyboard.LayoutType KeyboardLayout = Keyboard.LayoutType.Alpha;

        private const float KeyBoardPositionOffset = 0.045f;

        /// <summary>
        /// Override OnPointerClick to spawn keyboard
        /// </summary>
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            Keyboard.Instance.Close();
            Keyboard.Instance.PresentKeyboard(text, KeyboardLayout);

            if (reposition) // When inside another canvas, the keyboard repositions itself to a far off position
            {
                if (KeyboardSpawnPoint != null)
                {
                    Keyboard.Instance.RepositionKeyboard(KeyboardSpawnPoint, null, KeyBoardPositionOffset);
                }
                else
                {
                    Keyboard.Instance.RepositionKeyboard(transform, null, KeyBoardPositionOffset);
                }
            }

            // Subscribe to keyboard delegates
            Keyboard.Instance.OnTextUpdated += Keyboard_OnTextUpdated;

            Keyboard.Instance.OnClosed += Keyboard_OnClosed;

            //JON
            // Sent when the 'Enter' button is pressed
            Keyboard.Instance.OnTextSubmitted += StorageManager.Instance.Prompt_OnEnter;

            print("In on pointer click");
        }        

        /// <summary>
        /// Delegate function for getting keyboard input
        /// </summary>
        /// <param name="newText"></param>
        private void Keyboard_OnTextUpdated(string newText)
        {
            text = newText;
        }

        /// <summary>
        /// Delegate function for getting keyboard input
        /// </summary>
        /// <param name="sender"></param>
        private void Keyboard_OnClosed(object sender, EventArgs e)
        {
            // Unsubscribe from delegate functions
            Keyboard.Instance.OnTextUpdated -= Keyboard_OnTextUpdated;
            Keyboard.Instance.OnClosed -= Keyboard_OnClosed;

            //JON
            Keyboard.Instance.OnTextSubmitted -= StorageManager.Instance.Prompt_OnEnter;
            //print("On closed keyboard");
            //MeasureManager.Instance.SetDrawing(true,null);
            // whenever we close the keyboard, we also want to turn this gameobject off
            //this.gameObject.SetActive(false);
        }

        protected override void OnDisable()
        {
            if (Keyboard.Instance != null && Keyboard.Instance.gameObject.activeSelf)
            {
                //Keyboard.Instance.gameObject.SetActive(false);
                StorageManager.Instance.SetSaved();
                //MeasureManager.Instance.SetDrawing(true, null);
                
            }
        }
        protected override void OnEnable()
        {
            StartCoroutine(WaitForKeyboard());
        }

        IEnumerator WaitForKeyboard()
        {
            yield return new WaitUntil(() => Keyboard.Instance != null);

            Keyboard.Instance.Close();
            Keyboard.Instance.PresentKeyboard(text, KeyboardLayout);

            if (reposition) // When inside another canvas, the keyboard repositions itself to a far off position
            {
                if (KeyboardSpawnPoint != null)
                {
                    Keyboard.Instance.RepositionKeyboard(KeyboardSpawnPoint, null, KeyBoardPositionOffset);
                }
                else
                {
                    Keyboard.Instance.RepositionKeyboard(transform, null, KeyBoardPositionOffset);
                }
            }

            // Subscribe to keyboard delegates
            Keyboard.Instance.OnTextUpdated += Keyboard_OnTextUpdated;

            Keyboard.Instance.OnClosed += Keyboard_OnClosed;

            //JON
            // Sent when the 'Enter' button is pressed
            Keyboard.Instance.OnTextSubmitted += StorageManager.Instance.Prompt_OnEnter;

            // Should probably change where this gets set
            if(GameObject.Find("SaveSelection"))
                GameObject.Find("SaveSelection").SetActive(false);
        }

    }
}
