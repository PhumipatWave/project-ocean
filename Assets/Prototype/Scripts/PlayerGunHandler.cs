using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Game.Testing
{
    public class PlayerGunHandler : MonoBehaviour
    {
        [Header("Gun Object")]
        [SerializeField] private Transform gunSprite; 
        public Transform GunSprite {  get { return gunSprite; } }

        [SerializeField] private Transform bulletPrefab;
        [SerializeField] private Transform firePoint;
        public Transform FirePoint { get { return firePoint; } }

        [SerializeField] private Transform currentBulletObj;
        public Transform CurrentBulletObj { get { return currentBulletObj; } }

        [Header("Gun Status")]
        private Vector2 aimDir;
        public Vector2 AimDir { get { return aimDir; }  }

        private bool isAimming;
        public bool IsAimming {  get { return isAimming; } set { isAimming = value; } }

        private bool canAttack = true;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        private float HandlerGunRotation(int faceDir, float angle)
        {
            // Clamp angle by face direction
            float clampAngle;
            if (faceDir == 1)
            {
                clampAngle = Mathf.Clamp(angle, -45f, 45f);
            }
            else
            {
                if (angle > 0f)
                {
                    clampAngle = Mathf.Clamp(angle, 135f, 180f);
                }
                else
                {
                    clampAngle = Mathf.Clamp(angle, -180f, -135f);
                }
            }

            return clampAngle;
        }

        private void ResetGunRotation(int faceDir)
        {
            if (faceDir == 1)
            {
                gunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                gunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        public void Aimming(int faceDir, Vector2 mousePos)
        {
            if (isAimming)
            {
                if (!canAttack)
                {
                    ResetGunRotation(faceDir);

                    isAimming = false;
                    return;
                }

                Vector2 aimScreenPos = mousePos; //input.MousePos;
                // Convert screen position to world position
                Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(aimScreenPos);

                // Calculate direction vector pointing from player to mouse
                aimDir = (aimWorldPos - (Vector2)transform.position).normalized;
                // Convert direction to radian and convert to degree for use in unity rotation
                float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

                gunSprite.eulerAngles = new Vector3(0, 0, HandlerGunRotation(faceDir, angle));
            }
            else
            {
                ResetGunRotation(faceDir);
            }
        }

        public void Shooting()
        {
            currentBulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            currentBulletObj.GetComponent<Bullet>().Initialize(aimDir);
        }
    }
}
