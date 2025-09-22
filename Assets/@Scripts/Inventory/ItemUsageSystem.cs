namespace FpsHorrorKit
{
    using UnityEngine;

    public class ItemUsageSystem : MonoBehaviour
    {
        public static ItemUsageSystem Instance { get; private set; }

        [Header("Items")]
        [SerializeField] private Item itemPhone;

        [Header("Item Objects Flaslight")]
        public GameObject phone;
        public GameObject _light;
        public GameObject _lanternCanvas;

        [SerializeField ]private FpsAssetsInputs _input;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _input = FindAnyObjectByType<FpsAssetsInputs>();
        }
        private void Start()
        {
            // Oyun başladığında flashlight durumunu güncelle
            itemPhone.hasItem = true;
            itemPhone.canUseItem = false;
            itemPhone.isUsingItem = false;
            if (itemPhone.energyLevel > 0)
            {
                itemPhone.isEnergyEnough = true;
            }
            else
            {
                itemPhone.isEnergyEnough = false;
            }
        }
        private void Update()
        {
            CheckInputSelect();
            CheckInputUse();
        }

        private void CheckInputSelect()
        {
            if (_input.itemIndex == 1 && _input.isPressed)
            {
                SelectFlashlight();
                _input.isPressed = false;
                return;
            }
            if (_input.itemIndex == 2 && _input.isPressed)
            {
                DiSelectFlashlight();
                _input.isPressed = false;
                return;
            }
            if (_input.itemIndex == 3 && _input.isPressed)
            {
                DiSelectFlashlight();
                _input.isPressed = false;
                return;
            }
            if (_input.itemIndex == 4 && _input.isPressed)
            {
                DiSelectFlashlight();
                _input.isPressed = false;
                return;
            }
        }
        public void CheckInputUse()
        {
            if (_input.useFlashlight)
            {
                UseFlashlight();
                _input.useFlashlight = false;
            }

        }
        public void SelectFlashlight()
        {
            if (phone == null) { Debug.LogError("Flashlight Object not found!"); return; }
            if (_light == null) { Debug.LogError("Flashlight Light not found!"); return; }
            if (_lanternCanvas == null) { Debug.LogError("Flashlight Canvas not found!"); return; }

            if (itemPhone.hasItem)
            {
                itemPhone.canUseItem = _input.isSelectedItem;
                itemPhone.isUsingItem = false; // Bu değer fener seçildiğinde değil kullanılmaya başlandığında true olacak.

                phone.SetActive(_input.isSelectedItem);
                _lanternCanvas.SetActive(_input.isSelectedItem);
                _light.SetActive(false);
            }
        }
        public void DiSelectFlashlight()
        {
            if (phone == null) { Debug.LogError("Flashlight Object not found!"); return; }
            if (_light == null) { Debug.LogError("Flashlight Light not found!"); return; }
            if (_lanternCanvas == null) { Debug.LogError("Flashlight Canvas not found!"); return; }

            if (itemPhone.hasItem)
            {
                itemPhone.canUseItem = false;
                itemPhone.isUsingItem = false;

                phone.SetActive(false);
                _lanternCanvas.SetActive(false);
                _light.SetActive(false);
            }
        }
        public void UseFlashlight()
        {
            if (phone == null) { Debug.LogError("Flashlight Object not found!"); return; }
            if (_light == null) { Debug.LogError("Flashlight Light not found!"); return; }
            if (_lanternCanvas == null) { Debug.LogError("Flashlight Canvas not found!"); return; }

            if (itemPhone.hasItem && itemPhone.canUseItem)
            {
                itemPhone.isUsingItem = !itemPhone.isUsingItem;

                if (itemPhone.isEnergyEnough)
                {
                    _light.SetActive(!_light.activeSelf);
                }
            }
        }


    }
}