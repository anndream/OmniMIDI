/*
Keppy's Synthesizer blacklist system
*/

BOOL BlackListSystem(){
	// Blacklist system init
	TCHAR defaultstring[MAX_PATH];
	TCHAR userstring[MAX_PATH];
	TCHAR defaultblacklistdirectory[MAX_PATH];
	TCHAR userblacklistdirectory[MAX_PATH];
	TCHAR modulename[MAX_PATH];
	// VirtualMIDISynth 1.x ban init
	TCHAR vmidisynthpath[MAX_PATH];
	SHGetFolderPath(NULL, CSIDL_SYSTEM, NULL, 0, vmidisynthpath);
	PathAppend(vmidisynthpath, _T("\\VirtualMIDISynth\\VirtualMIDISynth.dll"));
	GetModuleFileName(NULL, modulename, MAX_PATH);
	PathStripPath(modulename);
	try {
		if (GetWindowsDirectory(defaultblacklistdirectory, MAX_PATH)) {
			_tcscat(defaultblacklistdirectory, L"\\keppymididrv.defaultblacklist");
			std::wifstream file(defaultblacklistdirectory);
			if (file) {
				// The default blacklist exists, continue
				OutputDebugString(defaultblacklistdirectory);
				while (file.getline(defaultstring, sizeof(defaultstring) / sizeof(*defaultstring)))
				{
					if (_tcsicmp(modulename, defaultstring) == 0) {
						return 0x0;
					}
				}
			}
			else {
				MessageBox(NULL, L"The default blacklist is missing, or the driver is not installed properly!\nFatal error, can not continue!\n\nPress OK to quit.", L"Keppy's Synthesizer - FATAL ERROR", MB_OK | MB_ICONERROR | MB_SYSTEMMODAL);
				exit(0);
			}
		}
		if (SUCCEEDED(SHGetFolderPath(NULL, CSIDL_PROFILE, NULL, 0, userblacklistdirectory))) {
			PathAppend(userblacklistdirectory, _T("\\Keppy's Synthesizer\\blacklist\\keppymididrv.blacklist"));
			std::wifstream file(userblacklistdirectory);
			OutputDebugString(userblacklistdirectory);
			while (file.getline(userstring, sizeof(userstring) / sizeof(*userstring)))
			{
				if (_tcsicmp(modulename, userstring) == 0) {
					std::wstring modulenamelpcwstr(modulename);
					std::wstring concatted_stdstr = L"Keppy's Synthesizer - " + modulenamelpcwstr + L" is blacklisted";
					LPCWSTR messageboxtitle = concatted_stdstr.c_str();
					MessageBox(NULL, L"This program has been manually blacklisted.\n\nThe driver will be automatically unloaded by WinMM.", messageboxtitle, MB_OK | MB_ICONEXCLAMATION | MB_SYSTEMMODAL);
					return 0x0;
				}
			}
		}
		return 0x1;
	}
	catch (std::exception & e) {
		OutputDebugStringA(e.what());
		exit;
	}
}

BOOL VMSBlackList(){
	// First, the VMS blacklist system, then the main one
	TCHAR modulename[MAX_PATH];
	TCHAR sndvol[MAX_PATH];
	TCHAR vmidisynthdll[MAX_PATH];
	TCHAR vmidisynth2exe[MAX_PATH];
	BOOL result = BlackListSystem();
	SHGetFolderPath(NULL, CSIDL_SYSTEM, NULL, 0, vmidisynthdll);
	SHGetFolderPath(NULL, CSIDL_SYSTEMX86, NULL, 0, vmidisynth2exe);
	PathAppend(vmidisynthdll, _T("\\VirtualMIDISynth\\VirtualMIDISynth.dll"));
	PathAppend(vmidisynth2exe, _T("\\VirtualMIDISynth\\VirtualMIDISynth.exe"));
	GetModuleFileName(NULL, modulename, MAX_PATH);
	PathStripPath(modulename);
	// Lel stuff
	_tcscpy_s(sndvol, _countof(sndvol), _T("sndvol.exe"));
	try {
		if (PathFileExists(vmidisynthdll)) {
			if (PathFileExists(vmidisynth2exe)) {
				return result;
			}
			else {
				if (!_tcsicmp(modulename, sndvol)) {
					return 0x0;
				}
				else {
					if (MessageBox(0, L"Please uninstall VirtualMIDISynth 1.x before using this driver.\n\nPress No if you want to use Keppy's Synthesizer anyway, or Yes to unload it from the application.\n\n(VirtualMIDISynth's outdated DLLs could cause performance degradation while using Keppy's Synthesizer)", L"Keppy's Synthesizer", MB_YESNO | MB_ICONWARNING | MB_SYSTEMMODAL) == IDYES)
					{
						return 0x0;
					}
					else {
						return result;
					}
				}
			}
		}
		else {
			return result;
		}
		return 0x0;
	}
	catch (std::exception & e) {
		OutputDebugStringA(e.what());
		exit;
	}
}

BOOL BannedSystemProcess() {
	// These processes are PERMANENTLY banned because of some internal bugs inside them.
	TCHAR bannedbattlenet[MAX_PATH];
	TCHAR bannedconsent[MAX_PATH];
	TCHAR bannedcsrss[MAX_PATH];
	TCHAR bannedexplorer[MAX_PATH];
	TCHAR bannedscratch[MAX_PATH];
	TCHAR bannedshare[MAX_PATH];
	TCHAR bannedshellinfrastructure[MAX_PATH];
	TCHAR bannedsndvol[MAX_PATH];
	TCHAR bannedvmware[MAX_PATH];

	TCHAR modulename[MAX_PATH];

	_tcscpy_s(bannedbattlenet, _countof(bannedbattlenet), _T("Battle.net Launcher.exe"));
	_tcscpy_s(bannedconsent, _countof(bannedconsent), _T("consent.exe"));
	_tcscpy_s(bannedcsrss, _countof(bannedcsrss), _T("csrss.exe"));
	_tcscpy_s(bannedexplorer, _countof(bannedexplorer), _T("explorer.exe"));
	_tcscpy_s(bannedscratch, _countof(bannedscratch), _T("scratch.exe"));
	_tcscpy_s(bannedshare, _countof(bannedshare), _T("NVIDIA Share.exe"));
	_tcscpy_s(bannedshellinfrastructure, _countof(bannedshellinfrastructure), _T("ShellExperienceHost.exe"));
	_tcscpy_s(bannedsndvol, _countof(bannedsndvol), _T("SndVol.exe"));
	_tcscpy_s(bannedvmware, _countof(bannedvmware), _T("vmware-hostd.exe"));

	GetModuleFileName(NULL, modulename, MAX_PATH);
	PathStripPath(modulename);
	if (!_tcsicmp(modulename, bannedbattlenet) |
		!_tcsicmp(modulename, bannedconsent) |
		!_tcsicmp(modulename, bannedcsrss) |
		!_tcsicmp(modulename, bannedexplorer) |
		!_tcsicmp(modulename, bannedscratch) |
		!_tcsicmp(modulename, bannedshare) |
		!_tcsicmp(modulename, bannedshellinfrastructure) |
		!_tcsicmp(modulename, bannedsndvol) |
		!_tcsicmp(modulename, bannedvmware)) {
		return TRUE;
		// It's a blacklisted process, so it can NOT create a BASS audio stream.
	}
	else {
		return FALSE;
		// It's not a blacklisted process, so it can create a BASS audio stream.
	}
}