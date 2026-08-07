using System.Text.Encodings.Web;
using System.Text.Json;
using AI_CAD_ENGINEER.InventorControl.Commands;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl;

public class InventorCommandDispatcher
{
    private readonly PingCommand
        _pingCommand;

    private readonly GetActiveDocumentCommand
        _getActiveDocumentCommand;

    private readonly UpdateActiveDocumentCommand
        _updateActiveDocumentCommand;

    private readonly GetDrawingViewsCommand
        _getDrawingViewsCommand;

    private readonly MoveDrawingViewCommand
        _moveDrawingViewCommand;

    private readonly SetDrawingViewScaleCommand
        _setDrawingViewScaleCommand;

    private readonly GetDrawingDimensionsCommand
        _getDrawingDimensionsCommand;

    private readonly MoveDrawingDimensionCommand
        _moveDrawingDimensionCommand;

    private readonly DeleteDrawingDimensionCommand
        _deleteDrawingDimensionCommand;

    private readonly DeleteDrawingViewCommand
        _deleteDrawingViewCommand;

    private readonly CreateBaseViewCommand
        _createBaseViewCommand;

    private readonly CreateProjectedViewCommand
        _createProjectedViewCommand;

    private readonly GetDrawingViewCommand
        _getDrawingViewCommand;

    private readonly RenameDrawingViewCommand
        _renameDrawingViewCommand;

    private readonly RotateDrawingViewCommand
        _rotateDrawingViewCommand;

    private readonly SetDrawingViewSuppressedCommand
        _setDrawingViewSuppressedCommand;

    private readonly SetDrawingViewStyleCommand
        _setDrawingViewStyleCommand;

    private readonly SetDrawingViewLabelVisibilityCommand
        _setDrawingViewLabelVisibilityCommand;

    private readonly SetDrawingViewScaleInheritanceCommand
        _setDrawingViewScaleInheritanceCommand;

    private readonly SetDrawingViewAlignmentCommand
        _setDrawingViewAlignmentCommand;

    private readonly GetOpenDocumentsCommand
        _getOpenDocumentsCommand;

    private readonly OpenDocumentCommand
        _openDocumentCommand;

    private readonly ActivateDocumentCommand
        _activateDocumentCommand;

    private readonly SaveDocumentCommand
        _saveDocumentCommand;

    private readonly SaveDocumentAsCommand
        _saveDocumentAsCommand;

    private readonly CloseDocumentCommand
        _closeDocumentCommand;

    private readonly GetSheetsCommand
        _getSheetsCommand;

    private readonly GetSheetCommand
        _getSheetCommand;

    private readonly ActivateSheetCommand
        _activateSheetCommand;

    private readonly RenameSheetCommand
        _renameSheetCommand;

    private readonly CreateSheetCommand
        _createSheetCommand;

    private readonly DeleteSheetCommand
        _deleteSheetCommand;

    private readonly SetSheetSizeCommand
        _setSheetSizeCommand;

    private readonly SetSheetOrientationCommand
        _setSheetOrientationCommand;

    private readonly GetBorderDefinitionsCommand
        _getBorderDefinitionsCommand;

    private readonly GetSheetBorderCommand
        _getSheetBorderCommand;

    private readonly GetTitleBlockDefinitionsCommand
        _getTitleBlockDefinitionsCommand;

    private readonly GetSheetTitleBlockCommand
        _getSheetTitleBlockCommand;

    private readonly SetSheetBorderCommand
        _setSheetBorderCommand;

    private readonly RemoveSheetBorderCommand
        _removeSheetBorderCommand;

    private readonly SetSheetTitleBlockCommand
        _setSheetTitleBlockCommand;

    private readonly RemoveSheetTitleBlockCommand
        _removeSheetTitleBlockCommand;

    private readonly GetDocumentPropertiesCommand
        _getDocumentPropertiesCommand;

    private readonly SetDocumentPropertyCommand
        _setDocumentPropertyCommand;

    private readonly GetTitleBlockFieldsCommand
        _getTitleBlockFieldsCommand;

    private readonly SetTitleBlockFieldCommand
        _setTitleBlockFieldCommand;

    private readonly GetDocumentPropertySetsCommand
        _getDocumentPropertySetsCommand;

    private readonly GetDocumentPropertyCommand
        _getDocumentPropertyCommand;

    private readonly GetDocumentPropertyByIdCommand
        _getDocumentPropertyByIdCommand;

