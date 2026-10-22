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

        /// <summary>Подпись потери лута.</summary>
        NewGame_Loot_Loss_Label,

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
        Data_Error_Missing
    }
}
