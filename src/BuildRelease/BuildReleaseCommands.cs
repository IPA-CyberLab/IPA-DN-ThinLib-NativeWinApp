// Thin Telework System Source Code
// 
// License: The Apache License, Version 2.0
// https://www.apache.org/licenses/LICENSE-2.0
// 
// Copyright (c) IPA CyberLab of Industrial Cyber Security Center.
// Copyright (c) NTT-East Impossible Telecom Mission Group.
// Copyright (c) Daiyuu Nobori.
// Copyright (c) SoftEther VPN Project, University of Tsukuba, Japan.
// Copyright (c) SoftEther Corporation.
// Copyright (c) all contributors on IPA-DN-ThinLib Library and SoftEther VPN Project in GitHub.
// 
// All Rights Reserved.
// 
// DISCLAIMER
// ==========
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// THIS SOFTWARE IS DEVELOPED IN JAPAN, AND DISTRIBUTED FROM JAPAN, UNDER
// JAPANESE LAWS. YOU MUST AGREE IN ADVANCE TO USE, COPY, MODIFY, MERGE, PUBLISH,
// DISTRIBUTE, SUBLICENSE, AND/OR SELL COPIES OF THIS SOFTWARE, THAT ANY
// JURIDICAL DISPUTES WHICH ARE CONCERNED TO THIS SOFTWARE OR ITS CONTENTS,
// AGAINST US (IPA, NTT-EAST, SOFTETHER PROJECT, SOFTETHER CORPORATION, DAIYUU NOBORI
// OR OTHER SUPPLIERS), OR ANY JURIDICAL DISPUTES AGAINST US WHICH ARE CAUSED BY ANY
// KIND OF USING, COPYING, MODIFYING, MERGING, PUBLISHING, DISTRIBUTING, SUBLICENSING,
// AND/OR SELLING COPIES OF THIS SOFTWARE SHALL BE REGARDED AS BE CONSTRUED AND
// CONTROLLED BY JAPANESE LAWS, AND YOU MUST FURTHER CONSENT TO EXCLUSIVE
// JURISDICTION AND VENUE IN THE COURTS SITTING IN TOKYO, JAPAN. YOU MUST WAIVE
// ALL DEFENSES OF LACK OF PERSONAL JURISDICTION AND FORUM NON CONVENIENS.
// PROCESS MAY BE SERVED ON EITHER PARTY IN THE MANNER AUTHORIZED BY APPLICABLE
// LAW OR COURT RULE.
// 
// USE ONLY IN JAPAN. DO NOT USE THIS SOFTWARE IN ANOTHER COUNTRY UNLESS YOU HAVE
// A CONFIRMATION THAT THIS SOFTWARE DOES NOT VIOLATE ANY CRIMINAL LAWS OR CIVIL
// RIGHTS IN THAT PARTICULAR COUNTRY. USING THIS SOFTWARE IN OTHER COUNTRIES IS
// COMPLETELY AT YOUR OWN RISK. IPA AND NTT-EAST HAS DEVELOPED AND
// DISTRIBUTED THIS SOFTWARE TO COMPLY ONLY WITH THE JAPANESE LAWS AND EXISTING
// CIVIL RIGHTS INCLUDING PATENTS WHICH ARE SUBJECTS APPLY IN JAPAN. OTHER
// COUNTRIES' LAWS OR CIVIL RIGHTS ARE NONE OF OUR CONCERNS NOR RESPONSIBILITIES.
// WE HAVE NEVER INVESTIGATED ANY CRIMINAL REGULATIONS, CIVIL LAWS OR
// INTELLECTUAL PROPERTY RIGHTS INCLUDING PATENTS IN ANY OF OTHER 200+ COUNTRIES
// AND TERRITORIES. BY NATURE, THERE ARE 200+ REGIONS IN THE WORLD, WITH
// DIFFERENT LAWS. IT IS IMPOSSIBLE TO VERIFY EVERY COUNTRIES' LAWS, REGULATIONS
// AND CIVIL RIGHTS TO MAKE THE SOFTWARE COMPLY WITH ALL COUNTRIES' LAWS BY THE
// PROJECT. EVEN IF YOU WILL BE SUED BY A PRIVATE ENTITY OR BE DAMAGED BY A
// PUBLIC SERVANT IN YOUR COUNTRY, THE DEVELOPERS OF THIS SOFTWARE WILL NEVER BE
// LIABLE TO RECOVER OR COMPENSATE SUCH DAMAGES, CRIMINAL OR CIVIL
// RESPONSIBILITIES. NOTE THAT THIS LINE IS NOT LICENSE RESTRICTION BUT JUST A
// STATEMENT FOR WARNING AND DISCLAIMER.
// 
// READ AND UNDERSTAND THE 'WARNING.TXT' FILE BEFORE USING THIS SOFTWARE.
// SOME SOFTWARE PROGRAMS FROM THIRD PARTIES ARE INCLUDED ON THIS SOFTWARE WITH
// LICENSE CONDITIONS WHICH ARE DESCRIBED ON THE 'THIRD_PARTY.TXT' FILE.
// 
// ---------------------
// 
// If you find a bug or a security vulnerability please kindly inform us
// about the problem immediately so that we can fix the security problem
// to protect a lot of users around the world as soon as possible.
// 
// Our e-mail address for security reports is:
// daiyuu.securityreport [at] dnobori.jp
// 
// Thank you for your cooperation.


