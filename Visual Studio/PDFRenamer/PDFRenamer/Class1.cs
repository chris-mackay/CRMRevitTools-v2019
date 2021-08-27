using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace PDFRenamer
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class Class1 : IExternalCommand
    {
        public static ExternalCommandData m_commandData;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_commandData = commandData;
            UIApplication uiApp = commandData.Application;
            
            MainWindow dialog = new MainWindow(uiApp);
            dialog.ShowDialog();

            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                return Result.Succeeded;
            else
                return Result.Cancelled;
        }
    }
}