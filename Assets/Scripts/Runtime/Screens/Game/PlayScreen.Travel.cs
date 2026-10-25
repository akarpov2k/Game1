#nullable enable
using System;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// Часть класса <see cref="PlayScreen"/>: переходы по маршруту, выход из хаба,
    /// вскрытие тайника и сообщения в нижней полосе.
    /// </summary>
    public sealed partial class PlayScreen
    {
        /// <summary>Показывает всплывающее сообщение в нижней полосе.</summary>
        /// <param name="text">Текст сообщения.</param>
        /// <param name="error">Показать сообщение как ошибку.</param>
        public void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? PlayPalette.Error : UiFactory.ToastColor;
        }

        private void OpenStash()
        {
            var session = _host.Session;
            if (session == null)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Common_No_Game), error: true);
                return;
            }

            StashOpenResult result;
            try
            {
                result = _host.Sessions.OpenStash(session);
            }
            catch (Exception error)
            {
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Game_Travel_Error, error.Message), error: true);
                return;
            }

            if (result.Status == StashOpenStatus.NoStash)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Stash_None), error: true);
                return;
            }

            if (result.Status == StashOpenStatus.AlreadyOpened)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Stash_Opened), error: true);
                Show();
                return;
            }

            var location = session.State.World.CurrentLocation;
            location.MarkObjectLooted();
            Show();
            if (result.Jackpot)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Jackpot));
            }

            if (result.Loot.Count > 0)
            {
                _host.ShowLoot(result.Loot, Show);
            }
        }

        private void Travel(string edgeId)
        {
            var session = _host.Session;
            if (session == null)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Common_No_Game), error: true);
                return;
            }

            TravelResult result;
            try
            {
                result = _host.Sessions.Travel(session, edgeId);
            }
            catch (Exception error)
            {
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Game_Travel_Error, error.Message), error: true);
                return;
            }

            if (!result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, PlayText.Travel(result.Status)), error: true);
                return;
            }

            Show();
            ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Travel_Success));
        }

        private void LeaveHub()
        {
            var session = _host.Session;
            if (session == null)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Common_No_Game), error: true);
                return;
            }

            TravelResult result;
            try
            {
                result = _host.Sessions.LeaveHub(session);
            }
            catch (Exception error)
            {
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Game_Travel_Error, error.Message), error: true);
                return;
            }

            if (!result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, PlayText.Travel(result.Status)), error: true);
                return;
            }

            Show();
            ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Travel_Success));
        }

        private void ReturnToHub()
        {
            var session = _host.Session;
            if (session == null)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Common_No_Game), error: true);
                return;
            }

            var result = _host.Sessions.ReturnToHub(session);
            if (!result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, PlayText.Travel(result.Status)), error: true);
                return;
            }

            Show();
            ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Travel_Success));
        }
    }
}
