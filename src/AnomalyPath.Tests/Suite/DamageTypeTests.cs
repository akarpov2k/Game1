#nullable enable
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Tests
{
    /// <summary>Типы урона из данных: загрузка, поля, доступ через репозиторий.</summary>
    public sealed class DamageTypeTests : TestBase
    {
        private readonly string _dataRoot;

        public DamageTypeTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var damageTypes = services.Resolve<IDamageTypeRepository>();

            Check("типы урона: пять типов загружены", () =>
            {
                Equal(5, damageTypes.All.Count, "типы урона");
            });

            Check("типы урона: пули — bullet", () =>
            {
                Equal(DamageKind.Bullet, damageTypes.Get("dmg_bullet").Kind, "вид");
            });

            Check("типы урона: аномалии — anomaly", () =>
            {
                Equal(DamageKind.Anomaly, damageTypes.Get("dmg_electric").Kind, "электро");
                Equal(DamageKind.Anomaly, damageTypes.Get("dmg_chemical").Kind, "химия");
                Equal(DamageKind.Anomaly, damageTypes.Get("dmg_fire").Kind, "огонь");
            });

            Check("типы урона: разрыв — physical", () =>
            {
                Equal(DamageKind.Physical, damageTypes.Get("dmg_rupture").Kind, "вид");
            });

            Check("типы урона: все ключи защиты существуют", () =>
            {
                True(damageTypes.Contains("dmg_bullet"), "пули");
                True(damageTypes.Contains("dmg_electric"), "электричество");
                True(damageTypes.Contains("dmg_chemical"), "химия");
                True(damageTypes.Contains("dmg_fire"), "огонь");
                True(damageTypes.Contains("dmg_rupture"), "разрыв");
                False(damageTypes.Contains("dmg_wood"), "неизвестный");
            });

            Check("типы урона: неизвестный id не найден", () =>
            {
                False(damageTypes.TryGet("dmg_wood", out _), "TryGet");
            });

            Check("DI: репозиторий типов урона доступен", () =>
            {
                True(services.IsRegistered<IDamageTypeRepository>(), "зарегистрирован");
            });
        }
    }
}
