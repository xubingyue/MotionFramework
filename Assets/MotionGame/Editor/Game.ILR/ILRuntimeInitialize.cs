using System.IO;
using UnityEditor;
using UnityEngine;
using MotionGame;

public class ILRuntimeInitialize
{
	[InitializeOnLoadMethod]
	static void CopyAssemblyFiles()
	{
		if (Directory.Exists(ILRDefine.StrMyAssemblyFolderPath) == false)
			Directory.CreateDirectory(ILRDefine.StrMyAssemblyFolderPath);

		// Copy DLL
		string dllSource = Path.Combine(ILRDefine.StrScriptAssembliesDir, ILRDefine.StrMyHotfixDLLFileName);
		string dllDest = Path.Combine(ILRDefine.StrMyAssemblyFolderPath, $"{ILRDefine.StrMyHotfixDLLFileName}.bytes");
		File.Copy(dllSource, dllDest, true);
		
		// Copy PDB
		string pdbSource = Path.Combine(ILRDefine.StrScriptAssembliesDir, ILRDefine.StrMyHotfixPDBFileName);
		string pdbDest = Path.Combine(ILRDefine.StrMyAssemblyFolderPath, $"{ILRDefine.StrMyHotfixPDBFileName}.bytes");
		File.Copy(pdbSource, pdbDest, true);

		Debug.Log("Copy hotfix assembly files done.");
		AssetDatabase.Refresh();
	}
}