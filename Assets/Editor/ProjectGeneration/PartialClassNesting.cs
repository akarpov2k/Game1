#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace AnomalyPath.Editor
{
    /// <summary>
    /// Группирует partial-части под основным файлом в Solution Explorer. Unity стирает
    /// <c>.csproj</c> при каждой генерации, поэтому вложенность добавляется здесь — в
    /// <c>AssetPostprocessor.OnGeneratedCSProject</c>, — а не правкой проекта руками.
    /// Часть <c>Имя.Область.cs</c> получает <c>DependentUpon</c> на <c>Имя.cs</c> из той же папки.
    /// </summary>
    public sealed class PartialClassNesting : AssetPostprocessor
    {
        /// <summary>Дополняет сгенерированный проект вложенностью partial-частей.</summary>
        /// <param name="path">Путь к сгенерированному проекту.</param>
        /// <param name="content">Содержимое проекта, подготовленное Unity.</param>
        /// <returns>Проект с метаданными вложенности; при ошибке — исходное содержимое.</returns>
        public static string OnGeneratedCSProject(string path, string content)
        {
            try
            {
                return NestPartialFiles(content);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Не удалось сгруппировать partial-части в {path}: {exception.Message}");
                return content;
            }
        }

        private static string NestPartialFiles(string content)
        {
            XDocument document = XDocument.Parse(content, LoadOptions.PreserveWhitespace);
            XElement? project = document.Root;
            if (project == null)
            {
                return content;
            }

            XNamespace xmlNamespace = project.Name.Namespace;
            List<XElement> compiles = new List<XElement>(project.Descendants(xmlNamespace + "Compile"));
            HashSet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (XElement compile in compiles)
            {
                string? include = (string?)compile.Attribute("Include");
                if (include != null)
                {
                    files.Add(Normalize(include));
                }
            }

            bool changed = false;
            foreach (XElement compile in compiles)
            {
                string? include = (string?)compile.Attribute("Include");
                if (include == null)
                {
                    continue;
                }

                string? mainFile = MainFileOf(include);
                if (mainFile == null || !files.Contains(mainFile))
                {
                    continue;
                }

                string dependentName = Path.GetFileName(mainFile);
                XElement? dependentUpon = compile.Element(xmlNamespace + "DependentUpon");
                if (dependentUpon == null)
                {
                    dependentUpon = new XElement(xmlNamespace + "DependentUpon");
                    compile.Add(dependentUpon);
                }

                if (dependentUpon.Value == dependentName)
                {
                    continue;
                }

                dependentUpon.Value = dependentName;
                changed = true;
            }

            return changed ? document.ToString(SaveOptions.DisableFormatting) : content;
        }

        private static string? MainFileOf(string include)
        {
            string file = Normalize(include);
            string name = Path.GetFileName(file);
            if (!name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            int separator = name.IndexOf('.');
            if (separator <= 0 || separator == name.Length - 3)
            {
                return null;
            }

            string mainName = name.Substring(0, separator) + ".cs";
            string? directory = Path.GetDirectoryName(file);
            return string.IsNullOrEmpty(directory) ? mainName : directory + "\\" + mainName;
        }

        private static string Normalize(string path)
        {
            return path.Replace('/', '\\');
        }
    }
}
