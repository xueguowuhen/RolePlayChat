﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
//Set properties in C# example codes.
using RainbowArt.CleanFlatUI;
public class ModalWindowCheckBoxDemo : MonoBehaviour
{  
    //The ModalWindowCheckBox Component.
    public ModalWindowCheckBox m_ModalWindow; 

    //Create a List of new options.
    List<string> mOptions = new List<string> {"Option 1", "Option 2", "Option 3", "Option 4"};  
    
    void Start()
    {
        //Add OnConfirm event listener.
        m_ModalWindow.OnConfirm.AddListener(ModalWindowConfirm);  
        //Add OnCancel event listener.
        m_ModalWindow.OnCancel.AddListener(ModalWindowCancel);           

        //Set start indexes.
        m_ModalWindow.StartSelectedIndexes = new int[] {1,3};  
        //Clear the old options.
        m_ModalWindow.ClearOptions();        
        //Add the new options.
        m_ModalWindow.AddOptions(mOptions);
         
        //Show Modal Window.
        m_ModalWindow.ShowModalWindow(); 
    }    

    void ModalWindowConfirm(int[] selectedIndexes)
    {
        Debug.Log("ModalWindowConfirm, index: ");
        foreach(int index in selectedIndexes)
        {
            Debug.Log(index +",");                
        }
    }

    void ModalWindowCancel(int[] selectedIndexes)
    {
        Debug.Log("ModalWindowCancel");
    }
}
*/

namespace RainbowArt.CleanFlatUI
{
    public class ModalWindowCheckBox : MonoBehaviour
    {
        protected internal class ContentItem : MonoBehaviour
        {
            public TextMeshProUGUI itemText;
            public Image itemImage;
            public Image itemSelect;
            public Image itemCheckmark;
            public Image itemLine;
            public Button button;
        }

        [SerializeField]
        Image iconTitle;

        [SerializeField]
        TextMeshProUGUI title;

        [SerializeField]
        Button buttonClose;

        [SerializeField]
        Button buttonConfirm;

        [SerializeField]
        Button buttonCancel;

        [SerializeField]
        Animator animator;

        [SerializeField]
        RectTransform contentRect;

        [SerializeField]
        GameObject itemTemplate;

        [SerializeField]
        TextMeshProUGUI itemText;

        [SerializeField]
        Image itemImage;

        [SerializeField]
        Image itemSelect;

        [SerializeField]
        Image itemCheckmark;

        [SerializeField]
        Image itemLine;

        [SerializeField]
        RectOffset padding = new RectOffset();

        [SerializeField]
        float spacing = 2;

        [SerializeField]
        List<int> startSelectedIndexes = new List<int>();

        [Serializable]
        public class OptionItem
        {
            public string text;
            public Sprite icon;

            public OptionItem()
            {
            }

            public OptionItem(string newText)
            {
                text = newText;
            }

            public OptionItem(Sprite newImage)
            {
                icon = newImage;
            }
            public OptionItem(string newText, Sprite newImage)
            {
                text = newText;
                icon = newImage;
            }
        }

        [SerializeField]
        List<OptionItem> options = new List<OptionItem>();

        List<ContentItem> contentItems = new List<ContentItem>();

        [Serializable]
        public class ModalWindowEvent : UnityEvent<int[]> { }

        [SerializeField]
        ModalWindowEvent onConfirm = new ModalWindowEvent();

        [SerializeField]
        ModalWindowEvent onCancel = new ModalWindowEvent();

        HashSet<int> selectedIndexes = new HashSet<int>();
        IEnumerator diableCoroutine;
        float disableTime = 0.5f;

        public int[] StartSelectedIndexes
        {
            get
            {
                int[] ret = startSelectedIndexes.ToArray();
                return ret;
            }
            set
            {
                startSelectedIndexes.Clear();
                if (value != null)
                {
                    foreach (int index in value)
                    {
                        if ((index >= 0) && (index < options.Count))
                        {
                            startSelectedIndexes.Add(index);
                        }
                    }
                }
            }
        }

        public int[] SelectedIndexes
        {
            get
            {
                int[] ret = new int[selectedIndexes.Count];
                selectedIndexes.CopyTo(ret);
                Array.Sort(ret);
                return ret;
            }
            set
            {
                selectedIndexes.Clear();
                if (value != null)
                {
                    foreach (int index in value)
                    {
                        if ((index >= 0) && (index < options.Count))
                        {
                            selectedIndexes.Add(index);
                        }
                    }
                }
            }
        }

