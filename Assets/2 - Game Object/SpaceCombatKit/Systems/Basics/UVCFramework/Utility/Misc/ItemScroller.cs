using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ItemScroller : MonoBehaviour
{

    [Header("Settings")]

    [SerializeField]
    protected Transform scrollContents;

    [SerializeField]
    protected float itemSpacing = 350;
  
    [SerializeField]
    protected int numItems;
    
    protected int scrollCurrentIndex;
    protected int scrollTargetIndex;
    protected Vector3 contentsStartPos = Vector3.zero;
    protected float scrollStartTime;
    protected bool scrolling = false;

    [SerializeField]
    protected int numItemsShowing;

    [SerializeField]
    protected GameObject scrollLeftButton;

    [SerializeField]
    protected GameObject scrollRightButton;

    


    private void Awake()
    {
        contentsStartPos = scrollContents.localPosition;
        UpdateScrollButtons();
    }

    public void StartScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    void UpdateScrollButtons()
    {
        // Enable/disable left scroll button
        if (scrollTargetIndex == 0)
        {
            scrollLeftButton.SetActive(false);
        }
        else
        {
            scrollLeftButton.SetActive(true);
        }

        // Enable/disable right scroll button
        if (scrollTargetIndex == Mathf.Max(0, numItems - numItemsShowing))
        {
            scrollRightButton.SetActive(false);
        }
        else
        {
            scrollRightButton.SetActive(true);
        }
    }

    
    public void ScrollBasicDemoButtons(bool scrollRight)
    {
        if (scrolling) return;
        scrollTargetIndex = Mathf.Clamp(scrollRight ? scrollCurrentIndex + 1 : scrollCurrentIndex - 1, 0, Mathf.Max(0, numItems - numItemsShowing));

        UpdateScrollButtons();

        scrollStartTime = Time.time;        
        scrolling = true;
        
    }
    
    private void Update()
    {
        if (scrolling)
        {
            if (Time.time - scrollStartTime < 0.5)
            {
                Vector3 pos = scrollContents.localPosition;
                pos.x = Mathf.Lerp(scrollCurrentIndex * -itemSpacing, scrollTargetIndex * -itemSpacing, (Time.time - scrollStartTime) / 0.5f);
                scrollContents.localPosition = pos;
            }
            else
            {
                Vector3 pos = scrollContents.localPosition;
                pos.x = scrollTargetIndex * -itemSpacing;
                scrollContents.localPosition = pos;
                scrollCurrentIndex = scrollTargetIndex;
                scrolling = false;
            }
        }
    }
}