using System;
using System.Threading;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CoreUtil;

namespace BuildRelease
{
	public static class BuildReleaseCommands
	{
		// Perform all
		[ConsoleCommandMethod(
			"Builds all sources and releases all packages.",
			"All [yes|no] [/NORMALIZESRC:yes|no] [/IGNOREERROR:yes|no] [/DEBUG:yes|no] [/SERIAL:yes|no]",
			"Builds all sources and releases all packages.",
			"[yes|no]:Specify 'yes' if you'd like to increment the build number.",
			"NORMALIZESRC:Specity 'yes' if you'd like to normalize the build infomations in the source codes and resource scripts.",
			"IGNOREERROR:Specify yes if you'd like to ignore the child process to show the error message.",
			"SERIAL:Specify yes not to use parallel mode.",
			"DEBUG:Specity yes to enable debug mode. (UNIX only)"
			)]
		static int All(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[yes|no]", ConsoleService.Prompt, "Increments build number (y/n) ? ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("IGNOREERROR"),
				new	ConsoleParam("DEBUG"),
				new ConsoleParam("SERIAL"),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			DateTime start = Time.NowDateTime;

			Win32BuildRelease.ExecCommand(Env.ExeFileName, string.Format("/CMD:BuildWin32 {0} /NORMALIZESRC:{1}",
				vl["[yes|no]"].BoolValue ? "yes" : "no",
				"yes"));

			Win32BuildRelease.ExecCommand(Env.ExeFileName, string.Format("/CMD:ReleaseWin32 all /IGNOREERROR:{0} /SERIAL:{1}",
				vl["IGNOREERROR"].BoolValue ? "yes" : "no",
				"yes"));

			Win32BuildRelease.ExecCommand(Env.ExeFileName, string.Format("/CMD:CopyRelease"));

			DateTime end = Time.NowDateTime;

			Con.WriteLine("Taken time: {0}.", (end - start));

			return 0;
		}

		// Copy the released files
		[ConsoleCommandMethod(
			"Copies all release files.",
			"CopyRelease",
			"Copies all release files."
			)]
		static int CopyRelease(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			int build, version;
			string name;
			DateTime date;

			Win32BuildRelease.ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);

			string baseName = string.Format("v{0}-{1}-{2}-{3:D4}.{4:D2}.{5:D2}",
									BuildHelper.VersionIntToString(version),
									build,
									name,
									date.Year, date.Month, date.Day);

			string destDirName = Paths.ReleaseDestDir;

			string publicDir = Path.Combine(destDirName, "Public");

			string filesReleaseDir = Path.Combine(publicDir, baseName);

			string autorunReleaseSrcDir = Path.Combine(publicDir, "autorun");

			IO.CopyDir(Paths.ReleaseDir, filesReleaseDir, null, false, true);

			string src_bindir = Path.Combine(Paths.SolutionBaseDirName, "bin");
			string vpnsmgr_zip_filename_relative = "";
			vpnsmgr_zip_filename_relative += 

