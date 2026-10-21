#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Templates
{
    /// <summary>Тип урона из данных: пули, аномалии, разрыв.</summary>
    public sealed class DamageTypeTemplate
    {
        /// <summary>Создаёт тип урона.</summary>
        /// <param name="id">Идентификатор, например <c>dmg_bullet</c>.</param>
        /// <param name="nameKey">Ключ названия в переводах.</param>
        /// <param name="kind">Вид урона: <c>bullet</c>, <c>anomaly</c>, <c>physical</c>, <c>elemental</c>.</param>
        /// <param name="stackable">Может ли складываться в один заход урона.</param>
        public DamageTypeTemplate(string id, string nameKey, string kind, bool stackable)
        {
            Id = id;
            NameKey = nameKey;
            Kind = kind;
            Stackable = stackable;
        }

        /// <summary>Идентификатор типа урона.</summary>
        public string Id { get; }

        /// <summary>Ключ названия в переводах.</summary>
        public string NameKey { get; }

        /// <summary>Вид урона.</summary>
        public string Kind { get; }

        /// <summary>Может ли тип складываться в один заход урона.</summary>
        public bool Stackable { get; }

        /// <summary>Проверяет, известен ли вид урона.</summary>
        /// <param name="kind">Значение поля <c>kind</c>.</param>
        /// <returns><c>true</c>, если вид поддерживается.</returns>
        public static bool IsKnownKind(string kind) =>
            kind == "bullet" || kind == "anomaly" || kind == "physical" || kind == "elemental";

        /// <summary>Читает тип урона из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Тип урона.</returns>
        /// <exception cref="DataException">Поле отсутствует или неверного типа.</exception>
        public static DamageTypeTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var nameKey = DataMap.RequireString(value, "nameKey", sourcePath);
            var kind = DataMap.RequireString(value, "damageKind", sourcePath);
            if (!IsKnownKind(kind))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownDamageTypeKind, kind);
            }

            var stackable = DataMap.OptionalBool(value, "stackable", false);
            return new DamageTypeTemplate(id, nameKey, kind, stackable);
        }
    }
}