        public string TitleValue
        {
            get
            {
                if (title != null)
                {
                    return title.text;
                }
                return "";
            }
            set
            {
                if (title != null)
                {
                    title.text = value;
                }
            }
        }

        public Sprite IconValue
        {
            get
            {
                if (iconTitle != null)
                {
                    return iconTitle.sprite;
                }
                return null;
            }
            set
            {
                if (iconTitle != null)
                {
                    if (value != null)
                    {
                        iconTitle.gameObject.SetActive(true);
                        iconTitle.sprite = value;
                    }
                    else
                    {
                        iconTitle.gameObject.SetActive(false);
                        iconTitle.sprite = null;
                    }
                }
            }
        }

        public ModalWindowEvent OnConfirm
        {
            get => onConfirm;
            set
            {
                onConfirm = value;
            }
        }

        public ModalWindowEvent OnCancel
        {
            get => onCancel;
            set
            {
                onCancel = value;
            }
        }

        public void ShowModalWindow()
        {
            gameObject.SetActive(true);
            UpdateSelectIndexes();
            InitButtons();
            InitAnimation();
            DestroyAllItems();
            SetupTemplate();
            CreateAllItems(options);
            UpdateAllItemSelectStatus();
            PlayAnimation(true);
        }

        public bool IsIndexSelected(int index)
        {
            return selectedIndexes.Contains(index);
        }

        public void SetIndexSelected(int index, bool selected)
        {
            if (IsIndexSelected(index) == selected)
            {
                return;
            }
            if (selected)
            {
                selectedIndexes.Add(index);
            }
            else
            {
                selectedIndexes.Remove(index);
            }
        }

        public void UnSelectAll()
        {
            selectedIndexes.Clear();
        }

