//
// ThreadPoolImportSource.cs
//
// Author:
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 Scott Peterson
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Threading;

using Hyena;
using Mono.Unix;

using Banshee.ServiceStack;
using Banshee.Sources;

namespace Banshee.Library
{
    public abstract class ThreadPoolImportSource : IImportSource
    {
        private bool importing;
        private UserJob user_job;
        private readonly object user_job_mutex = new object ();
        
        private void CreateUserJob ()
        {
            lock (user_job_mutex) {
                if(user_job != null) {
                    return;
                }
                
                user_job = new UserJob (UserJobTitle, UserJobTitle, Catalog.GetString ("Importing Songs"));
                user_job.IconNames = IconNames;
                user_job.CancelMessage = CancelMessage;
                user_job.CanCancel = CanCancel;
                user_job.Register ();
            }
        }
        
        private void DestroyUserJob ()
        {
            lock(user_job_mutex) {
                if(user_job == null) {
                    return;
                }
                
                user_job.Finish ();
                user_job = null;
            }
        }
        
        protected void UpdateUserJob (int processed, int count, string artist, string title)
        {
            user_job.Title = String.Format(
                Catalog.GetString("Importing {0} of {1}"),
                processed, count);
            user_job.Status = String.Format("{0} - {1}", artist, title);
            user_job.Progress = processed / (double)count;
        }
        
        protected void LogError (string path, Exception e)
        {
            LogError (path, e.Message);
        }

        protected void LogError (string path, string msg)
        {
            ErrorSource error_source = ServiceManager.SourceManager.Library.ErrorSource;
            error_source.AddMessage (Path.GetFileName (path), msg);
            
            Log.Error (path, msg, false);
        }
        
        protected bool CheckForCanceled ()
        {
            lock(user_job_mutex) {
                return user_job != null && user_job.IsCancelRequested;
            }
        }
        
        protected virtual string UserJobTitle {
            get { return String.Format (Catalog.GetString ("Importing Songs from {0}"), Name); }
        }
        
        protected virtual string CancelMessage {
            get { return Catalog.GetString ("The import process is currently running. Would you like to stop it?"); }
        }
    
        protected virtual bool CanCancel {
            get { return true; }
        }
        
#region IImportSource
        
    	public abstract string Name { get; }

    	public abstract string[] IconNames { get; }

    	public virtual bool CanImport {
    	    get { return true; }
    	}

        public void Import ()
        {
            if (importing) {
                return;
            }
            
            importing = true;
            CreateUserJob ();
            ThreadPool.QueueUserWorkItem (DoImport);
            DestroyUserJob ();
            importing = false;
        }
        
#endregion
        
        private void DoImport (object o)
        {
            DoImport ();
        }
        
        protected abstract void DoImport ();
        
    }
}