    private readonly SetDocumentPropertyByIdCommand
        _setDocumentPropertyByIdCommand;

    private readonly GetTitleBlockBindingsCommand
        _getTitleBlockBindingsCommand;

    private readonly GetTitleBlockBindingCommand
        _getTitleBlockBindingCommand;

    private readonly GetGostMetadataCommand
        _getGostMetadataCommand;

    private readonly GetApplicationAddInsCommand
        _getApplicationAddInsCommand;

    private readonly GetTitleBlockDefinitionTextCommand
        _getTitleBlockDefinitionTextCommand;

    private readonly SetTitleBlockDefinitionTextCommand
        _setTitleBlockDefinitionTextCommand;

    private readonly GetTitleBlockFieldMapCommand
        _getTitleBlockFieldMapCommand;

    private readonly SetTitleBlockFieldByNameCommand
        _setTitleBlockFieldByNameCommand;

    private readonly FillTitleBlockCommand
        _fillTitleBlockCommand;

    private readonly GetDrawingCurvesCommand
        _getDrawingCurvesCommand;

    private readonly CreateLinearDimensionCommand
        _createLinearDimensionCommand;

    private readonly CreateDiameterDimensionCommand
        _createDiameterDimensionCommand;

    private readonly CreateRadiusDimensionCommand
        _createRadiusDimensionCommand;

    private readonly GetHoleThreadNotesCommand
        _getHoleThreadNotesCommand;

    private readonly CreateHoleThreadNoteCommand
        _createHoleThreadNoteCommand;

    private readonly MoveHoleThreadNoteCommand
        _moveHoleThreadNoteCommand;

    private readonly DeleteHoleThreadNoteCommand
        _deleteHoleThreadNoteCommand;

    private readonly SetHoleThreadNoteFormatCommand
        _setHoleThreadNoteFormatCommand;

    private readonly GetGeneralNotesCommand
        _getGeneralNotesCommand;

    private readonly GetLeaderNotesCommand
        _getLeaderNotesCommand;

    private readonly GetDrawingTextObjectsCommand
        _getDrawingTextObjectsCommand;

    private readonly GetFeatureControlFramesCommand
        _getFeatureControlFramesCommand;

    private readonly GetSurfaceTextureSymbolsCommand
        _getSurfaceTextureSymbolsCommand;

    private readonly GetWeldingSymbolsCommand
        _getWeldingSymbolsCommand;

    private readonly GetBalloonsCommand
        _getBalloonsCommand;

    private readonly GetCenterMarksCommand
        _getCenterMarksCommand;

    private readonly GetCenterlinesCommand
        _getCenterlinesCommand;

    private readonly GetGeneralDimensionsDetailedCommand
        _getGeneralDimensionsDetailedCommand;

    private readonly MoveGeneralDimensionTextCommand
        _moveGeneralDimensionTextCommand;

    private readonly CenterGeneralDimensionTextCommand
        _centerGeneralDimensionTextCommand;

    private readonly GetDimensionGeometryCommand
        _getDimensionGeometryCommand;

    private readonly MoveLinearDimensionCommand
        _moveLinearDimensionCommand;

    private readonly DeleteGeneralDimensionCommand
        _deleteGeneralDimensionCommand;

    private readonly AnalyzeDimensionLayoutCommand
        _analyzeDimensionLayoutCommand;

    private readonly AutoArrangeDimensionsCommand
        _autoArrangeDimensionsCommand;

    private readonly GetAnnotationBoundsCommand
        _getAnnotationBoundsCommand;

    private readonly CheckAnnotationCollisionsCommand
        _checkAnnotationCollisionsCommand;

    private readonly AutoResolveAnnotationCollisionsCommand
        _autoResolveAnnotationCollisionsCommand;

    private readonly AnalyzeViewDimensionCandidatesCommand
        _analyzeViewDimensionCandidatesCommand;

    private readonly GetCurveModelReferenceCommand
        _getCurveModelReferenceCommand;

    private readonly GetViewModelReferencesCommand
        _getViewModelReferencesCommand;

    private readonly GetModelFeatureTreeCommand
        _getModelFeatureTreeCommand;

    private readonly GetHoleFeaturesCommand
        _getHoleFeaturesCommand;

    private readonly GetFeatureDetailsCommand
        _getFeatureDetailsCommand;

    private readonly GetModelParametersCommand
        _getModelParametersCommand;

    private readonly GetSurfaceBodiesCommand
        _getSurfaceBodiesCommand;

