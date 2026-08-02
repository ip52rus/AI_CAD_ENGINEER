using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;

    public DrawingManager(Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public bool CreateDrawingWithViews(Document modelDocument)
    {
        try
        {
            DrawingDocument drawingDocument =
                (DrawingDocument)_inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    "",
                    true);

            Sheet sheet = drawingDocument.ActiveSheet;

            const double viewGap = 2.5;

            Point2d temporaryBasePosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    sheet.Width / 2,
                    sheet.Height / 2);

            DrawingView baseView =
                sheet.DrawingViews.AddBaseView(
                    (Inventor._Document)modelDocument,
                    temporaryBasePosition,
                    1.0,
                    ViewOrientationTypeEnum.kFrontViewOrientation,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporaryTopPosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    baseView.Center.X,
                    baseView.Center.Y - baseView.Height - viewGap);

            DrawingView topView =
                sheet.DrawingViews.AddProjectedView(
                    baseView,
                    temporaryTopPosition,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporarySidePosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    baseView.Center.X + baseView.Width + viewGap,
                    baseView.Center.Y);

            DrawingView sideView =
                sheet.DrawingViews.AddProjectedView(
                    baseView,
                    temporarySidePosition,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            drawingDocument.Update();

            double selectedScale =
                CalculateGostScale(
                    baseView,
                    topView,
                    sideView,
                    sheet,
                    viewGap);

            baseView.Scale = selectedScale;

            drawingDocument.Update();

            ArrangeViews(
                baseView,
                topView,
                sideView,
                sheet,
                viewGap);

            drawingDocument.Update();
            drawingDocument.Activate();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ArrangeViews(
        DrawingView baseView,
        DrawingView topView,
        DrawingView sideView,
        Sheet sheet,
        double viewGap)
    {
        const double leftMargin = 2.0;
        const double rightMargin = 2.0;
        const double topMargin = 2.0;

        // Внизу оставляем увеличенный запас
        // для основной надписи и будущих размеров.
        const double bottomReservedArea = 6.0;

        double workingLeft = leftMargin;
        double workingBottom = bottomReservedArea;

        double workingWidth =
            sheet.Width - leftMargin - rightMargin;

        double workingHeight =
            sheet.Height - topMargin - bottomReservedArea;

        double groupWidth =
            baseView.Width +
            viewGap +
            sideView.Width;

        double groupHeight =
            topView.Height +
            viewGap +
            baseView.Height;

        double groupLeft =
            workingLeft +
            Math.Max(
                0,
                (workingWidth - groupWidth) / 2);

        double groupBottom =
            workingBottom +
            Math.Max(
                0,
                (workingHeight - groupHeight) / 2);

        double baseCenterX =
            groupLeft + baseView.Width / 2;

        double topCenterY =
            groupBottom + topView.Height / 2;

        double baseCenterY =
            groupBottom +
            topView.Height +
            viewGap +
            baseView.Height / 2;

        double sideCenterX =
            groupLeft +
            baseView.Width +
            viewGap +
            sideView.Width / 2;

        topView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                baseCenterX,
                topCenterY);

        baseView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                baseCenterX,
                baseCenterY);

        sideView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                sideCenterX,
                baseCenterY);
    }

    private static double CalculateGostScale(
        DrawingView baseView,
        DrawingView topView,
        DrawingView sideView,
        Sheet sheet,
        double viewGap)
    {
        const double leftMargin = 2.0;
        const double rightMargin = 2.0;
        const double topMargin = 2.0;
        const double bottomReservedArea = 6.0;

        double availableWidth =
            sheet.Width - leftMargin - rightMargin;

        double availableHeight =
            sheet.Height - topMargin - bottomReservedArea;

        double viewsWidthAtScaleOne =
            baseView.Width + sideView.Width;

        double viewsHeightAtScaleOne =
            baseView.Height + topView.Height;

        if (viewsWidthAtScaleOne <= 0 ||
            viewsHeightAtScaleOne <= 0)
        {
            return 0.1;
        }

        double maximumScaleByWidth =
            (availableWidth - viewGap) /
            viewsWidthAtScaleOne;

        double maximumScaleByHeight =
            (availableHeight - viewGap) /
            viewsHeightAtScaleOne;

        double maximumScale =
            Math.Min(
                maximumScaleByWidth,
                maximumScaleByHeight);

        double[] gostScales =
        {
            100.0,
            50.0,
            40.0,
            20.0,
            10.0,
            5.0,
            4.0,
            2.5,
            2.0,
            1.0,
            0.5,
            0.4,
            0.25,
            0.2,
            0.1,
            1.0 / 15.0,
            0.05,
            0.04,
            0.025,
            0.02,
            1.0 / 75.0,
            0.01,
            0.005,
            0.0025,
            0.002,
            0.00125,
            0.001
        };

        foreach (double scale in gostScales)
        {
            if (scale <= maximumScale)
            {
                return scale;
            }
        }

        return 0.001;
    }
}