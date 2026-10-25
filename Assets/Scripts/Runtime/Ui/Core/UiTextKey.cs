#nullable enable
namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Ключи видимых текстов интерфейса. Код ресурса — <c>ui.</c> + имя члена в нижнем регистре,
    /// сами тексты лежат в <c>strings.{ru,en}.json</c>.
    /// </summary>
    public enum UiTextKey
    {
        /// <summary>Кнопка «Назад».</summary>
        Common_Back,

        /// <summary>Кнопка «Отмена».</summary>
        Common_Cancel,

        /// <summary>Кнопка «Выход».</summary>
        Common_Quit,

        /// <summary>Кнопка «Удалить».</summary>
        Common_Delete,

        /// <summary>Заголовок подтверждения.</summary>
        Common_Confirm_Title,

        /// <summary>Сообщение «нет активной игры».</summary>
        Common_No_Game,

        /// <summary>Пустое значение слота.</summary>
        Common_Dash,

        /// <summary>Крестик закрытия или удаления.</summary>
        Common_Close,

        /// <summary>Метка выбранного элемента.</summary>
        Common_Selected,

        /// <summary>Деньги с подписью.</summary>
        Common_Money,

        /// <summary>Уровень строчными буквами.</summary>
        Common_Level,

        /// <summary>Уровень с заглавной буквы.</summary>
        Common_Level_Cap,

        /// <summary>Строка «день, часы» строчными.</summary>
        Common_Day_Hours,

        /// <summary>Строка «День, часы» с заглавной.</summary>
        Common_Day_Hours_Tile,

        /// <summary>Счётчик занятых клеток.</summary>
        Common_Count_Of,

        /// <summary>Строка переполнения набора.</summary>
        Common_Not_Placed,

        /// <summary>Название игры.</summary>
        MainMenu_Game_Title,

        /// <summary>Подзаголовок меню.</summary>
        MainMenu_Subtitle,

        /// <summary>Кнопка новой игры.</summary>
        MainMenu_New_Game,

        /// <summary>Кнопка загрузки.</summary>
        MainMenu_Load,

        /// <summary>Кнопка «Об игре».</summary>
        MainMenu_About,

        /// <summary>Кнопка «Авторы».</summary>
        MainMenu_Authors,

        /// <summary>Текст «Об игре».</summary>
        MainMenu_About_Text,

        /// <summary>Текст «Авторы».</summary>
        MainMenu_Authors_Text,

        /// <summary>Заголовок новой игры.</summary>
        NewGame_Title,

        /// <summary>Подпись поля имени.</summary>
        NewGame_Name_Label,

        /// <summary>Подсказка поля имени.</summary>
        NewGame_Name_Placeholder,

        /// <summary>Кнопка случайного имени.</summary>
        NewGame_Random_Name,

        /// <summary>Подпись выбора набора.</summary>
        NewGame_Loadout_Label,

        /// <summary>Подпись коэффициента страховки надетых вещей.</summary>
        NewGame_Insurance_Label,

        /// <summary>Подпись усложнения торговли.</summary>
        NewGame_Trade_Label,

        /// <summary>Кнопка «Начать».</summary>
        NewGame_Start,

        /// <summary>Нет доступных наборов (подпись).</summary>
        NewGame_No_Loadouts,

        /// <summary>Ошибка предпросмотра набора.</summary>
        NewGame_Preview_Failed,

        /// <summary>Статус: нет доступных наборов.</summary>
        NewGame_Status_No_Sets,

        /// <summary>Статус: введите имя.</summary>
        NewGame_Status_Enter_Name,

        /// <summary>Статус: имя слишком длинное.</summary>
        NewGame_Status_Name_Too_Long,

        /// <summary>Статус: ошибка старта.</summary>
        NewGame_Status_Error,

        /// <summary>Заголовок окна инвентаря набора.</summary>
        NewGame_Preview_Title,

        /// <summary>Окно инвентаря пусто.</summary>
        NewGame_Preview_Empty,

        /// <summary>Заголовок экрана загрузки.</summary>
        Load_Title,

        /// <summary>Нет сохранений.</summary>
        Load_Empty,

        /// <summary>Заголовок удаления сохранения.</summary>
        Load_Delete_Title,

        /// <summary>Вопрос удаления сохранения.</summary>
        Load_Delete_Question,

        /// <summary>Ошибка удаления.</summary>
        Load_Delete_Error,

        /// <summary>Сохранение удалено.</summary>
        Load_Deleted,

        /// <summary>Ошибка загрузки.</summary>
        Load_Load_Error,

        /// <summary>Заголовок экрана сохранения.</summary>
        SaveGame_Title,

        /// <summary>Подпись выбора ячейки.</summary>
        SaveGame_Section,

        /// <summary>Нет активной игры на экране сохранения.</summary>
        SaveGame_Status_No_Game,

        /// <summary>Сохранение записано.</summary>
        SaveGame_Toast_Saved,

        /// <summary>Ошибка сохранения.</summary>
        SaveGame_Status_Error,

        /// <summary>Заголовок диалога имени сохранения.</summary>
        SaveDialog_Title,

        /// <summary>Подпись поля имени сохранения.</summary>
        SaveDialog_Name_Label,

        /// <summary>Подсказка поля имени сохранения.</summary>
        SaveDialog_Name_Placeholder,

        /// <summary>Кнопка «Сохранить».</summary>
        SaveDialog_Save,

        /// <summary>Ошибка пустого имени.</summary>
        SaveDialog_Empty_Name,

        /// <summary>Заголовок диалога выхода.</summary>
        Exit_Title,

        /// <summary>Текст диалога выхода.</summary>
        Exit_Body,

        /// <summary>Кнопка «В меню».</summary>
        Exit_To_Menu,

        /// <summary>Кнопка «На рабочий стол».</summary>
        Exit_To_Desktop,

        /// <summary>Кнопка «Сохранить игру».</summary>
        Game_Save,

        /// <summary>Кнопка «Загрузить игру».</summary>
        Game_Load,

        /// <summary>Кнопка «Выйти».</summary>
        Game_Exit,

        /// <summary>Игра начата.</summary>
        Game_Started,

        /// <summary>Сводка состояния игрока.</summary>
        Game_Status,

        /// <summary>Заголовок меню игры.</summary>
        GameMenu_Title,

        /// <summary>Кнопка «Продолжить» в меню игры.</summary>
        GameMenu_Continue,

        /// <summary>Заголовок панели действий.</summary>
        Game_Actions,

        /// <summary>Кнопка перехода с названием локации.</summary>
        Game_Travel,

        /// <summary>Нет доступных действий.</summary>
        Game_No_Actions,

        /// <summary>В локации идёт бой.</summary>
        Game_In_Combat,

        /// <summary>Подсказка области объекта.</summary>
        Game_Object_Hint,

        /// <summary>Заголовок области объекта.</summary>
        Game_Object,

        /// <summary>Номер дня и время на часах основного экрана.</summary>
        Game_Clock,

        /// <summary>Сведения о локации на основном экране.</summary>
        Game_Location_Summary,

        /// <summary>Роль локации: хаб.</summary>
        Game_Node_Hub,

        /// <summary>Роль локации: дорога.</summary>
        Game_Node_Route,

        /// <summary>Роль локации: точка интереса.</summary>
        Game_Node_Site,

        /// <summary>Роль локации: помещение.</summary>
        Game_Node_Interior,

        /// <summary>Переход выполнен.</summary>
        Game_Travel_Success,

        /// <summary>Ребро перехода не найдено.</summary>
        Game_Travel_EdgeNotFound,

        /// <summary>Переход начинается не из текущей локации.</summary>
        Game_Travel_WrongLocation,

        /// <summary>Переход запрещён во время боя.</summary>
        Game_Travel_InCombat,

        /// <summary>Ошибка перехода с текстом ошибки.</summary>
        Game_Travel_Error,

        /// <summary>Заголовок экрана персонажа.</summary>
        Character_Title,

        /// <summary>Пункт меню «Снять».</summary>
        Character_Menu_Unequip,

        /// <summary>Пункт меню «Одеть».</summary>
        Character_Menu_Equip,

        /// <summary>Пункт меню «Разрядить».</summary>
        Character_Menu_Unload,

        /// <summary>Пункт меню «Зарядить».</summary>
        Character_Menu_Load,

        /// <summary>Пункт меню «Разобрать».</summary>
        Character_Menu_Disassemble,

        /// <summary>Пункт меню «Выбросить».</summary>
        Character_Menu_Discard,

        /// <summary>Заголовок инвентаря.</summary>
        Character_Inventory_Title,

        /// <summary>Строка переполнения инвентаря.</summary>
        Character_Overflow,

        /// <summary>Подпись слота шлема.</summary>
        Character_Slot_Helmet,

        /// <summary>Подпись слота брони.</summary>
        Character_Slot_Armor,

        /// <summary>Подпись слота рюкзака.</summary>
        Character_Slot_Backpack,

        /// <summary>Подпись слота пояса.</summary>
        Character_Slot_Belt,

        /// <summary>Подпись слота дополнительного оружия.</summary>
        Character_Slot_Secondary,

        /// <summary>Подпись слота основного оружия.</summary>
        Character_Slot_Main,

        /// <summary>Подпись характеристики здоровья.</summary>
        Character_Stat_Health,

        /// <summary>Подпись характеристики еды.</summary>
        Character_Stat_Food,

        /// <summary>Подпись характеристики воды.</summary>
        Character_Stat_Water,

        /// <summary>Подпись характеристики выносливости.</summary>
        Character_Stat_Stamina,

        /// <summary>Сообщение «надето».</summary>
        Character_Action_Equipped,

        /// <summary>Сообщение «снято».</summary>
        Character_Action_Unequipped,

        /// <summary>Сообщение «артефакт надет».</summary>
        Character_Action_Artifact_Equipped,

        /// <summary>Сообщение о разрядке магазина.</summary>
        Character_Action_Unloaded,

        /// <summary>Сообщение о зарядке магазина.</summary>
        Character_Action_Loaded,

        /// <summary>Сообщение о разборе предмета.</summary>
        Character_Action_Disassembled,

        /// <summary>Сообщение о выбросе предмета.</summary>
        Character_Action_Discarded,

        /// <summary>Результат «готово».</summary>
        Character_Result_Ok,

        /// <summary>Результат: предмет не найден.</summary>
        Character_Result_Item_Missing,

        /// <summary>Результат: неверный слот.</summary>
        Character_Result_Wrong_Slot,

        /// <summary>Результат: нет места.</summary>
        Character_Result_No_Room,

        /// <summary>Результат: не артефакт.</summary>
        Character_Result_Not_Artifact,

        /// <summary>Результат: нет свободного слота артефакта.</summary>
        Character_Result_No_Free_Artifact_Slot,

        /// <summary>Результат: не оружие.</summary>
        Character_Result_Not_A_Weapon,

        /// <summary>Результат: магазин пуст.</summary>
        Character_Result_Nothing_To_Unload,

        /// <summary>Результат: магазин уже полон.</summary>
        Character_Result_Magazine_Full,

        /// <summary>Результат: предмет не является патронами.</summary>
        Character_Result_Not_Ammo,

        /// <summary>Результат: калибр патронов не подходит к оружию.</summary>
        Character_Result_Ammo_Caliber_Mismatch,

        /// <summary>Результат: в инвентаре нет подходящих патронов.</summary>
        Character_Result_No_Ammo,

        /// <summary>Результат: нет патронов.</summary>
        Character_Result_Ammo_Missing,

        /// <summary>Результат: не получилось.</summary>
        Character_Result_Failed,

        /// <summary>Название нового сохранения.</summary>
        SaveSlot_New,

        /// <summary>Имя нового сохранения с номером.</summary>
        SaveSlot_New_Name,

        /// <summary>Подпись пустой ячейки.</summary>
        SaveSlot_Cell,

        /// <summary>Пустая ячейка.</summary>
        SaveSlot_Empty,

        /// <summary>Повреждённый слот.</summary>
        SaveSlot_Corrupted,

        /// <summary>Строка повреждённого слота.</summary>
        SaveSlot_Corrupted_Line,

        /// <summary>Строка прогресса слота.</summary>
        SaveSlot_Progress,

        /// <summary>Подпись страниц.</summary>
        SaveSlot_Pager,

        /// <summary>Ошибка данных: заголовок.</summary>
        Data_Error_Title,

        /// <summary>Ошибка данных: нет данных.</summary>
        Data_Error_Missing,

        /// <summary>Кнопка возврата в меню со старта игры.</summary>
        NewGame_Back,

        /// <summary>Маркер выбранного набора снаряжения.</summary>
        NewGame_Selected_Marker,

        /// <summary>Подпись режима постоянной смерти.</summary>
        NewGame_Permadeath_Label,

        /// <summary>Действие перехода к торговцу с его именем.</summary>
        Game_Trade,

        /// <summary>Заголовок списка товаров торговца.</summary>
        Trader_Section_Buy,

        /// <summary>Заголовок списка продажи.</summary>
        Trader_Section_Sell,

        /// <summary>Кнопка «Купить».</summary>
        Trader_Buy,

        /// <summary>Кнопка «Продать».</summary>
        Trader_Sell,

        /// <summary>Пустой список товаров или продажи.</summary>
        Trader_Empty,

        /// <summary>Остаток товара на складе.</summary>
        Trader_Stock,

        /// <summary>Цена товара.</summary>
        Trader_Price,

        /// <summary>Сообщение об успешной покупке.</summary>
        Trader_Bought,

        /// <summary>Сообщение об успешной продаже.</summary>
        Trader_Sold,

        /// <summary>Торговец недоступен.</summary>
        Trader_Unavailable,

        /// <summary>Нет спрайта портрета торговца.</summary>
        Trader_Portrait_Missing,

        /// <summary>Результат: торговец недоступен.</summary>
        Trader_Result_Unavailable,

        /// <summary>Результат: не хватает денег.</summary>
        Trader_Result_Not_Enough_Money,

        /// <summary>Результат: товар распродан.</summary>
        Trader_Result_No_Stock,

        /// <summary>Результат: нет места в инвентаре.</summary>
        Trader_Result_No_Room,

        /// <summary>Результат: товара нет в ассортименте.</summary>
        Trader_Result_Offer_Missing,

        /// <summary>Результат: предмет не найден.</summary>
        Trader_Result_Item_Missing,

        /// <summary>Результат: предмет не покупают.</summary>
        Trader_Result_Not_Sellable,

        /// <summary>Результат: неверное количество.</summary>
        Trader_Result_Invalid_Count,

        /// <summary>Результат: сделка не состоялась.</summary>
        Trader_Result_Failed,

        /// <summary>Действие «Задания» на основном экране.</summary>
        Game_Quests,

        /// <summary>Действие «Зачистить локацию» на целевом экземпляре.</summary>
        Game_Clear_Location,

        /// <summary>Локация зачищена.</summary>
        Game_Cleared,

        /// <summary>Кнопка «Задания» на экране торговца.</summary>
        Trader_Quests,

        /// <summary>Заголовок журнала заданий.</summary>
        Quest_Title,

        /// <summary>Заголовок заданий торговца с его именем.</summary>
        Quest_Trader_Title,

        /// <summary>Заголовок списка доступных заданий.</summary>
        Quest_Section_Offers,

        /// <summary>Заголовок списка активных заданий.</summary>
        Quest_Section_Active,

        /// <summary>Пустой список активных заданий.</summary>
        Quest_Empty_Active,

        /// <summary>Пустой список доступных заданий.</summary>
        Quest_Empty_Offers,

        /// <summary>Нет торговца для выдачи заданий.</summary>
        Quest_No_Trader,

        /// <summary>Кнопка «Взять».</summary>
        Quest_Take,

        /// <summary>Кнопка «Сдать».</summary>
        Quest_TurnIn,

        /// <summary>Кнопка «Отменить».</summary>
        Quest_Cancel,

        /// <summary>Предложение отменено.</summary>
        Quest_Offer_Cancelled,

        /// <summary>Активный квест отменён.</summary>
        Quest_Cancelled,

        /// <summary>Кнопка «Обновить».</summary>
        Quest_Refresh,

        /// <summary>Сложность задания.</summary>
        Quest_Difficulty,

        /// <summary>Награда задания.</summary>
        Quest_Reward,

        /// <summary>Строка прогресса.</summary>
        Quest_Progress,

        /// <summary>Готово к сдаче.</summary>
        Quest_Ready,

        /// <summary>Задание взято.</summary>
        Quest_Accepted,

        /// <summary>Задание сдано.</summary>
        Quest_Completed,

        /// <summary>Задание на откате.</summary>
        Quest_Cooldown,

        /// <summary>Целевая локация.</summary>
        Quest_Target_Location,

        /// <summary>Целевые актёры.</summary>
        Quest_Target_Actors,

        /// <summary>Целевые предметы.</summary>
        Quest_Target_Items,

        /// <summary>Прогресс по NPC.</summary>
        Quest_Npc_Progress,

        /// <summary>Задание не найдено.</summary>
        Quest_Result_Not_Found,

        /// <summary>Задание не повторяемое.</summary>
        Quest_Result_Not_Repeatable,

        /// <summary>Этот торговец не выдаёт такое задание.</summary>
        Quest_Result_Not_Offered,

        /// <summary>Торговец не найден.</summary>
        Quest_Result_Trader_Not_Found,

        /// <summary>Актёр не торговец.</summary>
        Quest_Result_Not_Trader,

        /// <summary>Действие только в хабе.</summary>
        Quest_Result_Wrong_Location,

        /// <summary>Торговец недоступен.</summary>
        Quest_Result_Unavailable,

        /// <summary>Условия не выполнены.</summary>
        Quest_Result_Requirements,

        /// <summary>Задание уже взято.</summary>
        Quest_Result_Already_Active,

        /// <summary>Слишком много активных заданий.</summary>
        Quest_Result_Active_Limit,

        /// <summary>Задание на откате.</summary>
        Quest_Result_Cooldown,

        /// <summary>Нет подходящих целей.</summary>
        Quest_Result_No_Targets,

        /// <summary>Цель ещё не выполнена.</summary>
        Quest_Result_Not_Ready,

        /// <summary>Задание выдал другой торговец.</summary>
        Quest_Result_Wrong_Giver,

        /// <summary>Нет места для награды.</summary>
        Quest_Result_No_Room,

        /// <summary>Действие недоступно.</summary>
        Quest_Result_Invalid,

        /// <summary>Заголовок экрана талантов.</summary>
        Talent_Title,

        /// <summary>Кнопка открытия дерева талантов.</summary>
        Talent_Open,

        /// <summary>Свободные очки талантов.</summary>
        Talent_Points,

        /// <summary>Сколько очков вложено в дерево.</summary>
        Talent_Spent,

        /// <summary>Ранг таланта: текущий и предельный.</summary>
        Talent_Rank,

        /// <summary>Кнопка изучения таланта.</summary>
        Talent_Learn,

        /// <summary>Талант изучен.</summary>
        Talent_Learned,

        /// <summary>Талант закрыт порогом вложенных очков.</summary>
        Talent_Locked,

        /// <summary>Талант изучен полностью.</summary>
        Talent_Max,

        /// <summary>Подсказка: нажатие на талант изучает его.</summary>
        Talent_Tooltip_Click,

        /// <summary>Дерево талантов пусто.</summary>
        Talent_Empty,

        /// <summary>Прибавка эффекта за один ранг.</summary>
        Talent_Effect_Per_Rank,

        /// <summary>Эффект: восстановление ОД за ход.</summary>
        Talent_Effect_Ap_Restore,

        /// <summary>Эффект: максимум ОД.</summary>
        Talent_Effect_Ap_Max,

        /// <summary>Эффект: максимум здоровья.</summary>
        Talent_Effect_Health,

        /// <summary>Эффект: меткость.</summary>
        Talent_Effect_Accuracy,

        /// <summary>Эффект: выносливость.</summary>
        Talent_Effect_Stamina,

        /// <summary>Эффект: удача.</summary>
        Talent_Effect_Luck,

        /// <summary>Эффект: строки инвентаря.</summary>
        Talent_Effect_Rows,

        /// <summary>Талант не найден.</summary>
        Talent_Result_Unknown,

        /// <summary>Талант уже изучен полностью.</summary>
        Talent_Result_Max,

        /// <summary>Талант ещё закрыт.</summary>
        Talent_Result_Locked,

        /// <summary>Нет свободных очков.</summary>
        Talent_Result_No_Points,

        /// <summary>Действие с талантом недоступно.</summary>
        Talent_Result_Invalid,

        /// <summary>Заголовок диалога лута.</summary>
        Loot_Title,

        /// <summary>Кнопка «Забрать всё».</summary>
        Loot_Take_All,

        /// <summary>Кнопка «Закрыть» диалога лута.</summary>
        Loot_Close,

        /// <summary>Противник ничего не оставил.</summary>
        Loot_Empty,

        /// <summary>Инвентарь полон, часть лута ушла в переполнение.</summary>
        Loot_Full,

        /// <summary>Подсказка диалога лута.</summary>
        Loot_Hint,

        /// <summary>ВРЕМЕННО: подпись блока тестового противника на основном экране.</summary>
        Game_Test_Enemy_Label,

        /// <summary>ВРЕМЕННО: кнопка «Убить» у тестового противника.</summary>
        Game_Test_Enemy_Kill,

        /// <summary>ВРЕМЕННО: тестовый противник убит, опыт получен.</summary>
        Game_Test_Victory,

        /// <summary>ВРЕМЕННО: тестовый бой не удалось завершить.</summary>
        Game_Test_Kill_Failed
    }
}