			string.Format(Vars.DistibutionPackagePrefix + "Thin_Telework_ZIP_Client_Only-v{0}.{1:D2}-{2}-{3}-{4:D4}.{5:D2}.{6:D2}.zip",
				version / 100, version % 100, build, name,
				date.Year, date.Month, date.Day);

			string exeonly_zip_filename_full = Path.Combine(filesReleaseDir, vpnsmgr_zip_filename_relative);

			ZipPacker zip = new ZipPacker();
			zip.AddFileSimple(Vars.APP_ID_PREFIX + "ThinClient.exe", DateTime.Now, FileAttributes.Normal,
				IO.ReadFile(Path.Combine(src_bindir, Vars.APP_ID_PREFIX + "ThinClient.exe")), true);
			zip.AddFileSimple("EntryPoint.dat", DateTime.Now, FileAttributes.Normal,
				IO.ReadFile(Path.Combine(src_bindir, "EntryPoint.dat")), true);
			zip.AddFileSimple("hamcore.se2", DateTime.Now, FileAttributes.Normal,
				IO.ReadFile(Path.Combine(src_bindir, @"BuiltHamcoreFiles\hamcore_win32\hamcore.se2")), true);
			zip.AddFileSimple("ReadMeFirst_License.txt", DateTime.Now, FileAttributes.Normal,
				IO.ReadFile(Path.Combine(src_bindir, @"hamcore\eula.txt")), true);
			zip.AddFileSimple("ReadMeFirst_Important_Notices_ja.txt", DateTime.Now, FileAttributes.Normal,
				IO.ReadFile(Path.Combine(src_bindir, @"hamcore\warning_ja.txt")), true);
			zip.Finish();
			byte[] zip_data = zip.GeneratedData.Read();
			IO.MakeDirIfNotExists(Path.GetDirectoryName(exeonly_zip_filename_full));
			IO.SaveFile(exeonly_zip_filename_full, zip_data);

			Con.WriteLine();
			Con.WriteLine("'{0}' に出力されました。", destDirName);

			return 0;
		}

		// UNIX release
		[ConsoleCommandMethod(
			"Builds UNIX installer package files.",
			"ReleaseUnix [id] [/IGNOREERROR:yes|no] [/DEBUG:yes|no] [/SERIAL:yes|no]",
			"Builds Unix installer package files.",
			"[id]:Specify target package ID which you'd like to build. If you'd like to erase and rebuild all packages, specify 'all'. Specify 'clean' to delete all release files.",
			"IGNOREERROR:Specify yes if you'd like to ignore the child process to show the error message.",
			"SERIAL:Specify yes not to use parallel mode.",
			"DEBUG:Specity yes to enable debug mode."
			)]
		static int ReleaseUnix(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[id]"),
				new ConsoleParam("IGNOREERROR"),
				new	ConsoleParam("DEBUG"),
				new ConsoleParam("SERIAL"),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			int version, build;
			string name;
			DateTime date;

			Win32BuildRelease.ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);

			BuildSoftware[] softs = BuildSoftwareList.List;
			bool serial = vl["SERIAL"].BoolValue;

