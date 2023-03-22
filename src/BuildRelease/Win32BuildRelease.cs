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
	// Build utility for Win32
	public static class Win32BuildRelease
	{
        // Generate a version information resource
        public static void GenerateVersionInfoResource(string targetExeName, string outName, string rc_name, string product_name, string postfix, string commitId, string copyright, string companyName, string verLabel)
		{
            int build, version;
            string name;
            DateTime date;

            if (Str.IsEmptyStr(commitId))
            {
                commitId = "unknown";
            }

            ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);

            if (Str.IsEmptyStr(rc_name))
            {
                rc_name = "VersionInfo.rc";
            }

            string templateFileName = Path.Combine(Paths.SolutionBaseDirName, @"Vars\" + rc_name);
            string body = Str.ReadTextFile(templateFileName);

            string exeFileName = Path.GetFileName(targetExeName);

            exeFileName += " (ThinLib: " + verLabel + ", " + commitId + ")";

            if (Str.IsEmptyStr(product_name))
            {
                product_name = Vars.ProductName;
            }

            string internalName = product_name;

            if (Str.IsEmptyStr(postfix) == false)
            {
                internalName += " " + postfix;
            }

            if (Str.IsEmptyStr(copyright)) copyright = Vars.Copyright;
            if (Str.IsEmptyStr(companyName)) companyName = Vars.CompanyName;

            body = Str.ReplaceStr(body, "$PRODUCTNAME$", product_name);
            body = Str.ReplaceStr(body, "$COPYRIGHT$", copyright);
            body = Str.ReplaceStr(body, "$COMPANYNAME$", companyName);
            body = Str.ReplaceStr(body, "$INTERNALNAME$", internalName);
            body = Str.ReplaceStr(body, "$YEAR$", date.Year.ToString());
            body = Str.ReplaceStr(body, "$FILENAME$", exeFileName);
            body = Str.ReplaceStr(body, "$VER_MAJOR$", (version / 100).ToString());
            body = Str.ReplaceStr(body, "$VER_MINOR$", (version % 100).ToString());
            body = Str.ReplaceStr(body, "$VER_BUILD$", build.ToString());

            IO f = IO.CreateTempFileByExt(".rc");
            string filename = f.Name;

            f.Write(Str.AsciiEncoding.GetBytes(body));

            f.Close();

            ExecCommand(Paths.RcFilename, "/nologo \"" + filename + "\"");

            string rcDir = Path.GetDirectoryName(filename);
            string rcFilename = Path.GetFileName(filename);
            string rcFilename2 = Path.GetFileNameWithoutExtension(rcFilename);

            string resFilename = Path.Combine(rcDir, rcFilename2) + ".res";

            IO.MakeDirIfNotExists(Path.GetDirectoryName(outName));

            IO.FileCopy(resFilename, outName, true, false);
        }

        // Flush to disk
        public static void Flush()
		{
			string txt = IO.CreateTempFileNameByExt(".txt");
			byte[] ret = Secure.Rand(64);

			FileStream f = File.Create(txt);

			f.Write(ret, 0, ret.Length);

			f.Flush();

			f.Close();

			File.Delete(txt);
		}

        static string UpdateBuildName(string name)
        {
            string verLabel = Paths.GetThinLibVersionLabel();
            int i = name.IndexOf(".");
            if (i == -1) return verLabel;

            return name.Substring(0, i) + "." + verLabel;
        }

        // Increment the build number
        public static void IncrementBuildNumber()
		{
			int build, version;
			string name;
			DateTime date;

			ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);
			build++;

            name = UpdateBuildName(name);

            WriteBuildInfoToTextFile(null, build, version, name, date);

			SetNowDate();

			Con.WriteLine("New build number: {0}", build);
		}

		// Set the date and time
		public static void SetNowDate()
		{
			int build, version;
			string name;
			DateTime date;

            ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);

            name = UpdateBuildName(name);

            date = DateTime.Now;

			WriteBuildInfoToTextFile(null, build, version, name, date);
		}

		// Write the build number and the version number in the text file
		public static void WriteBuildInfoToTextFile(string filename, int build, int version, string name, DateTime date)
		{
            if (string.IsNullOrEmpty(filename))
            {
                filename = Path.Combine(Paths.SolutionBaseDirName, "CurrentBuild.txt");
            }

            using (StreamWriter w = new StreamWriter(filename))
			{
				w.WriteLine("BUILD_NUMBER {0}", build);
				w.WriteLine("VERSION {0}", version);
				w.WriteLine("BUILD_NAME {0}", name);
				w.WriteLine("BUILD_DATE {0}", Str.DateTimeToStrShort(date));

				w.Flush();
				w.Close();
			}
		}

		// Read the build number and the version number from a text file
		public static void ReadBuildInfoFromTextFile(string filename, out int build, out int version, out string name, out DateTime date)
		{
			if (string.IsNullOrEmpty(filename))
			{
				filename = Path.Combine(Paths.SolutionBaseDirName, "CurrentBuild.txt");
			}

			char[] seps = { '\t', ' ', };
			name = "";
			date = new DateTime(0);

			using (StreamReader r = new StreamReader(filename))
			{
				build = version = 0;

				while (true)
				{
					string line = r.ReadLine();
					if (line == null)
					{
						break;
					}

					string[] tokens = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length == 2)
					{
						if (tokens[0].Equals("BUILD_NUMBER", StringComparison.InvariantCultureIgnoreCase))
						{
							build = int.Parse(tokens[1]);
						}

						if (tokens[0].Equals("VERSION", StringComparison.InvariantCultureIgnoreCase))
						{
							version = int.Parse(tokens[1]);
						}

						if (tokens[0].Equals("BUILD_NAME", StringComparison.InvariantCultureIgnoreCase))
						{
							name = tokens[1];

							name = Str.ReplaceStr(name, "-", "_");
						}

						if (tokens[0].Equals("BUILD_DATE", StringComparison.InvariantCultureIgnoreCase))
						{
							date = Str.StrToDateTime(tokens[1]);
						}
                    }
                }

				r.Close();

				if (build == 0 || version == 0 || Str.IsEmptyStr(name) || date.Ticks == 0)
				{
					throw new ApplicationException(string.Format("Wrong file data: '{0}'", filename));
				}
			}
		}

		// Normalize the build information
		public static void NormalizeBuildInfo()
		{
			SetNowDate();

			int build, version;
			string name;
			DateTime date;

			ReadBuildInfoFromTextFile(null, out build, out version, out name, out date);

			string username = Env.UserName;
			string pcname = Env.MachineName;

			NormalizeSourceCode(build, version, username, pcname, date, Paths.GetThinLibSubmoduleCommitId(), Paths.GetThinLibVersionLabel());
		}

		// Apply build number, version number, user name, and PC name to the source code
		public static void NormalizeSourceCode(int buildNumber, int version, string userName, string pcName, DateTime date, string commitId, string verLabel)
		{
			DateTime now = date;
			char[] seps = { '\t', ' ', };

			int i = pcName.IndexOf(".");
			if (i != -1)
			{
				pcName = pcName.Substring(0, i);
			}

			userName = userName.ToLower();
			pcName = pcName.ToLower();

			string[] files = Util.CombineArray<string>(
				Directory.GetFiles(Paths.SolutionBaseDirName, "*.h", SearchOption.AllDirectories));

			foreach (string file in files)
			{
				string dir = Path.GetDirectoryName(file);
				if (Str.InStr(dir, @"\.svn\") == false &&
					Str.InStr(dir, @"\submodules\") == false &&
					Str.InStr(IO.GetRelativeFileName(file, Paths.SolutionBaseDirName), @"tmp\") == false)
				{
					byte[] srcData = File.ReadAllBytes(file);

					int bomSize;
					Encoding enc = Str.GetEncoding(srcData, out bomSize);
					if (enc == null)
					{
						enc = Str.Utf8Encoding;
					}
					StringReader r = new StringReader(enc.GetString(Util.ExtractByteArray(srcData, bomSize, srcData.Length - bomSize)));
					StringWriter w = new StringWriter();
					bool somethingChanged = false;

					while (true)
					{
						string line = r.ReadLine();
						if (line == null)
						{
							break;
						}
						string newLine = null;

						string[] tokens = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

						if (tokens.Length >= 1)
						{
							if (file.EndsWith(".h", StringComparison.InvariantCultureIgnoreCase))
							{
								if (tokens.Length == 3)
								{
									// Build number portion of the source code
									if (tokens[0].Equals("//") && tokens[1].Equals("Build") && Str.IsNumber(tokens[2]))
									{
										newLine = line.Replace(tokens[2], buildNumber.ToString());
									}
								}
							}

							if (file.EndsWith(".h", StringComparison.InvariantCultureIgnoreCase))
							{
								if (tokens.Length == 3)
								{
									// String part of the version information of Cedar.h
									if (tokens[0].Equals("#define") && tokens[1].Equals("CEDAR_BUILD"))
									{
										newLine = line.Replace(tokens[2], buildNumber.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("CEDAR_VER"))
									{
										newLine = line.Replace(tokens[2], version.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILDER_NAME"))
									{
										newLine = line.Replace(tokens[2], "\"" + userName + "\"");
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_PLACE"))
									{
										newLine = line.Replace(tokens[2], "\"" + pcName + "\"");
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_Y"))
									{
										newLine = line.Replace(tokens[2], date.Year.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_M"))
									{
										newLine = line.Replace(tokens[2], date.Month.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_D"))
									{
										newLine = line.Replace(tokens[2], date.Day.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_HO"))
									{
										newLine = line.Replace(tokens[2], date.Hour.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_MI"))
									{
										newLine = line.Replace(tokens[2], date.Minute.ToString());
									}

									if (tokens[0].Equals("#define") && tokens[1].Equals("BUILD_DATE_SE"))
									{
										newLine = line.Replace(tokens[2], date.Second.ToString());
									}

                                    if (tokens[0].Equals("#define") && tokens[1].Equals("THINLIB_COMMIT_ID"))
                                    {
                                        newLine = line.Replace(tokens[2], "\"" + commitId + "\"");
                                    }

                                    if (tokens[0].Equals("#define") && tokens[1].Equals("THINLIB_VER_LABEL"))
                                    {
                                        newLine = line.Replace(tokens[2], "\"" + verLabel + "\"");
                                    }
                                }

                                if (tokens.Length >= 3)
								{
									if (tokens[0].Equals("#define") && tokens[1].Equals("SUPPORTED_WINDOWS_LIST"))
									{
										newLine = "#define\tSUPPORTED_WINDOWS_LIST\t\t\"" + OSList.Windows.OSSimpleList + "\"";
									}
								}
							}
						}

						if (newLine == null || newLine == line)
						{
							w.WriteLine(line);
						}
						else
						{
							w.WriteLine(newLine);

							somethingChanged = true;
						}
					}

					if (somethingChanged)
					{
						byte[] retData = Str.ConvertEncoding(Str.Utf8Encoding.GetBytes(w.ToString()), enc, bomSize != 0);

						File.WriteAllBytes(file, retData);

						Con.WriteLine("Modified: '{0}'.", file);
					}
				}
			}
		}

		//// Get the DebugSnapshot directory name
		//public static string GetDebugSnapstotDirName()
		//{
		//	return Path.Combine(Paths.DebugSnapshotBaseDir, Str.DateTimeToStrShort(BuildSoftwareList.ListCreatedDateTime));
		//}

		//// Copy DebugSnapshot
		//public static void CopyDebugSnapshot()
		//{
		//	string snapDir = GetDebugSnapstotDirName();

		//	CopyDebugSnapshot(snapDir);
		//}
		//public static void CopyDebugSnapshot(string snapDir, params string[] exclude_exts)
		//{
		//	IO.CopyDir(Path.Combine(Paths.SolutionBaseDirName, @"..\"), snapDir,
		//		delegate(FileInfo fi)
		//		{
		//			string srcPath = fi.FullName;
		//			string[] exts_default =
		//			{
		//				".ncb", ".aps", ".suo", ".old", ".scc", ".vssscc", ".vspscc", ".cache", ".psess", ".tmp", ".dmp",
		//			};

		//			List<string> exts = new List<string>();

		//			foreach (string ext in exts_default)
		//			{
		//				exts.Add(ext);
		//			}

		//			foreach (string ext in exclude_exts)
		//			{
		//				exts.Add(ext);
		//			}

  //                  if (Str.InStr(srcPath, @"\.vs\", false))
  //                  {
  //                      return false;
  //                  }

  //                  if (Str.InStr(srcPath, @"\.git\", false))
  //                  {
  //                      return false;
  //                  }

  //                  if (Str.InStr(srcPath, @"\.svn\", false))
		//			{
		//				return false;
		//			}

		//			if (Str.InStr(srcPath.Substring(3), @"\tmp\", false))
		//			{
		//				return false;
		//			}

		//			if (Str.InStr(srcPath, @"_log\", false))
		//			{
		//				return false;
		//			}

		//			if (Str.InStr(srcPath, @"\backup.vpn_", false))
		//			{
		//				return false;
		//			}

		//			if (Str.InStr(srcPath, @"\node_modules\", false))
		//			{
		//				return false;
		//			}

		//			if (Str.InStr(srcPath, @"\wwwroot\", false))
		//			{
		//				return true;
		//			}

		//			foreach (string ext in exts)
		//			{
		//				if (srcPath.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
		//				{
		//					return false;
		//				}
		//			}

		//			if (Str.InStr(srcPath, @"\hamcore\", false))
		//			{
		//				return true;
		//			}

		//			if (Str.InStr(srcPath, @"\hamcore_", false))
		//			{
		//				return true;
		//			}

		//			return true;
		//		},
		//		false, true, false, false);
		//}

		// Execute building in Visual Studio
		public static void BuildMain()
		{
			Mutex x = new Mutex(false, "VpnBuilderWin32_BuildMain");

			x.WaitOne();

			try
			{
				// Generate the contents of the batch file
				string batFileName = Path.Combine(Paths.TmpDirName, "vc_build.cmd");
				StreamWriter bat = new StreamWriter(batFileName, false, Str.ShiftJisEncoding);
				bat.WriteLine("call \"{0}\"", Paths.GetVsDevCmdFilePath());
				bat.WriteLine("echo on");
				bat.WriteLine("\"{0}\" /verbosity:detailed /target:Clean /property:Configuration=Release /property:Platform=x86 \"{1}\"",
					"msbuild.exe", Paths.VisualStudioSolutionFileName);
				bat.WriteLine("IF ERRORLEVEL 1 GOTO LABEL_ERROR");

				bat.WriteLine("\"{0}\" /verbosity:detailed /target:Clean /property:Configuration=Release /property:Platform=x64 \"{1}\"",
					"msbuild.exe", Paths.VisualStudioSolutionFileName);
				bat.WriteLine("IF ERRORLEVEL 1 GOTO LABEL_ERROR");

				bat.WriteLine("\"{0}\" /verbosity:detailed /target:Rebuild /maxcpucount:{2} /property:Configuration=Release /property:Platform=x86 \"{1}\"",
					"msbuild.exe", Paths.VisualStudioSolutionFileName, Paths.MsBuildMaxCpu);
				bat.WriteLine("IF ERRORLEVEL 1 GOTO LABEL_ERROR");

                bat.WriteLine("\"{0}\" /verbosity:detailed /target:Rebuild /maxcpucount:{2} /property:Configuration=Release /property:Platform=x64 \"{1}\"",
                    "msbuild.exe", Paths.VisualStudioSolutionFileName, Paths.MsBuildMaxCpu);
                bat.WriteLine("IF ERRORLEVEL 1 GOTO LABEL_ERROR");

                bat.WriteLine(":LABEL_ERROR");

				bat.WriteLine("EXIT %ERRORLEVEL%");

				bat.Close();

				ExecCommand(Paths.CmdFileName, string.Format("/C \"{0}\"", batFileName));

				//BuildReplaceHeader();
			}
			finally
			{
				x.ReleaseMutex();
			}
		}

		// Command execution
        public static void ExecCommand(string exe, string arg, bool shell_execute = false, bool no_stdout = false)
		{
            string outputStr = "";

			Process p = new Process();
			p.StartInfo.FileName = exe;
			p.StartInfo.Arguments = arg;
			p.StartInfo.UseShellExecute = shell_execute;

            if (no_stdout)
            {
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
            }

			if (shell_execute)
			{
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			}

			Con.WriteLine("Executing '{0} {1}'...", exe, arg);

			p.Start();

			p.WaitForExit();

            if (no_stdout)
            {
                string s1 = p.StandardOutput.ReadToEnd();
                string s2 = p.StandardError.ReadToEnd();
                outputStr = "---\r\n" + s1 + "\r\n" + s2 + "\r\n---\r\n";
            }

			int ret = p.ExitCode;
			if (ret != 0)
			{
                throw new ApplicationException(string.Format("Child process '{0}' returned error code {1}.\r\n{2}", exe, ret, outputStr));
			}

			Kernel.SleepThread(50);
		}

		// Get whether the specified fileis a target of signature
		public static bool IsFileSignable(string fileName)
		{
			if (fileName.IndexOf(@".svn", StringComparison.InvariantCultureIgnoreCase) != -1 ||
				fileName.StartsWith(".svn", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
            if (fileName.IndexOf(@".vs", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                fileName.StartsWith(".vs", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (fileName.IndexOf(@".git", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                fileName.StartsWith(".git", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (fileName.EndsWith("vpn16.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("BuildRelease.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
            if (fileName.EndsWith("BuildTool.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (fileName.EndsWith("BuildReleaseTmp.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
            if (fileName.EndsWith("BuildToolTmp.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (fileName.EndsWith("CoreUtil.dll", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("npptools.dll", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("winpcap_installer.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("winpcap_installer_win9x.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("VpnGatePlugin_x64.dll", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("cabarc.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (fileName.EndsWith("VpnGatePlugin_x86.dll", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (Str.InStr(fileName, "DriverPackages", false))
			{
				return false;
			}
			if (Str.InStr(fileName, "_nosign", false))
			{
				return false;
			}

			if (fileName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
				fileName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
				fileName.EndsWith(".ocx", StringComparison.InvariantCultureIgnoreCase) ||
				fileName.EndsWith(".sys", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		// Sign for all binary files (series mode)
		public static void SignAllBinaryFilesSerial()
		{
			string[] files = Directory.GetFiles(Paths.SolutionBinDirName, "*", SearchOption.AllDirectories);

			foreach (string file in files)
			{
				if (IsFileSignable(file))
				{
					bool isDriver = file.EndsWith(".sys", StringComparison.InvariantCultureIgnoreCase);

					// Check whether this file is signed
					bool isSigned = ExeSignChecker.CheckFileDigitalSignature(file);
					if (isSigned && isDriver)
					{
						isSigned = ExeSignChecker.IsKernelModeSignedFile(file);
					}

					Con.WriteLine("The file '{0}': {1}.", file, isSigned ? "Already signed" : "Not yet signed");

					if (isSigned == false)
					{
						Con.WriteLine("Signing...");

						CodeSign.SignFile(file, file, BuildConfig.SignComment, isDriver, false);
					}
				}
			}
		}

		// Sign for all binary files (parallel mode)
		public static void SignAllBinaryFiles()
		{
			//// 2020/4/18 なんか一時的に遅いので Serial に
			//SignAllBinaryFilesSerial();
			//return;

			string[] files = Directory.GetFiles(Paths.SolutionBinDirName, "*", SearchOption.AllDirectories);

			List<string> filename_list = new List<string>();

			foreach (string file in files)
			{
				if (IsFileSignable(file))
				{
					bool isDriver = file.EndsWith(".sys", StringComparison.InvariantCultureIgnoreCase);

					// Check whether this file is signed
					bool isSigned = ExeSignChecker.CheckFileDigitalSignature(file);
					if (isSigned && isDriver)
					{
						isSigned = ExeSignChecker.IsKernelModeSignedFile(file);
					}

					Con.WriteLine("The file '{0}': {1}.", file, isSigned ? "Already signed" : "Not yet signed");

					if (isSigned == false)
					{
						filename_list.Add(file);
					}
				}
			}

			Con.WriteLine("Start ProcessWorkQueue for Signing...\n");
			ThreadObj.ProcessWorkQueue(sign_thread, 40, filename_list.ToArray());
			Con.WriteLine("ProcessWorkQueue for Signing completed.\n");
		}

		// Binary file signature thread
		static void sign_thread(object param)
		{
			string filename = (string)param;
			bool isDriver = filename.EndsWith(".sys", StringComparison.InvariantCultureIgnoreCase);

			Con.WriteLine("Signing...");

			CodeSign.SignFile(filename, filename, BuildConfig.SignComment, isDriver, false);
		}
	}
}
