// WarSetupPlugin.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "WarSetupPlugin.h"


#define VC_DIRS_SECTION _T("VC\\VC_OBJECTS_PLATFORM_INFO\\Win32\\Directories")


#ifdef _MANAGED
#pragma managed(push, off)
#endif

typedef std::set<std::basic_string<TCHAR> > paths_t;
static paths_t *seen_paths = NULL;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

int LogMessage(MSIHANDLE hInstall, LPCTSTR szMsg, INSTALLMESSAGE msgType = INSTALLMESSAGE_INFO)
{
	PMSIHANDLE hRecord = MsiCreateRecord(1);
	MsiRecordSetString(hRecord, 0, szMsg);
	int rval = MsiProcessMessage(hInstall, INSTALLMESSAGE(msgType), hRecord);
	MsiCloseHandle(hRecord);

#ifdef _DEBUG
	//MessageBox(NULL, szMsg, _T("Test"), MB_OK);
#endif

	return rval;
} 


class MyException : public std::exception
{
public:
	MyException(LPCTSTR expl)
		: mNativeMessage(expl)
	{
#ifdef UNICODE
		char buf[1024 * 8];
		memset(buf, 0, sizeof(buf));
		WideCharToMultiByte(CP_ACP, WC_NO_BEST_FIT_CHARS, expl, -1, buf, (int)sizeof(buf) -1, NULL, NULL);
		mMessage = buf;
#else
		mMessage = expl;
#endif
	}

	virtual const char *what( ) const
	{
		return mMessage.c_str();
	}

	LPCTSTR GetMessage() const
	{
		return mNativeMessage.c_str();
	}

private:
	std::string mMessage;
	str_t mNativeMessage;
};

// Support-functions
str_t GetProperty(MSIHANDLE hInstaller, LPTSTR name) throw (MyException)
{
	TCHAR value[1024 * 8];
    DWORD value_size = sizeof(value);
    UINT rc = MsiGetProperty(hInstaller, name, &value[0], &value_size);
    if (rc != ERROR_SUCCESS) 
    {
		std::basic_stringstream<TCHAR> msg;
		msg << "GetProperty(" << name << ") failed.";
		throw MyException(msg.str().c_str());
    }

	return value;
}

// Split a ';' separated list into a string-array;
void Split(MSIHANDLE hInstaller, str_vector_t& dst, LPCTSTR src)
{
	const TCHAR *p, *p_start;

	
	for(p = src; *p;)
	{
		for(; ';' == *p; p++)
			;

		for(p_start = p; *p && (';' != *p); p++)
			;
		
		if (p != p_start)
		{
			str_t segment;
			segment.insert(0, p_start, (p - p_start));
			dst.push_back(segment);
		}
	}
}

// Support to add paths to C:\Documents and Settings\jgaa\Local Settings\Application Data\Microsoft\VisualStudio\7.1\VCComponents.dat
// This is a normal ini-file with semicoloumn-separated paths. Unfortunately, wix can't add parts to ini-files, unless the parts
// are sepatated with comma.


void AddPath(MSIHANDLE hInstaller, LPCTSTR iniFile, LPCTSTR key, LPCTSTR pathToAdd)
{
	str_t path_to_add = pathToAdd;
	TCHAR original_path[1024 * 8];
	memset(original_path, 0, sizeof(original_path));
	DWORD result = GetPrivateProfileString(
		VC_DIRS_SECTION,
		key, 
		_T(""),
		original_path,
		(sizeof(original_path) -1) / sizeof(TCHAR),
		iniFile);

	{
		std::basic_stringstream<TCHAR> msg;
		msg << _T("GetPrivateProfileString(") << iniFile << _T(") returned #") << (int)result
			<< _T(", Content: \"") << original_path << _T("\".");
		LogMessage(hInstaller, msg.str().c_str());
	}

	// Break the string into it's segments
	str_vector_t segments;
	Split(hInstaller, segments, original_path);

	// See if the path already exist
	str_vector_t::const_iterator segment;
	for(segment = segments.begin(); segment != segments.end(); segment++)
	{
		if (*segment == path_to_add)
		{
			std::basic_stringstream<TCHAR> msg;
			msg << _T("The path \"") << segment->c_str() << _T("\" already exist. Skipping.");
			//LogMessage(hInstaller, msg.str().c_str());
			return; 
		}
	}

	// Add the path and write it back
	str_t new_path = original_path;
	if (!segments.empty())
		new_path += _T(";");
	new_path += path_to_add;

	result = WritePrivateProfileString(
		VC_DIRS_SECTION,
		key,
		new_path.c_str(),
		iniFile);
}

void RemovePath(MSIHANDLE hInstaller, LPCTSTR iniFile, LPCTSTR key, LPCTSTR pathToAdd)
{
	str_t path_to_add = pathToAdd;
	TCHAR original_path[1024 * 8];
	memset(original_path, 0, sizeof(original_path));
	DWORD result = GetPrivateProfileString(
		VC_DIRS_SECTION, 
		key, 
		_T(""),
		original_path,
		(sizeof(original_path) -1) / sizeof(TCHAR),
		iniFile);

	// Break the string into it's segments
	str_vector_t segments;
	Split(hInstaller, segments, original_path);

	// See if the path exist
	str_t new_path;
	str_vector_t::const_iterator segment;
	for(segment = segments.begin(); segment != segments.end(); segment++)
	{
		if (*segment != path_to_add)
		{
			if (!new_path.empty())
				new_path += _T(";");
			new_path += *segment;
		}
	}

	// Write back the modified path
	result = WritePrivateProfileString(
		VC_DIRS_SECTION,
		key,
		new_path.c_str(),
		iniFile);
}

