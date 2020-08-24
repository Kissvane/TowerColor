using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColorManager : MonoBehaviour
{
    public List<Color> availableColorList;
    public List<Material> availableMaterials;
    public List<Color> selectedColorForThisLevel;
    public Material baseTowerPartMaterial;
    public List<Material> selectedTowerPartMaterials;
    public GameObject ball;
    

    public void Awake()
    {
        int level = 1;
        if (PlayerPrefs.HasKey("Level"))
        {
            level = PlayerPrefs.GetInt("Level");
        }

        int minNumberColorsAdded = level / 10;

        SelectColorForThisLevel(Mathf.Min(2+minNumberColorsAdded,6));
        MyEventSystem.instance.RegisterDynamicData("GetRandomSelectedColorIndex", this, GetRandomSelectedColorIndex);
    }

    public void SelectColorForThisLevel(int numberToSelect)
    {
        List<Color> validAvailableColorList = new List<Color>();
        validAvailableColorList.AddRange(availableColorList);

        for (int i = 0; i < numberToSelect; i++)
        {
            int randomIndex = Random.Range(0, validAvailableColorList.Count);
            Color color = validAvailableColorList[randomIndex];
            selectedColorForThisLevel.Add(color);
            validAvailableColorList.RemoveAt(randomIndex);
            Material mat = new Material(baseTowerPartMaterial);
            mat.color = color;
            selectedTowerPartMaterials.Add(mat);
        }

        MyEventSystem.instance.Set("selectedTowerPartMaterials", selectedTowerPartMaterials);
        MyEventSystem.instance.Set("selectedColorForThisLevel", selectedColorForThisLevel);
    }

    dynamic GetRandomSelectedColorIndex(GenericDictionary args = null)
    {
        return Random.Range(0, selectedColorForThisLevel.Count);

    }
}
