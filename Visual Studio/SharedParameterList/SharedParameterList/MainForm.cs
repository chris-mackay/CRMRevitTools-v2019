//    Copyright(C) 2020 Christopher Ryan Mackay

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System.IO;

namespace SharedParameterList
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        #region CLASS_LEVEL_VARIABLES

        UIApplication uiApp = null;
        Document doc = null;

        public string REVIT_VERSION = "v2019";

        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(UIApplication incomingUIApp)
        {
            InitializeComponent();

            uiApp = incomingUIApp;
            doc = uiApp.ActiveUIDocument.Document;
        }

        #region Voids
        private void cxmnuCopyGUID_Click(object sender, EventArgs e)
        {
            if (dgvParameters.Rows.Count > 0 && dgvParameters.SelectedRows.Count > 0)
            {
                string guid = dgvParameters.CurrentRow.Cells["GUID"].Value.ToString();
                Clipboard.SetText(guid);
            }
        }

        private void cxmnuCopyElementId_Click(object sender, EventArgs e)
        {
            if (dgvParameters.Rows.Count > 0 && dgvParameters.SelectedRows.Count > 0)
            {
                string id = dgvParameters.CurrentRow.Cells["ElementId"].Value.ToString();
                Clipboard.SetText(id);
            }
        }

        private void cxmnuCopyName_Click(object sender, EventArgs e)
        {
            if (dgvParameters.Rows.Count > 0 && dgvParameters.SelectedRows.Count > 0)
            {
                string name = dgvParameters.CurrentRow.Cells["ParamName"].Value.ToString();
                Clipboard.SetText(name);
            }
        }

        private void dgvParameters_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu contextMenu = new ContextMenu();
                contextMenu = TableContextMenu();
                contextMenu.Show(dgvParameters, new System.Drawing.Point(e.X, e.Y));
            }
        }

        private void LoadParameters()
        {
            dgvParameters.Rows.Clear();

            // The code below will loop through each FamilySymbol loaded 
            // into the project (in the Project Browser)

            // While looping through the FamilySymbols, the Instance parameters will not be available.
            // It is not guarenteed that there will be at least one FamilyInstance of each Family in the model.
            // You may not be able to fill out the family name, type, and category for each Instance parameter.

            // The only ways to get Instance paramters are below
            // > Loop through all the FamilyInstances in the project
            // > Read the BindingMap. It's not guarenteed that shared parameters have been inserted into Project Parameters.

            DrawingControl.SetDoubleBuffered(dgvParameters);
            DrawingControl.SuspendDrawing(dgvParameters);

            // ParameterType
            // GUID
            // ParameterName
            // Family Name
            // Family Type Name
            // Categories

            if (rbProjectParameters.Checked)
            {
                BindingMap map;
                map = doc.ParameterBindings;

                if (!map.IsEmpty)
                {
                    DefinitionBindingMapIterator it;
                    it = map.ForwardIterator();
                    it.Reset();

                    while (it.MoveNext())
                    {
                        var def = (InternalDefinition)it.Key;
                        var sp = doc.GetElement(def.Id) as SharedParameterElement;

                        Autodesk.Revit.DB.Binding b = it.Current as Autodesk.Revit.DB.Binding;

                        if (sp != null)
                            if (b is InstanceBinding)
                                dgvParameters.Rows.Add("", "Instance", def.ParameterType, sp.GuidValue, def.Name, "");
                            else if (b is TypeBinding)
                                dgvParameters.Rows.Add("", "Type", def.ParameterType, sp.GuidValue, def.Name, "");
                    }
                }
            }
            else if (rbTypeParameters.Checked)
            {
                List<BuiltInCategory> cats = Categories();

                foreach (BuiltInCategory bic in cats)
                {
                    IEnumerable<Family> families = GetFamiliesOfCategory(doc, bic);

                    foreach (Family f in families)
                    {
                        ISet<ElementId> ids = f.GetFamilySymbolIds();

                        foreach (ElementId id in ids)
                        {
                            Element e = doc.GetElement(id);
                            FamilySymbol fs = e as FamilySymbol;

                            IList<Parameter> ps = fs.GetOrderedParameters();

                            foreach (Parameter p in ps)
                            {
                                if (p.IsShared)
                                    dgvParameters.Rows.Add("", "Type", p.Definition.ParameterType, p.GUID, p.Definition.Name, f.Name);
                            }
                        }
                    }
                }
            }
            else if (rbInstanceParameters.Checked)
            {
                FilteredElementCollector eCol = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                eCol.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false),new ElementIsElementTypeFilter(true)));
                
                List<Element> elems = new List<Element>();

                foreach (Element e in eCol)
                    if (e.Category != null)
                        elems.Add(e);

                foreach (Element e in elems)
                {
                    IList<Parameter> set = e.GetOrderedParameters();
                    FamilyInstance fi = e as FamilyInstance;

                    if (fi != null)
                    {
                        Family f = fi.Symbol.Family;

                        foreach (Parameter p in set)
                            if (p.IsShared)
                                dgvParameters.Rows.Add(e.Id, "Instance", p.Definition.ParameterType, p.GUID, p.Definition.Name, f.Name);
                    }
                }
            }

            DrawingControl.ResumeDrawing(dgvParameters);
        }

        #endregion

        #region Functions

        private List<BuiltInCategory> Categories()
        {
            List<BuiltInCategory> cats = new List<BuiltInCategory>();

            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StackedWalls_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassTags_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassSurface_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassFloor_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Mass_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallRefPlanes_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StickSymbols_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RemovedGridSeg_Obsolete_IdInWrongRange);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PointClouds);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyOrigin_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyOrigin_Planes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyOrigin_Points);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyOrigin);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LinksAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FoundationSlabAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallFoundationAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IsolatedFoundationAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BraceAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Planes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Points);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RigidLinksAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FoundationSlabAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallFoundationAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IsolatedFoundationAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnEndSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnStartSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BraceEndSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BraceStartSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BraceAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamEndSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamStartSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamAnalytical);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassSecondaryMonth);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassPrimaryMonth);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassSectionFilled);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MultiSurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunSurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Analemma);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunsetText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassSection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassOuter);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunriseText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CompassInner);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunPath2);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunPath1);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sun);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SunStudy);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTrussStickSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTrussHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussChord);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussWeb);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussBottomChordCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussTopChordCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussVertWebCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussDiagWebCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTransitionHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTerminationHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemRailHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTopRailHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHandRailBracketHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHandRailHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemPanelBracketHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemPanelHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemBalusterHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemPostHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemSegmentHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairStringer2012HiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairTread2012HiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairLanding2012HiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairRun2012HiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Stairs2012HiddenLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtaSystemHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_OBSOLETE_ElemArrayHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EntourageHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlantingHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipmentHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TopographyHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingSystemHiddenLines_Obsolete);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SiteHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoadsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ParkingHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlumbingFixturesHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixturesHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FurnitureSystemsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalFixturesHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalEquipmentHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CaseworkHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DetailComponentsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ShaftOpeningHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModelHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallMullionsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanelsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailingHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FurnitureHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LinesHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionProfilesTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionHoleTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CouplerHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CouplerTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Coupler);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionWeldTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionShearStudTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionAnchorTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionBoltTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionPlateTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructSubConnections);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionModifiers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionWelds);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionHoles);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionShearStuds);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionNobleWarning);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionOthers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionBolts);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionAnchors);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionPlates);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionProfiles);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionReference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionFailed);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionStale);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructWeldLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricAreaBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricReinSpanSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricReinforcementWire);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricReinforcementBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarSetToggle);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricAreaTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricReinforcementTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReinTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricAreaSketchSheetsLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricAreaSketchEnvelopeLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricAreas);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricReinforcement);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarCover);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CoverType);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarShape);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PathReinBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PathReinTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PathReinSpanSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PathRein);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Cage);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReinXVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReinBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReinSpanSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReinSketchOverride);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaRein);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RebarSketchLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Rebar);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkLining);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainmentDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainmentRise);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkRise);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainmentSymbology);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainmentCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationContainment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkSymbology);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipeworkTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPipework);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkSymbology);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkRise);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationHangerTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationHangers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_OBSOLETE_FabricationPartsTmpGraphicDropDrag);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPartsTmpGraphicDrag);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_OBSOLETE_FabricationPartsTmpGraphicDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationPartsTmpGraphicEnd);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LayoutNodes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationServiceElements);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctworkTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FabricationDuctwork);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LayoutPathBase_Pipings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_NumberingSchemas);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DivisionRules);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_Shade);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticSurfaces);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticSpaces);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_OpeningAir);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_NonSlidingDoor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_SlidingDoor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_OperableSkylight);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_FixedSkylight);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_OperableWindow);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_FixedWindow);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_UndergroundCeiling);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_UndergroundSlab);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_UndergroundWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_SurfaceAir);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_Ceiling);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_InteriorFloor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_InteriorWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_SlabOnGrade);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_RaisedFloor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_Roof);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_gbXML_ExteriorWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DivisionProfile);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SplitterProfile);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeSegments);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GraphicalWarning_OpenConnector);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlaceHolderPipes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlaceHolderDucts);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipingSystem_Reference_Visibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipingSystem_Reference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctSystem_Reference_Visibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctSystem_Reference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeInsulationsTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctLiningsTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctInsulationsTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalInternalCircuits);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PanelScheduleGraphics);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayRun);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitRun);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ParamElemElectricalLoadClassification);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DataPanelScheduleTemplates);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SwitchboardScheduleTemplates);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BranchPanelScheduleTemplates);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitStandards);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalLoadClassifications);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalDemandFactorDefinitions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitFittingCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayFittingCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitRiseDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayRiseDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Conduit);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTray);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitFittingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConduitFitting);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayFittingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayFitting);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoutingPreferences);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctLinings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctInsulations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeInsulations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Load_Schedules);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Load_Building_Types);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Load_Space_Types);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_Reference_Visibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_InteriorFill_Visibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_ColorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ZoneTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LayoutPath_Bases);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireTemperatureRatings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireInsulations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireMaterials);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_Reference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_InteriorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones_Boundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Zones);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Fluids);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeSchedules);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeMaterials);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeConnections);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EAConstructions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SwitchSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SprinklerTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sprinklers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RouteCurveBranch);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RouteCurveMain);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RouteCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXML_Opening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXML_SType_Underground);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXML_SType_Shade);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXML_SType_Exterior);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXML_SType_Interior);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GbXMLFaces);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireHomeRunArrows);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FireAlarmDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FireAlarmDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DataDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DataDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CommunicationDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CommunicationDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SecurityDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SecurityDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_NurseCallDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_NurseCallDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TelephoneDeviceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TelephoneDevices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireTickMarks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeFittingInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeFittingCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeCurvesInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurvesInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurvesDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctFittingLining);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctFittingInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctFittingCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctCurvesInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesLining);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctFittingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeFittingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeColorFills);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeColorFillLegends);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WireTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeAccessoryTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeAccessory);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurvesRiseDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeCurvesPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeCurvesContour);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeCurvesCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeCurves);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeFitting);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexPipeTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurvesContour);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurvesCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeCurves);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipingSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalDemandFactor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElecDistributionSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalVoltage);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Wire);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalCircuitTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalCircuit);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesRiseDrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctCurvesPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctCurvesContour);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctCurvesCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctCurves);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctAccessoryTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctAccessory);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctTerminalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctTerminal);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctFitting);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctColorFills);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FlexDuctTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesContour);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurvesCenterLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctCurves);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DuctColorFillLegends);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConnectorElemZAxis);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConnectorElemYAxis);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConnectorElemXAxis);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ConnectorElem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeBearingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeGirderTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeFoundationTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeDeckTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeArchTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeCableTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeTowerTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgePierTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeAbutmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeBearingHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeGirderHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeFoundationHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeDeckHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeArchHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeCableHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeTowerHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgePierHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeAbutmentHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeBearings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeGirders);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeFoundations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeDecks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeArches);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeCables);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeTowers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgePiers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BridgeAbutments);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DesignOptions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DesignOptionSets);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralBracePlanReps);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructConnectionSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralAnnotations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RevisionCloudTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Revisions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RevisionClouds);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EditCutProfile);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElevationMarks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GridHeads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LevelHeads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DecalType);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DecalElement);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VolumeOfInterest);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BoundaryConditions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalAreaLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalLineLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalPointLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LineLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PointLoadTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesSeismic);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesTemperature);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesAccidental);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesRoofLive);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesSnow);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesWind);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesLive);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCasesDead);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LoadCases);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalAreaLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalLineLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalPointLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InternalLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LineLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PointLoads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Loads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamSystemTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FootingSpanDirectionSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpanDirectionSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotSlopesSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotCoordinateSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotElevSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralConnectionHandlerTags_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_KeynoteTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DetailComponentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MaterialTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtaSystemTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MultiCategoryTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlantingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundationTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumnTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ParkingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SiteTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModelTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanelTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlumbingFixtureTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixtureTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FurnitureSystemTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FurnitureTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalFixtureTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalEquipmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CaseworkTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Tags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceColorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceReference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceInteriorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceReferenceVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceInteriorFillVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StackedWalls);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassGlazingAll);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassFloorsAll);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassWallsAll);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassExteriorWallUnderground);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassSlab);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassShade);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassSkylights);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassGlazing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassRoof);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassExteriorWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassInteriorWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassZone);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassAreaFaceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostTemplate);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassFaceSplitter);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassCutter);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ZoningEnvelope);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassForm);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassFloor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Mass);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_DiscardedDivisionLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurfaceBelt);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TilePatterns);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AlwaysExcludedInAllViews);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_TransparentFace);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_PreDividedSurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_PatternFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_PatternLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_Gridlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface_Nodes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedSurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RepeatingDetailLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsDownArrow);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsUpArrow);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsDownText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsUpText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsStringerAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsStringer);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RampsIncomplete);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TrussDummy);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ZoneSchemes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaSchemes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Areas);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ProjectInformation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sheets);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ProfileFamilies);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DetailComponents);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofSoffit);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EdgeSlab);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Gutter);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Fascia);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Entourage);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Planting);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Blocks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffenerHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumnLocationLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingLocationLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffenerTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FootingAnalyticalGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RvtLinks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Automatic);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalyticalRigidLinks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SecondaryTopographyContours);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TopographyContours);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TopographySurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Topography);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TopographyLink);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumnStickSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenStructuralColumnLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalRigidLinks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalyticalGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FramingAnalyticalGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenStructuralFramingLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_KickerBracing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenStructuralFoundationLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BasePointAxisZ);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BasePointAxisY);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BasePointAxisX);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SharedBasePoint);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ProjectBasePoint);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SiteRegion);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SitePropertyLineSegmentTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SitePropertyLineSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SitePropertyTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SitePointBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SiteProperty);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BuildingPad);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SitePoint);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SiteSurface);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Site);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sewer);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Property);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Parking);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlumbingFixtures);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixtureSource);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixtures);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FurnitureSystems);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalFixtures);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ElectricalEquipment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Casework);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ArcWallRectOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DormerOpeningIncomplete);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SWallRectOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ShaftOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RiseDropSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PipeHydronicSeparationSymbols);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentSetBoundaryLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentSetTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentSet);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalPipeConnectionLineSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalPipeConnections);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Coordination_Model);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MultistoryStairs);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenStructuralConnectionLines_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralConnectionHandler_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CoordinateSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FndSlabLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BraceLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColumnLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BeamLocalCoordSys);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MultiReferenceAnnotations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_LeaderTickMarkStyleId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_InteriorTickMarkStyleId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_ArrowHeadStyleId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_CenterlineTickMarkStyleId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_CenterlinePatternCatId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_DimStyleHeavyEndCategoryId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_DimStyleHeavyEndCatId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_DimStyleTickCategoryId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_LineAndTextAttrFontId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DSR_LineAndTextAttrCategoryId);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_NodeAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LinkAnalyticalTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingRailPathExtensionLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingRailPathLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSupports);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingHandRailAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingTopRailAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingTermination);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSupport);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingHandRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingTopRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSketchPathLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsTriserNumbers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsTriserTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSupportTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsLandingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRunTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsPathsAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsPaths);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRiserLinesAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRiserLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsOutlinesAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsNosingLinesAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsNosingLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsCutMarksAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsCutMarks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ComponentRepeaterSlot);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ComponentRepeater);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DividedPath);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRoomCalculationPoint);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PropertySet);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AppearanceAsset);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairStringer2012_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsTrisers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsLandings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRuns);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Stair2012_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTransition);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTermination);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemTopRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHandRailBracket);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHandRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemHardware);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemPanel);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemBaluster);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemPost);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystemSegment);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AdaptivePoints_HiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AdaptivePoints_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AdaptivePoints_Planes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AdaptivePoints_Points);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AdaptivePoints);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallRefPlanes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructLocationLineControl);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DimLockControlLeader);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceSeparationLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaPolylines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomPolylines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InstanceDrivenLineStyle);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RemovedGridSeg);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSOpening);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSTilePatternGrid);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ControlLocal);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ControlAxisZ);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ControlAxisY);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ControlAxisX);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRayConstrainedProfileEdge);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRayImplicitPathCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRayPathPoint);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRayPathCurve);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRaySideEdge);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_XRayProfileEdge);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferencePoints_HiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferencePoints_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferencePoints_Planes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferencePoints_Points);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferencePoints);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Materials);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsCutPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsDefault);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsFinish2);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsFinish1);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsSubstrate);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsStructure);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsMembrane);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsInteriorEdges);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsCutPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenFloorLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsDefault);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsFinish2);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsFinish1);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsSubstrate);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsStructure);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsMembrane);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsInteriorEdges);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsCutPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsDefault);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsFinish2);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsFinish1);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsSubstrate);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsStructure);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsMembrane);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsCutPattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenWallLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsDefault);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsFinish2);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsFinish1);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsSubstrate);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsInsulation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsStructure);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsMembrane);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PreviewLegendComponents);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LegendComponents);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Schedules);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ScheduleGraphics);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RasterImages);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColorFillSchema);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomColorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ColorFillLegends);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnnotationCropSpecial);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CropBoundarySpecial);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnnotationCrop);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsAnalyticalGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsAnalyticalGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CalloutLeaderLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsSurfacePattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsSurfacePattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsSurfacePattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsSurfacePattern);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CalloutBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CalloutHeads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Callouts);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CropBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Elev);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AxisZ);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AxisY);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AxisX);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Lights);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ViewportLabel);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Viewports);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Camera_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Cameras);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionHeadWideLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionHeadMediumLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionHeadThinLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionHeads);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ContourLabels);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtaSystemFaceManager);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtaSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReport_Arc_Minus);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReport_Arc_Plus);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReport_Boundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReport_Triangle);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainGridsCurtaSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainGridsSystem);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainGridsWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainGridsRoof);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinHF);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinWall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinCeiling);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinRoof);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFinFloor);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HostFin);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalysisDisplayStyle);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AnalysisResults);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RenderRegions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionBox);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TextNotes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Divisions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Catalogs);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DirectionEdgeLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CenterLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_LinesBeyond);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DemolishedLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_OverheadLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TitleBlockWideLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TitleBlockMediumLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TitleBlockThinLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_TitleBlocks);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Views);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Viewers);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PartHiddenLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PartTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Parts);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Assemblies);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotSlopes);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotCoordinates);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SpotElevations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Constraints);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WeakDims);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Dimensions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Levels);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DisplacementPath);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DisplacementElements);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GridChains);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Grids);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_BrokenSectionLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SectionLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sections);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferenceViewer);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferenceViewerSymbol);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ImportObjectStyles);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ModelText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MaskingRegion);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Matchline);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FaceSplitter);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_PlanRegion);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FilledRegion);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassingProjectionOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MassingCutOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Massing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Reveals);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Cornices);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ramps);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingBalusterRailCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RailingBalusterRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Railings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainGrids);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallMullionsCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallMullions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaInteriorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomReference);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomInteriorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaColorFill);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaReferenceVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaInteriorFillVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomReferenceVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomInteriorFillVisibility);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GenericAnnotation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Fixtures);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailingTags);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailingAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsDownArrows);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsUpArrows);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsDownText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailingRail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailingBaluster);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsRailing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsUpText);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSupportsAboveCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsStringerCarriage);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsAboveCut_ToBeDeprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsIncomplete_Deprecated);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Stairs);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSNavWheelPivotBall);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRoomComputationHeight);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRoomUpperLowerLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSDragBoxInverted);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSDragBox);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Phases);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOS_GeoSite);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOS_GeoLocations);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSFabricReinSpanSymbolCtrl);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GuideGrid);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EPS_Future);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EPS_Temporary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EPS_New);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EPS_Demolished);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_EPS_Existing);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSMeasureLineScreenSize);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRebarSystemSpanSymbolCtrl);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRoomTagToRoomLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSAttachedDetailGroups);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSDetailGroups);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSModelGroups);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSuspendedSketch);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSWallCoreBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSMeasureLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSArrays);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Curtain_Systems);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSBBoxScreenSize);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSlabShapeEditorPointInterior);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSlabShapeEditorPointBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSlabShapeEditorBoundary);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSlabShapeEditorAutoCrease);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSlabShapeEditorExplitCrease);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ReferenceLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSNotSilhouette);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FillPatterns);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Furniture);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AreaSchemeLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GenericLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InsulationLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CloudLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRoomPerimeterLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSCuttingGeometry);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSCrashGraphics);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSGroups);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSGhost);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSketchLandingCenterLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSketchRunLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSketchRiserLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StairsSketchBoundaryLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoomSeparationLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_AxisOfRotation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_InvisibleLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSThinPixel_DashDot);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSThinPixel_Dash);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSThinPixel_Dot);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Extrusions);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOS);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CutOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSThinPixel);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSFlipControl);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSketchGrid);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSSuspendedSketch_obsolete);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSFreeSnapLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSDatumPlane);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Lines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSConstructionLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSAlignmentGraphics);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSAligningLine);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSBackedUpElements);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRegeneratedElements);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_SketchLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurvesWideLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurvesMediumLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurvesThinLines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Curves);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CeilingsCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_RoofsCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_FloorsCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsGlassProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsGlassCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsFrameMullionProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsFrameMullionCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsOpeningProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsOpeningCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsPanelProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_DoorsPanelCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsOpeningProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsOpeningCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsSillHeadProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsSillHeadCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsFrameMullionProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsFrameMullionCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsGlassProjection);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WindowsGlassCut);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsProjectionOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_WallsCutOutlines);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_IOSRegenerationFailure);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_ScheduleViewParamGroup);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchSiteComponent);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchProfile);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchDetail);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchAnnotation);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchModel);
            cats.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MatchAll);

            return cats;
        }

        private ContextMenu TableContextMenu()
        {
            ContextMenu mnu = new ContextMenu();

            MenuItem cxmnuCopyElementId = new MenuItem("Copy Element Id");
            MenuItem cxmnuCopyGUID = new MenuItem("Copy GUID");
            MenuItem cxmnuCopyName = new MenuItem("Copy Parameter Name");

            cxmnuCopyElementId.Click += new EventHandler(cxmnuCopyElementId_Click);
            cxmnuCopyGUID.Click += new EventHandler(cxmnuCopyGUID_Click);
            cxmnuCopyName.Click += new EventHandler(cxmnuCopyName_Click);

            mnu.MenuItems.Add(cxmnuCopyElementId);
            mnu.MenuItems.Add(cxmnuCopyGUID);
            mnu.MenuItems.Add(cxmnuCopyName);

            return mnu;
        }

        static bool FamilyFirstSymbolCategoryEquals(Family f, BuiltInCategory bic)
        {
            Document doc = f.Document;

            ISet<ElementId> ids = f.GetFamilySymbolIds();

            Category cat = (0 == ids.Count)
              ? null
              : doc.GetElement(ids.First<ElementId>()).Category;

            return null != cat
              && cat.Id.IntegerValue.Equals((int)bic);
        }

        static IEnumerable<Family> GetFamiliesOfCategory(Document doc, BuiltInCategory bic)
        {
            IEnumerable<Family> families = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where<Family>(f =>
                 FamilyFirstSymbolCategoryEquals(f, bic));

            return families;
        }

        #endregion
                          
        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();

            sd.Title = "Specify file to save";
            sd.InitialDirectory = "C:\\";

            if (rbProjectParameters.Checked)
                sd.FileName = "project_parameters.txt";
            else if(rbTypeParameters.Checked)
                sd.FileName = "type_parameters.txt";
            else if (rbInstanceParameters.Checked)
                sd.FileName = "instance_parameters.txt";

            if (sd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sd.FileName);

                foreach (DataGridViewRow row in dgvParameters.Rows)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        sb.Append(cell.Value + "|");
                    }

                    string line = sb.ToString();
                    string entry = line.Remove(line.Length - 2, 1);

                    sw.WriteLine(entry);
                    sb = null;
                }
                sw.Close();
            }
        }

        public static class DrawingControl
        {
            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr _hWnd, Int32 _wMsg, bool _wParam, Int32 _lParam);

            private const int WM_SETREDRAW = 11;

            public static void SetDoubleBuffered(System.Windows.Forms.Control _ctrl)
            {
                if (!SystemInformation.TerminalServerSession)
                {
                    typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered", (System.Reflection.BindingFlags.SetProperty
                                    | (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)), null, _ctrl, new object[] {
                            true});
                }
            }

            public static void SetDoubleBuffered_ListControls(List<System.Windows.Forms.Control> _ctrlList)
            {
                if (!SystemInformation.TerminalServerSession)
                {
                    foreach (System.Windows.Forms.Control ctrl in _ctrlList)
                    {
                        typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered", (System.Reflection.BindingFlags.SetProperty
                                        | (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)), null, ctrl, new object[] {
                                true});
                    }
                }
            }

            public static void SuspendDrawing(System.Windows.Forms.Control _ctrl)
            {
                SendMessage(_ctrl.Handle, WM_SETREDRAW, false, 0);
            }

            public static void SuspendDrawing_ListControls(List<System.Windows.Forms.Control> _ctrlList)
            {
                foreach (System.Windows.Forms.Control ctrl in _ctrlList)
                {
                    SendMessage(ctrl.Handle, WM_SETREDRAW, false, 0);
                }
            }

            public static void ResumeDrawing(System.Windows.Forms.Control _ctrl)
            {
                SendMessage(_ctrl.Handle, WM_SETREDRAW, true, 0);
                _ctrl.Refresh();
            }

            public static void ResumeDrawing_ListControls(List<System.Windows.Forms.Control> _ctrlList)
            {
                foreach (System.Windows.Forms.Control ctrl in _ctrlList)
                {
                    SendMessage(ctrl.Handle, WM_SETREDRAW, true, 0);
                    ctrl.Refresh();
                }
            }
        }

        private void RadioButtonChecked(object sender, EventArgs e)
        {
            LoadParameters();
        }
    }
}