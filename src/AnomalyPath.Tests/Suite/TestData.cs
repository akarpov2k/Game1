#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Tests
{
    /// <summary>Тестовые предметы и вспомогательные данные для фикстур.</summary>
    internal static class TestData
    {
        internal const string Rifle = "wpn_test";
        internal const string Ammo = "ammo_test";
        internal const string Crate = "box_test";
        internal const string Part = "part_test";

        internal const string RifleJson =
            "{\"kind\":\"items\",\"id\":\"wpn_test\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[2,3],\"maxStack\":1,\"tier\":1,\"icon\":\"i\"," +
            "\"weapon\":{\"classId\":\"w\",\"damage\":1,\"accuracy\":0.5,\"magazine\":10,\"ammoCaliber\":\"c\",\"reloadTime\":2,\"durabilityMax\":100,\"durabilityPerShot\":0.1}}";

        internal const string AmmoJson =
            "{\"kind\":\"items\",\"id\":\"ammo_test\",\"nameKey\":\"n\",\"category\":\"ammo\",\"footprint\":[1,1],\"maxStack\":60,\"caliber\":\"c\",\"tier\":1,\"icon\":\"i\"}";

        internal const string CrateJson =
            "{\"kind\":\"items\",\"id\":\"box_test\",\"nameKey\":\"n\",\"category\":\"material\",\"footprint\":[1,2],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"rotatable\":false}";

        internal const string PartJson =
            "{\"kind\":\"items\",\"id\":\"part_test\",\"nameKey\":\"n\",\"category\":\"material\",\"footprint\":[1,1],\"maxStack\":5,\"tier\":1,\"icon\":\"i\"," +
            "\"weapon\":{\"classId\":\"w\",\"damage\":1,\"accuracy\":0.5,\"magazine\":1,\"ammoCaliber\":\"c\",\"reloadTime\":1,\"durabilityMax\":100,\"durabilityPerShot\":0.1}}";

        /// <summary>Источник данных, считающий реальные чтения файлов.</summary>
        internal sealed class CountingSource : IDataSource
        {
            private readonly IDataSource _inner;

            public CountingSource(IDataSource inner)
            {
                _inner = inner;
            }

            public int Reads { get; private set; }

            public bool Exists(string relativePath) => _inner.Exists(relativePath);

            public string ReadAllText(string relativePath)
            {
                Reads++;
                return _inner.ReadAllText(relativePath);
            }

            public IReadOnlyList<string> EnumerateFiles() => _inner.EnumerateFiles();
        }
    }
}
