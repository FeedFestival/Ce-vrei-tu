using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCategoryPanelController : MonoBehaviour
{
    public List<CategoryButton> CategoryButtons;

    public InGameClock InGameClock;

    public delegate void OnChosingCategoryCallback(int categoryId);
    public OnChosingCategoryCallback OnChosingCategory;

    // Start is called before the first frame update
    public void Init(List<Category> categories)
    {
        
        for (int i = 0; i < CategoryButtons.Count; i++)
        {
            if (i > (categories.Count - 1))
                CategoryButtons[i].SetActive(false);
            else
            {
                CategoryButtons[i].SetActive(true, categories[i].Name, categories[i].Color, categories[i].Id, (categoryId) => {

                    // do a nice animation 
                    // then
                    OnChosingCategory(categoryId);
                });
            }
        }

        InGameClock.InitClock(30f);
        InGameClock.StartClock();
    }
}
