using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;

namespace AlignmentTool.Command
{
    public static class AlignmentCreator
    {
        public static void CreateAlignment(string alignmentName, string layerName, string alignmentStyle, bool addCurvesBetweenTangents, ObjectId polylineId)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                CivilDocument civilDoc = CivilApplication.ActiveDocument;

                // Ensure the layer exists or create it if necessary
                ObjectId layerId = GetOrCreateLayer(db, layerName);

                ObjectId alignmentStyleId = GetAlignmentStyleId(civilDoc, alignmentStyle);
                if (alignmentStyleId == ObjectId.Null)
                {
                    ed.WriteMessage("\nError: Alignment style not found.");
                    return;
                }

                ObjectId labelSetId = GetDefaultLabelSetId(civilDoc) ?? ObjectId.Null;

                // Initialize PolylineOptions with selected polyline
                PolylineOptions polylineOptions = new PolylineOptions
                {
                    AddCurvesBetweenTangents = addCurvesBetweenTangents,
                    EraseExistingEntities = false,
                    PlineId = polylineId
                };

                // Create the alignment and store the ObjectId
                ObjectId alignmentId = Alignment.Create(civilDoc, polylineOptions, alignmentName, ObjectId.Null, layerId, alignmentStyleId, labelSetId);
                ed.WriteMessage($"\nAlignment '{alignmentName}' created successfully.");
                tr.Commit();
            }
        }

        public static bool DoesAlignmentExist(string alignmentName)
        {
            CivilDocument civilDoc = CivilApplication.ActiveDocument;

            if (civilDoc == null)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError: Civil 3D document is not available.");
                return false;
            }

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId alignmentId in civilDoc.GetAlignmentIds())
                {
                    if (alignmentId.IsValid && !alignmentId.IsNull)
                    {
                        Alignment alignment = tr.GetObject(alignmentId, OpenMode.ForRead) as Alignment;
                        if (alignment != null && alignment.Name == alignmentName)
                        {
                            return true;
                        }
                    }
                }
                tr.Commit();
            }
            return false;
        }

        private static ObjectId GetOrCreateLayer(Database db, string layerName)
        {
            LayerTable layerTable = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            if (!layerTable.Has(layerName))
            {
                using (LayerTableRecord layerRecord = new LayerTableRecord())
                {
                    layerRecord.Name = layerName;
                    layerTable.UpgradeOpen();
                    layerTable.Add(layerRecord);
                    db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(layerRecord, true);
                }
            }
            return layerTable[layerName];
        }

        private static ObjectId GetAlignmentStyleId(CivilDocument civilDoc, string styleName)
        {
            foreach (ObjectId id in civilDoc.Styles.AlignmentStyles)
            {
                AlignmentStyle style = id.GetObject(OpenMode.ForRead) as AlignmentStyle;
                if (style != null && style.Name == styleName)
                {
                    return id;
                }
            }
            return ObjectId.Null;
        }

        private static ObjectId? GetDefaultLabelSetId(CivilDocument civilDoc)
        {
            foreach (ObjectId id in civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles)
            {
                AlignmentLabelSetStyle labelSet = id.GetObject(OpenMode.ForRead) as AlignmentLabelSetStyle;
                if (labelSet != null)
                {
                    return id;
                }
            }
            return null;
        }
    }
}
