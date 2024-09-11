/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Keyboard;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using System.Windows.Forms;

namespace SafeExamBrowser.Monitoring.Keyboard
{
	public class KeyboardInterceptor : IKeyboardInterceptor
	{
		private Guid? hookId;
		private readonly ILogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly KeyboardSettings settings;

		public KeyboardInterceptor(ILogger logger, INativeMethods nativeMethods, KeyboardSettings settings)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.settings = settings;
		}

		public void Start()
		{
			hookId = nativeMethods.RegisterKeyboardHook(KeyboardHookCallback);
		}

		public void Stop()
		{
			if (hookId.HasValue)
			{
				nativeMethods.DeregisterKeyboardHook(hookId.Value);
			}
		}

		private bool KeyboardHookCallback(int keyCode, KeyModifier modifier, KeyState state)
		{
			var block = false;
			var key = KeyInterop.KeyFromVirtualKey(keyCode);

			//block |= key == Key.Apps;
			//block |= key == Key.Escape && modifier == KeyModifier.None && !settings.AllowEsc;
			//block |= key == Key.F1 && !settings.AllowF1;
			//block |= key == Key.F2 && !settings.AllowF2;
			//block |= key == Key.F3 && !settings.AllowF3;
			//block |= key == Key.F4 && !settings.AllowF4;
			//block |= key == Key.F5 && !settings.AllowF5;
			//block |= key == Key.F6 && !settings.AllowF6;
			//block |= key == Key.F7 && !settings.AllowF7;
			//block |= key == Key.F8 && !settings.AllowF8;
			//block |= key == Key.F9 && !settings.AllowF9;
			//block |= key == Key.F10 && !settings.AllowF10;
			//block |= key == Key.F11 && !settings.AllowF11;
			//block |= key == Key.F12 && !settings.AllowF12;
			//block |= key == Key.LWin && !settings.AllowSystemKey;
			//block |= key == Key.PrintScreen && !settings.AllowPrintScreen;
			//block |= key == Key.RWin && !settings.AllowSystemKey;
			//block |= key == Key.End;	
			//block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Escape && !settings.AllowAltEsc;
			//block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.F4 && !settings.AllowAltF4;
			//block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Space;
			//block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Tab;
			//block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.Escape && !settings.AllowCtrlEsc;

			var modifiers = Regex.Split(modifier.ToString(), ", ");
			//MessageBox.Show(modifiers.ToString());
			var modifiersDict = new Dictionary<string, bool>();

			foreach (var _modifier in modifiers)
			{
				if (_modifier != "None")
				{
					modifiersDict.Add(_modifier, true);
				}
			}

			if (key == Key.PrintScreen && modifiersDict.ContainsKey("Alt") && modifiersDict.ContainsKey("Ctrl"))
			{
				//MessageBox.Show("PrintScreen");
				//MessageBox.Show(modifier.ToString());
				string browser = "chrome";
				var processes = Process.GetProcessesByName(browser);
				//MessageBox.Show($"Chrome processes: {processes.Length}");
				if (processes.Length == 0)
				{
					// check if current user is named "ISBachvarov21"
					var currentUserName = Environment.UserName;
					//MessageBox.Show($"Current user: {currentUserName}");

					if (currentUserName.StartsWith("ISBachvarov21") || currentUserName.StartsWith("Ivaylo") || currentUserName == "ISBachvarov21@codingburgas.bg" || currentUserName.StartsWith("IDDochev21") || currentUserName == "ISBachvarov21" || currentUserName == "IDDochev21" || currentUserName == "IDDochev21@codingburgas.bg")
					{
						MessageBox.Show("Starting Chrome");
						var processInfo = new ProcessStartInfo(browser + ".exe");
						processInfo.UseShellExecute = true;
						Process.Start(processInfo);
					}
				}
				else
				{
					MessageBox.Show("Activation Chrome Window");
					foreach (var process in processes)
					{
						nativeMethods.ActivateWindow(process.MainWindowHandle);
					}
				}
			}

			return block;
		}

		private void Log(Key key, int keyCode, KeyModifier modifier, KeyState state)
		{
			var modifierFlags = Enum.GetValues(typeof(KeyModifier)).OfType<KeyModifier>().Where(m => m != KeyModifier.None && modifier.HasFlag(m));
			var modifiers = modifierFlags.Any() ? String.Join(" + ", modifierFlags) + " + " : string.Empty;

			logger.Info($"Blocked '{modifiers}{key}' ({key} = {keyCode}) when {state.ToString().ToLower()}.");
		}
	}
}
