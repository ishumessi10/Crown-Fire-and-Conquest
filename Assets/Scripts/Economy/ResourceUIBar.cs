using UnityEngine;
using TMPro;

public class ResourceUIBar : MonoBehaviour {
    public TMP_Text foodText, woodText, fibreText, metalText, goldText;

    bool subscribed = false;

    void OnEnable(){
        TrySubscribe();
        RefreshAll();
    }

    void Start(){
        // In case OnEnable ran before ResourceBank existed
        TrySubscribe();
        RefreshAll();
    }

    void Update(){
        // If the bank gets created later (scene load order), auto-subscribe once
        if (!subscribed) {
            TrySubscribe();
            if (subscribed) RefreshAll();
        }
    }

    void OnDisable(){
        if (ResourceBank.I != null)
            ResourceBank.I.OnChanged -= HandleChanged;
        subscribed = false;
    }

    void TrySubscribe(){
        if (subscribed) return;
        if (ResourceBank.I == null) return;
        ResourceBank.I.OnChanged += HandleChanged;
        subscribed = true;
    }

    void HandleChanged(ResourceType t, int v){
        switch (t){
            case ResourceType.Food:  if (foodText)  foodText.text  = v.ToString(); break;
            case ResourceType.Wood:  if (woodText)  woodText.text  = v.ToString(); break;
            case ResourceType.Fibre: if (fibreText) fibreText.text = v.ToString(); break;
            case ResourceType.Metal: if (metalText) metalText.text = v.ToString(); break;
            case ResourceType.Gold:  if (goldText)  goldText.text  = v.ToString(); break;
        }
    }

    public void RefreshAll(){
        if (ResourceBank.I == null) return;
        HandleChanged(ResourceType.Food,  ResourceBank.I.Get(ResourceType.Food));
        HandleChanged(ResourceType.Wood,  ResourceBank.I.Get(ResourceType.Wood));
        HandleChanged(ResourceType.Fibre, ResourceBank.I.Get(ResourceType.Fibre));
        HandleChanged(ResourceType.Metal, ResourceBank.I.Get(ResourceType.Metal));
        HandleChanged(ResourceType.Gold,  ResourceBank.I.Get(ResourceType.Gold));
    }
}
