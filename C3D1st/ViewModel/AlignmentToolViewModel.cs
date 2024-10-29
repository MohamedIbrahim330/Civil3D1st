using CoreApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using C3D1st.Model;
using AlignmentTool.Command;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using Autodesk.Civil.DatabaseServices.Styles;

namespace AlignmentTool.ViewModel
{
    public class AlignmentToolViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private List<ObjectId> _selectedPolylineIds; // List to store multiple selected polylines

        #region Constructor
        public AlignmentToolViewModel()
        {
            // Initialize commands
            CreateAlignmentCommand = new MyCommand(CreateAlignment, CanCreateAlignment);
            CancelCommand = new MyCommand(Cancel, CanCancel);
            SelectPolylineCommand = new MyCommand(SelectPolylines); 

            // Initialize collections
            Layers = new ObservableCollection<string>();
            AlignmentStyles = new ObservableCollection<string>();

            // Initialize selected alignment model
            SelectedAlignment = new AlignmentModel();
            _selectedPolylineIds = new List<ObjectId>();

            // Set up DispatcherTimer to delay loading by 2 seconds
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nLoading Layers and Styles...");
                LoadLayers();
                LoadAlignmentStyles();
            }
            catch (Exception ex)
            {
                AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nException occurred: {ex.Message}");
            }
        }

        #endregion

        #region PropertyChanged Event Handling
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Commands

        public MyCommand CreateAlignmentCommand { get; set; }
        public MyCommand CancelCommand { get; set; }
        public MyCommand SelectPolylineCommand { get; set; }

        #endregion

        #region Properties

        public ObservableCollection<string> Layers { get; set; }
        public ObservableCollection<string> AlignmentStyles { get; set; }

        private AlignmentModel _selectedAlignment;
        public AlignmentModel SelectedAlignment
        {
            get => _selectedAlignment;
            set
            {
                _selectedAlignment = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Data Loading Methods

        private void LoadLayers()
        {
            Document doc = AcadApplication.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)tr.GetObject(doc.Database.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId layerId in layerTable)
                {
                    LayerTableRecord layer = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                    Layers.Add(layer.Name);
                }
                tr.Commit();
            }
            AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nLoaded {Layers.Count} layers");
        }

        private void LoadAlignmentStyles()
        {
            try
            {
                AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nStarting to load alignment styles...");
                CivilDocument civilDoc = CivilApplication.ActiveDocument;

                if (civilDoc == null || civilDoc.Styles.AlignmentStyles == null)
                {
                    AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError: AlignmentStyles collection is not available.");
                    return;
                }

                using (Transaction tr = AcadApplication.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId styleId in civilDoc.Styles.AlignmentStyles)
                    {
                        if (!styleId.IsValid) continue;

                        AlignmentStyle style = tr.GetObject(styleId, OpenMode.ForRead) as AlignmentStyle;
                        if (style != null)
                        {
                            AlignmentStyles.Add(style.Name);
                        }
                    }
                    tr.Commit();
                }

                AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nTotal alignment styles loaded: {AlignmentStyles.Count}");
            }
            catch (Exception ex)
            {
                AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nError loading alignment styles: {ex.Message}");
            }
        }

        #endregion

        #region Command Methods

        private void CreateAlignment(object parameter)
        {
            if (_selectedPolylineIds.Count == 0)
            {
                MessageBox.Show("Please select at least one polyline before creating the alignment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Iterate through each selected polyline and create an alignment for each
            int suffix = 1;
            foreach (var polylineId in _selectedPolylineIds)
            {
                string uniqueAlignmentName = GenerateUniqueAlignmentName(SelectedAlignment.Name, suffix++);
                AlignmentCreator.CreateAlignment(uniqueAlignmentName, SelectedAlignment.Layer, SelectedAlignment.Style, SelectedAlignment.AddCurvesBetweenTangents, polylineId);
            }

            if (parameter is Window window)
            {
                window.Close();
            }
        }


        private string GenerateUniqueAlignmentName(string baseName, int startSuffix)
        {
            int suffix = startSuffix;
            string proposedName = $"{baseName}_{suffix}";

            // Check if alignment with proposedName already exists
            while (AlignmentCreator.DoesAlignmentExist(proposedName))
            {
                suffix++;
                proposedName = $"{baseName}_{suffix}";
            }

            return proposedName;
        }


        private void SelectPolylines(object parameter)
        {
            var doc = AcadApplication.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            // Prompt for multiple polyline selection
            var options = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect polylines to convert to alignments:",
                RejectObjectsOnLockedLayers = true
            };
            var filter = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "LWPOLYLINE") });

            var result = ed.GetSelection(options, filter);
            if (result.Status == PromptStatus.OK)
            {
                _selectedPolylineIds.Clear(); // Clear previous selections
                foreach (SelectedObject obj in result.Value)
                {
                    if (obj != null)
                    {
                        _selectedPolylineIds.Add(obj.ObjectId);
                        ed.WriteMessage($"\nPolyline selected: {obj.ObjectId}");
                    }
                }
            }
            else
            {
                ed.WriteMessage("\nPolyline selection canceled.");
            }
        }

        private bool CanCreateAlignment(object parameter) => true;

        private void Cancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            _timer?.Stop();
        }

        private bool CanCancel(object parameter) => true;

        #endregion
    }
}
