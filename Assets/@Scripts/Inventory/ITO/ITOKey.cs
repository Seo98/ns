namespace FpsHorrorKit
{
    using UnityEngine;

    public class ITOKey : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item key;
        public DoorSystem compatibleDoor;
        [SerializeField] private string interactText = "Take key [E]";

        public void Interact()
        {
            compatibleDoor.hasKey = true;
            bool result = Inventory.Instance.AddItem(key, 1);
            if (result)
            {
                InteractMessageScript.Instance?.ShowMessage("Get Key");
                UIInventory.Instance.UpdateUI();
                Destroy(gameObject);
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