    private readonly GetBodyFacesCommand
        _getBodyFacesCommand;

    private readonly GetFaceEdgesCommand
        _getFaceEdgesCommand;

    private readonly GetSketchesCommand
        _getSketchesCommand;

    private readonly GetSketchGeometryCommand
        _getSketchGeometryCommand;

    private readonly GetSketchConstraintsCommand
        _getSketchConstraintsCommand;

    private readonly GetSketchDimensionsCommand
        _getSketchDimensionsCommand;

    private readonly GetWorkFeaturesCommand
        _getWorkFeaturesCommand;

    private readonly GetAssemblySummaryCommand
        _getAssemblySummaryCommand;

    private readonly GetAssemblyOccurrencesCommand
        _getAssemblyOccurrencesCommand;

    private readonly GetAssemblyConstraintsCommand
        _getAssemblyConstraintsCommand;

    private readonly GetAssemblyBomCommand
        _getAssemblyBomCommand;

    private readonly GetAssemblyReferencedDocumentsCommand
        _getAssemblyReferencedDocumentsCommand;

    private readonly GetDrawingSheetsCommand
        _getDrawingSheetsCommand;

    private readonly GetDrawingViewsDetailedCommand
        _getDrawingViewsDetailedCommand;

    private readonly GetDrawingViewRelationshipsCommand
        _getDrawingViewRelationshipsCommand;

    private readonly GetDrawingAnnotationSummaryCommand
        _getDrawingAnnotationSummaryCommand;

    private readonly GetDrawingTablesCommand
        _getDrawingTablesCommand;

    private readonly GetDrawingTableCollectionsCommand
        _getDrawingTableCollectionsCommand;

    private readonly GetCustomTablesCommand
        _getCustomTablesCommand;

    private readonly GetPartsListsCommand
        _getPartsListsCommand;

    private readonly GetRevisionTablesCommand
        _getRevisionTablesCommand;

