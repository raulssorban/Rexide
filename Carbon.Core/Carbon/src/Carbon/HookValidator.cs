﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
///

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carbon.Hooks;
using Carbon.Oxide.Metadata;
using Newtonsoft.Json;

namespace Carbon.Core;

public class HookValidator
{
	public static List<string> CarbonHooks { get; private set; } = new List<string>(500);
	public static HookPackage OxideHooks { get; private set; }

	public static void Refresh()
	{
		CarbonHooks.Clear();
		CarbonHooks = Community.Runtime.HookProcessorEx.LoadedStaticHooksName
			.Concat(Community.Runtime.HookProcessorEx.LoadedDynamicHooksName).ToList();
		Logger.Debug($"Refreshed {CarbonHooks.Count} loaded hooks.");

		Community.Runtime.CorePlugin.webrequest.Enqueue("https://raw.githubusercontent.com/OxideMod/Oxide.Rust/develop/resources/Rust.opj", null, (error, data) =>
		{
			OxideHooks = JsonConvert.DeserializeObject<HookPackage>(data);
			Logger.Debug($"Refreshed {OxideHooksCount} oxide hooks.");
		}, null);
	}

	public static bool IsIncompatibleOxideHook(string hook)
	{
		if (CarbonHooks.Contains(hook)) return false;

		if (OxideHooks != null)
		{
			foreach (var manifest in OxideHooks.Manifests)
			{
				foreach (var entry in manifest.Hooks)
				{
					var hookName = entry.Hook.HookName.Split(' ')[0];
					if (hookName.Contains("/") || hookName != hook) continue;
					return true;
				}
			}
		}

		return false;
	}

	private static int OxideHooksCount
	{
		get
		{
			if (OxideHooks == null) return 0;

			int count = 0;
			foreach (var manifest in OxideHooks.Manifests)
			foreach (var entry in manifest.Hooks)
				count++;
			return count;
		}
	}
}