UINT __stdcall VCComponentsFileAdd(MSIHANDLE hInstaller)
{
	LogMessage(hInstaller, _T("VCComponentsFileAdd(): Called"));
	
	try
	{
		str_t target_file = GetProperty(hInstaller, _T("VCCOMPONENTSFILEPATH"));
		str_t lib_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILELIBDIR"));
		str_t header_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILEHEADERDIR"));
		str_t exec_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILEEXECDIR"));

		str_t msg;
		msg = _T("target_file is \"");
		msg += target_file;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());
			
		msg = _T("lib_dir is \"");
		msg += lib_dir;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());

		msg = _T("header_dir is \"");
		msg += header_dir;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());

		msg = _T("exec_dir is \"");
		msg += exec_dir;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());

		if (NULL != seen_paths)
		{
			if (seen_paths->find(target_file.c_str()) == seen_paths->end())
			{
				seen_paths->insert(target_file);
				if (::GetFileAttributes(target_file.c_str()) == INVALID_FILE_ATTRIBUTES)
				{
					str_t vc_version = GetProperty(hInstaller, _T("VC_VERSION"));
					std::basic_stringstream<TCHAR> msg;
					msg << _T("The file \"" << target_file << "\" does not exist. ")
						<< _T("I can add libraries to "
						<< vc_version
						<< _T(" if it is installed, ")
						<< _T("and you have entered the Tools/Options menu and selected ")
						<< _T("Projects/C++ Directories. (That's when this file is created).")
						<< _T("This install-package may target several versions of Visual Studio, so ")
						<< _T("this may not be a problem if the components will be installed ")
						<< _T("for another version. You can also add the paths manually after the	")
						<< _T("package is installed.\r\n\r\n ")
						<< _T("Press [YES] to continue the installastion, or [NO] to abort in order to fix ")
						<< _T("this issue.");

					std::basic_stringstream<TCHAR> caption;
					caption << _T("Cannot integrate with ") << vc_version);

					/*	The "right" wayt to do this is to use MsiProcessMessage(), but it will
						only show part of the message du to it's size, so we have to 
						show a normal message-box.
					*/
					if (MessageBox(NULL, msg.str().c_str(), caption.str().c_str(), 
						MB_YESNO | MB_ICONHAND | MB_DEFBUTTON1) 
						== IDNO)
					{
						LogMessage(hInstaller, _T("The user cancelled the installastion"));
						return ERROR_INSTALL_FAILURE;
					}
				}
			}
		}

		if (::GetFileAttributes(target_file.c_str()) == INVALID_FILE_ATTRIBUTES)
			return ERROR_SUCCESS; // The user has OK'ed this.


		if (!target_file.empty())
		{
			if (!lib_dir.empty())
				AddPath(hInstaller, target_file.c_str(), _T("Library Dirs"), lib_dir.c_str());
			if (!header_dir.empty())
				AddPath(hInstaller, target_file.c_str(), _T("Include Dirs"), header_dir.c_str());
			if (!exec_dir.empty())
				AddPath(hInstaller, target_file.c_str(), _T("Path Dirs"), exec_dir.c_str());
		}
	}
	catch(const MyException& ex)
	{
		LogMessage(hInstaller, ex.GetMessage(), INSTALLMESSAGE_ERROR);
        return ERROR_INSTALL_FAILURE;
	}
    
    return ERROR_SUCCESS;
}


UINT __stdcall VCComponentsFileRemove(MSIHANDLE hInstaller)
{
	LogMessage(hInstaller, _T("VCComponentsFileRemove(): Called"));

	try
	{
		str_t target_file = GetProperty(hInstaller, _T("VCCOMPONENTSFILEPATH"));
		str_t lib_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILELIBDIR"));
		str_t header_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILEHEADERDIR"));
		str_t exec_dir = GetProperty(hInstaller, _T("VCCOMPONENTSFILEEXECDIR"));

		str_t msg;
		msg = _T("target_file is \"");
		msg += target_file;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());
			

		msg = _T("lib_dir is \"");
		msg += lib_dir;
		msg += _T("\".");
		LogMessage(hInstaller, msg.c_str());

		if (!target_file.empty())
		{
			if (::GetFileAttributes(target_file.c_str()) != INVALID_FILE_ATTRIBUTES)
			{
				if (!lib_dir.empty())
					RemovePath(hInstaller, target_file.c_str(), _T("Library Dirs"), lib_dir.c_str());
				if (!header_dir.empty())
					RemovePath(hInstaller, target_file.c_str(), _T("Include Dirs"), header_dir.c_str());
				if (!exec_dir.empty())
					RemovePath(hInstaller, target_file.c_str(), _T("Path Dirs"), exec_dir.c_str());
			}
		}
	}
	catch(const MyException& ex)
	{
		LogMessage(hInstaller, ex.GetMessage(), INSTALLMESSAGE_ERROR);
        return ERROR_INSTALL_FAILURE;
	}
    
    return ERROR_SUCCESS;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

