using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace PDFRenamer
{
    public partial class MainWindow : Window
    {
        #region CLASS_LEVEL_VARIABLES

        UIApplication myRevitUIApp = null;
        Document myRevitDoc = null;

        public string projectNumber = string.Empty;
        public IList<Element> viewSheetSets = null;
        public string REVIT_VERSION = "v2019";

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(UIApplication incomingUIApp)
        {
            InitializeComponent();

            Dictionary<string, string> settings = new Dictionary<string, string>();

            settings.Add("DefaultDirectory", "");

            XMLSettings.AppSettingsFile = @"C:\Users\" + Environment.UserName + @"\Documents\CRMRevitTools\v2019\Commands\PDFRenamer.xml";
            XMLSettings.InitializeSettings(settings);

            txtDrawingDirectory.Text = XMLSettings.GetSettingsValue("DefaultDirectory");

            myRevitUIApp = incomingUIApp;
            myRevitDoc = myRevitUIApp.ActiveUIDocument.Document;

            FilteredElementCollector sheetSetsCol = new FilteredElementCollector(myRevitDoc);

            viewSheetSets = sheetSetsCol.OfClass(typeof(ViewSheetSet)).ToElements(); //GET ALL THE SHEETSETS IN THE PROJECT

            projectNumber = myRevitDoc.ProjectInformation.LookupParameter("Project Number").AsString();

            //LOOPS THROUGH ALL THE SHEETSETS IN THE PROJECT AND FILL COMBOBOX FOR SELECTION
            foreach (ViewSheetSet vss in viewSheetSets)
            {
                cbSheetSets.Items.Add(vss.Name);
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            string def = XMLSettings.GetSettingsValue("DefaultDirectory");

            if (def == "")
            {
                dialog.InitialDirectory = "C:\\";
            }
            else
            {
                dialog.InitialDirectory = def;
            }

            dialog.IsFolderPicker = true;
            dialog.Title = "Select the directory where the PDF files are located";

            //GET DIRECTORY WHERE THE DRAWINGS ARE SAVED
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string dir = string.Empty;
                dir = dialog.FileName;
                txtDrawingDirectory.Text = dir.Trim();
            }
            else
            {
                dialog = null;
            }
        }

        private bool DrawingDirectoryIsDefault(string dir)
        {
            bool flag = false;
            dir = txtDrawingDirectory.Text;

            string savedDir = XMLSettings.GetSettingsValue("DefaultDirectory");

            if (dir != string.Empty && System.IO.Directory.Exists(dir))
            {
                if (dir == savedDir)
                    flag = true;
                else
                    flag = false;
            }

            return flag;
        }

        private void ckbDefault_Checked(object sender, RoutedEventArgs e)
        {
            string dir = txtDrawingDirectory.Text;
            bool? isChecked = ckbDefault.IsChecked;

            if ((bool)isChecked)
                if (dir != string.Empty && System.IO.Directory.Exists(dir))
                {
                    XMLSettings.SetSettingsValue("DefaultDirectory", dir);
                    ckbDefault.IsEnabled = false;
                }
        }

        private void txtDrawingDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            string dir = txtDrawingDirectory.Text;

            if (!DrawingDirectoryIsDefault(dir))
            {
                ckbDefault.IsChecked = false;
                ckbDefault.IsEnabled = true;
            }
            else
            {
                ckbDefault.IsChecked = true;
                ckbDefault.IsEnabled = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Autodesk.Revit.UI.TaskDialog taskDialog = new Autodesk.Revit.UI.TaskDialog("PDF Renamer");

            string dir = txtDrawingDirectory.Text;

            if (dir == string.Empty)
            {
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                taskDialog.MainInstruction = "No directory provided.";
                taskDialog.Show();
            }
            else if (!System.IO.Directory.Exists(dir))
            {
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                taskDialog.MainInstruction = "The directory provided does not exist.";
                taskDialog.Show();
            }
            else if (cbSheetSets.SelectedIndex < 0)
            {
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                taskDialog.MainInstruction = "No sheet set provided.";
                taskDialog.Show();
            }
            else
            {
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconNone;
                taskDialog.MainInstruction = "Are you sure you want to rename all the sheets in the directory below?";
                taskDialog.MainContent = dir;
                taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

                if (taskDialog.Show() == Autodesk.Revit.UI.TaskDialogResult.Yes)
                {
                    ViewSet viewSet = null;

                    foreach (ViewSheetSet vs in viewSheetSets) // Get the selected sheet set
                    {
                        if (vs.Name == cbSheetSets.SelectedItem.ToString())
                        {
                            viewSet = vs.Views; // Get all the sheets in the sheet set
                        }
                    }

                    string[] files = Directory.GetFiles(dir, "*.pdf", SearchOption.TopDirectoryOnly);
                    List<string> oldFiles = new List<string>();

                    foreach (string file in files) oldFiles.Add(file);

                    // <Key>   Old file to be renamed
                    // <Value> New file name
                    Dictionary<string, string> fileDic = new Dictionary<string, string>();

                    foreach (ViewSheet v in viewSet) // Loop through all the sheets in the sheet set
                    {
                        string sheetNumber = string.Empty;
                        string sheetName = string.Empty;

                        sheetNumber = v.SheetNumber;
                        sheetName = v.Name;

                        // SHEET NUMBER needs to be checked for the following special characters below

                        // These need to be replaced with '-'
                        // / * " .

                        // Revit checks for the following characters below and don't need to be handled
                        // \ : {} [] ; < > ? ` ~

                        // REVIT & WINDOWS all the following characters below in file names
                        // ! @ # $ % ^ & * ( ) _ + = - ' ,

                        if (sheetNumber.Contains(@"/"))
                        {
                            sheetNumber = sheetNumber.Replace(@"/", "-");
                        }

                        if (sheetNumber.Contains("*"))
                        {
                            sheetNumber = sheetNumber.Replace("*", "-");
                        }

                        if (sheetNumber.Contains("\""))
                        {
                            sheetNumber = sheetNumber.Replace("\"", "-");
                        }

                        if (sheetNumber.Contains("."))
                        {
                            sheetNumber = sheetNumber.Replace(".", "-");
                        }

                        string rev = string.Empty;

                        rev = v.LookupParameter("Current Revision").AsString();

                        string newFileName = string.Empty;
                        string newFile = string.Empty;

                        newFileName = projectNumber + "-" + sheetNumber + "_" + rev + ".pdf";
                        newFile = dir + "\\" + newFileName;

                        string pattern = "- " + sheetNumber + " -";
                        string oldFile = oldFiles.Find(a => a.Contains(pattern));
                        fileDic.Add(oldFile, newFile);
                    }

                    foreach (KeyValuePair<string, string> entry in fileDic)
                    {
                        string oldFile = entry.Key;
                        string newFile = entry.Value;

                        try
                        {
                            if (File.Exists(newFile))
                            {
                                File.Delete(newFile);
                            }

                            File.Move(oldFile, newFile);
                        }
                        catch (Exception ex)
                        {
                            Autodesk.Revit.UI.TaskDialog errorTaskDialog = new Autodesk.Revit.UI.TaskDialog("PDF Renamer");
                            errorTaskDialog.MainInstruction = "An error occured while renaming the files. See message below.";
                            errorTaskDialog.MainContent = "Error Message: " + ex.Message + "\nError Source: " + ex.Source;
                            errorTaskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                            errorTaskDialog.Show();
                            return;
                        }
                    }
                    Autodesk.Revit.UI.TaskDialog completeTaskDialog = new Autodesk.Revit.UI.TaskDialog("PDF Renamer");
                    completeTaskDialog.MainInstruction = "The sheets have been renamed successfully";
                    completeTaskDialog.MainContent = "";
                    completeTaskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                    completeTaskDialog.Show();
                }
            }
        }
    }
}