// 
// ProjectBuilder.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using System.Globalization;
using Microsoft.Build.Execution;

namespace MonoDevelop.Projects.MSBuild
{
	partial class BuildEngine
	{
		static CultureInfo uiCulture;
		readonly Dictionary<string, string> unsavedProjects = new Dictionary<string, string> ();
		readonly ProjectCollection engine = new ProjectCollection { DefaultToolsVersion = MSBuildConsts.Version };
		MSBuildLoggerAdapter loggerAdapter;

		IEngineLogWriter sessionLogWriter;

		public void SetCulture (CultureInfo uiCulture)
		{
			BuildEngine.uiCulture = uiCulture;
		}

		public void SetGlobalProperties (IDictionary<string, string> properties)
		{
			foreach (var p in properties)
				engine.SetGlobalProperty (p.Key, p.Value);
		}

		public ProjectBuilder LoadProject (string file)
		{
			return new ProjectBuilder (this, engine, file);
		}
		
		public void UnloadProject (ProjectBuilder pb)
		{
			pb.Dispose ();
		}

		internal void SetUnsavedProjectContent (string file, string content)
		{
			lock (unsavedProjects)
				unsavedProjects[file] = content;
		}

		internal string GetUnsavedProjectContent (string file)
		{
			lock (unsavedProjects) {
				string content;
				unsavedProjects.TryGetValue (file, out content);
				return content;
			}
		}

		internal void UnloadProject (string file)
		{
			lock (unsavedProjects)
				unsavedProjects.Remove (file);

			RunSTA (delegate
			{
				// Unloading the project file is not enough because the project
				// may be referencing other target files which may have
				// changed and which are cached.
				engine.UnloadAllProjects();
			});
		}

		public bool BuildOperationStarted { get; set; }

		void BeginBuildOperation (IEngineLogWriter logWriter, MSBuildVerbosity verbosity)
		{
			// Start a new MSBuild build session, sending log to the provided writter
			BuildOperationStarted = true;
			BuildParameters parameters = new BuildParameters (engine);
			sessionLogWriter = logWriter;
			loggerAdapter = new MSBuildLoggerAdapter (logWriter, verbosity);
			parameters.Loggers = loggerAdapter.Loggers;
			BuildManager.DefaultBuildManager.BeginBuild (parameters);
		}

		void EndBuildOperation ()
		{
			// End the MSBuild build session started in BeginBuildOperation

			BuildOperationStarted = false;
			BuildManager.DefaultBuildManager.EndBuild ();

			// Dispose the loggers. This will flush pending output.
			loggerAdapter.Dispose ();
			loggerAdapter = null;
			sessionLogWriter = null;
		}

		public MSBuildLoggerAdapter StartProjectSessionBuild (IEngineLogWriter logWriter)
		{
			// Sets the client logger to which to send build output.
			// In the client, each project has its own logger,
			// but in the builder there is a single logger for the
			// whole builder session. To send log to the correct
			// client logger, the logger will be changed every time
			// a new project build starts

			loggerAdapter.BuildResult.Clear ();
			loggerAdapter.EngineLogWriter = logWriter;
			return loggerAdapter;
		}

		public void EndProjectSessionBuild ()
		{
			loggerAdapter.EngineLogWriter = sessionLogWriter;
		}
	}
}