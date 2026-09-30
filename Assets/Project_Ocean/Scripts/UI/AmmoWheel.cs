using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class AmmoWheel : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private RectTransform wheel;
        [SerializeField] private GameObject wheelImage;
        [SerializeField] private GameObject ammoWheel;

        public AmmoType currentAmmoType;

        private int ammoCount = 4;

        private int ammoIndex = 0;
        public int AmmoIndex { get { return ammoIndex; } }

        private bool isSelectingAmmo;

        private void Start()
        {
            player.input.OnAmmoSelected += HandleAmmoWheel;
            ammoIndex = 0;

            ammoWheel.SetActive(false);
        }

        private void Update()
        {
            WheelSelected(isSelectingAmmo);
        }

        private void HandleAmmoWheel(bool state)
        {
            isSelectingAmmo = state;
        }

        private void WheelSelected(bool isSelectAmmo)
        {
            if (isSelectAmmo)
            {
                ammoWheel.SetActive(true);

                Vector2 wheelScreenPos = RectTransformUtility.WorldToScreenPoint(null, wheel.position);
                Vector2 dir = (player.input.MousePos - wheelScreenPos);

                if (dir.magnitude < 50f)
                    return;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (angle < 0)
                    angle += 360f;

                float sliceSize = 360f / ammoCount;
                float adjustedAngle = angle + (sliceSize / 2f);

                ammoIndex = Mathf.FloorToInt(adjustedAngle / sliceSize) % ammoCount;

                wheelImage.transform.eulerAngles = new Vector3(0, 0, angle - 90f);

                //Debug.Log($"Index : {ammoIndex}, Angle : {angle}");
            }
            else
            {
                //Debug.Log($"Selected Ammo {ammoIndex}");
                currentAmmoType = (AmmoType)ammoIndex;
                ammoWheel.SetActive(false);
            }
        }
    }
}
