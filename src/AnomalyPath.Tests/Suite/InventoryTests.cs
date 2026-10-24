#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Tests
{
    public sealed class InventoryTests : TestBase
    {



        public override void Run()
        {
            Check("инвентарь: предмет встаёт в пустую сетку", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                True(service.TryAdd(grid, TestData.Rifle, 1, out var overflow), "добавление");
                Equal(0, overflow.Count, "переполнение");
                Equal(6, grid.UsedCells, "занятые клетки");
                Equal("wpn_test", grid.Items[0].ItemId, "id предмета");
                Equal(0, grid.Items[0].X, "X");
                Equal(0, grid.Items[0].Y, "Y");
            });

            Check("инвентарь: поворот, когда иначе не влезает", () =>
            {
                var (service, grid) = NewInventory(4, 2);
                True(service.TryAdd(grid, TestData.Rifle, 1, out _), "добавление");
                True(grid.Items[0].Rotated, "предмет должен быть повёрнут");
                Equal(3, grid.Items[0].PlacedWidth, "ширина после поворота");
                Equal(2, grid.Items[0].PlacedHeight, "высота после поворота");
            });

            Check("инвентарь: невместившийся предмет уходит в переполнение", () =>
            {
                var (service, grid) = NewInventory(1, 1);
                True(service.TryAdd(grid, TestData.Rifle, 1, out var overflow), "добавление");
                Equal(1, overflow.Count, "переполнение");
                True(grid.Items[0].Overflow, "флаг переполнения");
                Equal(0, grid.UsedCells, "занятые клетки");
                Equal(1, service.Count(grid, TestData.Rifle), "количество");
            });

            Check("инвентарь: пересечение не допускается", () =>
            {
                var (service, grid) = NewInventory(2, 3);
                True(service.TryAdd(grid, TestData.Rifle, 2, out var overflow), "добавление");
                Equal(1, overflow.Count, "второй обязан уйти в переполнение");
                Equal(1, grid.UsedCells > 0 ? 1 : 0, "что-то placed");
            });

            Check("инвентарь: неповорачиваемый предмет не поворачивается", () =>
            {
                var (service, grid) = NewInventory(2, 1);
                True(service.TryAdd(grid, TestData.Crate, 1, out var overflow), "добавление");
                Equal(1, overflow.Count, "переполнение");
                Equal(0, grid.UsedCells, "занятые клетки");
            });

            Check("инвентарь: одинаковые стопки сливаются", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Ammo, 30, out _);
                service.TryAdd(grid, TestData.Ammo, 30, out _);
                Equal(1, PlacedCount(grid), "одна стопка");
                Equal(60, service.Count(grid, TestData.Ammo), "количество");
            });

            Check("инвентарь: предел стопки соблюдается", () =>
            {
                var (service, grid) = NewInventory(1, 1);
                True(service.TryAdd(grid, TestData.Ammo, 130, out var overflow), "добавление");
                Equal(2, overflow.Count, "лишнее ушло в переполнение");
                Equal(130, service.Count(grid, TestData.Ammo), "всё количество сохранено");
                True(overflow[0].Stack <= 60, "стопка не больше предела");
            });

            Check("инвентарь: разная прочность не сливается", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Part, 1, 0, null, out _);
                service.TryAdd(grid, TestData.Part, 1, 100, null, out _);
                service.TryAdd(grid, TestData.Part, 1, 0, null, out _);
                Equal(3, service.Count(grid, TestData.Part), "количество");
                Equal(2, PlacedCount(grid), "стопки: 0 и 100");
            });

            Check("инвентарь: граничные значения прочности стакаются порознь", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Part, 1, 0, null, out _);
                service.TryAdd(grid, TestData.Part, 1, 0, null, out _);
                Equal(1, PlacedCount(grid), "нулевая прочность — одна стопка");
                Equal(2, grid.Items[0].Stack, "в стопке две штуки");
            });

            Check("инвентарь: новое оружие (запас торговца) получает пустой магазин", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Rifle, 1, out _);
                Equal(0, grid.Items[0].MagazineRounds, "магазин пуст");
            });

            Check("инвентарь: число патронов в магазине переносится при добавлении", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Rifle, 1, null, null, 5, out _);
                Equal(5, grid.Items[0].MagazineRounds, "магазин сохранён");
            });

            Check("инвентарь: TakeAmmo берёт из самой маленькой стопки", () =>
            {
                var (service, grid) = NewInventory(8, 8);
                var template = Template(TestData.AmmoJson);
                var large = PlaceAmmo(grid, template, 0, 9);
                var small = PlaceAmmo(grid, template, 1, 2);

                var taken = service.TakeAmmo(grid, "c", 5);

                Equal(5, taken, "взято");
                Equal(6, large.Stack, "из большой стопки добирали остаток");
                True(grid.Find(small.InstanceId) == null, "маленькая стопка израсходована");
                Equal(1, PlacedCount(grid), "осталась одна стопка");
            });

            Check("инвентарь: TakeAmmo расходует неполную стопку раньше полной", () =>
            {
                var (service, grid) = NewInventory(8, 8);
                var template = Template(TestData.AmmoJson);
                var full = PlaceAmmo(grid, template, 0, 60);
                var part = PlaceAmmo(grid, template, 1, 10);

                var taken = service.TakeAmmo(grid, "c", 5);

                Equal(5, taken, "взято");
                Equal(5, part.Stack, "неполная стопка");
                Equal(60, full.Stack, "полная стопка не тронута");
            });

            Check("инвентарь: TakeAmmo не берёт чужой калибр и переполнение", () =>
            {
                var (service, grid) = NewInventory(8, 8);
                var template = Template(TestData.AmmoJson);
                PlaceAmmo(grid, template, 0, 5);
                var overflow = new ItemInstance("overflow", TestData.Ammo, 20);
                grid.AddOverflow(overflow);

                Equal(0, service.TakeAmmo(grid, "other", 3), "чужой калибр");
                Equal(3, service.TakeAmmo(grid, "c", 3), "свой калибр");
                Equal(20, overflow.Stack, "переполнение не тронуто");
            });

            Check("инвентарь: TakeAmmoFrom берёт только из указанной стопки", () =>
            {
                var (service, grid) = NewInventory(8, 8);
                var template = Template(TestData.AmmoJson);
                var first = PlaceAmmo(grid, template, 0, 5);
                var second = PlaceAmmo(grid, template, 1, 20);

                var taken = service.TakeAmmoFrom(grid, second.InstanceId, 7);

                Equal(7, taken, "взято");
                Equal(5, first.Stack, "первая стопка не тронута");
                Equal(13, second.Stack, "вторая стопка");
                Equal(0, service.TakeAmmoFrom(grid, second.InstanceId, 0), "нулевой запрос");
                Equal(0, service.TakeAmmo(grid, "c", 0), "нулевой запрос по калибру");
            });

            Check("инвентарь: TakeAmmoFrom не берёт из не-патронов", () =>
            {
                var (service, grid) = NewInventory(8, 8);
                service.TryAdd(grid, TestData.Part, 1, out _);

                Equal(0, service.TakeAmmoFrom(grid, grid.Items[0].InstanceId, 1), "не патроны");
            });

            Check("инвентарь: перемещение в свободную клетку", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Rifle, 1, out _);
                var id = grid.Items[0].InstanceId;
                True(service.TryMove(grid, id, 1, 1, false), "перемещение");
                Equal(1, grid.Items[0].X, "X");
                Equal(1, grid.Items[0].Y, "Y");
                Equal(6, grid.UsedCells, "занятые клетки не изменились");
            });

            Check("инвентарь: перемещение на занятую клетку запрещено", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Crate, 1, out _);
                service.TryAdd(grid, TestData.Crate, 1, out _);
                var second = grid.Items[1];
                False(service.TryMove(grid, second.InstanceId, 0, 0, false), "перемещение");
            });

            Check("инвентарь: удаление освобождает клетки", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Rifle, 1, out _);
                var id = grid.Items[0].InstanceId;
                True(service.Remove(grid, id), "удаление");
                Equal(0, grid.UsedCells, "занятые клетки");
                Equal(16, grid.FreeCells, "свободные клетки");
            });

            Check("инвентарь: неизвестный предмет — ошибка", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                Throws<DataException>(() => service.TryAdd(grid, "wpn_unknown", 1, out _), "неизвестный id");
            });

            Check("инвентарь: инвариант клеток", () =>
            {
                var (service, grid) = NewInventory(4, 4);
                service.TryAdd(grid, TestData.Rifle, 1, out _);
                service.TryAdd(grid, TestData.Ammo, 10, out _);
                Equal(grid.Width * grid.Rows, grid.UsedCells + grid.FreeCells, "сумма клеток");
            });

            Check("инвентарь: нулевая прочность означает сломанный предмет", () =>
            {
                True(new ItemInstance("a", TestData.Part, 1, 0, 40).IsBroken, "0");
                False(new ItemInstance("b", TestData.Part, 1, 1, 40).IsBroken, "1");
                False(new ItemInstance("c", TestData.Part, 1).IsBroken, "без прочности");
            });

            Check("DI: сервис инвентаря доступен", () =>
            {
                var container = Bootstrap.Build(ResolveDataRootForTests());
                True(container.IsRegistered<IInventoryService>(), "зарегистрирован");
                True(container.IsRegistered<IInstanceIdGenerator>(), "генератор id");
            });
        }
        private static ItemInstance PlaceAmmo(GridContainer grid, ItemTemplate template, int x, int count)
        {
            var stack = new ItemInstance("ammo_" + x + "_" + count, TestData.Ammo, count);
            grid.Place(stack, template, x, 0, false);
            return stack;
        }
    }
}
