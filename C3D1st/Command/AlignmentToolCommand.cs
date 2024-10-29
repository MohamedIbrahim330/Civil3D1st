using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using C3D1st.View; 

namespace AlignmentTool.Command
{
    public class AlignmentToolCommand
    {
        // Register command without ribbon, accessible from the AutoCAD command line (NetLoad)
        [CommandMethod("ALIGNMENT_TOOL")]
        public void RunAlignmentTool()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Window1 window = new Window1
                {
                    DataContext = new ViewModel.AlignmentToolViewModel()
                };
                window.ShowDialog();
            }
            else
            {
                doc.Editor.WriteMessage("\nError: No active document found.");
            }
        }
    }
}
