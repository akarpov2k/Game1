#nullable enable
namespace AnomalyPath.Runtime.Screens.Talents
{
    /// <summary>Размеры и тайминги экрана талантов в единицах канваса.</summary>
    public static class TalentMetrics
    {
        /// <summary>Боковой отступ.</summary>
        public const float Side = 40f;

        /// <summary>Верх заголовка.</summary>
        public const float TitleTop = 20f;

        /// <summary>Высота заголовка.</summary>
        public const float TitleHeight = 56f;

        /// <summary>Верх строки очков.</summary>
        public const float PointsTop = 24f;

        /// <summary>Верх описания дерева.</summary>
        public const float DescriptionTop = 86f;

        /// <summary>Высота описания дерева.</summary>
        public const float DescriptionHeight = 52f;

        /// <summary>Верх прокручиваемого дерева.</summary>
        public const float ListTop = 150f;

        /// <summary>Высота окна дерева.</summary>
        public const float ListHeight = 760f;

        /// <summary>Ширина окна дерева.</summary>
        public const float ListWidth = 1000f;

        /// <summary>Ширина карточки таланта.</summary>
        public const float NodeWidth = 160f;

        /// <summary>Высота карточки таланта: иконка, подпись и рамка.</summary>
        public const float NodeHeight = 184f;

        /// <summary>Сторона иконки таланта.</summary>
        public const float IconSize = 120f;

        /// <summary>Отступ иконки от края карточки: он же рамка.</summary>
        public const float IconInset = 10f;

        /// <summary>Ширина плашки ранга.</summary>
        public const float RankWidth = 52f;

        /// <summary>Высота плашки ранга.</summary>
        public const float RankHeight = 26f;

        /// <summary>Верх подписи под иконкой.</summary>
        public const float NodeTitleTop = IconInset + IconSize + 6f;

        /// <summary>Высота подписи под иконкой.</summary>
        public const float NodeTitleHeight = NodeHeight - NodeTitleTop;

        /// <summary>Зазор между колонками дерева.</summary>
        public const float ColumnGap = 40f;

        /// <summary>Шаг колонки дерева: карточка и зазор.</summary>
        public const float ColumnStep = NodeWidth + ColumnGap;

        /// <summary>Зазор между строками дерева.</summary>
        public const float RowGap = 16f;

        /// <summary>Шаг строки дерева: карточка и зазор.</summary>
        public const float RowStep = NodeHeight + RowGap;

        /// <summary>Минимальный отступ дерева от левого края окна.</summary>
        public const float PaddingX = 40f;

        /// <summary>Отступ первой карточки сверху.</summary>
        public const float PaddingTop = 24f;

        /// <summary>Отступ под последней карточкой.</summary>
        public const float PaddingBottom = 24f;

        /// <summary>Сколько держать курсор на таланте до показа подсказки, секунды.</summary>
        public const float TooltipDelay = 1f;

        /// <summary>Ширина подсказки.</summary>
        public const float TooltipWidth = 360f;

        /// <summary>Смещение подсказки от курсора.</summary>
        public const float TooltipOffset = 18f;

        /// <summary>Отступ подсказки от края экрана.</summary>
        public const float TooltipMargin = 8f;

        /// <summary>Ширина кнопки изучения.</summary>
        public const float ButtonWidth = 180f;

        /// <summary>Высота кнопки изучения.</summary>
        public const float ButtonHeight = 44f;

        /// <summary>Высота нижней кнопки.</summary>
        public const float BottomButtonHeight = 52f;

        /// <summary>Ширина связи между талантами.</summary>
        public const float LinkWidth = 4f;
    }
}
