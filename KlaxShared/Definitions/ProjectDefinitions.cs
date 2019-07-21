using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Attributes;

namespace KlaxShared.Definitions
{
	public static class ProjectDefinitions
	{
		[CVar("ProjectPath")]
		//private static string ProjectPath { get; set; } = @"D:/Documents/KlaxEngine/ProjectFiles/";
		private static string ProjectPath { get; set; } = "ProjectFiles/";

		[CVar]
		private static int IsProjectRelativeToExecutable { get; set; } = 1;

		public static string GetProjectPath()
		{
			if (IsProjectRelativeToExecutable < 1)
			{
				if (!ProjectPath.EndsWith("/"))
				{
					return ProjectPath + "/";
				}

				return ProjectPath;
			}
			else
			{
				string processPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
				string combinedPath = Path.Combine(processPath, ProjectPath);
				combinedPath = combinedPath.Replace("\\", "/");
				if (!combinedPath.EndsWith("/"))
				{
					combinedPath += "/";
				}

				return combinedPath;
			}
		}

		/// <summary>
		/// Returns an absolute path from a path relative to the project root
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static string GetAbsolutePath(string relativePath)
		{
			return Path.Combine(GetProjectPath(), relativePath);
		}

		/// <summary>
		/// Returns a path relative to the project folder
		/// </summary>
		/// <param name="absolutePath"></param>
		/// <returns></returns>
		public static string GetRelativePath(string absolutePath)
		{
			Uri projectUri = new Uri(GetProjectPath());
			Uri absoluteUri = new Uri(absolutePath);
			return projectUri.MakeRelativeUri(absoluteUri).ToString();
		}
	}
}