    public InventorCommandDispatcher(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _pingCommand =
            new PingCommand();

        _getActiveDocumentCommand =
            new GetActiveDocumentCommand(
                inventor);

        _updateActiveDocumentCommand =
            new UpdateActiveDocumentCommand(
                inventor);

        _getDrawingViewsCommand =
            new GetDrawingViewsCommand(
                inventor);

        _moveDrawingViewCommand =
            new MoveDrawingViewCommand(
                inventor);

        _setDrawingViewScaleCommand =
            new SetDrawingViewScaleCommand(
                inventor);

        _getDrawingDimensionsCommand =
            new GetDrawingDimensionsCommand(
                inventor);

        _moveDrawingDimensionCommand =
            new MoveDrawingDimensionCommand(
                inventor);

        _deleteDrawingDimensionCommand =
            new DeleteDrawingDimensionCommand(
                inventor);

        _deleteDrawingViewCommand =
            new DeleteDrawingViewCommand(
                inventor);

        _createBaseViewCommand =
            new CreateBaseViewCommand(
                inventor);

        _createProjectedViewCommand =
            new CreateProjectedViewCommand(
                inventor);

        _getDrawingViewCommand =
            new GetDrawingViewCommand(
                inventor);

        _renameDrawingViewCommand =
            new RenameDrawingViewCommand(
                inventor);

        _rotateDrawingViewCommand =
            new RotateDrawingViewCommand(
                inventor);

        _setDrawingViewSuppressedCommand =
            new SetDrawingViewSuppressedCommand(
                inventor);

        _setDrawingViewStyleCommand =
            new SetDrawingViewStyleCommand(
                inventor);

        _setDrawingViewLabelVisibilityCommand =
            new SetDrawingViewLabelVisibilityCommand(
                inventor);

        _setDrawingViewScaleInheritanceCommand =
            new SetDrawingViewScaleInheritanceCommand(
                inventor);

        _setDrawingViewAlignmentCommand =
            new SetDrawingViewAlignmentCommand(
                inventor);

        _getOpenDocumentsCommand =
            new GetOpenDocumentsCommand(
                inventor);

        _openDocumentCommand =
            new OpenDocumentCommand(
                inventor);

        _activateDocumentCommand =
            new ActivateDocumentCommand(
                inventor);

        _saveDocumentCommand =
            new SaveDocumentCommand(
                inventor);

        _saveDocumentAsCommand =
            new SaveDocumentAsCommand(
                inventor);

        _closeDocumentCommand =
            new CloseDocumentCommand(
                inventor);

        _getSheetsCommand =
            new GetSheetsCommand(
                inventor);

        _getSheetCommand =
            new GetSheetCommand(
                inventor);

        _activateSheetCommand =
            new ActivateSheetCommand(
                inventor);

        _renameSheetCommand =
            new RenameSheetCommand(
                inventor);

        _createSheetCommand =
            new CreateSheetCommand(
                inventor);

        _deleteSheetCommand =
            new DeleteSheetCommand(
                inventor);

        _setSheetSizeCommand =
            new SetSheetSizeCommand(
                inventor);

        _setSheetOrientationCommand =
            new SetSheetOrientationCommand(
                inventor);

        _getBorderDefinitionsCommand =
            new GetBorderDefinitionsCommand(
                inventor);

        _getSheetBorderCommand =
            new GetSheetBorderCommand(
                inventor);

        _getTitleBlockDefinitionsCommand =
            new GetTitleBlockDefinitionsCommand(
                inventor);

        _getSheetTitleBlockCommand =
            new GetSheetTitleBlockCommand(
                inventor);

        _setSheetBorderCommand =
            new SetSheetBorderCommand(
                inventor);

        _removeSheetBorderCommand =
            new RemoveSheetBorderCommand(
                inventor);

        _setSheetTitleBlockCommand =
            new SetSheetTitleBlockCommand(
                inventor);

        _removeSheetTitleBlockCommand =
            new RemoveSheetTitleBlockCommand(
                inventor);

        _getDocumentPropertiesCommand =
            new GetDocumentPropertiesCommand(
                inventor);

        _setDocumentPropertyCommand =
            new SetDocumentPropertyCommand(
                inventor);

        _getTitleBlockFieldsCommand =
            new GetTitleBlockFieldsCommand(
                inventor);

        _setTitleBlockFieldCommand =
            new SetTitleBlockFieldCommand(
                inventor);

        _getDocumentPropertySetsCommand =
            new GetDocumentPropertySetsCommand(
                inventor);

        _getDocumentPropertyCommand =
            new GetDocumentPropertyCommand(
                inventor);

        _getDocumentPropertyByIdCommand =
            new GetDocumentPropertyByIdCommand(
                inventor);

        _setDocumentPropertyByIdCommand =
            new SetDocumentPropertyByIdCommand(
                inventor);

        _getTitleBlockBindingsCommand =
            new GetTitleBlockBindingsCommand(
                inventor);

        _getTitleBlockBindingCommand =
            new GetTitleBlockBindingCommand(
                inventor);

        _getGostMetadataCommand =
            new GetGostMetadataCommand(
                inventor);

        _getApplicationAddInsCommand =
            new GetApplicationAddInsCommand(
                inventor);

        _getTitleBlockDefinitionTextCommand =
            new GetTitleBlockDefinitionTextCommand(
                inventor);

        _setTitleBlockDefinitionTextCommand =
            new SetTitleBlockDefinitionTextCommand(
                inventor);

        _getTitleBlockFieldMapCommand =
            new GetTitleBlockFieldMapCommand(
                inventor);

        _setTitleBlockFieldByNameCommand =
            new SetTitleBlockFieldByNameCommand(
                inventor);

        _fillTitleBlockCommand =
            new FillTitleBlockCommand(
                inventor);

        _getDrawingCurvesCommand =
            new GetDrawingCurvesCommand(
                inventor);

        _createLinearDimensionCommand =
            new CreateLinearDimensionCommand(
                inventor);

        _createDiameterDimensionCommand =
            new CreateDiameterDimensionCommand(
                inventor);

        _createRadiusDimensionCommand =
            new CreateRadiusDimensionCommand(
                inventor);

        _getHoleThreadNotesCommand =
            new GetHoleThreadNotesCommand(
                inventor);

        _createHoleThreadNoteCommand =
            new CreateHoleThreadNoteCommand(
                inventor);

        _moveHoleThreadNoteCommand =
            new MoveHoleThreadNoteCommand(
                inventor);

        _deleteHoleThreadNoteCommand =
            new DeleteHoleThreadNoteCommand(
                inventor);

        _setHoleThreadNoteFormatCommand =
            new SetHoleThreadNoteFormatCommand(
                inventor);

        _getGeneralNotesCommand =
            new GetGeneralNotesCommand(
                inventor);

        _getLeaderNotesCommand =
            new GetLeaderNotesCommand(
                inventor);

        _getDrawingTextObjectsCommand =
            new GetDrawingTextObjectsCommand(
                inventor);

        _getFeatureControlFramesCommand =
            new GetFeatureControlFramesCommand(
                inventor);

        _getSurfaceTextureSymbolsCommand =
            new GetSurfaceTextureSymbolsCommand(
                inventor);

        _getWeldingSymbolsCommand =
            new GetWeldingSymbolsCommand(
                inventor);

        _getBalloonsCommand =
            new GetBalloonsCommand(
                inventor);

        _getCenterMarksCommand =
            new GetCenterMarksCommand(
                inventor);

        _getCenterlinesCommand =
            new GetCenterlinesCommand(
                inventor);

        _getGeneralDimensionsDetailedCommand =
            new GetGeneralDimensionsDetailedCommand(
                inventor);

        _moveGeneralDimensionTextCommand =
            new MoveGeneralDimensionTextCommand(
                inventor);

        _centerGeneralDimensionTextCommand =
            new CenterGeneralDimensionTextCommand(
                inventor);

        _getDimensionGeometryCommand =
            new GetDimensionGeometryCommand(
                inventor);

        _moveLinearDimensionCommand =
            new MoveLinearDimensionCommand(
                inventor);

        _deleteGeneralDimensionCommand =
            new DeleteGeneralDimensionCommand(
                inventor);

        _analyzeDimensionLayoutCommand =
            new AnalyzeDimensionLayoutCommand(
                inventor);

        _autoArrangeDimensionsCommand =
            new AutoArrangeDimensionsCommand(
                inventor);

        _getAnnotationBoundsCommand =
            new GetAnnotationBoundsCommand(
                inventor);

        _checkAnnotationCollisionsCommand =
            new CheckAnnotationCollisionsCommand(
                inventor);

        _autoResolveAnnotationCollisionsCommand =
            new AutoResolveAnnotationCollisionsCommand(
                inventor);

        _analyzeViewDimensionCandidatesCommand =
            new AnalyzeViewDimensionCandidatesCommand(
                inventor);

        _getCurveModelReferenceCommand =
            new GetCurveModelReferenceCommand(
                inventor);

        _getViewModelReferencesCommand =
            new GetViewModelReferencesCommand(
                inventor);

        _getModelFeatureTreeCommand =
            new GetModelFeatureTreeCommand(
                inventor);

        _getHoleFeaturesCommand =
            new GetHoleFeaturesCommand(
                inventor);

        _getFeatureDetailsCommand =
            new GetFeatureDetailsCommand(
                inventor);

        _getModelParametersCommand =
            new GetModelParametersCommand(
                inventor);

        _getSurfaceBodiesCommand =
            new GetSurfaceBodiesCommand(
                inventor);

        _getBodyFacesCommand =
            new GetBodyFacesCommand(
                inventor);

        _getFaceEdgesCommand =
            new GetFaceEdgesCommand(
                inventor);

        _getSketchesCommand =
            new GetSketchesCommand(
                inventor);

        _getSketchGeometryCommand =
            new GetSketchGeometryCommand(
                inventor);

        _getSketchConstraintsCommand =
            new GetSketchConstraintsCommand(
                inventor);

        _getSketchDimensionsCommand =
            new GetSketchDimensionsCommand(
                inventor);

        _getWorkFeaturesCommand =
            new GetWorkFeaturesCommand(
                inventor);

        _getAssemblySummaryCommand =
            new GetAssemblySummaryCommand(
                inventor);

        _getAssemblyOccurrencesCommand =
            new GetAssemblyOccurrencesCommand(
                inventor);

        _getAssemblyConstraintsCommand =
            new GetAssemblyConstraintsCommand(
                inventor);

        _getAssemblyBomCommand =
            new GetAssemblyBomCommand(
                inventor);

        _getAssemblyReferencedDocumentsCommand =
            new GetAssemblyReferencedDocumentsCommand(
                inventor);

        _getDrawingSheetsCommand =
            new GetDrawingSheetsCommand(
                inventor);

        _getDrawingViewsDetailedCommand =
            new GetDrawingViewsDetailedCommand(
                inventor);

        _getDrawingViewRelationshipsCommand =
            new GetDrawingViewRelationshipsCommand(
                inventor);

        _getDrawingAnnotationSummaryCommand =
            new GetDrawingAnnotationSummaryCommand(
                inventor);

        _getDrawingTablesCommand =
            new GetDrawingTablesCommand(
                inventor);

        _getDrawingTableCollectionsCommand =
            new GetDrawingTableCollectionsCommand(
                inventor);

        _getCustomTablesCommand =
            new GetCustomTablesCommand(
                inventor);

        _getPartsListsCommand =
            new GetPartsListsCommand(
                inventor);

        _getRevisionTablesCommand =
            new GetRevisionTablesCommand(
                inventor);
    }

