﻿/* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

// Copyright (C) 2016 Cameron Angus. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace KantanDocGen
{
    struct classStructure
    {
        public string savegame { get; set; }
        public string refactor { get; set; }
        public string short_tooltip { get; set; }
        public string native { get; set; }
    }
    class Program
    {

        static string ParseArgumentValue(List<string> ArgumentList, string Prefix, string DefaultValue)
        {
            for (int Idx = 0; Idx < ArgumentList.Count; Idx++)
            {
                if (ArgumentList[Idx].StartsWith(Prefix))
                {
                    string Value = ArgumentList[Idx].Substring(Prefix.Length);
                    ArgumentList.RemoveAt(Idx);
                    return Value;
                }
            }
            return DefaultValue;
        }

        static string ParseArgumentPath(List<string> ArgumentList, string Prefix, string DefaultValue)
        {
            string Value = ParseArgumentValue(ArgumentList, Prefix, DefaultValue);
            if (Value != null)
            {
                Value = Path.GetFullPath(Value);
            }
            return Value;
        }

        static string ParseArgumentDirectory(List<string> ArgumentList, string Prefix, string DefaultValue)
        {
            string Value = ParseArgumentPath(ArgumentList, Prefix, DefaultValue);
            if (Value != null && !Directory.Exists(Value))
            {
                Directory.CreateDirectory(Value);
            }
            return Value;
        }

        static private void ProcessOutputReceived(Object Sender, DataReceivedEventArgs Line)
        {
            if (Line.Data != null && Line.Data.Length > 0)
            {
                Console.WriteLine(Line.Data);
            }
        }

        public static void SafeCreateDirectory(string Path)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
        }

        private static void CopyWholeDirectory(string SourceDir, string DestDir)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(SourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + SourceDir);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(DestDir))
            {
                Directory.CreateDirectory(DestDir);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(DestDir, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(DestDir, subdir.Name);
                CopyWholeDirectory(subdir.FullName, temppath);
            }
        }

        // @NOTE: Currently unused, seemingly no way to use Slate for the node rendering when running commandlet.
        // Instead this tool is now invoked by a plugin.

        static bool RunXmlDocGenCommandlet(string EngineDir, string EditorPath, string OutputDir)
        {
            // Create the output directory
            SafeCreateDirectory(OutputDir);

            string Arguments = "-run=KantanDocs -path=" + OutputDir + " -name=BlueprintAPI -stdout -FORCELOGFLUSH -CrashForUAT -unattended -AllowStdOutLogVerbosity";
            Console.WriteLine("Running: {0} {1}", EditorPath, Arguments);

            using (Process NewProcess = new Process())
            {
                NewProcess.StartInfo.WorkingDirectory = EngineDir;
                NewProcess.StartInfo.FileName = EditorPath;
                NewProcess.StartInfo.Arguments = Arguments;
                NewProcess.StartInfo.UseShellExecute = false;
                NewProcess.StartInfo.RedirectStandardOutput = true;
                NewProcess.StartInfo.RedirectStandardError = true;

                NewProcess.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputReceived);
                NewProcess.ErrorDataReceived += new DataReceivedEventHandler(ProcessOutputReceived);

                try
                {
                    NewProcess.Start();
                    NewProcess.BeginOutputReadLine();
                    NewProcess.BeginErrorReadLine();
                    NewProcess.WaitForExit();
                    if (NewProcess.ExitCode != 0)
                    {
                        Console.WriteLine("Error: Xml doc generation commandlet failed, aborting.\nIs the plugin installed?");
                        return false;
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString() + "\n" + Ex.StackTrace);
                    return false;
                }
            }

            return true;
        }

        static bool IsDebug = true;


        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            List<string> ArgumentList = new List<string>(args);

            Console.WriteLine("KantanDocGen invoked with arguments:");
            foreach (string Arg in ArgumentList)
            {
                Console.WriteLine(Arg);
            }

            string DocsTitle = ParseArgumentValue(ArgumentList, "-name=", null);
            DocsTitle = "HubrisVR";
            if (DocsTitle == null)
            {
                Console.WriteLine("KantanDocGen: Error: Documentation title (-name=) required. Aborting.");
                return;
            }

            // Get the default paths

            // If unspecified, assume the directory containing our binary is one level below the base directory
            string DocGenBaseDir = ParseArgumentDirectory(ArgumentList, "-basedir=", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            string OutputRoot = ParseArgumentDirectory(ArgumentList, "-outputdir=", Directory.GetCurrentDirectory());
            string OutputDir = Path.Combine(OutputRoot, DocsTitle);

            //string MsxslPath = ParseArgumentPath(ArgumentList, "-xslproc=", Path.Combine(EngineDir, "Binaries/ThirdParty/Msxsl/msxsl.exe"));

            // Xsl transform files - if not specified explicitly, look for defaults relative to base directory
            string IndexTransformPath, ClassTransformPath, NodeTransformPath;

            if (IsDebug)
            {
                IndexTransformPath = ParseArgumentPath(ArgumentList, "-indexxsl=", Path.Combine(DocGenBaseDir, "DeploymentFiles/xslt/index_xform.xsl"));
                ClassTransformPath = ParseArgumentPath(ArgumentList, "-classxsl=", Path.Combine(DocGenBaseDir, "DeploymentFiles/xslt/class_docs_xform.xsl"));
                NodeTransformPath = ParseArgumentPath(ArgumentList, "-nodexsl=", Path.Combine(DocGenBaseDir, "DeploymentFiles/xslt/node_docs_xform.xsl"));
            }
            else
            {
                IndexTransformPath = ParseArgumentPath(ArgumentList, "-indexxsl=", Path.Combine(DocGenBaseDir, "xslt/index_xform.xsl"));
                ClassTransformPath = ParseArgumentPath(ArgumentList, "-classxsl=", Path.Combine(DocGenBaseDir, "xslt/class_docs_xform.xsl"));
                NodeTransformPath = ParseArgumentPath(ArgumentList, "-nodexsl=", Path.Combine(DocGenBaseDir, "xslt/node_docs_xform.xsl"));
            }


            bool bFromIntermediate = ArgumentList.Contains("-fromintermediate");
            string IntermediateDir;
            if (bFromIntermediate)
            {
                // Intermediate docs already created, we need to have been passed an intermediate directory to locate them
                IntermediateDir = ParseArgumentDirectory(ArgumentList, "-intermediatedir=", null);
                if (IntermediateDir == null)
                {
                    Console.WriteLine("KantanDocGen: Error: -fromintermediate requires -intermediatedir to be set. Aborting.");
                    return;
                }

                if (!Directory.Exists(IntermediateDir))
                {
                    Console.WriteLine("KantanDocGen: Error: Specified intermediate directory not found. Aborting.");
                    return;
                }
            }
            else
            {
                // @TODO: This doesn't work, since commandlet cannot create Slate windows!
                // Can reenable this path if manage to get a Program target type to build against the engine.
                IntermediateDir = "C:\\Repositries\\KantanDocGenTool\\Intermediate\\HubrisVR";
                Console.WriteLine("KantanDocGen: creating from debug location, make sure to include the proper directory if creating in project");
                //return;

                /*				IntermediateDir = ParseArgumentDirectory(ArgumentList, "-intermediatedir=", Path.Combine(EngineDir, "Intermediate\\KantanDocGen"));

								// Need to generate intermediate docs first
								// Run editor commandlet to generate XML and image files
								string EditorPath = Path.Combine(EngineDir, "Binaries\\Win64\\UE4Editor-Cmd.exe");
								if (!RunXmlDocGenCommandlet(EngineDir, EditorPath, IntermediateDir))
								{
									return;
								}
				*/
            }

            const bool bCleanOutput = true;
            bool bHardClean = ArgumentList.Contains("-cleanoutput");
            if (bCleanOutput)
            {
                // If the output directory exists, attempt to delete it (this will fail if bHardClean is false and the directory contains files/subfolders)
                if (Directory.Exists(OutputDir))
                {
                    try
                    {
                        Directory.Delete(OutputDir, true);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("KantanDocGen: Error: Output directory '{0}' exists and not empty/couldn't delete. Remove and rerun, or specify -cleanoutput (If running from plugin console, add 'clean' parameter).", OutputDir);
                        return;
                    }
                }
            }

            //var XslXform = new MsxslXform(MsxslPath);
            var IndexXform = new SaxonXform();
            var ClassXform = new SaxonXform();
            var NodeXform = new SaxonXform();

            // Initialize the transformations
            if (!IndexXform.Initialize(IndexTransformPath, ProcessOutputReceived))
            {
                Console.WriteLine("Error: Failed to initialize xslt processor.");
                return;
            }
            if (!ClassXform.Initialize(ClassTransformPath, ProcessOutputReceived))
            {
                Console.WriteLine("Error: Failed to initialize xslt processor.");
                return;
            }
            if (!NodeXform.Initialize(NodeTransformPath, ProcessOutputReceived))
            {
                Console.WriteLine("Error: Failed to initialize xslt processor.");
                return;
            }

            // Loop over all generated xml files and apply the transformation
            int Success = 0;
            int Failed = 0;

            Dictionary<string, classStructure> ClassStructureDictionary = new Dictionary<string, classStructure>();

            // @TODO: Should iterate over index/class xml entries rather than enumerate files and directories
            var SubFolders = Directory.EnumerateDirectories(IntermediateDir);
            foreach (string Sub in SubFolders)
            {
                string ClassTitle = Path.GetFileName(Sub);
                string OutputClassDir = Path.Combine(OutputDir, ClassTitle);
                SafeCreateDirectory(OutputClassDir);
                string NodeDir = Path.Combine(Sub, "nodes");



                if (Directory.Exists(NodeDir))
                {
                    string OutputNodesDir = Path.Combine(OutputClassDir, "nodes");
                    SafeCreateDirectory(OutputNodesDir);

                    XmlDocument doc = new XmlDocument();
                    doc.Load(Path.Combine(Sub, ClassTitle + ".xml"));
                    XmlElement root = doc.DocumentElement;
                    //XmlNode nodesNode = root.SelectSingleNode("descendant::nodes");

                    root.RemoveChild(root.LastChild);

                    XmlElement functionListElement = doc.CreateElement("FunctionList");

                    var InputFiles = Directory.EnumerateFiles(NodeDir, "*.xml", SearchOption.TopDirectoryOnly);

                    foreach (string FilePath in InputFiles)
                    {
                        XmlElement functionRoot = doc.CreateElement("Function");

                        XmlDocument functionDoc = new XmlDocument();
                        functionDoc.Load(FilePath);

                        foreach (XmlNode childElement in functionDoc.DocumentElement.ChildNodes)
                        {
                            //XmlNode nodeCopy = childElement.CloneNode(true);				
                            XmlNode nodeCopy = doc.ImportNode(childElement, true);
                            functionRoot.AppendChild(nodeCopy);
                        }
                        doc.ImportNode(functionRoot, true);
                        functionListElement.AppendChild(functionRoot);

                        ++Success;
                        //doc.Save(Console.Out);
                    }

                    root.AppendChild(functionListElement);


                    var thisStruct = new classStructure
                    {
                        savegame = doc.DocumentElement["savegame"].InnerText,
                        refactor = doc.DocumentElement["refactor"].InnerText,
                        short_tooltip = doc.DocumentElement["short_tooltip"].InnerText,
                        native = doc.DocumentElement["native"].InnerText
                    };

                    ClassStructureDictionary[ClassTitle] = thisStruct;

                    doc.Save(Path.Combine(Sub, ClassTitle + ".xml"));
                    string OutputClassPath = Path.Combine(OutputClassDir, ClassTitle + ".html");
                    ClassXform.TransformXml(Path.Combine(Sub, ClassTitle + ".xml"), OutputClassPath);
                }

                //Copy the images for this class to the output directory

                CopyWholeDirectory(Path.Combine(Sub, "img"), Path.Combine(OutputClassDir, "img"));
            }


            XmlDocument indexXMLDoc = new XmlDocument();
            indexXMLDoc.Load(Path.Combine(IntermediateDir, "index.xml"));
            XmlElement indexRoot = indexXMLDoc.DocumentElement;

            foreach (XmlNode item in indexRoot.GetElementsByTagName("class"))
            {
                var saveGame = indexXMLDoc.CreateElement("savegame");
                saveGame.InnerText = ClassStructureDictionary[item["id"].InnerText].savegame;
                item.AppendChild(saveGame);

                var refactor = indexXMLDoc.CreateElement("refactor");
                refactor.InnerText = ClassStructureDictionary[item["id"].InnerText].refactor;
                item.AppendChild(refactor);

                var shortTooltip = indexXMLDoc.CreateElement("short_tooltip");
                shortTooltip.InnerText = ClassStructureDictionary[item["id"].InnerText].short_tooltip;
                item.AppendChild(shortTooltip);

                var native = indexXMLDoc.CreateElement("native");
                native.InnerText = ClassStructureDictionary[item["id"].InnerText].native;
                item.AppendChild(native);


                //Console.WriteLine(item.InnerText);
            }

            indexXMLDoc.Save(Path.Combine(IntermediateDir, "index_adjusted.xml"));

            string OutputIndexPath = Path.Combine(OutputDir, "index.html");

            //todo update the entire index XML with the required information we need of the classes (Refactor, )

            IndexXform.TransformXml(Path.Combine(IntermediateDir, "index_adjusted.xml"), OutputIndexPath);


            //debug

            if (IsDebug)
            {
                CopyWholeDirectory(Path.Combine(DocGenBaseDir, "DeploymentFiles/css"), Path.Combine(OutputDir, "css"));
                CopyWholeDirectory(Path.Combine(DocGenBaseDir, "DeploymentFiles/img"), Path.Combine(OutputDir, "img"));
            }
            else
            {
                CopyWholeDirectory(Path.Combine(DocGenBaseDir, "img"), Path.Combine(OutputDir, "img"));
                CopyWholeDirectory(Path.Combine(DocGenBaseDir, "css"), Path.Combine(OutputDir, "css"));
            }


            Console.WriteLine("KantanDocGen completed:");
            Console.WriteLine("{0} node docs successfully transformed.", Success);
            Console.WriteLine("{0} failed.", Failed);
        }
    }
}

