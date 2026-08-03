using System.Text;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraphReport
{
    public string Build(
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            graph);

        graph.RefreshMetadata();

        StringBuilder report =
            new();

        report.AppendLine(
            "========================================");

        report.AppendLine(
            "ENGINEERING FEATURE GRAPH");

        report.AppendLine(
            "========================================");

        report.AppendLine();

        report.AppendLine(
            $"Документ: " +
            $"{graph.Metadata.SourceDocumentName}");

        report.AppendLine(
            $"Путь: " +
            $"{graph.Metadata.SourceDocumentPath}");

        report.AppendLine(
            $"Создан: " +
            $"{graph.Metadata.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        report.AppendLine();

        report.AppendLine(
            $"Узлов: " +
            $"{graph.Metadata.NodeCount}");

        report.AppendLine(
            $"Связей: " +
            $"{graph.Metadata.RelationshipCount}");

        report.AppendLine();

        AppendFeatureSummary(
            report,
            graph);

        AppendNodes(
            report,
            graph);

        AppendRelationships(
            report,
            graph);

        report.AppendLine(
            "========================================");

        return report.ToString();
    }

    private static void AppendFeatureSummary(
        StringBuilder report,
        FeatureGraph graph)
    {
        report.AppendLine(
            "СВОДКА ПО ТИПАМ");

        report.AppendLine(
            "----------------------------------------");

        if (graph.Metadata.FeatureCounts.Count == 0)
        {
            report.AppendLine(
                "Граф не содержит инженерных элементов.");

            report.AppendLine();

            return;
        }

        foreach (KeyValuePair<FeatureType, int> entry
                 in graph.Metadata.FeatureCounts
                     .OrderBy(
                         item => item.Key))
        {
            report.AppendLine(
                $"{entry.Key}: {entry.Value}");
        }

        report.AppendLine();
    }

    private static void AppendNodes(
        StringBuilder report,
        FeatureGraph graph)
    {
        report.AppendLine(
            "УЗЛЫ");

        report.AppendLine(
            "----------------------------------------");

        if (graph.Nodes.Count == 0)
        {
            report.AppendLine(
                "Узлы отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (FeatureNode node
                 in graph.Nodes)
        {
            index++;

            report.AppendLine(
                $"{index}. {node.Type}");

            report.AppendLine(
                $"   Id: {node.Id}");

            report.AppendLine(
                $"   Имя: {node.Name}");

            if (node.Parent != null)
            {
                report.AppendLine(
                    $"   Родитель: " +
                    $"{node.Parent.Id}");
            }

            report.AppendLine(
                $"   Дочерних элементов: " +
                $"{node.Children.Count}");

            AppendSpecializedNodeData(
                report,
                node);

            AppendParameters(
                report,
                node);

            report.AppendLine();
        }
    }

    private static void AppendSpecializedNodeData(
        StringBuilder report,
        FeatureNode node)
    {
        if (node is not HoleFeatureNode hole)
        {
            return;
        }

        report.AppendLine(
            $"   Диаметр: " +
            $"{hole.DiameterMillimeters:F3} мм");

        report.AppendLine(
            $"   Глубина: " +
            $"{hole.DepthMillimeters:F3} мм");

        report.AppendLine(
            $"   Тип завершения: " +
            $"{hole.TerminationType}");

        report.AppendLine(
            $"   Сквозное: " +
            $"{FormatBoolean(
                hole.IsThroughHole)}");

        report.AppendLine(
            $"   Количество экземпляров: " +
            $"{hole.InstanceCount}");

        report.AppendLine(
            $"   Ось: {hole.Axis}");

        report.AppendLine(
            $"   Центр: " +
            $"X={hole.CenterXMillimeters:F3}; " +
            $"Y={hole.CenterYMillimeters:F3}; " +
            $"Z={hole.CenterZMillimeters:F3} мм");

        report.AppendLine(
            $"   Исходная операция: " +
            $"{hole.SourceFeatureName}");
    }

    private static void AppendParameters(
        StringBuilder report,
        FeatureNode node)
    {
        if (node.Parameters.Count == 0)
        {
            return;
        }

        report.AppendLine(
            "   Параметры:");

        foreach (KeyValuePair<string, double> parameter
                 in node.Parameters
                     .OrderBy(
                         item => item.Key))
        {
            report.AppendLine(
                $"     {parameter.Key}: " +
                $"{parameter.Value:F6}");
        }
    }

    private static void AppendRelationships(
        StringBuilder report,
        FeatureGraph graph)
    {
        report.AppendLine(
            "СВЯЗИ");

        report.AppendLine(
            "----------------------------------------");

        if (graph.Relationships.Count == 0)
        {
            report.AppendLine(
                "Связи отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (FeatureRelationship relationship
                 in graph.Relationships)
        {
            index++;

            report.AppendLine(
                $"{index}. {relationship.Type}");

            report.AppendLine(
                $"   Id: {relationship.Id}");

            report.AppendLine(
                $"   Source: " +
                $"{relationship.SourceFeatureId}");

            report.AppendLine(
                $"   Target: " +
                $"{relationship.TargetFeatureId}");

            report.AppendLine(
                $"   Описание: " +
                $"{relationship.Description}");

            if (relationship.Parameters.Count > 0)
            {
                report.AppendLine(
                    "   Параметры:");

                foreach (KeyValuePair<string, double> parameter
                         in relationship.Parameters
                             .OrderBy(
                                 item => item.Key))
                {
                    report.AppendLine(
                        $"     {parameter.Key}: " +
                        $"{parameter.Value:F6}");
                }
            }

            report.AppendLine();
        }
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}