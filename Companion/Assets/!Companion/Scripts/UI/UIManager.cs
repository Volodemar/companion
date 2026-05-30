using System.Collections.Generic;
using System.Linq;
using Companion.Core;
using UnityEngine;

namespace Companion.UI
{
	/// <summary>
	/// Центральный менеджер UI: хранит окна и попапы, даёт к ним доступ.
	/// </summary>
	public class UIManager : BaseGameObject
	{
		public static UIManager Instance { get; private set; }

		private List<UIWindow> uiWindows = new List<UIWindow>();

		private List<UIPopup> uIPopups = new List<UIPopup>();

		private void Awake()
		{
			Instance = this;
		}

		public void Init()
		{
			// Найти все дочерние окна и попапы
			foreach (var window in this.transform.GetComponentsInChildren<UIWindow>(true))
			{
				uiWindows.Add(window);
			}

			foreach (var popup in this.transform.GetComponentsInChildren<UIPopup>(true))
			{
				uIPopups.Add(popup);
			}
		}

		/// <summary>
		/// Возвращает окно по типу. Если окна нет, возвращает null.
		/// </summary>
		public T GetWindow<T>() where T : UIWindow
		{
			foreach (var window in uiWindows)
			{
				if (window is T windowsType)
				{
					return windowsType;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает окно по имени типа (например "UIWindowMain"). Если окна нет, возвращает null.
		/// </summary>
		public UIWindow GetWindowByName(string typeName)
		{
			foreach (var window in uiWindows)
			{
				if (window.GetType().Name == typeName)
				{
					return window;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает попап по типу. Если попапа нет, возвращает null.
		/// </summary>
		public T GetPopup<T>() where T : UIPopup
		{
			foreach (var popup in uIPopups)
			{
				if (popup is T popupType)
				{
					return popupType;
				}
			}
			return null;
		}

		public void HideAllUIWindows(UIWindow except = null)
		{
			foreach (var window in uiWindows.Where(w => w.gameObject.activeSelf && w != except))
			{
				window.Hide();
			}
		}

		public void HideAllUIPopups()
		{
			foreach (var popup in uIPopups.Where(popup => popup.gameObject.activeSelf))
			{
				if (popup.IsCanHide)
					popup.Hide();
			}
		}
	}
}