    public string Execute(
        string commandJson)
    {
        if (string.IsNullOrWhiteSpace(
                commandJson))
        {
            return CreateError(
                "Команда не передана.");
        }

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(
                    commandJson);

            JsonElement root =
                document.RootElement;

            if (!root.TryGetProperty(
                    "command",
                    out JsonElement commandElement))
            {
                return CreateError(
                    "Не найдено поле command.");
            }

            string command =
                commandElement
                    .GetString()?
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            return command switch
            {
                "ping" =>
                    _pingCommand.Execute(
                        root),

                "get_active_document" =>
                    _getActiveDocumentCommand.Execute(
                        root),

                "update_active_document" =>
                    _updateActiveDocumentCommand.Execute(
                        root),

                "get_drawing_views" =>
                    _getDrawingViewsCommand.Execute(
                        root),

                "move_drawing_view" =>
                    _moveDrawingViewCommand.Execute(
                        root),

                "set_drawing_view_scale" =>
                    _setDrawingViewScaleCommand.Execute(
                        root),

                "get_drawing_dimensions" =>
                    _getDrawingDimensionsCommand.Execute(
                        root),

                "move_drawing_dimension" =>
                    _moveDrawingDimensionCommand.Execute(
                        root),

                "delete_drawing_dimension" =>
                    _deleteDrawingDimensionCommand.Execute(
                        root),

                "delete_drawing_view" =>
                    _deleteDrawingViewCommand.Execute(
                        root),

                "create_base_view" =>
                    _createBaseViewCommand.Execute(
                        root),

                "create_projected_view" =>
                    _createProjectedViewCommand.Execute(
                        root),

                "get_drawing_view" =>
                    _getDrawingViewCommand.Execute(
                        root),

                "rename_drawing_view" =>
                    _renameDrawingViewCommand.Execute(
                        root),

                "rotate_drawing_view" =>
                    _rotateDrawingViewCommand.Execute(
                        root),

                "set_drawing_view_suppressed" =>
                    _setDrawingViewSuppressedCommand.Execute(
                        root),

                "set_drawing_view_style" =>
                    _setDrawingViewStyleCommand.Execute(
                        root),

                "set_drawing_view_label_visibility" =>
                    _setDrawingViewLabelVisibilityCommand.Execute(
                        root),

                "set_drawing_view_scale_inheritance" =>
                    _setDrawingViewScaleInheritanceCommand.Execute(
                        root),

                "set_drawing_view_alignment" =>
                    _setDrawingViewAlignmentCommand.Execute(
                        root),

                "get_open_documents" =>
                    _getOpenDocumentsCommand.Execute(
                        root),

                "open_document" =>
                    _openDocumentCommand.Execute(
                        root),

                "activate_document" =>
                    _activateDocumentCommand.Execute(
                        root),

                "save_document" =>
                    _saveDocumentCommand.Execute(
                        root),

                "save_document_as" =>
                    _saveDocumentAsCommand.Execute(
                        root),

                "close_document" =>
                    _closeDocumentCommand.Execute(
                        root),

                "get_sheets" =>
                    _getSheetsCommand.Execute(
                        root),

                "get_sheet" =>
                    _getSheetCommand.Execute(
                        root),

                "activate_sheet" =>
                    _activateSheetCommand.Execute(
                        root),

                "rename_sheet" =>
                    _renameSheetCommand.Execute(
                        root),

                "create_sheet" =>
                    _createSheetCommand.Execute(
                        root),

                "delete_sheet" =>
                    _deleteSheetCommand.Execute(
                        root),

                "set_sheet_size" =>
                    _setSheetSizeCommand.Execute(
                        root),

                "set_sheet_orientation" =>
                    _setSheetOrientationCommand.Execute(
                        root),

                "get_border_definitions" =>
                    _getBorderDefinitionsCommand.Execute(
                        root),

                "get_sheet_border" =>
                    _getSheetBorderCommand.Execute(
                        root),

                "get_title_block_definitions" =>
                    _getTitleBlockDefinitionsCommand.Execute(
                        root),

                "get_sheet_title_block" =>
                    _getSheetTitleBlockCommand.Execute(
                        root),

                "set_sheet_border" =>
                    _setSheetBorderCommand.Execute(
                        root),

                "remove_sheet_border" =>
                    _removeSheetBorderCommand.Execute(
                        root),

                "set_sheet_title_block" =>
                    _setSheetTitleBlockCommand.Execute(
                        root),

                "remove_sheet_title_block" =>
                    _removeSheetTitleBlockCommand.Execute(
                        root),

                "get_document_properties" =>
                    _getDocumentPropertiesCommand.Execute(
                        root),

                "set_document_property" =>
                    _setDocumentPropertyCommand.Execute(
                        root),

                "get_title_block_fields" =>
                    _getTitleBlockFieldsCommand.Execute(
                        root),

                "set_title_block_field" =>
                    _setTitleBlockFieldCommand.Execute(
                        root),

                "get_document_property_sets" =>
                    _getDocumentPropertySetsCommand.Execute(
                        root),

                "get_document_property" =>
                    _getDocumentPropertyCommand.Execute(
                        root),

                "get_document_property_by_id" =>
                    _getDocumentPropertyByIdCommand.Execute(
                        root),

                "set_document_property_by_id" =>
                    _setDocumentPropertyByIdCommand.Execute(
                        root),

                "get_title_block_bindings" =>
                    _getTitleBlockBindingsCommand.Execute(
                        root),

                "get_title_block_binding" =>
                    _getTitleBlockBindingCommand.Execute(
                        root),

                "get_gost_metadata" =>
                    _getGostMetadataCommand.Execute(
                        root),

                "get_application_addins" =>
                    _getApplicationAddInsCommand.Execute(
                        root),

                "get_title_block_definition_text" =>
                    _getTitleBlockDefinitionTextCommand.Execute(
                        root),

                "set_title_block_definition_text" =>
                    _setTitleBlockDefinitionTextCommand.Execute(
                        root),

                "get_title_block_field_map" =>
                    _getTitleBlockFieldMapCommand.Execute(
                        root),

                "set_title_block_field_by_name" =>
                    _setTitleBlockFieldByNameCommand.Execute(
                        root),

                "fill_title_block" =>
                    _fillTitleBlockCommand.Execute(
                        root),

                "get_drawing_curves" =>
                    _getDrawingCurvesCommand.Execute(
                        root),

                "create_linear_dimension" =>
                    _createLinearDimensionCommand.Execute(
                        root),

                "create_diameter_dimension" =>
                    _createDiameterDimensionCommand.Execute(
                        root),

                "create_radius_dimension" =>
                    _createRadiusDimensionCommand.Execute(
                        root),

                "get_hole_thread_notes" =>
                    _getHoleThreadNotesCommand.Execute(
                        root),

                "create_hole_thread_note" =>
                    _createHoleThreadNoteCommand.Execute(
                        root),

                "move_hole_thread_note" =>
                    _moveHoleThreadNoteCommand.Execute(
                        root),

                "delete_hole_thread_note" =>
                    _deleteHoleThreadNoteCommand.Execute(
                        root),

                "set_hole_thread_note_format" =>
                    _setHoleThreadNoteFormatCommand.Execute(
                        root),

                "get_general_notes" =>
                    _getGeneralNotesCommand.Execute(
                        root),

                "get_leader_notes" =>
                    _getLeaderNotesCommand.Execute(
                        root),

                "get_drawing_text_objects" =>
                    _getDrawingTextObjectsCommand.Execute(
                        root),

                "get_feature_control_frames" =>
                    _getFeatureControlFramesCommand.Execute(
                        root),

                "get_surface_texture_symbols" =>
                    _getSurfaceTextureSymbolsCommand.Execute(
                        root),

                "get_welding_symbols" =>
                    _getWeldingSymbolsCommand.Execute(
                        root),

                "get_balloons" =>
                    _getBalloonsCommand.Execute(
                        root),

                "get_center_marks" =>
                    _getCenterMarksCommand.Execute(
                        root),

                "get_centerlines" =>
                    _getCenterlinesCommand.Execute(
                        root),

                "get_general_dimensions_detailed" =>
                    _getGeneralDimensionsDetailedCommand.Execute(
                        root),

                "move_general_dimension_text" =>
                    _moveGeneralDimensionTextCommand.Execute(
                        root),

                "center_general_dimension_text" =>
                    _centerGeneralDimensionTextCommand.Execute(
                        root),

                "get_dimension_geometry" =>
                    _getDimensionGeometryCommand.Execute(
                        root),

                "move_linear_dimension" =>
                    _moveLinearDimensionCommand.Execute(
                        root),

                "delete_general_dimension" =>
                    _deleteGeneralDimensionCommand.Execute(
                        root),

                "analyze_dimension_layout" =>
                    _analyzeDimensionLayoutCommand.Execute(
                        root),

                "auto_arrange_dimensions" =>
                    _autoArrangeDimensionsCommand.Execute(
                        root),

                "get_annotation_bounds" =>
                    _getAnnotationBoundsCommand.Execute(
                        root),

                "check_annotation_collisions" =>
                    _checkAnnotationCollisionsCommand.Execute(
                        root),

                "auto_resolve_annotation_collisions" =>
                    _autoResolveAnnotationCollisionsCommand.Execute(
                        root),

                "analyze_view_dimension_candidates" =>
                    _analyzeViewDimensionCandidatesCommand.Execute(
                        root),

                "get_curve_model_reference" =>
                    _getCurveModelReferenceCommand.Execute(
                        root),

                "get_view_model_references" =>
                    _getViewModelReferencesCommand.Execute(
                        root),

                "get_model_feature_tree" =>
                    _getModelFeatureTreeCommand.Execute(
                        root),

                "get_hole_features" =>
                    _getHoleFeaturesCommand.Execute(
                        root),

                "get_feature_details" =>
                    _getFeatureDetailsCommand.Execute(
                        root),

                "get_model_parameters" =>
                    _getModelParametersCommand.Execute(
                        root),

                "get_surface_bodies" =>
                    _getSurfaceBodiesCommand.Execute(
                        root),

                "get_body_faces" =>
                    _getBodyFacesCommand.Execute(
                        root),

                "get_face_edges" =>
                    _getFaceEdgesCommand.Execute(
                        root),

                "get_sketches" =>
                    _getSketchesCommand.Execute(
                        root),

                "get_sketch_geometry" =>
                    _getSketchGeometryCommand.Execute(
                        root),

                "get_sketch_constraints" =>
                    _getSketchConstraintsCommand.Execute(
                        root),

                "get_sketch_dimensions" =>
                    _getSketchDimensionsCommand.Execute(
                        root),

                "get_work_features" =>
                    _getWorkFeaturesCommand.Execute(
                        root),

                "get_assembly_summary" =>
                    _getAssemblySummaryCommand.Execute(
                        root),

                "get_assembly_occurrences" =>
                    _getAssemblyOccurrencesCommand.Execute(
                        root),

                "get_assembly_constraints" =>
                    _getAssemblyConstraintsCommand.Execute(
                        root),

                "get_assembly_bom" =>
                    _getAssemblyBomCommand.Execute(
                        root),

                "get_assembly_referenced_documents" =>
                    _getAssemblyReferencedDocumentsCommand.Execute(
                        root),

                "get_drawing_sheets" =>
                    _getDrawingSheetsCommand.Execute(
                        root),

                "get_drawing_views_detailed" =>
                    _getDrawingViewsDetailedCommand.Execute(
                        root),

                "get_drawing_view_relationships" =>
                    _getDrawingViewRelationshipsCommand.Execute(
                        root),

                "get_drawing_annotation_summary" =>
                    _getDrawingAnnotationSummaryCommand.Execute(
                        root),

                "get_drawing_tables" =>
                    _getDrawingTablesCommand.Execute(
                        root),

                "get_drawing_table_collections" =>
                    _getDrawingTableCollectionsCommand.Execute(
                        root),

                "get_custom_tables" =>
                    _getCustomTablesCommand.Execute(
                        root),

                "get_parts_lists" =>
                    _getPartsListsCommand.Execute(
                        root),

                "get_revision_tables" =>
                    _getRevisionTablesCommand.Execute(
                        root),

                _ =>
                    CreateError(
                        $"Неизвестная команда: {command}")
            };
        }
        catch (JsonException exception)
        {
            return CreateError(
                "Некорректный JSON.",
                exception.Message);
        }
        catch (Exception exception)
        {
            return CreateError(
                "Ошибка выполнения команды.",
                exception.Message);
        }
    }

    private static string CreateError(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                details
            },
            new JsonSerializerOptions
            {
                WriteIndented =
                    true,

                Encoder =
                    JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
            });
    }
}