			if (Str.IsEmptyStr(vl.DefaultParam.StrValue))
			{
				Con.WriteLine("IDs:");
				foreach (BuildSoftware soft in softs)
				{
					if (soft.Os.IsWindows == false)
					{
						soft.SetBuildNumberVersionName(build, version, name, date);
						Con.WriteLine("  {0}", soft.IDString);
						Con.WriteLine("    - \"{0}\"", soft.OutputFileName);
					}
				}
			}
			else
			{
				string key = vl.DefaultParam.StrValue;
				bool all = false;

				if ("all".StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					all = true;
				}

				if ("clean".StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					// Delete the release directory
					Paths.DeleteAllReleaseTarGz();
					Con.WriteLine("Clean completed.");
					return 0;
				}

				List<BuildSoftware> o = new List<BuildSoftware>();

				foreach (BuildSoftware soft in softs)
				{
					soft.SetBuildNumberVersionName(build, version, name, date);

					if (soft.Os.IsWindows == false)
					{
						if (all || soft.IDString.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							o.Add(soft);
						}
					}
				}

				if (o.Count == 0)
				{
					throw new ApplicationException(string.Format("Software ID '{0}' not found.", key));
				}
				else
				{
					if (all)
					{
						// Delete the release directory
						Paths.DeleteAllReleaseTarGz();
					}
					else
					{
						IO.MakeDir(Paths.ReleaseDir);
					}

					if (serial)
					{
						// Build in series
						int i;
						for (i = 0; i < o.Count; i++)
						{
							Con.WriteLine("{0} / {1}: Executing for '{2}'...",
								i + 1, o.Count, o[i].IDString);

							BuildHelper.BuildMain(o[i], vl["DEBUG"].BoolValue);
						}
					}
					else if (o.Count == 1)
					{
						// To build
						BuildHelper.BuildMain(o[0], vl["DEBUG"].BoolValue);
					}
					else
					{
						// Make a child process build
						Process[] procs = new Process[o.Count];

						int i;

						for (i = 0; i < o.Count; i++)
						{
							Con.WriteLine("{0} / {1}: Executing for '{2}'...",
								i + 1, o.Count, o[i].IDString);

							procs[i] = Kernel.Run(Env.ExeFileName,
								string.Format("/PAUSEIFERROR:{1} /DT:{2} /CMD:ReleaseUnix /DEBUG:{3} {0}",
								o[i].IDString, vl["IGNOREERROR"].BoolValue ? "no" : "yes", Str.DateTimeToStrShort(BuildSoftwareList.ListCreatedDateTime), vl["DEBUG"].BoolValue ? "yes" : "no")
								);
						}

						Con.WriteLine("Waiting child processes...");

						int numError = 0;

						for (i = 0; i < o.Count; i++)
						{
							procs[i].WaitForExit();

							bool ok = procs[i].ExitCode == 0;

							if (ok == false)
							{
								numError++;
							}

							Con.WriteLine("{0} / {1} ({2}):", i + 1, o.Count, o[i].IDString);
							Con.WriteLine("       {0}", ok ? "Success" : "* Error *");
						}

						Con.WriteLine();
						if (numError != 0)
						{
							throw new ApplicationException(string.Format("{0} Errors.", numError));
						}
						Con.WriteLine("No Errors.");
					}
				}
			}

