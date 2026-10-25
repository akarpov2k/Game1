#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Ui.Art;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AnomalyPath.Editor.Art
{
    /// <summary>
    /// Собирает каталог спрайтов из <c>Assets/Art</c>, кладёт его ассетом и назначает на <c>GameScreen</c> в сцене Main.
    /// </summary>
    public static class SpriteCatalogBuilder
    {
        /// <summary>Путь к ассету каталога.</summary>
        public const string CatalogPath = "Assets/Art/SpriteCatalog.asset";

        /// <summary>Корень арта.</summary>
        public const string ArtRoot = "Assets/Art";

        /// <summary>Папка иконок: файл <c>wpn_ak74.png</c> получает id <c>icons/wpn_ak74</c>.</summary>
        public const string IconsFolder = "Assets/Art/Icons";

        /// <summary>Папка фонов: файл <c>bg_hub_settlement.png</c> получает одноимённый id.</summary>
        public const string BackgroundsFolder = "Assets/Art/Backgrounds";

        /// <summary>Папка тематических задников окон: id с префиксом <c>backdrops/</c>.</summary>
        public const string BackdropsFolder = "Assets/Art/Backdrops";

        /// <summary>Папка портретов актёров: id с префиксом <c>portraits/</c>.</summary>
        public const string PortraitsFolder = "Assets/Art/Portraits";

        /// <summary>Сцена, где лежит <c>GameScreen</c>.</summary>
        public const string MainScene = "Assets/Scenes/Main.unity";

        private const string GameScreenField = "spriteCatalog";

        /// <summary>Пересобирает каталог, импортирует PNG как спрайты и назначает каталог в сцену.</summary>
        [MenuItem("Tools/AnomalyPath/Собрать каталог спрайтов")]
        public static void Rebuild()
        {
            EnsureFolders();
            EnsureSpriteImporters();

            var entries = CollectEntries();
            var catalog = LoadOrCreateCatalog();
            catalog.SetEntries(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            WireScene(catalog);
            ReportMissing(entries);

            AssetDatabase.Refresh();
            Debug.Log($"AnomalyPath: каталог спрайтов собран — {entries.Count} записей: {CatalogPath}");
        }

        /// <summary>Создаёт папки арта, если их нет.</summary>
        internal static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(ArtRoot))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }

            if (!AssetDatabase.IsValidFolder(IconsFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "Icons");
            }

            if (!AssetDatabase.IsValidFolder(BackgroundsFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "Backgrounds");
            }

            if (!AssetDatabase.IsValidFolder(BackdropsFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "Backdrops");
            }

            if (!AssetDatabase.IsValidFolder(PortraitsFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "Portraits");
            }
        }

        /// <summary>Переводит PNG в режим Sprite, чтобы из них получались спрайты для uGUI.</summary>
        internal static void EnsureSpriteImporters()
        {
            foreach (var folder in new[] { IconsFolder, BackgroundsFolder, BackdropsFolder, PortraitsFolder })
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
                {
                    var path = file.Replace('\\', '/');
                    if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                    {
                        continue;
                    }

                    var changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }

                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        changed = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static List<SpriteCatalog.Entry> CollectEntries()
        {
            var entries = new List<SpriteCatalog.Entry>();
            AddFolder(entries, IconsFolder, "icons/");
            AddFolder(entries, BackgroundsFolder, string.Empty);
            AddFolder(entries, BackdropsFolder, "backdrops/");
            AddFolder(entries, PortraitsFolder, "portraits/");
            return entries;
        }

        private static void AddFolder(List<SpriteCatalog.Entry> entries, string folder, string prefix)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
            {
                var path = file.Replace('\\', '/');
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    continue;
                }

                entries.Add(new SpriteCatalog.Entry(prefix + Path.GetFileNameWithoutExtension(path), sprite));
            }
        }

        private static SpriteCatalog LoadOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>(CatalogPath);
            if (catalog != null)
            {
                return catalog;
            }

            catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        private static void WireScene(SpriteCatalog catalog)
        {
            var scenePath = Path.GetFullPath(MainScene);
            if (!File.Exists(scenePath))
            {
                UnityEngine.Debug.LogWarning($"AnomalyPath: сцена {scenePath} не найдена, каталог не назначен");
                return;
            }

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEngine.Debug.LogWarning("AnomalyPath: сцена не сохранена, каталог не назначен");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var screen = UnityEngine.Object.FindAnyObjectByType<GameScreen>();
            if (screen == null)
            {
                UnityEngine.Debug.LogWarning("AnomalyPath: на сцене Main нет GameScreen");
                return;
            }

            var serialized = new SerializedObject(screen);
            var property = serialized.FindProperty(GameScreenField);
            if (property == null)
            {
                UnityEngine.Debug.LogWarning($"AnomalyPath: у GameScreen нет поля {GameScreenField}");
                return;
            }

            property.objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            var saved = EditorSceneManager.SaveScene(scene, scenePath);
            var written = saved && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(catalog, out var guid, out _)
                ? File.ReadAllText(scenePath).Contains(guid, StringComparison.Ordinal)
                : false;
            if (written)
            {
                UnityEngine.Debug.Log($"AnomalyPath: каталог назначен на GameScreen в {MainScene}");
            }
            else
            {
                UnityEngine.Debug.LogWarning(
                    $"AnomalyPath: каталог не записался в сцену {MainScene} — назначьте поле spriteCatalog на GameScreen вручную");
            }
        }

        private static void ReportMissing(List<SpriteCatalog.Entry> entries)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                ids.Add(entry.Id);
            }

            var missing = new List<string>();
            foreach (var icon in DataArtIds.Icons())
            {
                if (!ids.Contains(icon.Id))
                {
                    missing.Add(icon.Id);
                }
            }

            foreach (var background in DataArtIds.Backgrounds())
            {
                if (!ids.Contains(background))
                {
                    missing.Add(background);
                }
            }

            foreach (var portrait in DataArtIds.Portraits())
            {
                if (!ids.Contains(portrait.Id))
                {
                    missing.Add(portrait.Id);
                }
            }

            if (missing.Count > 0)
            {
                UnityEngine.Debug.LogWarning(
                    "AnomalyPath: нет спрайтов для id: " + string.Join(", ", missing) +
                    ". Каталог собирается только из готовых PNG — файлы рисует " +
                    "Tools/AnomalyPath/Сгенерировать арт-заглушки");
            }
        }
    }
}
