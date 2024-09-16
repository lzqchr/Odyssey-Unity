using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
	/// <summary>
	/// 管理和显示保存cardList
	/// </summary>
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save List")]
	public class UISaveList : MonoBehaviour
	{
		public bool focusFirstElement = true;
		public UISaveCard card;
		public RectTransform container;

		protected List<UISaveCard> m_cardList = new List<UISaveCard>();

		protected virtual void Awake()
		{
			var data = GameSaver.instance.LoadList();

			for (int i = 0; i < data.Length; i++)
			{
				//复制卡牌 并且赋值
				m_cardList.Add(Instantiate(this.card, container));
				m_cardList[i].Fill(i, data[i]);
			}

			if (focusFirstElement)
			{
				if (m_cardList[0].isFilled)
				{
					//设置选中
					EventSystem.current.SetSelectedGameObject(m_cardList[0].loadButton.gameObject);
				}
				else
				{
					EventSystem.current.SetSelectedGameObject(m_cardList[0].newGameButton.gameObject);
				}
			}
		}
	}
}
