#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnomalyPath.Editor.Art
{
    /// <summary>
    /// Рисует арт-заглушки для всех id из данных: иконки предметов и фоны локаций.
    /// Иконка изображает сам предмет — оружие, шлем, броню, рюкзак, пояс, патроны, артефакт, —
    /// а не подпись к нему. Файлы кладутся в <c>Assets/Art</c>, после чего пересобирается каталог спрайтов.
    /// </summary>
    public static class PlaceholderArtGenerator
    {
        private const int IconCell = 96;
        private const int IconDesignSize = 128;
        private const int BackgroundWidth = 1280;
        private const int BackgroundHeight = 720;

        /// <summary>Генерирует заглушки и пересобирает каталог.</summary>
        [MenuItem("Tools/AnomalyPath/Сгенерировать арт-заглушки")]
        public static void Generate()
        {
            SpriteCatalogBuilder.EnsureFolders();

            foreach (var icon in DataArtIds.Icons())
            {
                var name = icon.Id.StartsWith("icons/", StringComparison.Ordinal)
                    ? icon.Id.Substring("icons/".Length)
                    : icon.Id;
                var path = Path.Combine(SpriteCatalogBuilder.IconsFolder, name + ".png").Replace('\\', '/');
                WriteIcon(path, icon);
            }

            foreach (var background in DataArtIds.Backgrounds())
            {
                var path = Path.Combine(SpriteCatalogBuilder.BackgroundsFolder, background + ".png").Replace('\\', '/');
                WriteBackground(path, background);
            }

            foreach (var portrait in DataArtIds.Portraits())
            {
                var name = portrait.Id.StartsWith("portraits/", StringComparison.Ordinal)
                    ? portrait.Id.Substring("portraits/".Length)
                    : portrait.Id;
                var path = Path.Combine(SpriteCatalogBuilder.PortraitsFolder, name + ".png").Replace('\\', '/');
                WritePortrait(path, portrait);
            }

            var backdropPath = Path.Combine(SpriteCatalogBuilder.BackdropsFolder, ScreenBackdropName + ".png").Replace('\\', '/');
            WriteScreenBackdrop(backdropPath);

            AssetDatabase.Refresh();
            SpriteCatalogBuilder.Rebuild();
            Debug.Log("AnomalyPath: арт-заглушки сгенерированы, каталог собран");
        }

        private const string ScreenBackdropName = "screen";

        private static void WriteIcon(string path, DataArtIds.IconId icon)
        {
            var width = Mathf.Max(1, icon.FootprintWidth) * IconCell;
            var height = Mathf.Max(1, icon.FootprintHeight) * IconCell;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var baseColor = Shade(CategoryColor(icon.Category), icon.Id);
            var dark = Color.Lerp(baseColor, Color.black, 0.55f);
            var border = 5;
            var scale = Mathf.Min(width, height) / (float)IconDesignSize;
            var centerX = (width - 1) / 2f;
            var centerY = (height - 1) / 2f;
            var pixels = new Color32[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var t = (float)y / (height - 1);
                    var color = Color.Lerp(baseColor, dark, t * 0.8f);
                    if (x < border || y < border || x >= width - border || y >= height - border)
                    {
                        color = Color.Lerp(dark, Color.black, 0.35f);
                    }

                    var designX = (IconDesignSize / 2f) + ((x - centerX) / scale);
                    var designY = (IconDesignSize / 2f) + ((y - centerY) / scale);
                    if (designX >= 0 && designX < IconDesignSize && designY >= 0 && designY < IconDesignSize &&
                        InGlyph(icon, (int)designX, (int)designY))
                    {
                        color = Color.Lerp(color, Color.white, 0.85f);
                    }

                    pixels[(y * width) + x] = color;
                }
            }

            Save(texture, pixels, path);
        }

        private static void WriteBackground(string path, string seed)
        {
            var texture = new Texture2D(BackgroundWidth, BackgroundHeight, TextureFormat.RGBA32, false);
            var hash = StableHash(seed);
            var tint = Color.HSVToRGB((hash % 1000) / 1000f, 0.35f, 0.45f);
            var skyTop = Color.Lerp(tint, Color.black, 0.25f);
            var skyBottom = Color.Lerp(tint, Color.white, 0.25f);
            var ground = Color.Lerp(tint, Color.black, 0.65f);
            var horizon = (int)(BackgroundHeight * 0.62f);
            var pixels = new Color32[BackgroundWidth * BackgroundHeight];

            for (var y = 0; y < BackgroundHeight; y++)
            {
                for (var x = 0; x < BackgroundWidth; x++)
                {
                    Color color;
                    if (y < horizon)
                    {
                        color = Color.Lerp(skyBottom, skyTop, (float)y / horizon);
                    }
                    else
                    {
                        var t = (float)(y - horizon) / (BackgroundHeight - horizon);
                        color = Color.Lerp(ground, Color.Lerp(ground, Color.black, 0.5f), t);
                    }

                    pixels[(y * BackgroundWidth) + x] = color;
                }
            }

            var random = new System.Random(hash);
            var groundDark = Color.Lerp(ground, Color.black, 0.5f);
            for (var index = 0; index < 9; index++)
            {
                var width = random.Next(60, 180);
                var height = random.Next(40, 200);
                var left = random.Next(0, BackgroundWidth - width);
                var top = horizon - height;
                var silhouette = Color.Lerp(groundDark, ground, 0.25f);
                for (var y = top; y < horizon; y++)
                {
                    if (y < 0 || y >= BackgroundHeight)
                    {
                        continue;
                    }

                    for (var x = left; x < left + width; x++)
                    {
                        pixels[(y * BackgroundWidth) + x] = silhouette;
                    }
                }
            }

            Save(texture, pixels, path);
        }

        private static void WritePortrait(string path, DataArtIds.PortraitId portrait)
        {
            var width = 512;
            var height = string.Equals(portrait.Mode, "waist", StringComparison.Ordinal) ? 512 : 768;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            var hash = StableHash(portrait.Id);
            var baseColor = Color.HSVToRGB((hash % 1000) / 1000f, 0.35f, 0.55f);
            var dark = Color.Lerp(baseColor, Color.black, 0.55f);

            var headRadius = height * 0.075f;
            var headY = height * (height == 512 ? 0.72f : 0.82f);
            var shoulderY = height * (height == 512 ? 0.52f : 0.68f);
            var torsoBottom = height * (height == 512 ? 0.10f : 0.30f);
            var torsoHalf = width * 0.17f;
            var centerX = width / 2f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = x - centerX;
                    var inHead = (dx * dx) + ((y - headY) * (y - headY)) <= headRadius * headRadius;
                    var inTorso = Math.Abs(dx) <= torsoHalf && y >= torsoBottom && y <= shoulderY;
                    var inLegs = height != 512 &&
                                 y >= height * 0.04f && y < torsoBottom &&
                                 ((dx >= -torsoHalf * 0.9f && dx <= -torsoHalf * 0.1f) ||
                                  (dx >= torsoHalf * 0.1f && dx <= torsoHalf * 0.9f));
                    if (!inHead && !inTorso && !inLegs)
                    {
                        continue;
                    }

                    var t = (float)y / height;
                    pixels[(y * width) + x] = Color.Lerp(dark, baseColor, t * 0.8f);
                }
            }

            Save(texture, pixels, path);
        }

        private static void WriteScreenBackdrop(string path)
        {            const int width = 1280;
            const int height = 720;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var top = new Color(0.05f, 0.075f, 0.08f, 1f);
            var bottom = new Color(0.14f, 0.18f, 0.16f, 1f);
            var horizon = Color.Lerp(bottom, Color.black, 0.45f);
            var pixels = new Color32[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var t = (float)y / (height - 1);
                    var color = Color.Lerp(bottom, top, t * 0.9f);

                    var dx = (x - (width / 2f)) / (width / 2f);
                    var dy = (y - (height / 2f)) / (height / 2f);
                    var vignette = Mathf.Clamp01(Mathf.Sqrt((dx * dx) + (dy * dy)) * 1.15f);
                    color = Color.Lerp(color, Color.black, vignette * 0.25f);

                    var noise = StableHash($"{x}:{y}") % 100 / 100f;
                    color = Color.Lerp(color, Color.black, noise * 0.03f);

                    if (y < height * 0.14f)
                    {
                        var ridge = Mathf.PerlinNoise(x / 90f, 0.3f);
                        if (y < height * 0.14f * ridge)
                        {
                            color = horizon;
                        }
                    }

                    pixels[(y * width) + x] = color;
                }
            }

            Save(texture, pixels, path);
        }

        private static void Save(Texture2D texture, Color32[] pixels, string path)
        {
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static Color CategoryColor(string category)
        {
            switch (category)
            {
                case "weapon":
                    return new Color(0.35f, 0.47f, 0.33f, 1f);
                case "ammo":
                    return new Color(0.60f, 0.52f, 0.28f, 1f);
                case "armor":
                case "helmet":
                case "backpack":
                case "belt":
                    return new Color(0.33f, 0.43f, 0.55f, 1f);
                case "artifact":
                    return new Color(0.50f, 0.36f, 0.58f, 1f);
                case "talent":
                    return new Color(0.50f, 0.46f, 0.28f, 1f);
                default:
                    return new Color(0.42f, 0.42f, 0.38f, 1f);
            }
        }

        private static Color Shade(Color color, string id)
        {
            var shift = (StableHash(id) % 21) / 100f;
            return new Color(
                Mathf.Clamp01(color.r * (0.88f + shift)),
                Mathf.Clamp01(color.g * (0.88f + shift)),
                Mathf.Clamp01(color.b * (0.88f + shift)),
                color.a);
        }

        private static bool InGlyph(DataArtIds.IconId icon, int x, int y)
        {
            switch (icon.Category)
            {
                case "weapon":
                    return icon.FootprintHeight >= 3 ? Rifle(x, y) : Pistol(x, y);
                case "ammo":
                    return Cartridges(x, y);
                case "helmet":
                    return Helmet(x, y);
                case "armor":
                    return Armor(x, y);
                case "backpack":
                    return Backpack(x, y);
                case "belt":
                    return Belt(x, y);
                case "artifact":
                    return Math.Abs(x - 64) + Math.Abs(y - 64) <= 32;
                case "talent":
                    return TalentGlyph(icon.Id, x, y);
                default:
                    return Circle(x, y);
            }
        }

        private static bool Rifle(int x, int y)
        {
            var barrel = x >= 30 && x <= 120 && y >= 62 && y <= 72;
            var upper = x >= 46 && x <= 98 && y >= 72 && y <= 82;
            var stock = x >= 8 && x <= 32 && y >= 52 && y <= 84;
            var grip = x >= 74 && x <= 88 && y >= 38 && y <= 62;
            var magazine = x >= 52 && x <= 70 && y >= 34 && y <= 62;
            var sight = x >= 44 && x <= 54 && y >= 82 && y <= 90;
            return barrel || upper || stock || grip || magazine || sight;
        }

        private static bool Pistol(int x, int y)
        {
            var slide = x >= 26 && x <= 108 && y >= 58 && y <= 74;
            var grip = x >= 74 && x <= 100 && y >= 20 && y <= 58;
            var trigger = x >= 56 && x <= 74 && y >= 44 && y <= 54;
            return slide || grip || trigger;
        }

        private static bool Helmet(int x, int y)
        {
            var dx = (x - 64) / 40.0;
            var dy = (y - 58) / 34.0;
            var dome = (dx * dx) + (dy * dy) <= 1.0 && y >= 58;
            var brim = x >= 18 && x <= 110 && y >= 48 && y <= 58;
            return dome || brim;
        }

        private static bool Armor(int x, int y)
        {
            var body = x >= 36 && x <= 92 && y >= 20 && y <= 104;
            var shoulders = x >= 20 && x <= 108 && y >= 88 && y <= 104;
            var neck = x >= 56 && x <= 72 && y >= 96 && y <= 104;
            return (body || shoulders) && !neck;
        }

        private static bool Backpack(int x, int y)
        {
            var body = x >= 34 && x <= 94 && y >= 24 && y <= 100;
            var handle = x >= 54 && x <= 74 && y >= 100 && y <= 110;
            var strap = (x >= 42 && x <= 48) || (x >= 80 && x <= 86);
            return body || handle || (strap && y >= 24 && y <= 100);
        }

        private static bool Belt(int x, int y)
        {
            var band = x >= 12 && x <= 116 && y >= 54 && y <= 74;
            var buckle = x >= 54 && x <= 74 && y >= 46 && y <= 82;
            return band || buckle;
        }

        private static bool Cartridges(int x, int y) =>
            ((x >= 30 && x <= 46) || (x >= 56 && x <= 72) || (x >= 82 && x <= 98)) && y >= 34 && y <= 94;

        private static bool Circle(int x, int y) =>
            ((x - 64) * (x - 64)) + ((y - 64) * (y - 64)) <= 24 * 24;

        private static bool TalentGlyph(string id, int x, int y)
        {
            var dx = x - 64;
            var dy = y - 64;
            var ax = Math.Abs(dx);
            var ay = Math.Abs(dy);
            switch (StableHash(id) % 4)
            {
                case 0:
                    return ax + ay <= 34 && ax + ay >= 18;
                case 1:
                    return ((dx * dx) + (dy * dy) <= 34 * 34 && (dx * dx) + (dy * dy) >= 18 * 18) ||
                           (ax <= 4 && ay <= 34) ||
                           (ay <= 4 && ax <= 34);
                case 2:
                    return (ax <= 8 && ay <= 34) || (ay <= 8 && ax <= 34);
                default:
                    return (ax <= 6 && ay <= 34) || (ay <= 6 && ax <= 34) ||
                           (ax + ay <= 34 && ax + ay >= 18);
            }
        }

        private static int StableHash(string text)
        {
            var hash = 17;
            foreach (var symbol in text)
            {
                hash = unchecked((hash * 31) + symbol);
            }

            return hash == int.MinValue ? int.MaxValue : Math.Abs(hash);
        }
    }
}
