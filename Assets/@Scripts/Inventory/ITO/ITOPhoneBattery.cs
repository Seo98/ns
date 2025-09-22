using UnityEngine;

namespace FpsHorrorKit
{
    public class ITOPhoneBattery : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item phoneBattey;
        [SerializeField] private string interactText = "Take lantern fuel [E]";

        public void Interact()
        {
            bool result = Inventory.Instance.AddItem(phoneBattey, 1);
            if (result)
            {
                InteractMessageScript.Instance?.ShowMessage("Lantern fuel taken! To use, open inventory(press I) and press the use button");
                UIInventory.Instance.UpdateUI();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
        public void Highlight()
        {
            PlayerInteract.Instance.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}