			return 0;
		}

		// Win32 Release
		[ConsoleCommandMethod(
			"Builds Win32 installer package files.",
			"ReleaseWin32 [id] [/IGNOREERROR:yes|no] [/SERIAL:yes|no]",
			"Builds Win32 installer package files.",
			"[id]:Specify target package ID which you'd like to build. If you'd like to erase and rebuild all packages, specify 'all'. Specify 'clean' to delete all release files.",
			"SERIAL:Specify yes not to use parallel mode.",
			"IGNOREERROR:Specify yes if you'd like to ignore the child process to show the error message."
			)]
		static int ReleaseWin32(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[id]"),
				new ConsoleParam("IGNOREERROR"),
				new ConsoleParam("SERIAL"),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			bool serial = vl["SERIAL"].BoolValue;
			int version, build;
			string name;
			DateTime date;

			Win32BuildRelease.ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);
			BuildSoftware[] softs = BuildSoftwareList.List;

			if (Str.IsEmptyStr(vl.DefaultParam.StrValue))
			{
				Con.WriteLine("IDs:");
				foreach (BuildSoftware soft in softs)
				{
					if (soft.Os.IsWindows)
					{
						soft.SetBuildNumberVersionName(build, version, name, date);
						Con.WriteLine("  {0}", soft.IDString);
						Con.WriteLine("    - \"{0}\"", soft.OutputFileName);
					}
				}
			}
			else
			{
				string key = vl.DefaultParam.StrValue;
				bool all = false;

				if ("all".StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					all = true;
				}

				if ("clean".StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					// Delete the release directory
					Paths.DeleteAllReleaseExe();
					Con.WriteLine("Clean completed.");
					return 0;
				}

				List<BuildSoftware> o = new List<BuildSoftware>();

				foreach (BuildSoftware soft in softs)
				{
					soft.SetBuildNumberVersionName(build, version, name, date);

					if (soft.Os.IsWindows)
					{
						if (all || soft.IDString.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							o.Add(soft);
						}
					}
				}

				if (o.Count == 0)
				{
					throw new ApplicationException(string.Format("Software ID '{0}' not found.", key));
				}
				else
				{
					if (all)
					{
						// Delete the release directory
						Paths.DeleteAllReleaseExe();
					}

					IO.MakeDir(Paths.ReleaseDir);

					if (serial)
					{
						// Build in series
						int i;
						for (i = 0; i < o.Count; i++)
						{
							Con.WriteLine("{0} / {1}: Executing for '{2}'...",
								i + 1, o.Count, o[i].IDString);

							BuildHelper.BuildMain(o[i], false);
						}
					}
					else if (o.Count == 1)
					{
						// To build
						BuildHelper.BuildMain(o[0], false);
					}
					else
					{
						// Make a child process build
						Process[] procs = new Process[o.Count];

						int i;

						for (i = 0; i < o.Count; i++)
						{
							Con.WriteLine("{0} / {1}: Executing for '{2}'...",
								i + 1, o.Count, o[i].IDString);

							procs[i] = Kernel.Run(Env.ExeFileName,
								string.Format("/PAUSEIFERROR:{1} /CMD:ReleaseWin32 {0}",
								o[i].IDString, vl["IGNOREERROR"].BoolValue ? "no" : "yes"));
						}

						Con.WriteLine("Waiting child processes...");

						int numError = 0;

						for (i = 0; i < o.Count; i++)
						{
							procs[i].WaitForExit();

							bool ok = procs[i].ExitCode == 0;

							if (ok == false)
							{
								numError++;
							}

							Con.WriteLine("{0} / {1} ({2}):", i + 1, o.Count, o[i].IDString);
							Con.WriteLine("       {0}", ok ? "Success" : "* Error *");
						}

						Con.WriteLine();
						if (numError != 0)
						{
							throw new ApplicationException(string.Format("{0} Errors.", numError));
						}
						Con.WriteLine("No Errors.");
					}
				}
			}

			return 0;
		}

		// Copy the Unix source
		[ConsoleCommandMethod(
			"Copies source codes for Unix.",
			"CopyUnixSrc [destdir]",
			"Copies source codes for Unix.",
			"[destdir]:Specify the destination directory."
			)]
		static int CopyUnixSrc(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[destdir]", ConsoleService.Prompt, "Destination directory : ", ConsoleService.EvalNotEmpty, null),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			//((BuildSoftwareUnix)BuildSoftwareList.vpnbridge_linux_x86_ja).CopyUnixSrc(vl.DefaultParam.StrValue);

			return 0;
		}

		// Win32 build
		[ConsoleCommandMethod(
			"Builds all executable files for win32 and HamCore for all OS.",
			"BuildWin32 [yes|no] [/NORMALIZESRC:yes|no]",
			"Builds all executable files for win32 and HamCore for all OS.",
			"[yes|no]:Specify 'yes' if you'd like to increment the build number.",
			"NORMALIZESRC:Specity 'yes' if you'd like to normalize the build infomations in the source codes and resource scripts."
			)]
		static int BuildWin32(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[yes|no]", ConsoleService.Prompt, "Increments build number (y/n) ? ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("NORMALIZESRC", ConsoleService.Prompt, "Normalizes source codes (y/n) ? ", ConsoleService.EvalNotEmpty, null)
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			if (vl.DefaultParam.BoolValue)
			{
				Win32BuildRelease.IncrementBuildNumber();
			}
			if (vl.DefaultParam.BoolValue || vl["NORMALIZESRC"].BoolValue)
			{
				Win32BuildRelease.NormalizeBuildInfo();
			}

			Paths.DeleteAllReleaseTarGz();
			Paths.DeleteAllReleaseExe();

			Win32BuildRelease.BuildMain();
			Win32BuildRelease.SignAllBinaryFiles();
			HamCoreBuildRelease.BuildHamcore();
			//Win32BuildRelease.CopyDebugSnapshot();

			return 0;
		}

		// Process of post-build
		[ConsoleCommandMethod(
			"Process necessary tasks after building.",
			"PostBuild",
			"Process necessary tasks after building."
			)]
		static int PostBuild(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			Win32BuildRelease.SignAllBinaryFiles();
			HamCoreBuildRelease.BuildHamcore();

			return 0;
		}

		// Increment the build number
		[ConsoleCommandMethod(
			"Increments the build number.",
			"IncrementBuildNumber",
			"Increments the build number written in 'CurrentBuild.txt' text file."
			)]
		static int IncrementBuildNumber(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			Win32BuildRelease.IncrementBuildNumber();

			return 0;
		}


		// Test processing
		[ConsoleCommandMethod(
			"Run Test Procedure.",
			"Test",
			"Run Test Procedure."
			)]
		static int Test(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			TestClass.Test();

			return 0;
		}

		// Build a HamCore
		[ConsoleCommandMethod(
			"Builds a HamCore file.",
			"BuildHamCore",
			"Builds a HamCore file."
			)]
		static int BuildHamCore(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			HamCoreBuildRelease.BuildHamcore();

			return 0;
		}

		// Sign a binary file
		[ConsoleCommandMethod(
			"Sign all binary files.",
			"SignAll",
			"Sign all binary files."
			)]
		static int SignAll(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			Win32BuildRelease.SignAllBinaryFiles();

			return 0;
		}

		// Set the version of the PE to 4
		[ConsoleCommandMethod(
			"Set the version of the PE file to 4.",
			"SetPE4 [filename]",
			"Set the version of the PE file to 4.",
			"[filename]:Specify the target filename."
			)]
		static int SetPE4(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[filename]", ConsoleService.Prompt, "Filename: ", ConsoleService.EvalNotEmpty, null)
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			PEUtil.SetPEVersionTo4(vl.DefaultParam.StrValue);

			return 0;
		}

		// Set the Manifest
		[ConsoleCommandMethod(
			"Set the manifest to the executable file.",
			"SetManifest [filename] [/MANIFEST:manifest_file_name]",
			"Set the manifest to the executable file.",
			"[filename]:Specify the target executable filename.",
			"MANIFEST:Specify the manifest XML file."
			)]
		static int SetManifest(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[filename]", ConsoleService.Prompt, "Target Filename: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("MANIFEST", ConsoleService.Prompt, "Manifest Filename: ", ConsoleService.EvalNotEmpty, null),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			PEUtil.SetManifest(vl.DefaultParam.StrValue, vl["MANIFEST"].StrValue);

			return 0;
		}

		// Generate a version information resource
		[ConsoleCommandMethod(
			"Generate a Version Information Resource File.",
			"GenerateVersionResource [targetFileName] [/OUT:destFileName]",
			"Generate a Version Information Resource File.",
			"[targetFileName]:Specify the target exe/dll file name.",
			"OUT:Specify the output .res file.",
			"RC:Specify a template RC file name.")]
		static int GenerateVersionResource(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[targetFileName]", ConsoleService.Prompt, "Target Filename: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("OUT", ConsoleService.Prompt, "Dst Filename: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("PRODUCT"),
				new ConsoleParam("RC"),
				new ConsoleParam("POSTFIX"),
				new ConsoleParam("COPYRIGHT"),
				new ConsoleParam("COMPANYNAME"),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			string targetFilename = vl.DefaultParam.StrValue;
			string outFilename = vl["OUT"].StrValue;
			string product_name = vl["PRODUCT"].StrValue;
			string postfix = vl["POSTFIX"].StrValue;
			string copyright = vl["COPYRIGHT"].StrValue;
			string companyname = vl["COMPANYNAME"].StrValue;

			Win32BuildRelease.GenerateVersionInfoResource(targetFilename, outFilename, vl["RC"].StrValue, product_name, postfix,
				Paths.GetThinLibSubmoduleCommitId(), copyright, companyname, Paths.GetThinLibVersionLabel());

			return 0;
		}

		// Measure the number of lines of code
		[ConsoleCommandMethod(
			"Count the number of lines of the sources.",
			"Count [DIR]",
			"Count the number of lines of the sources.",
			"[DIR]:dir name.")]
		static int Count(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[DIR]", null, null, null, null),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			string dir = vl.DefaultParam.StrValue;
			if (Str.IsEmptyStr(dir))
			{
				dir = Paths.SolutionBaseDirName;
			}

			string[] files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

			int numLines = 0;
			int numBytes = 0;
			int numComments = 0;
			int totalLetters = 0;

			Dictionary<string, int> commentsDict = new Dictionary<string, int>();

			foreach (string file in files)
			{
				string ext = Path.GetExtension(file);

				if (Str.StrCmpi(ext, ".c") || Str.StrCmpi(ext, ".cpp") || Str.StrCmpi(ext, ".h") ||
                    Str.StrCmpi(ext, ".rc") || Str.StrCmpi(ext, ".stb") || Str.StrCmpi(ext, ".cs")
                     || Str.StrCmpi(ext, ".fx") || Str.StrCmpi(ext, ".hlsl"))
				{
					if (Str.InStr(file, "\\.svn\\") == false && Str.InStr(file, "\\seedll\\") == false && Str.InStr(file, "\\see\\") == false && Str.InStr(file, "\\openssl\\") == false)
					{
						string[] lines = File.ReadAllLines(file);

						numLines += lines.Length;
						numBytes += (int)new FileInfo(file).Length;

						foreach (string line in lines)
						{
							if (Str.InStr(line, "//") && Str.InStr(line, "// Validate arguments") == false)
							{
								if (commentsDict.ContainsKey(line) == false)
								{
									commentsDict.Add(line, 1);
								}
								numComments++;

								totalLetters += line.Trim().Length - 3;
							}
						}
					}
				}
			}

			Con.WriteLine("{0} Lines,  {1} Bytes.  {2} Comments ({3} distinct, aver: {4})", Str.ToStr3(numLines), Str.ToStr3(numBytes),
				Str.ToStr3(numComments), commentsDict.Count, totalLetters / numComments);

			return 0;
		}
		

		// Copy the file
		[ConsoleCommandMethod(
			"Copy a File.",
			"FileCopy [src] [/DEST:dest]",
			"Copy a File.",
			"[src]:Specify the source file.",
			"DEST:Specify the destination file.")]
		static int FileCopy(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[src]", ConsoleService.Prompt, "Src Filename: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("DEST", ConsoleService.Prompt, "Dst Filename: ", ConsoleService.EvalNotEmpty, null),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			string destFileName = vl["DEST"].StrValue;
			string srcFileName = vl.DefaultParam.StrValue;

			IO.FileCopy(srcFileName, destFileName, true, false);

			return 0;
		}

		// Sign the file
		[ConsoleCommandMethod(
			"Sign files using Authenticode certificates.",
			"SignCode [filename] [/DEST:destfilename] [/COMMENT:comment] [/KERNEL:yes|no]",
			"Sign files using Authenticode certificates.",
			"[filename]:Specify the target filename.",
			"DEST:Specify the destination filename. If this parameter is not specified, the target file will be overwritten.",
			"COMMENT:Provide a description of the signed content.",
			"KERNEL:Specify \"yes\" if Windows Vista / 7 Kernel Mode Driver Signing is needed."
			)]
		static int SignCode(ConsoleService c, string cmdName, string str)
		{
			ConsoleParam[] args =
			{
				new ConsoleParam("[filename]", ConsoleService.Prompt, "Filename: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("DEST"),
				new ConsoleParam("COMMENT", ConsoleService.Prompt, "Comment: ", ConsoleService.EvalNotEmpty, null),
				new ConsoleParam("KERNEL"),
				new ConsoleParam("CERTID"),
				new ConsoleParam("SHAMODE"),
			};
			ConsoleParamValueList vl = c.ParseCommandList(cmdName, str, args);

			string destFileName = vl["DEST"].StrValue;
			string srcFileName = vl.DefaultParam.StrValue;
			if (Str.IsEmptyStr(destFileName))
			{
				destFileName = srcFileName;
			}
			string comment = vl["COMMENT"].StrValue;
			bool kernel = vl["KERNEL"].BoolValue;

			int certid = vl["CERTID"].IntValue;
			int shamode = vl["SHAMODE"].IntValue;

			CodeSign.SignFile(destFileName, srcFileName, comment, kernel, false);

			return 0;
		}
	}
}

