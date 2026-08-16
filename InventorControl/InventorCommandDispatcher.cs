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

    private readonly CreateDrawingDocumentCommand
        _createDrawingDocumentCommand;

    private readonly CreateSectionLineCommand
        _createSectionLineCommand;

    private readonly CreateSectionViewCommand
        _createSectionViewCommand;

    private readonly CreateDetailViewCommand
        _createDetailViewCommand;

    private readonly CreateAuxiliaryViewCommand
        _createAuxiliaryViewCommand;

    private readonly AddDrawingViewBreakCommand
        _addDrawingViewBreakCommand;

    private readonly CreatePartsListCommand
        _createPartsListCommand;

    private readonly MovePartsListCommand
        _movePartsListCommand;

    private readonly DeletePartsListCommand
        _deletePartsListCommand;

    private readonly CreateBalloonCommand
        _createBalloonCommand;

    private readonly MoveBalloonCommand
        _moveBalloonCommand;

    private readonly DeleteBalloonCommand
        _deleteBalloonCommand;

    private readonly CreateCenterMarkCommand
        _createCenterMarkCommand;

    private readonly CreateCenterlineBisectorCommand
        _createCenterlineBisectorCommand;

    private readonly CreateCenterlineCenteredPatternCommand
        _createCenterlineCenteredPatternCommand;

    private readonly ActivateDocumentCommand
        _activateDocumentCommand;

    private readonly SaveDocumentCommand
        _saveDocumentCommand;

    private readonly SaveDocumentAsCommand
        _saveDocumentAsCommand;

    private readonly ExportPdfCommand
        _exportPdfCommand;

    private readonly ExportDwgCommand
        _exportDwgCommand;

    private readonly ExportDxfCommand
        _exportDxfCommand;

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

    private readonly CreateAngularDimensionCommand
        _createAngularDimensionCommand;

    private readonly CreateOrdinateDimensionCommand
        _createOrdinateDimensionCommand;

    private readonly CreateBaselineDimensionCommand
        _createBaselineDimensionCommand;

    private readonly CreateChainDimensionCommand
        _createChainDimensionCommand;

    private readonly SetGeneralDimensionFormattedTextCommand
        _setGeneralDimensionFormattedTextCommand;

    private readonly SetGeneralDimensionHideValueCommand
        _setGeneralDimensionHideValueCommand;

    private readonly SetGeneralDimensionPrecisionCommand
        _setGeneralDimensionPrecisionCommand;

    private readonly SetGeneralDimensionModelValueOverrideCommand
        _setGeneralDimensionModelValueOverrideCommand;

    private readonly ClearGeneralDimensionModelValueOverrideCommand
        _clearGeneralDimensionModelValueOverrideCommand;

    private readonly SetGeneralDimensionStyleCommand
        _setGeneralDimensionStyleCommand;

    private readonly SetGeneralDimensionLayerCommand
        _setGeneralDimensionLayerCommand;

    private readonly GetGeneralDimensionToleranceCommand
        _getGeneralDimensionToleranceCommand;

    private readonly SetGeneralDimensionToleranceDefaultCommand
        _setGeneralDimensionToleranceDefaultCommand;

    private readonly SetGeneralDimensionToleranceBasicCommand
        _setGeneralDimensionToleranceBasicCommand;

    private readonly SetGeneralDimensionToleranceReferenceCommand
        _setGeneralDimensionToleranceReferenceCommand;

    private readonly SetGeneralDimensionToleranceSymmetricCommand
        _setGeneralDimensionToleranceSymmetricCommand;

    private readonly SetGeneralDimensionToleranceDeviationCommand
        _setGeneralDimensionToleranceDeviationCommand;

    private readonly SetGeneralDimensionToleranceLimitsCommand
        _setGeneralDimensionToleranceLimitsCommand;

    private readonly SetGeneralDimensionToleranceFitsCommand
        _setGeneralDimensionToleranceFitsCommand;

    private readonly GetDrawingViewOriginIndicatorCommand
        _getDrawingViewOriginIndicatorCommand;

    private readonly CreateDrawingViewOriginIndicatorCommand
        _createDrawingViewOriginIndicatorCommand;

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

    private readonly CreateGeneralNoteFittedCommand
        _createGeneralNoteFittedCommand;

    private readonly SetGeneralNoteFormattedTextCommand
        _setGeneralNoteFormattedTextCommand;

    private readonly MoveGeneralNoteCommand
        _moveGeneralNoteCommand;

    private readonly DeleteGeneralNoteCommand
        _deleteGeneralNoteCommand;

    private readonly GetLeaderNotesCommand
        _getLeaderNotesCommand;

    private readonly CreateLeaderNoteCommand
        _createLeaderNoteCommand;

    private readonly SetLeaderNoteFormattedTextCommand
        _setLeaderNoteFormattedTextCommand;

    private readonly MoveLeaderNoteCommand
        _moveLeaderNoteCommand;

    private readonly DeleteLeaderNoteCommand
        _deleteLeaderNoteCommand;

    private readonly GetDrawingTextObjectsCommand
        _getDrawingTextObjectsCommand;

    private readonly GetSketchedSymbolDefinitionsCommand
        _getSketchedSymbolDefinitionsCommand;

    private readonly CreateSketchedSymbolCommand
        _createSketchedSymbolCommand;

    private readonly MoveSketchedSymbolCommand
        _moveSketchedSymbolCommand;

    private readonly DeleteSketchedSymbolCommand
        _deleteSketchedSymbolCommand;

    private readonly GetFeatureControlFramesCommand
        _getFeatureControlFramesCommand;

    private readonly CreateFeatureControlFrameCommand
        _createFeatureControlFrameCommand;

    private readonly MoveFeatureControlFrameCommand
        _moveFeatureControlFrameCommand;

    private readonly DeleteFeatureControlFrameCommand
        _deleteFeatureControlFrameCommand;

    private readonly GetSurfaceTextureSymbolsCommand
        _getSurfaceTextureSymbolsCommand;

    private readonly CreateSurfaceTextureSymbolCommand
        _createSurfaceTextureSymbolCommand;

    private readonly MoveSurfaceTextureSymbolCommand
        _moveSurfaceTextureSymbolCommand;

    private readonly DeleteSurfaceTextureSymbolCommand
        _deleteSurfaceTextureSymbolCommand;

    private readonly GetWeldingSymbolsCommand
        _getWeldingSymbolsCommand;

    private readonly CreateWeldingSymbolCommand
        _createWeldingSymbolCommand;

    private readonly MoveWeldingSymbolCommand
        _moveWeldingSymbolCommand;

    private readonly DeleteWeldingSymbolCommand
        _deleteWeldingSymbolCommand;

    private readonly GetRevisionCloudsCommand
        _getRevisionCloudsCommand;

    private readonly CreateRevisionCloudCommand
        _createRevisionCloudCommand;

    private readonly MoveRevisionCloudCommand
        _moveRevisionCloudCommand;

    private readonly DeleteRevisionCloudCommand
        _deleteRevisionCloudCommand;

    private readonly GetEdgeSymbolsCommand
        _getEdgeSymbolsCommand;

    private readonly CreateEdgeSymbolCommand
        _createEdgeSymbolCommand;

    private readonly MoveEdgeSymbolCommand
        _moveEdgeSymbolCommand;

    private readonly DeleteEdgeSymbolCommand
        _deleteEdgeSymbolCommand;

    private readonly GetTransitionSymbolsCommand
        _getTransitionSymbolsCommand;

    private readonly CreateTransitionSymbolCommand
        _createTransitionSymbolCommand;

    private readonly MoveTransitionSymbolCommand
        _moveTransitionSymbolCommand;

    private readonly DeleteTransitionSymbolCommand
        _deleteTransitionSymbolCommand;

    private readonly CreateBendNoteCommand
        _createBendNoteCommand;

    private readonly MoveBendNoteCommand
        _moveBendNoteCommand;

    private readonly DeleteBendNoteCommand
        _deleteBendNoteCommand;

    private readonly CreateChamferNoteCommand
        _createChamferNoteCommand;

    private readonly MoveChamferNoteCommand
        _moveChamferNoteCommand;

    private readonly DeleteChamferNoteCommand
        _deleteChamferNoteCommand;

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

    private readonly GetThreadFeaturesCommand
        _getThreadFeaturesCommand;

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

    private readonly CreateCustomTableCommand
        _createCustomTableCommand;

    private readonly MoveCustomTableCommand
        _moveCustomTableCommand;

    private readonly DeleteCustomTableCommand
        _deleteCustomTableCommand;

    private readonly GetPartsListsCommand
        _getPartsListsCommand;

    private readonly GetRevisionTablesCommand
        _getRevisionTablesCommand;

    private readonly CreateRevisionTableCommand
        _createRevisionTableCommand;

    private readonly MoveRevisionTableCommand
        _moveRevisionTableCommand;

    private readonly DeleteRevisionTableCommand
        _deleteRevisionTableCommand;

    private readonly GetHoleTablesCommand
        _getHoleTablesCommand;

    private readonly CreateHoleTableCommand
        _createHoleTableCommand;

    private readonly MoveHoleTableCommand
        _moveHoleTableCommand;

    private readonly DeleteHoleTableCommand
        _deleteHoleTableCommand;

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

        _createDrawingDocumentCommand =
            new CreateDrawingDocumentCommand(
                inventor);

        _createSectionLineCommand =
            new CreateSectionLineCommand(
                inventor);

        _createSectionViewCommand =
            new CreateSectionViewCommand(
                inventor);

        _createDetailViewCommand =
            new CreateDetailViewCommand(
                inventor);

        _createAuxiliaryViewCommand =
            new CreateAuxiliaryViewCommand(
                inventor);

        _addDrawingViewBreakCommand =
            new AddDrawingViewBreakCommand(
                inventor);

        _createPartsListCommand =
            new CreatePartsListCommand(
                inventor);

        _movePartsListCommand =
            new MovePartsListCommand(
                inventor);

        _deletePartsListCommand =
            new DeletePartsListCommand(
                inventor);

        _createBalloonCommand =
            new CreateBalloonCommand(
                inventor);

        _moveBalloonCommand =
            new MoveBalloonCommand(
                inventor);

        _deleteBalloonCommand =
            new DeleteBalloonCommand(
                inventor);

        _createCenterMarkCommand =
            new CreateCenterMarkCommand(
                inventor);

        _createCenterlineBisectorCommand =
            new CreateCenterlineBisectorCommand(
                inventor);

        _createCenterlineCenteredPatternCommand =
            new CreateCenterlineCenteredPatternCommand(
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

        _exportPdfCommand =
            new ExportPdfCommand(
                inventor);

        _exportDwgCommand =
            new ExportDwgCommand(
                inventor);

        _exportDxfCommand =
            new ExportDxfCommand(
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

        _createAngularDimensionCommand =
            new CreateAngularDimensionCommand(
                inventor);

        _createOrdinateDimensionCommand =
            new CreateOrdinateDimensionCommand(
                inventor);

        _createBaselineDimensionCommand =
            new CreateBaselineDimensionCommand(
                inventor);

        _createChainDimensionCommand =
            new CreateChainDimensionCommand(
                inventor);

        _setGeneralDimensionFormattedTextCommand =
            new SetGeneralDimensionFormattedTextCommand(
                inventor);

        _setGeneralDimensionHideValueCommand =
            new SetGeneralDimensionHideValueCommand(
                inventor);

        _setGeneralDimensionPrecisionCommand =
            new SetGeneralDimensionPrecisionCommand(
                inventor);

        _setGeneralDimensionModelValueOverrideCommand =
            new SetGeneralDimensionModelValueOverrideCommand(
                inventor);

        _clearGeneralDimensionModelValueOverrideCommand =
            new ClearGeneralDimensionModelValueOverrideCommand(
                inventor);

        _setGeneralDimensionStyleCommand =
            new SetGeneralDimensionStyleCommand(
                inventor);

        _setGeneralDimensionLayerCommand =
            new SetGeneralDimensionLayerCommand(
                inventor);

        _getGeneralDimensionToleranceCommand =
            new GetGeneralDimensionToleranceCommand(
                inventor);

        _setGeneralDimensionToleranceDefaultCommand =
            new SetGeneralDimensionToleranceDefaultCommand(
                inventor);

        _setGeneralDimensionToleranceBasicCommand =
            new SetGeneralDimensionToleranceBasicCommand(
                inventor);

        _setGeneralDimensionToleranceReferenceCommand =
            new SetGeneralDimensionToleranceReferenceCommand(
                inventor);

        _setGeneralDimensionToleranceSymmetricCommand =
            new SetGeneralDimensionToleranceSymmetricCommand(
                inventor);

        _setGeneralDimensionToleranceDeviationCommand =
            new SetGeneralDimensionToleranceDeviationCommand(
                inventor);

        _setGeneralDimensionToleranceLimitsCommand =
            new SetGeneralDimensionToleranceLimitsCommand(
                inventor);

        _setGeneralDimensionToleranceFitsCommand =
            new SetGeneralDimensionToleranceFitsCommand(
                inventor);

        _getDrawingViewOriginIndicatorCommand =
            new GetDrawingViewOriginIndicatorCommand(
                inventor);

        _createDrawingViewOriginIndicatorCommand =
            new CreateDrawingViewOriginIndicatorCommand(
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

        _createGeneralNoteFittedCommand =
            new CreateGeneralNoteFittedCommand(
                inventor);

        _setGeneralNoteFormattedTextCommand =
            new SetGeneralNoteFormattedTextCommand(
                inventor);

        _moveGeneralNoteCommand =
            new MoveGeneralNoteCommand(
                inventor);

        _deleteGeneralNoteCommand =
            new DeleteGeneralNoteCommand(
                inventor);

        _getLeaderNotesCommand =
            new GetLeaderNotesCommand(
                inventor);

        _createLeaderNoteCommand =
            new CreateLeaderNoteCommand(
                inventor);

        _setLeaderNoteFormattedTextCommand =
            new SetLeaderNoteFormattedTextCommand(
                inventor);

        _moveLeaderNoteCommand =
            new MoveLeaderNoteCommand(
                inventor);

        _deleteLeaderNoteCommand =
            new DeleteLeaderNoteCommand(
                inventor);

        _getDrawingTextObjectsCommand =
            new GetDrawingTextObjectsCommand(
                inventor);

        _getSketchedSymbolDefinitionsCommand =
            new GetSketchedSymbolDefinitionsCommand(
                inventor);

        _createSketchedSymbolCommand =
            new CreateSketchedSymbolCommand(
                inventor);

        _moveSketchedSymbolCommand =
            new MoveSketchedSymbolCommand(
                inventor);

        _deleteSketchedSymbolCommand =
            new DeleteSketchedSymbolCommand(
                inventor);

        _getFeatureControlFramesCommand =
            new GetFeatureControlFramesCommand(
                inventor);

        _createFeatureControlFrameCommand =
            new CreateFeatureControlFrameCommand(
                inventor);

        _moveFeatureControlFrameCommand =
            new MoveFeatureControlFrameCommand(
                inventor);

        _deleteFeatureControlFrameCommand =
            new DeleteFeatureControlFrameCommand(
                inventor);

        _getSurfaceTextureSymbolsCommand =
            new GetSurfaceTextureSymbolsCommand(
                inventor);

        _createSurfaceTextureSymbolCommand =
            new CreateSurfaceTextureSymbolCommand(
                inventor);

        _moveSurfaceTextureSymbolCommand =
            new MoveSurfaceTextureSymbolCommand(
                inventor);

        _deleteSurfaceTextureSymbolCommand =
            new DeleteSurfaceTextureSymbolCommand(
                inventor);

        _getWeldingSymbolsCommand =
            new GetWeldingSymbolsCommand(
                inventor);

        _createWeldingSymbolCommand =
            new CreateWeldingSymbolCommand(
                inventor);

        _moveWeldingSymbolCommand =
            new MoveWeldingSymbolCommand(
                inventor);

        _deleteWeldingSymbolCommand =
            new DeleteWeldingSymbolCommand(
                inventor);

        _getRevisionCloudsCommand =
            new GetRevisionCloudsCommand(
                inventor);

        _createRevisionCloudCommand =
            new CreateRevisionCloudCommand(
                inventor);

        _moveRevisionCloudCommand =
            new MoveRevisionCloudCommand(
                inventor);

        _deleteRevisionCloudCommand =
            new DeleteRevisionCloudCommand(
                inventor);

        _getEdgeSymbolsCommand =
            new GetEdgeSymbolsCommand(
                inventor);

        _createEdgeSymbolCommand =
            new CreateEdgeSymbolCommand(
                inventor);

        _moveEdgeSymbolCommand =
            new MoveEdgeSymbolCommand(
                inventor);

        _deleteEdgeSymbolCommand =
            new DeleteEdgeSymbolCommand(
                inventor);

        _getTransitionSymbolsCommand =
            new GetTransitionSymbolsCommand(
                inventor);

        _createTransitionSymbolCommand =
            new CreateTransitionSymbolCommand(
                inventor);

        _moveTransitionSymbolCommand =
            new MoveTransitionSymbolCommand(
                inventor);

        _deleteTransitionSymbolCommand =
            new DeleteTransitionSymbolCommand(
                inventor);

        _createBendNoteCommand =
            new CreateBendNoteCommand(
                inventor);

        _moveBendNoteCommand =
            new MoveBendNoteCommand(
                inventor);

        _deleteBendNoteCommand =
            new DeleteBendNoteCommand(
                inventor);

        _createChamferNoteCommand =
            new CreateChamferNoteCommand(
                inventor);

        _moveChamferNoteCommand =
            new MoveChamferNoteCommand(
                inventor);

        _deleteChamferNoteCommand =
            new DeleteChamferNoteCommand(
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

        _getThreadFeaturesCommand =
            new GetThreadFeaturesCommand(
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

        _createCustomTableCommand =
            new CreateCustomTableCommand(
                inventor);

        _moveCustomTableCommand =
            new MoveCustomTableCommand(
                inventor);

        _deleteCustomTableCommand =
            new DeleteCustomTableCommand(
                inventor);

        _getPartsListsCommand =
            new GetPartsListsCommand(
                inventor);

        _getRevisionTablesCommand =
            new GetRevisionTablesCommand(
                inventor);

        _createRevisionTableCommand =
            new CreateRevisionTableCommand(
                inventor);

        _moveRevisionTableCommand =
            new MoveRevisionTableCommand(
                inventor);

        _deleteRevisionTableCommand =
            new DeleteRevisionTableCommand(
                inventor);

        _getHoleTablesCommand =
            new GetHoleTablesCommand(
                inventor);

        _createHoleTableCommand =
            new CreateHoleTableCommand(
                inventor);

        _moveHoleTableCommand =
            new MoveHoleTableCommand(
                inventor);

        _deleteHoleTableCommand =
            new DeleteHoleTableCommand(
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

                "create_drawing_document" =>
                    _createDrawingDocumentCommand.Execute(
                        root),

                "create_section_line" =>
                    _createSectionLineCommand.Execute(
                        root),

                "create_section_view" =>
                    _createSectionViewCommand.Execute(
                        root),

                "create_detail_view" =>
                    _createDetailViewCommand.Execute(
                        root),

                "create_auxiliary_view" =>
                    _createAuxiliaryViewCommand.Execute(
                        root),

                "add_drawing_view_break" =>
                    _addDrawingViewBreakCommand.Execute(
                        root),

                "create_parts_list" =>
                    _createPartsListCommand.Execute(
                        root),

                "move_parts_list" =>
                    _movePartsListCommand.Execute(
                        root),

                "delete_parts_list" =>
                    _deletePartsListCommand.Execute(
                        root),

                "create_balloon" =>
                    _createBalloonCommand.Execute(
                        root),

                "move_balloon" =>
                    _moveBalloonCommand.Execute(
                        root),

                "delete_balloon" =>
                    _deleteBalloonCommand.Execute(
                        root),

                "create_center_mark" =>
                    _createCenterMarkCommand.Execute(
                        root),

                "create_centerline_bisector" =>
                    _createCenterlineBisectorCommand.Execute(
                        root),

                "create_centerline_centered_pattern" =>
                    _createCenterlineCenteredPatternCommand.Execute(
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

                "export_pdf" =>
                    _exportPdfCommand.Execute(
                        root),

                "export_dwg" =>
                    _exportDwgCommand.Execute(
                        root),

                "export_dxf" =>
                    _exportDxfCommand.Execute(
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

                "create_angular_dimension" =>
                    _createAngularDimensionCommand.Execute(
                        root),

                "create_ordinate_dimension" =>
                    _createOrdinateDimensionCommand.Execute(
                        root),

                "create_baseline_dimension" =>
                    _createBaselineDimensionCommand.Execute(
                        root),

                "create_chain_dimension" =>
                    _createChainDimensionCommand.Execute(
                        root),

                "set_general_dimension_formatted_text" =>
                    _setGeneralDimensionFormattedTextCommand.Execute(
                        root),

                "set_general_dimension_hide_value" =>
                    _setGeneralDimensionHideValueCommand.Execute(
                        root),

                "set_general_dimension_precision" =>
                    _setGeneralDimensionPrecisionCommand.Execute(
                        root),

                "set_general_dimension_model_value_override" =>
                    _setGeneralDimensionModelValueOverrideCommand.Execute(
                        root),

                "clear_general_dimension_model_value_override" =>
                    _clearGeneralDimensionModelValueOverrideCommand.Execute(
                        root),

                "set_general_dimension_style" =>
                    _setGeneralDimensionStyleCommand.Execute(
                        root),

                "set_general_dimension_layer" =>
                    _setGeneralDimensionLayerCommand.Execute(
                        root),

                "get_general_dimension_tolerance" =>
                    _getGeneralDimensionToleranceCommand.Execute(
                        root),

                "set_general_dimension_tolerance_default" =>
                    _setGeneralDimensionToleranceDefaultCommand.Execute(
                        root),

                "set_general_dimension_tolerance_basic" =>
                    _setGeneralDimensionToleranceBasicCommand.Execute(
                        root),

                "set_general_dimension_tolerance_reference" =>
                    _setGeneralDimensionToleranceReferenceCommand.Execute(
                        root),

                "set_general_dimension_tolerance_symmetric" =>
                    _setGeneralDimensionToleranceSymmetricCommand.Execute(
                        root),

                "set_general_dimension_tolerance_deviation" =>
                    _setGeneralDimensionToleranceDeviationCommand.Execute(
                        root),

                "set_general_dimension_tolerance_limits" =>
                    _setGeneralDimensionToleranceLimitsCommand.Execute(
                        root),

                "set_general_dimension_tolerance_fits" =>
                    _setGeneralDimensionToleranceFitsCommand.Execute(
                        root),

                "get_drawing_view_origin_indicator" =>
                    _getDrawingViewOriginIndicatorCommand.Execute(
                        root),

                "create_drawing_view_origin_indicator" =>
                    _createDrawingViewOriginIndicatorCommand.Execute(
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

                "create_general_note_fitted" =>
                    _createGeneralNoteFittedCommand.Execute(
                        root),

                "set_general_note_formatted_text" =>
                    _setGeneralNoteFormattedTextCommand.Execute(
                        root),

                "move_general_note" =>
                    _moveGeneralNoteCommand.Execute(
                        root),

                "delete_general_note" =>
                    _deleteGeneralNoteCommand.Execute(
                        root),

                "get_leader_notes" =>
                    _getLeaderNotesCommand.Execute(
                        root),

                "create_leader_note" =>
                    _createLeaderNoteCommand.Execute(
                        root),

                "set_leader_note_formatted_text" =>
                    _setLeaderNoteFormattedTextCommand.Execute(
                        root),

                "move_leader_note" =>
                    _moveLeaderNoteCommand.Execute(
                        root),

                "delete_leader_note" =>
                    _deleteLeaderNoteCommand.Execute(
                        root),

                "get_drawing_text_objects" =>
                    _getDrawingTextObjectsCommand.Execute(
                        root),

                "get_sketched_symbol_definitions" =>
                    _getSketchedSymbolDefinitionsCommand.Execute(
                        root),

                "create_sketched_symbol" =>
                    _createSketchedSymbolCommand.Execute(
                        root),

                "move_sketched_symbol" =>
                    _moveSketchedSymbolCommand.Execute(
                        root),

                "delete_sketched_symbol" =>
                    _deleteSketchedSymbolCommand.Execute(
                        root),

                "get_feature_control_frames" =>
                    _getFeatureControlFramesCommand.Execute(
                        root),

                "create_feature_control_frame" =>
                    _createFeatureControlFrameCommand.Execute(
                        root),

                "move_feature_control_frame" =>
                    _moveFeatureControlFrameCommand.Execute(
                        root),

                "delete_feature_control_frame" =>
                    _deleteFeatureControlFrameCommand.Execute(
                        root),

                "get_surface_texture_symbols" =>
                    _getSurfaceTextureSymbolsCommand.Execute(
                        root),

                "create_surface_texture_symbol" =>
                    _createSurfaceTextureSymbolCommand.Execute(
                        root),

                "move_surface_texture_symbol" =>
                    _moveSurfaceTextureSymbolCommand.Execute(
                        root),

                "delete_surface_texture_symbol" =>
                    _deleteSurfaceTextureSymbolCommand.Execute(
                        root),

                "get_welding_symbols" =>
                    _getWeldingSymbolsCommand.Execute(
                        root),

                "create_welding_symbol" =>
                    _createWeldingSymbolCommand.Execute(
                        root),

                "move_welding_symbol" =>
                    _moveWeldingSymbolCommand.Execute(
                        root),

                "delete_welding_symbol" =>
                    _deleteWeldingSymbolCommand.Execute(
                        root),

                "get_revision_clouds" =>
                    _getRevisionCloudsCommand.Execute(
                        root),

                "create_revision_cloud" =>
                    _createRevisionCloudCommand.Execute(
                        root),

                "move_revision_cloud" =>
                    _moveRevisionCloudCommand.Execute(
                        root),

                "delete_revision_cloud" =>
                    _deleteRevisionCloudCommand.Execute(
                        root),

                "get_edge_symbols" =>
                    _getEdgeSymbolsCommand.Execute(
                        root),

                "create_edge_symbol" =>
                    _createEdgeSymbolCommand.Execute(
                        root),

                "move_edge_symbol" =>
                    _moveEdgeSymbolCommand.Execute(
                        root),

                "delete_edge_symbol" =>
                    _deleteEdgeSymbolCommand.Execute(
                        root),

                "get_transition_symbols" =>
                    _getTransitionSymbolsCommand.Execute(
                        root),

                "create_transition_symbol" =>
                    _createTransitionSymbolCommand.Execute(
                        root),

                "move_transition_symbol" =>
                    _moveTransitionSymbolCommand.Execute(
                        root),

                "delete_transition_symbol" =>
                    _deleteTransitionSymbolCommand.Execute(
                        root),

                "create_bend_note" =>
                    _createBendNoteCommand.Execute(
                        root),

                "move_bend_note" =>
                    _moveBendNoteCommand.Execute(
                        root),

                "delete_bend_note" =>
                    _deleteBendNoteCommand.Execute(
                        root),

                "create_chamfer_note" =>
                    _createChamferNoteCommand.Execute(
                        root),

                "move_chamfer_note" =>
                    _moveChamferNoteCommand.Execute(
                        root),

                "delete_chamfer_note" =>
                    _deleteChamferNoteCommand.Execute(
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

                "get_thread_features" =>
                    _getThreadFeaturesCommand.Execute(
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

                "create_custom_table" =>
                    _createCustomTableCommand.Execute(
                        root),

                "move_custom_table" =>
                    _moveCustomTableCommand.Execute(
                        root),

                "delete_custom_table" =>
                    _deleteCustomTableCommand.Execute(
                        root),

                "get_parts_lists" =>
                    _getPartsListsCommand.Execute(
                        root),

                "get_revision_tables" =>
                    _getRevisionTablesCommand.Execute(
                        root),

                "create_revision_table" =>
                    _createRevisionTableCommand.Execute(
                        root),

                "move_revision_table" =>
                    _moveRevisionTableCommand.Execute(
                        root),

                "delete_revision_table" =>
                    _deleteRevisionTableCommand.Execute(
                        root),

                "get_hole_tables" =>
                    _getHoleTablesCommand.Execute(
                        root),

                "create_hole_table" =>
                    _createHoleTableCommand.Execute(
                        root),

                "move_hole_table" =>
                    _moveHoleTableCommand.Execute(
                        root),

                "delete_hole_table" =>
                    _deleteHoleTableCommand.Execute(
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
