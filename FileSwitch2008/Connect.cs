using System;
using System.IO;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

namespace FileSwitch2008
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
            //System.Windows.Forms.MessageBox.Show("got onconnec5tion with " + connectMode.ToString());
            //System.Diagnostics.Debugger.Break();
            if (connectMode == ext_ConnectMode.ext_cm_Startup)
			{
                //System.Windows.Forms.MessageBox.Show("got connect setup");
				object []contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
				//try
				{
					//Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "NextFile", "FileSwitch NextFile", "Open next related file.", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                    command.Bindings = "GLOBAL::ctrl+`";
				}
                //catch(System.ArgumentException)
                //{
                //    //If we are here, then the exception is probably because a command with that name
                //    //  already exists. If so there is no need to recreate the command and we can 
                //    //  safely ignore the exception.
                //}

                //try
                {
                    //Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "PrevFile", "FileSwitch PrevFile", "Open previous related file.", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                    command.Bindings = "GLOBAL::ctrl+shift+`";
                }
                //catch (System.ArgumentException)
                //{
                //    //If we are here, then the exception is probably because a command with that name
                //    //  already exists. If so there is no need to recreate the command and we can 
                //    //  safely ignore the exception.
                //}
            }
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
            //System.Windows.Forms.MessageBox.Show("got startupcomplete");
        }

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}

        bool AcceptedCommand(string commandName)
        {
            if (commandName == "FileSwitch2008.Connect.NextFile" || commandName == "FileSwitch2008.Connect.PrevFile")
                return true;

            return false;
        }

		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
                if (AcceptedCommand(commandName))
				{
                    //System.Windows.Forms.MessageBox.Show("query status good");

					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

        public class FileGroup
        {
            HashSet<string> files = new HashSet<string>();

            string[] sorted;

            private int _index = -1;
            
            public int Index
            {
                get
                {
                    System.Diagnostics.Debug.Assert(_index != -1);
                    return _index;
                }
            }

            public string Next()
            {
                System.Diagnostics.Debug.Assert(_index != -1);
                _index += 1;
                if (_index >= sorted.Length)
                    _index = 0;

                return sorted[_index];
            }

            public string Prev()
            {
                System.Diagnostics.Debug.Assert(_index != -1);
                _index -= 1;
                if (_index < 0)
                    _index = sorted.Length-1;

                return sorted[_index];
            }

            public FileGroup(Project project, string initialfile)
            {
                string name = Path.GetFileNameWithoutExtension(initialfile);

                GetDirectoryFiles(initialfile, files);
                GetMatchingFiles(project.ProjectItems, name, files);

                sorted = new string[files.Count];

                files.CopyTo(sorted);

                System.Array.Sort(sorted);

                _index = 0;
                foreach (string s in sorted)
                {
                    if (s == initialfile)
                    {
                        break;
                    }

                    _index++;
                }

            }

            static void GetDirectoryFiles(string fullname, HashSet<string> matches)
            {
                string name = Path.GetFileNameWithoutExtension(fullname);
                string path = Path.GetDirectoryName(fullname);

                DirectoryInfo dir = new DirectoryInfo(path);

                FileInfo [] files = dir.GetFiles(name + ".*", SearchOption.AllDirectories);

                foreach (FileInfo f in files)
                {
                    matches.Add(f.FullName);
                }
                
            }

            static void GetMatchingFiles(ProjectItems items, string name, HashSet<string> matches)
            {
                foreach (ProjectItem p in items)
                {
                    if(p.ProjectItems.Count > 0)
                    {
                        GetMatchingFiles(p.ProjectItems, name, matches);
                    }
                    else
                    {
                        bool isFile = p.Kind.CompareTo(Constants.vsProjectItemKindPhysicalFile) == 0;

                        if (isFile)
                        {
                            if (p.Name.Contains(name))
                            {
                                matches.Add((string)p.Properties.Item("FullPath").Value);
                            }
                        }
                    }
                }
                
            }
        }



		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
            //System.Windows.Forms.MessageBox.Show("exec");

			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault && AcceptedCommand(commandName))
			{
                bool bForward = true;
				if(commandName == "FileSwitch2008.Connect.PrevFile")
				{
                    bForward = false;
                }

                Project fileproject = _addInInstance.DTE.ActiveDocument.ProjectItem.ContainingProject;
                string fullname =_addInInstance.DTE.ActiveDocument.FullName;

                FileGroup group = new FileGroup(fileproject, fullname);

                string newfile = bForward ? group.Next() : group.Prev();

                _applicationObject.ItemOperations.OpenFile(newfile, Constants.vsViewKindCode);

				handled = true;
				return;
			}
		}
		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}