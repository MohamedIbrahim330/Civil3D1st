//using Autodesk.AutoCAD.Runtime;
//using Autodesk.Windows;
//using System.Windows.Input;
//using System.Windows.Controls;
//using System.Windows.Media.Imaging;
//using Autodesk.AutoCAD.ApplicationServices;
//using System;

//namespace AlignmentTool.Command
//{
//    public class RibbonInitializer
//    {
//        public static void InitializeRibbon()
//        {
//            // Create a new ribbon tab
//            RibbonControl ribbonControl = ComponentManager.Ribbon;
//            if (ribbonControl == null)
//                return; // If ribbon control is null, exit.

//            RibbonTab ribbonTab = new RibbonTab
//            {
//                Title = "Alignment Tool",
//                Id = "AlignmentToolTab"
//            };

//            // Add tab to the ribbon control if it doesn't already exist
//            if (ribbonControl.FindTab(ribbonTab.Id) == null)
//            {
//                ribbonControl.Tabs.Add(ribbonTab);
//            }

//            // Create a new ribbon panel within this tab
//            RibbonPanelSource panelSource = new RibbonPanelSource
//            {
//                Title = "Alignment Creation"
//            };
//            RibbonPanel ribbonPanel = new RibbonPanel
//            {
//                Source = panelSource
//            };
//            ribbonTab.Panels.Add(ribbonPanel);

//            // Create the button for launching the alignment tool
//            RibbonButton alignmentButton = new RibbonButton
//            {
//                Text = "Create Alignment",
//                ShowText = true,
//                ShowImage = true,
//                CommandHandler = new RelayCommandHandler(), // Attach command handler
//                Image = LoadIcon("AlignmentTool.Resources.Icon16.png"), // Small icon (16x16)
//                LargeImage = LoadIcon("AlignmentTool.Resources.Icon32.png"), // Large icon (32x32)
//                ToolTip = "Open the Alignment Creation Tool"
//            };

//            // Add the button to the panel
//            panelSource.Items.Add(alignmentButton);
//        }

//        // Method to load an image from resources (update path to your resource folder)
//        private static System.Windows.Media.ImageSource LoadIcon(string resourcePath)
//        {
//            var uri = new Uri($"pack://application:,,,/{resourcePath}", UriKind.RelativeOrAbsolute);
//            return new System.Windows.Media.Imaging.BitmapImage(uri);
//        }
//    }

//    // Command handler class
//    public class RelayCommandHandler : ICommand
//    {
//        public event EventHandler CanExecuteChanged;
//        public bool CanExecute(object parameter) => true;

//        public void Execute(object parameter)
//        {
//            // Execute the command defined in AlignmentToolCommand
//            AlignmentToolCommand command = new AlignmentToolCommand();
//            command.RunAlignmentTool();
//        }
//    }
//}
