using Inventor;

namespace AI_CAD_ENGINEER.Drawing;

public class CenterAnnotationGenerator
{
    public void Create(params DrawingView[] drawingViews)
    {
        ArgumentNullException.ThrowIfNull(drawingViews);

        foreach (DrawingView drawingView in drawingViews)
        {
            if (drawingView == null)
            {
                continue;
            }

            try
            {
                drawingView.SetAutomatedCenterlineSettings();
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    "Не удалось создать центровые элементы " +
                    $"для вида: {exception.Message}");
            }
        }
    }
}