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
        /// <param name="kind">Вид урона.</param>
        /// <param name="stackable">Может ли складываться в один заход урона.</param>
        public DamageTypeTemplate(string id, string nameKey, DamageKind kind, bool stackable)
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
        public DamageKind Kind { get; }

        /// <summary>Может ли тип складываться в один заход урона.</summary>
        public bool Stackable { get; }

        /// <summary>Читает тип урона из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Тип урона.</returns>
        /// <exception cref="DataException">Поле отсутствует или неверного типа.</exception>
        public static DamageTypeTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var nameKey = DataMap.RequireString(value, "nameKey", sourcePath);
            var kindCode = DataMap.RequireString(value, "damageKind", sourcePath);
            if (!SchemaEnum.TryParse(kindCode, out DamageKind kind))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownDamageTypeKind, kindCode);
            }

            var stackable = DataMap.OptionalBool(value, "stackable", false);
            return new DamageTypeTemplate(id, nameKey, kind, stackable);
        }
    }
}
