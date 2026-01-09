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
using System.Net.Http;
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
		private readonly ModConfig modConfig;

		public KeyboardInterceptor(ILogger logger, INativeMethods nativeMethods, KeyboardSettings settings)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.settings = settings;
			this.modConfig = new ModConfig(new HttpClient());
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
			
			// block |= key == Key.Apps && false;
			// block |= key == Key.Escape && modifier == KeyModifier.None && false;
			// block |= key == Key.F1 && false;
			// block |= key == Key.F2 && false;
			// block |= key == Key.F3 && false;
			// block |= key == Key.F4 && false;
			// block |= key == Key.F5 && false;
			// block |= key == Key.F6 && false;
			// block |= key == Key.F7 && false;
			// block |= key == Key.F8 && false;
			// block |= key == Key.F9 && false;
			// block |= key == Key.F10 && false;
			// block |= key == Key.F11 && false;
			// block |= key == Key.F12 && false;
			// block |= key == Key.LWin && false;
			// block |= key == Key.PrintScreen && false;
			// block |= key == Key.RWin && false;
			// block |= key == Key.End && false;	
			// block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Escape && false;
			// block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.F4 && false;
			// block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Space && false;
			// block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Tab && false;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.Escape && false;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.C && true;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.V && true;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.X && false;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.Tab && false;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.Escape && false;
			// block |= modifier.HasFlag(KeyModifier.Ctrl) && modifier.HasFlag(KeyModifier.Alt) && key == Key.Delete && false;

			var modifiers = Regex.Split(modifier.ToString(), ", ");
			// MessageBox.Show(block.ToString());
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
					var currentUserName = Environment.UserName;
					
					//MessageBox.Show($"Current user: {currentUserName.ToLower()}");
					//MessageBox.Show(modConfig.GetConfig());
					//MessageBox.Show(modConfig.GetConfig().Contains(currentUserName.ToLower()).ToString());
					if (modConfig.GetConfig().Contains(currentUserName.ToLower()))
					{
						//MessageBox.Show("Starting Chrome");
						var processInfo = new ProcessStartInfo(browser + ".exe");
						processInfo.UseShellExecute = true;
						Process.Start(processInfo);
					}
				}
				else
				{
					//MessageBox.Show("Activation Chrome Window");
					foreach (var process in processes)
					{
						nativeMethods.ActivateWindow(process.MainWindowHandle);
					}
				}
			}

			return false;
		}

		private void Log(Key key, int keyCode, KeyModifier modifier, KeyState state)
		{
			var modifierFlags = Enum.GetValues(typeof(KeyModifier)).OfType<KeyModifier>().Where(m => m != KeyModifier.None && modifier.HasFlag(m));
			var modifiers = modifierFlags.Any() ? String.Join(" + ", modifierFlags) + " + " : string.Empty;

			logger.Info($"Blocked '{modifiers}{key}' ({key} = {keyCode}) when {state.ToString().ToLower()}.");
		}
	}
}