        public void HideModalWindow()
        {
            PlayAnimation(false);
            if (animator != null)
            {
                if (diableCoroutine != null)
                {
                    StopCoroutine(diableCoroutine);
                    diableCoroutine = null;
                }
                diableCoroutine = DisableTransition();
                StartCoroutine(diableCoroutine);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        IEnumerator DisableTransition()
        {
            yield return new WaitForSeconds(disableTime);
            gameObject.SetActive(false);
        }

        void UpdateSelectIndexes()
        {
            selectedIndexes.Clear();
            foreach (int index in startSelectedIndexes)
            {
                if ((index >= 0) && (index < options.Count))
                {
                    selectedIndexes.Add(index);
                }
            }
        }

        void InitButtons()
        {
            if (buttonClose != null)
            {
                buttonClose.onClick.RemoveAllListeners();
                buttonClose.onClick.AddListener(OnCloseClick);
            }
            if (buttonConfirm != null)
            {
                buttonConfirm.onClick.RemoveAllListeners();
                buttonConfirm.onClick.AddListener(OnConfirmClick);
            }
            if (buttonCancel != null)
            {
                buttonCancel.onClick.RemoveAllListeners();
                buttonCancel.onClick.AddListener(OnCancelClick);
            }
        }

        void OnCloseClick()
        {
            OnCancelClick();
        }

        void OnCancelClick()
        {
            HideModalWindow();
            onCancel.Invoke(null);
        }

        void OnConfirmClick()
        {
            HideModalWindow();
            onConfirm.Invoke(SelectedIndexes);
        }

        void InitAnimation()
        {
            if (animator != null)
            {
                animator.enabled = false;
                animator.gameObject.transform.localScale = Vector3.one;
                animator.gameObject.transform.localEulerAngles = Vector3.zero;
            }
        }

        void PlayAnimation(bool bShow)
        {
            if (animator != null)
            {
                if (animator.enabled == false)
                {
                    animator.enabled = true;
                }
                if (bShow)
                {
                    animator.Play("In", 0, 0);
                }
                else
                {
                    animator.Play("Out", 0, 0);
                }
            }
        }

        public void AddOptions(List<OptionItem> optionList)
        {
            options.AddRange(optionList);
        }

        public void AddOptions(List<string> optionList)
        {
            for (int i = 0; i < optionList.Count; i++)
            {
                options.Add(new OptionItem(optionList[i]));
            }
        }

        public void AddOptions(List<Sprite> optionList)
        {
            for (int i = 0; i < optionList.Count; i++)
            {
                options.Add(new OptionItem(optionList[i]));
            }
        }

        public void ClearOptions()
        {
            options.Clear();
        }

        void SetupTemplate()
        {
            ContentItem contentItem = itemTemplate.GetComponent<ContentItem>();
            if (contentItem == null)
            {
                contentItem = itemTemplate.AddComponent<ContentItem>();
                contentItem.itemText = itemText;
                contentItem.itemImage = itemImage;
                contentItem.itemSelect = itemSelect;
                contentItem.itemCheckmark = itemCheckmark;
                contentItem.itemLine = itemLine;
                contentItem.button = itemTemplate.GetComponent<Button>();
            }
            itemTemplate.SetActive(false);
        }

        void CreateAllItems(List<OptionItem> options)
        {
            float itemWidth = itemTemplate.GetComponent<RectTransform>().rect.width;
            RectTransform templateParentTransform = itemTemplate.transform.parent as RectTransform;
            int dataCount = options.Count;
            float curY = -padding.top;
            for (int i = 0; i < dataCount; ++i)
            {
                OptionItem itemData = options[i];
                int index = i;
                GameObject go = Instantiate(itemTemplate) as GameObject;
                go.transform.SetParent(itemTemplate.gameObject.transform.parent, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.SetActive(true);
                go.name = "Item" + i;
                ContentItem curItem = go.GetComponent<ContentItem>();
                contentItems.Add(curItem);
                curItem.itemText.text = itemData.text;
                if (itemData.icon == null)
                {
                    curItem.itemImage.gameObject.SetActive(false);
                    curItem.itemImage.sprite = null;
                }
                else
                {
                    curItem.itemImage.gameObject.SetActive(true);
                    curItem.itemImage.sprite = itemData.icon;
                }
                if (curItem.itemLine != null)
                {
                    if (i == (dataCount - 1))
                    {
                        curItem.itemLine.gameObject.SetActive(false);
                    }
                    else
                    {
                        curItem.itemLine.gameObject.SetActive(true);
                    }
                }
                curItem.button.onClick.RemoveAllListeners();
                curItem.button.onClick.AddListener(delegate { OnItemClicked(index); });

                RectTransform curRectTransform = go.GetComponent<RectTransform>();
                curRectTransform.anchoredPosition3D = new Vector3(padding.left, curY, 0);
                float curItemHeight = curRectTransform.rect.height;
                curY = curY - curItemHeight;
                if (i < (dataCount - 1))
                {
                    curY = curY - spacing;
                }
            }
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(curY) + padding.bottom);
            contentRect.anchoredPosition3D = new Vector3(0, 0, 0);
        }

        void DestroyAllItems()
        {
            var itemsCount = contentItems.Count;
            for (int i = 0; i < itemsCount; i++)
            {
                if (contentItems[i] != null)
                {
                    Destroy(contentItems[i].gameObject);
                }
            }
            contentItems.Clear();
        }

        void OnItemClicked(int index)
        {
            ContentItem curItem = contentItems[index];
            if (IsIndexSelected(index))
            {
                SetIndexSelected(index, false);
                if (curItem.itemSelect != null)
                {
                    curItem.itemSelect.gameObject.SetActive(false);
                }
                curItem.itemCheckmark.gameObject.SetActive(false);
            }
            else
            {
                SetIndexSelected(index, true);
                if (curItem.itemSelect != null)
                {
                    curItem.itemSelect.gameObject.SetActive(true);
                }
                curItem.itemCheckmark.gameObject.SetActive(true);
            }
        }

        void UpdateAllItemSelectStatus()
        {
            for (int index = 0; index < contentItems.Count; ++index)
            {
                ContentItem curItem = contentItems[index];
                if (IsIndexSelected(index))
                {
                    if (curItem.itemSelect != null)
                    {
                        curItem.itemSelect.gameObject.SetActive(true);
                    }
                    curItem.itemCheckmark.gameObject.SetActive(true);
                }
                else
                {
                    if (curItem.itemSelect != null)
                    {
                        curItem.itemSelect.gameObject.SetActive(false);
                    }
                    curItem.itemCheckmark.gameObject.SetActive(false);
                }
            }
        }
    }
}